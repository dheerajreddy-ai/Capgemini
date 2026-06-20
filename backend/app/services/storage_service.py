"""Cloudflare R2 image storage via the S3-compatible API.

Objects are stored under:
  storyboards/{storyboard_id}/shots/{shot_id}.jpg

The public URL served through the user's R2 custom domain (ASSET_CDN_BASE) is
what we save in shots.image_url — zero egress cost compared to S3.

Demo / missing-config mode: upload() returns None when R2 is not configured.
"""

from __future__ import annotations

import uuid

from app.core.config import get_settings
from app.core.logging import get_logger

log = get_logger(__name__)


def _build_key(storyboard_id: uuid.UUID, shot_id: uuid.UUID) -> str:
    return f"storyboards/{storyboard_id}/shots/{shot_id}.jpg"


def upload(
    image_bytes: bytes,
    storyboard_id: uuid.UUID,
    shot_id: uuid.UUID,
) -> str | None:
    """Upload image bytes to R2 and return the public CDN URL, or None if not configured."""
    settings = get_settings()

    if not all([settings.R2_ACCOUNT_ID, settings.R2_ACCESS_KEY_ID, settings.R2_SECRET_ACCESS_KEY, settings.R2_BUCKET_NAME]):
        log.info("r2_demo_mode", reason="R2 credentials not fully configured")
        return None

    # Lazy import keeps boto3 out of the import path when not needed.
    import boto3  # noqa: PLC0415

    key = _build_key(storyboard_id, shot_id)
    endpoint = f"https://{settings.R2_ACCOUNT_ID}.r2.cloudflarestorage.com"

    s3 = boto3.client(
        "s3",
        endpoint_url=endpoint,
        aws_access_key_id=settings.R2_ACCESS_KEY_ID,
        aws_secret_access_key=settings.R2_SECRET_ACCESS_KEY,
        region_name="auto",
    )

    s3.put_object(
        Bucket=settings.R2_BUCKET_NAME,
        Key=key,
        Body=image_bytes,
        ContentType="image/jpeg",
        CacheControl="public, max-age=31536000, immutable",
    )

    cdn_base = settings.ASSET_CDN_BASE.rstrip("/")
    public_url = f"{cdn_base}/{key}"
    log.info("r2_upload_success", key=key, url=public_url)
    return public_url
