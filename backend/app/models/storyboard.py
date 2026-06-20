"""Storyboard model — one generated scene breakdown."""

from __future__ import annotations

import enum
import uuid

from sqlalchemy import Enum, ForeignKey, String, Text
from sqlalchemy.dialects.postgresql import UUID as PG_UUID
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.db.base import Base, SoftDeleteMixin, TimestampMixin, UUIDMixin


class StoryboardStatus(str, enum.Enum):
    pending = "pending"
    processing = "processing"
    completed = "completed"
    failed = "failed"


class Storyboard(UUIDMixin, TimestampMixin, SoftDeleteMixin, Base):
    __tablename__ = "storyboards"

    user_id: Mapped[uuid.UUID] = mapped_column(
        PG_UUID(as_uuid=True),
        ForeignKey("users.id", ondelete="CASCADE"),
        index=True,
        nullable=False,
    )

    title: Mapped[str | None] = mapped_column(String(255), nullable=True)
    scene_description: Mapped[str] = mapped_column(Text, nullable=False)
    scene_style: Mapped[str | None] = mapped_column(String(50), nullable=True)
    location_desc: Mapped[str | None] = mapped_column(Text, nullable=True)
    character_desc: Mapped[str | None] = mapped_column(Text, nullable=True)
    director_note: Mapped[str | None] = mapped_column(Text, nullable=True)

    status: Mapped[StoryboardStatus] = mapped_column(
        Enum(StoryboardStatus, name="storyboard_status"),
        default=StoryboardStatus.pending,
        index=True,
        nullable=False,
    )

    user: Mapped["User"] = relationship(back_populates="storyboards")  # noqa: F821
    shots: Mapped[list["Shot"]] = relationship(  # noqa: F821
        back_populates="storyboard",
        cascade="all, delete-orphan",
        order_by="Shot.shot_number",
    )
