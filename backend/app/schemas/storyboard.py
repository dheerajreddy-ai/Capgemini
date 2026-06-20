"""Storyboard API schemas (Pydantic v2)."""

from __future__ import annotations

import uuid
from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field

from app.models.storyboard import ImagesStatus, StoryboardStatus


# ── Request ───────────────────────────────────────────────────

class GenerateRequest(BaseModel):
    scene_description: str = Field(
        ...,
        min_length=20,
        max_length=2000,
        description="Plain-English description of the scene (20–2000 chars).",
    )
    scene_style: str = Field(
        default="Cinematic",
        max_length=50,
        description="Visual mood: Cinematic, Moody, Warm, Dramatic, Vintage, Cold …",
    )
    location_desc: str | None = Field(
        default=None,
        max_length=500,
        description="Optional location description for richer shot suggestions.",
    )
    character_desc: str | None = Field(
        default=None,
        max_length=500,
        description="Optional character descriptions.",
    )


# ── Inner response ────────────────────────────────────────────

class ShotOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    shot_number: int
    shot_name: str | None
    shot_type: str | None
    camera_angle: str | None
    camera_movement: str | None
    lens: str | None
    lighting: str | None
    mood: str | None
    duration: str | None
    prompt: str | None
    explanation: str | None
    image_url: str | None


class StoryboardOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    title: str | None
    scene_description: str
    scene_style: str | None
    director_note: str | None
    status: StoryboardStatus
    images_status: ImagesStatus
    shots: list[ShotOut]
    created_at: datetime


class StoryboardSummary(BaseModel):
    """Lightweight item for the history list."""

    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    title: str | None
    scene_style: str | None
    status: StoryboardStatus
    shot_count: int = 0
    created_at: datetime
