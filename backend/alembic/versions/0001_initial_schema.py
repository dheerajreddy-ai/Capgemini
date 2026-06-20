"""initial schema: users, storyboards, shots

Revision ID: 0001_initial
Revises:
Create Date: 2026-06-20
"""
from __future__ import annotations

from collections.abc import Sequence

import sqlalchemy as sa
from alembic import op
from sqlalchemy.dialects import postgresql

revision: str = "0001_initial"
down_revision: str | None = None
branch_labels: str | Sequence[str] | None = None
depends_on: str | Sequence[str] | None = None


def upgrade() -> None:
    # pgcrypto powers gen_random_uuid() server-side defaults.
    op.execute('CREATE EXTENSION IF NOT EXISTS "pgcrypto"')

    user_plan = postgresql.ENUM(
        "free", "starter", "pro", "studio", name="user_plan", create_type=True
    )
    storyboard_status = postgresql.ENUM(
        "pending", "processing", "completed", "failed",
        name="storyboard_status", create_type=True,
    )

    op.create_table(
        "users",
        sa.Column("id", postgresql.UUID(as_uuid=True),
                  server_default=sa.text("gen_random_uuid()"), primary_key=True),
        sa.Column("firebase_uid", sa.String(128), nullable=False),
        sa.Column("email", sa.String(255), nullable=False),
        sa.Column("display_name", sa.String(255), nullable=True),
        sa.Column("avatar_url", sa.String(1024), nullable=True),
        sa.Column("plan", user_plan, server_default="free", nullable=False),
        sa.Column("credits", sa.Integer(), server_default="3", nullable=False),
        sa.Column("is_active", sa.Boolean(), server_default=sa.true(), nullable=False),
        sa.Column("deleted_at", sa.DateTime(timezone=True), nullable=True),
        sa.Column("created_at", sa.DateTime(timezone=True),
                  server_default=sa.text("now()"), nullable=False),
        sa.Column("updated_at", sa.DateTime(timezone=True),
                  server_default=sa.text("now()"), nullable=False),
    )
    op.create_index("ix_users_firebase_uid", "users", ["firebase_uid"], unique=True)
    op.create_index("ix_users_email", "users", ["email"], unique=True)

    op.create_table(
        "storyboards",
        sa.Column("id", postgresql.UUID(as_uuid=True),
                  server_default=sa.text("gen_random_uuid()"), primary_key=True),
        sa.Column("user_id", postgresql.UUID(as_uuid=True),
                  sa.ForeignKey("users.id", ondelete="CASCADE"), nullable=False),
        sa.Column("title", sa.String(255), nullable=True),
        sa.Column("scene_description", sa.Text(), nullable=False),
        sa.Column("scene_style", sa.String(50), nullable=True),
        sa.Column("location_desc", sa.Text(), nullable=True),
        sa.Column("character_desc", sa.Text(), nullable=True),
        sa.Column("director_note", sa.Text(), nullable=True),
        sa.Column("status", storyboard_status, server_default="pending", nullable=False),
        sa.Column("deleted_at", sa.DateTime(timezone=True), nullable=True),
        sa.Column("created_at", sa.DateTime(timezone=True),
                  server_default=sa.text("now()"), nullable=False),
        sa.Column("updated_at", sa.DateTime(timezone=True),
                  server_default=sa.text("now()"), nullable=False),
    )
    op.create_index("ix_storyboards_user_id", "storyboards", ["user_id"])
    op.create_index("ix_storyboards_status", "storyboards", ["status"])

    op.create_table(
        "shots",
        sa.Column("id", postgresql.UUID(as_uuid=True),
                  server_default=sa.text("gen_random_uuid()"), primary_key=True),
        sa.Column("storyboard_id", postgresql.UUID(as_uuid=True),
                  sa.ForeignKey("storyboards.id", ondelete="CASCADE"), nullable=False),
        sa.Column("shot_number", sa.SmallInteger(), nullable=False),
        sa.Column("shot_name", sa.String(255), nullable=True),
        sa.Column("shot_type", sa.String(100), nullable=True),
        sa.Column("camera_angle", sa.String(100), nullable=True),
        sa.Column("camera_movement", sa.String(100), nullable=True),
        sa.Column("lens", sa.String(100), nullable=True),
        sa.Column("lighting", sa.String(100), nullable=True),
        sa.Column("mood", sa.String(100), nullable=True),
        sa.Column("duration", sa.String(50), nullable=True),
        sa.Column("prompt", sa.Text(), nullable=True),
        sa.Column("explanation", sa.Text(), nullable=True),
        sa.Column("image_url", sa.String(1024), nullable=True),
        sa.Column("created_at", sa.DateTime(timezone=True),
                  server_default=sa.text("now()"), nullable=False),
        sa.Column("updated_at", sa.DateTime(timezone=True),
                  server_default=sa.text("now()"), nullable=False),
    )
    op.create_index("ix_shots_storyboard_id", "shots", ["storyboard_id"])


def downgrade() -> None:
    op.drop_table("shots")
    op.drop_table("storyboards")
    op.drop_table("users")
    op.execute("DROP TYPE IF EXISTS storyboard_status")
    op.execute("DROP TYPE IF EXISTS user_plan")
