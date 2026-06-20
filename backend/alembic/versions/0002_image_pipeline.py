"""add images_status to storyboards

Revision ID: 0002_image_pipeline
Revises: 0001_initial
Create Date: 2026-06-20
"""
from __future__ import annotations

from collections.abc import Sequence

import sqlalchemy as sa
from alembic import op
from sqlalchemy.dialects import postgresql

revision: str = "0002_image_pipeline"
down_revision: str | None = "0001_initial"
branch_labels: str | Sequence[str] | None = None
depends_on: str | Sequence[str] | None = None


def upgrade() -> None:
    images_status = postgresql.ENUM(
        "pending", "generating", "done", "failed",
        name="images_status",
        create_type=True,
    )
    images_status.create(op.get_bind(), checkfirst=True)

    op.add_column(
        "storyboards",
        sa.Column(
            "images_status",
            sa.Enum("pending", "generating", "done", "failed", name="images_status"),
            server_default="pending",
            nullable=False,
        ),
    )


def downgrade() -> None:
    op.drop_column("storyboards", "images_status")
    op.execute("DROP TYPE IF EXISTS images_status")
