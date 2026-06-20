"""Data-access layer for storyboards and shots.

All DB queries live here — services and routes never touch SQLAlchemy directly.
"""

from __future__ import annotations

import uuid
from datetime import datetime, timezone

from sqlalchemy import select
from sqlalchemy.orm import Session, selectinload

from app.models.shot import Shot
from app.models.storyboard import Storyboard, StoryboardStatus


class StoryboardRepository:
    def __init__(self, db: Session):
        self.db = db

    # ── Storyboard CRUD ───────────────────────────────────────

    def create(
        self,
        *,
        user_id: uuid.UUID,
        scene_description: str,
        scene_style: str | None,
        location_desc: str | None,
        character_desc: str | None,
    ) -> Storyboard:
        sb = Storyboard(
            user_id=user_id,
            scene_description=scene_description,
            scene_style=scene_style,
            location_desc=location_desc,
            character_desc=character_desc,
            status=StoryboardStatus.processing,
        )
        self.db.add(sb)
        self.db.flush()
        return sb

    def get_by_id(self, storyboard_id: uuid.UUID, user_id: uuid.UUID) -> Storyboard | None:
        stmt = (
            select(Storyboard)
            .options(selectinload(Storyboard.shots))
            .where(
                Storyboard.id == storyboard_id,
                Storyboard.user_id == user_id,
                Storyboard.deleted_at.is_(None),
            )
        )
        return self.db.scalar(stmt)

    def list_by_user(self, user_id: uuid.UUID, *, limit: int = 20, offset: int = 0) -> list[Storyboard]:
        stmt = (
            select(Storyboard)
            .options(selectinload(Storyboard.shots))
            .where(Storyboard.user_id == user_id, Storyboard.deleted_at.is_(None))
            .order_by(Storyboard.created_at.desc())
            .limit(limit)
            .offset(offset)
        )
        return list(self.db.scalars(stmt))

    def mark_completed(
        self,
        storyboard: Storyboard,
        *,
        title: str,
        director_note: str,
    ) -> Storyboard:
        storyboard.title = title
        storyboard.director_note = director_note
        storyboard.status = StoryboardStatus.completed
        self.db.flush()
        return storyboard

    def mark_failed(self, storyboard: Storyboard) -> Storyboard:
        storyboard.status = StoryboardStatus.failed
        self.db.flush()
        return storyboard

    def soft_delete(self, storyboard: Storyboard) -> None:
        storyboard.deleted_at = datetime.now(timezone.utc)
        self.db.flush()

    # ── Shot bulk insert ──────────────────────────────────────

    def bulk_create_shots(
        self,
        storyboard_id: uuid.UUID,
        shots_data: list[dict],
    ) -> list[Shot]:
        shots = [
            Shot(
                storyboard_id=storyboard_id,
                shot_number=s.get("shot_number", i + 1),
                shot_name=s.get("shot_name"),
                shot_type=s.get("shot_type"),
                camera_angle=s.get("camera_angle"),
                camera_movement=s.get("camera_movement"),
                lens=s.get("lens"),
                lighting=s.get("lighting"),
                mood=s.get("mood"),
                duration=s.get("duration"),
                prompt=s.get("prompt"),
                explanation=s.get("explanation"),
            )
            for i, s in enumerate(shots_data)
        ]
        self.db.add_all(shots)
        self.db.flush()
        return shots
