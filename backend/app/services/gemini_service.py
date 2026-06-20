"""Gemini 2.5 Flash — cinematography prompt builder.

Translates a plain-English scene description into a structured shot list with
professional cinematography details and beginner-friendly explanations.

When GEMINI_API_KEY is not set the service returns demo data so the full app
flow works without an API key during development (mirrors SuperFit's mock-auth
pattern). Set the key in .env or Railway vars to get real AI output.
"""

from __future__ import annotations

import json

import httpx

from app.core.config import get_settings
from app.core.logging import get_logger

log = get_logger(__name__)

_GEMINI_URL = (
    "https://generativelanguage.googleapis.com/v1beta/models"
    "/{model}:generateContent?key={key}"
)

_SYSTEM_PROMPT = """You are a professional cinematographer and film director with 20+ years of experience.
You help complete beginners create professional shot lists from plain-English scene descriptions.

Analyze the scene and create exactly 6-8 individual shots a real director would use.
For each shot provide full technical details AND a clear educational explanation.

Return ONLY a valid JSON object — no markdown, no extra text — matching this exact schema:
{
  "title": "Short evocative scene title (max 8 words)",
  "director_note": "One sentence describing the overall visual approach",
  "shots": [
    {
      "shot_number": 1,
      "shot_name": "Descriptive name for this shot",
      "shot_type": "Full name + abbreviation e.g. 'Close-Up (CU)'",
      "camera_angle": "e.g. Eye Level | Low Angle | High Angle | Dutch Angle | Bird's Eye | Worm's Eye",
      "camera_movement": "e.g. Static | Slow Pan | Dolly In | Tracking Shot | Handheld | Crane Up",
      "lens": "e.g. 24mm wide angle | 50mm normal | 85mm portrait | 135mm telephoto",
      "lighting": "e.g. Golden hour backlight | Soft diffused natural | High contrast dramatic",
      "prompt": "Complete AI image-generation prompt. Must include: shot type, lens mm, camera angle, subject description, lighting, mood, atmosphere, '2.39:1 cinematic aspect ratio', film grain, e.g. Kodak Vision3 colour grade.",
      "explanation": "2-3 sentences explaining WHY this shot works. Teach the beginner: what emotion it creates, why this lens/angle/movement was chosen, what the audience feels watching it.",
      "mood": "Single word e.g. Intimate | Tense | Expansive | Melancholic | Urgent",
      "duration": "e.g. 4-6 seconds | 8-12 seconds"
    }
  ]
}"""


def _build_user_message(
    scene_description: str,
    scene_style: str,
    location_desc: str | None,
    character_desc: str | None,
) -> str:
    parts = [
        f"Scene description: {scene_description}",
        f"Visual style / mood: {scene_style}",
    ]
    if location_desc:
        parts.append(f"Location: {location_desc}")
    if character_desc:
        parts.append(f"Characters: {character_desc}")
    return "\n".join(parts)


def _call_gemini(prompt: str, settings) -> dict:
    url = _GEMINI_URL.format(model=settings.GEMINI_MODEL, key=settings.GEMINI_API_KEY)
    payload = {
        "systemInstruction": {"parts": [{"text": _SYSTEM_PROMPT}]},
        "contents": [{"parts": [{"text": prompt}]}],
        "generationConfig": {
            "responseMimeType": "application/json",
            "temperature": 0.75,
            "maxOutputTokens": 4096,
        },
    }
    try:
        resp = httpx.post(url, json=payload, timeout=60)
        resp.raise_for_status()
    except httpx.TimeoutException as exc:
        raise RuntimeError("Gemini API timed out. Please try again.") from exc
    except httpx.HTTPStatusError as exc:
        log.error("gemini_api_error", status=exc.response.status_code, body=exc.response.text[:400])
        raise RuntimeError(f"Gemini API error ({exc.response.status_code}).") from exc

    body = resp.json()
    try:
        raw_text = body["candidates"][0]["content"]["parts"][0]["text"]
        return json.loads(raw_text)
    except (KeyError, IndexError, json.JSONDecodeError) as exc:
        log.error("gemini_parse_error", body=str(body)[:400])
        raise RuntimeError("Could not parse Gemini response.") from exc


