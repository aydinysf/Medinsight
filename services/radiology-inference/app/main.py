"""Radiology Inference Service — bağımsız Python mikroservisi (ADR-010).

MedInsight .NET AI Orchestration katmanı, iç ağdan POST /inference çağırır.
Çıktı MVP'de yalnızca bilgilendirici katmandır; zorunlu disclaimer kontrattan
çıkarılamaz (bkz. docs/architecture/radiology-inference-service.md).
"""

import os

import httpx
from fastapi import FastAPI, HTTPException

from app.schemas import InferenceRequest, InferenceResponse

app = FastAPI(title="MedInsight Radiology Inference", version="0.1.0")

BACKEND = os.getenv("RADIOLOGY_BACKEND", "stub")

if BACKEND == "monai":
    from app.backends import monai_backend as backend
else:
    from app.backends import stub as backend


@app.get("/health")
def health() -> dict:
    return {"status": "ok", "backend": BACKEND, "model": backend.MODEL_NAME}


@app.post("/inference", response_model=InferenceResponse)
async def inference(request: InferenceRequest) -> InferenceResponse:
    dicom_files: list[bytes] = []
    async with httpx.AsyncClient(timeout=60) as client:
        for url in request.dicomSeriesUrls:
            try:
                response = await client.get(url)
                response.raise_for_status()
                dicom_files.append(response.content)
            except httpx.HTTPError as exc:
                raise HTTPException(status_code=422, detail=f"DICOM indirilemedi: {exc}") from exc

    try:
        findings = backend.analyze(dicom_files)
    except NotImplementedError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc

    # Kontrat garantisi: disclaimer'sız bulgu asla dönmez.
    for finding in findings:
        if not finding.disclaimer:
            raise HTTPException(status_code=500, detail="Disclaimer eksik — kontrat ihlali.")

    return InferenceResponse(studyId=request.studyId, findings=findings)
