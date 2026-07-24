"""Deterministik stub backend — GPU/ağırlık gerektirmez.

Gerçek model (MONAI + nnU-Net/BraTS) monai_backend.py'de; RADIOLOGY_BACKEND=monai
ve requirements-ml.txt kurulumuyla etkinleşir (TODO(radiology-model)).
DICOM metadata'sını gerçekten okur; yorum/segmentasyon ÜRETMEZ.
"""

import io
import uuid

import pydicom

from app.schemas import Finding

MODEL_NAME = "stub-metadata-inspector-0.1"


def analyze(dicom_files: list[bytes]) -> list[Finding]:
    modalities: set[str] = set()
    slice_count = 0
    pixel_stats: dict = {}

    for content in dicom_files:
        try:
            ds = pydicom.dcmread(io.BytesIO(content), force=True)
        except Exception:
            continue

        slice_count += 1
        modality = getattr(ds, "Modality", None)
        if modality:
            modalities.add(str(modality))

        if not pixel_stats and "PixelData" in ds:
            try:
                arr = ds.pixel_array
                pixel_stats = {
                    "shape": list(arr.shape),
                    "min": int(arr.min()),
                    "max": int(arr.max()),
                }
            except Exception:
                pixel_stats = {}

    if slice_count == 0:
        return []

    raw = {
        "backend": "stub",
        "sliceCount": slice_count,
        "modalities": sorted(modalities),
        "pixelStats": pixel_stats or None,
    }

    description = (
        f"{'/'.join(sorted(modalities)) or 'Bilinmeyen'} çalışması işlendi: "
        f"{slice_count} kesit okundu. (Stub backend — segmentasyon üretilmedi.)"
    )

    return [
        Finding(
            findingId=str(uuid.uuid4()),
            modelName=MODEL_NAME,
            outputType="Classification",
            description=description,
            rawOutput=raw,
        )
    ]