def _demo_result(scene_description: str) -> dict:
    """Returned when GEMINI_API_KEY is not configured — keeps dev flow working."""
    short = scene_description[:60] + ("…" if len(scene_description) > 60 else "")
    return {
        "title": f"Demo: {short}",
        "director_note": "Set GEMINI_API_KEY to get real AI-generated shot lists.",
        "shots": [
            {
                "shot_number": 1,
                "shot_name": "Establishing wide",
                "shot_type": "Extreme Long Shot (ELS)",
                "camera_angle": "Eye Level",
                "camera_movement": "Slow Pan Right",
                "lens": "24mm wide angle",
                "lighting": "Golden hour natural light",
                "prompt": "Extreme long shot, 24mm wide angle lens, eye level, slow pan right, golden hour natural lighting, establishing scene context, 2.39:1 cinematic aspect ratio, Kodak Vision3 colour grade, film grain",
                "explanation": "An establishing shot tells the audience WHERE the story happens and sets the emotional scale of the world. A 24mm wide lens captures more of the environment, making the character feel small inside a large space. Golden hour adds warmth and visual beauty instantly.",
                "mood": "Expansive",
                "duration": "5-8 seconds",
            },
            {
                "shot_number": 2,
                "shot_name": "Character introduction",
                "shot_type": "Medium Shot (MS)",
                "camera_angle": "Slightly Low Angle",
                "camera_movement": "Slow Dolly In",
                "lens": "50mm normal",
                "lighting": "Soft key light with rim light",
                "prompt": "Medium shot, 50mm normal lens, slightly low angle, slow dolly in, soft key light from left, warm rim light from behind, subject centred, shallow focus, 2.39:1 cinematic ratio, film grain, warm colour grade",
                "explanation": "A medium shot shows body language AND facial expression together — the most informative framing for introducing a character. A slightly low angle gives them authority and presence. The dolly-in creates momentum and draws the audience emotionally closer.",
                "mood": "Authoritative",
                "duration": "6-10 seconds",
            },
            {
                "shot_number": 3,
                "shot_name": "Searching eyes",
                "shot_type": "Close-Up (CU)",
                "camera_angle": "Eye Level",
                "camera_movement": "Static",
                "lens": "85mm portrait",
                "lighting": "Soft diffused natural backlight",
                "prompt": "Close-up shot, 85mm portrait lens, eye level, static camera, soft diffused natural backlight through window, shallow depth of field, bokeh background, emotional expression, 2.39:1 cinematic ratio, Kodak Vision3, film grain",
                "explanation": "Close-ups are the most powerful tool for emotion in cinema. At eye level the audience meets the character as an equal. The 85mm portrait lens gives a flattering, natural perspective with beautiful background separation. Backlight creates a subtle glow that makes the subject feel special.",
                "mood": "Intimate",
                "duration": "4-6 seconds",
            },
        ],
    }


def generate_shot_list(
    *,
    scene_description: str,
    scene_style: str,
    location_desc: str | None,
    character_desc: str | None,
) -> dict:
    """Return a shot-list dict from Gemini, or demo data if key is not set."""
    settings = get_settings()

    if not settings.GEMINI_API_KEY:
        log.info("gemini_demo_mode", reason="GEMINI_API_KEY not set")
        return _demo_result(scene_description)

    prompt = _build_user_message(scene_description, scene_style, location_desc, character_desc)
    log.info("gemini_request", style=scene_style, desc_len=len(scene_description))
    result = _call_gemini(prompt, settings)
    log.info("gemini_response", shot_count=len(result.get("shots", [])))
    return result
