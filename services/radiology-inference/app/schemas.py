"""API kontratı — bkz. docs/architecture/radiology-inference-service.md (ADR-010).

Disclaimer alanı kontrattan ÇIKARILAMAZ: klinik olarak doğrulanmamış açık kaynak
model çıktısı, tek başına karar dayanağı olamaz.
"""

from pydantic import BaseModel, Field

MANDATORY_DISCLAIMER = (
    "Bu bulgu, klinik olarak doğrulanmamış açık kaynaklı bir model tarafından "
    "üretilmiştir; tek başına karar dayanağı olamaz."
)


class InferenceRequest(BaseModel):
    studyId: str
    dicomSeriesUrls: list[str] = Field(min_length=1)


class Finding(BaseModel):
    findingId: str
    modelName: str
    modelSource: str = "OpenSource"
    outputType: str  # "Segmentation" | "Classification"
    description: str
    rawOutput: dict
    disclaimer: str = MANDATORY_DISCLAIMER


class InferenceResponse(BaseModel):
    studyId: str
    findings: list[Finding]
