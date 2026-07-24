"""MONAI + nnU-Net/BraTS backend (ADR-010) — TODO(radiology-model).

Etkinleştirme:
  1. pip install -r requirements-ml.txt   (torch + monai, ~2-3 GB)
  2. BraTS eğitimli ağırlıkları MONAI Model Zoo'dan indirip MODEL_DIR'e koy
  3. RADIOLOGY_BACKEND=monai

Çıktı yine yalnızca bilgilendiricidir: DifferentialDiagnosis'u beslemez,
confidence eşiği mantığına dahil edilmez (bkz. ADR-010 zorunlu sınırlar).
Yeni bir açık kaynak model entegrasyonu YENİ BİR ADR gerektirir.
"""

from app.schemas import Finding

MODEL_NAME = "nnUNet-BraTS-v1"


def analyze(dicom_files: list[bytes]) -> list[Finding]:
    raise NotImplementedError(
        "MONAI backend henüz etkin değil — requirements-ml.txt kurulumu ve "
        "model ağırlıkları gerekli (TODO(radiology-model))."
    )
