"""Nano Banana Pro — AI image generation from cinematography prompts.

Sends a shot's AI prompt to Nano Banana Pro and returns the raw image bytes
for upload to Cloudflare R2. Cost: ~$0.02 per image.

Demo mode: when NANO_BANANA_API_KEY is not set the function returns None so the
pipeline skips image generation and the mobile shows its letterbox placeholder.
This mirrors the Gemini demo-mode pattern — the full app flow works without a key.

API contract (Nano Banana Pro v1):
  POST {NANO_BANANA_API_URL}/v1/images/generate
  Authorization: Bearer <key>
  {
    "model":  "stable-diffusion-xl-base",
    "prompt": "...",
    "negative_prompt": "watermark, text, logo, cropped, low quality",
    "width":  1432,
    "height": 600,
    "num_outputs": 1,
    "guidance_scale": 7.5,
    "num_inference_steps": 30
  }
  Response 200:
    {"images": [{"url": "https://cdn.nanobananapro.com/..."}]}
  or with base64:
    {"images": [{"base64": "<data>"}]}

If the shape of the response ever changes, only _parse_response() needs updating.
"""

from __future__ import annotations

import base64

import httpx

from app.core.config import get_settings
from app.core.logging import get_logger

log = get_logger(__name__)

_NEGATIVE_PROMPT = (
    "watermark, text, logo, cropped, deformed, ugly, blurry, low quality, "
    "oversaturated, cartoon, anime, drawing, worst quality, signature"
)


def _parse_response(body: dict) -> bytes:
    """Extract image bytes from the API response, handling URL or base64 forms."""
    images = body.get("images") or body.get("output") or []
    if not images:
        raise RuntimeError("No images in Nano Banana response.")

    first = images[0]

    # Base64 inline (fastest — no second request needed).
    if isinstance(first, dict) and first.get("base64"):
        data = first["base64"]
        # Strip data-URI prefix if present.
        if "," in data:
            data = data.split(",", 1)[1]
        return base64.b64decode(data)

    # URL form — download the image.
    url = first.get("url") if isinstance(first, dict) else first
    if not url:
        raise RuntimeError("Nano Banana response contained no image URL or base64.")

    resp = httpx.get(url, timeout=30, follow_redirects=True)
    resp.raise_for_status()
    return resp.content


def generate_image(prompt: str) -> bytes | None:
    """Return JPEG/PNG image bytes for the given prompt, or None in demo mode."""
    settings = get_settings()

    if not settings.NANO_BANANA_API_KEY:
        log.info("nano_banana_demo_mode", reason="NANO_BANANA_API_KEY not set")
        return None

    url = f"{settings.NANO_BANANA_API_URL.rstrip('/')}/v1/images/generate"
    payload = {
        "model": settings.NANO_BANANA_MODEL,
        "prompt": prompt,
        "negative_prompt": _NEGATIVE_PROMPT,
        "width": settings.NANO_BANANA_IMAGE_WIDTH,
        "height": settings.NANO_BANANA_IMAGE_HEIGHT,
        "num_outputs": 1,
        "guidance_scale": 7.5,
        "num_inference_steps": 30,
    }
    headers = {
        "Authorization": f"Bearer {settings.NANO_BANANA_API_KEY}",
        "Content-Type": "application/json",
    }

    log.info("nano_banana_request", model=settings.NANO_BANANA_MODEL)
    try:
        resp = httpx.post(url, json=payload, headers=headers, timeout=90)
        resp.raise_for_status()
    except httpx.TimeoutException as exc:
        raise RuntimeError("Nano Banana API timed out.") from exc
    except httpx.HTTPStatusError as exc:
        log.error(
            "nano_banana_api_error",
            status=exc.response.status_code,
            body=exc.response.text[:400],
        )
        raise RuntimeError(f"Nano Banana API error ({exc.response.status_code}).") from exc

    body = resp.json()
    image_bytes = _parse_response(body)
    log.info("nano_banana_success", size_kb=round(len(image_bytes) / 1024, 1))
    return image_bytes
