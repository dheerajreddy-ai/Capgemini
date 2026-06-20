"""Shot model — one frame within a storyboard."""

from __future__ import annotations

import uuid

from sqlalchemy import ForeignKey, SmallInteger, String, Text
from sqlalchemy.dialects.postgresql import UUID as PG_UUID
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.db.base import Base, TimestampMixin, UUIDMixin


class Shot(UUIDMixin, TimestampMixin, Base):
    __tablename__ = "shots"

    storyboard_id: Mapped[uuid.UUID] = mapped_column(
        PG_UUID(as_uuid=True),
        ForeignKey("storyboards.id", ondelete="CASCADE"),
        index=True,
        nullable=False,
    )

    shot_number: Mapped[int] = mapped_column(SmallInteger, nullable=False)
    shot_name: Mapped[str | None] = mapped_column(String(255), nullable=True)

    # Cinematography breakdown (populated in Phase 2 by Gemini).
    shot_type: Mapped[str | None] = mapped_column(String(100), nullable=True)
    camera_angle: Mapped[str | None] = mapped_column(String(100), nullable=True)
    camera_movement: Mapped[str | None] = mapped_column(String(100), nullable=True)
    lens: Mapped[str | None] = mapped_column(String(100), nullable=True)
    lighting: Mapped[str | None] = mapped_column(String(100), nullable=True)
    mood: Mapped[str | None] = mapped_column(String(100), nullable=True)
    duration: Mapped[str | None] = mapped_column(String(50), nullable=True)

    prompt: Mapped[str | None] = mapped_column(Text, nullable=True)
    explanation: Mapped[str | None] = mapped_column(Text, nullable=True)

    # Cloudflare R2 URL (populated in Phase 3 by the image pipeline).
    image_url: Mapped[str | None] = mapped_column(String(1024), nullable=True)

    storyboard: Mapped["Storyboard"] = relationship(back_populates="shots")  # noqa: F821
