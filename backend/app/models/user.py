"""User model — backed by Firebase identity.

We never store passwords. Firebase is the identity provider (Google/Apple
sign-in); we store the Firebase UID plus profile data and our own app-level
fields (plan, credits). The `firebase_uid` is the join key between Firebase's
auth and our domain data.
"""

from __future__ import annotations

import enum

from sqlalchemy import Boolean, Enum, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.db.base import Base, SoftDeleteMixin, TimestampMixin, UUIDMixin


class UserPlan(str, enum.Enum):
    free = "free"
    starter = "starter"
    pro = "pro"
    studio = "studio"


class User(UUIDMixin, TimestampMixin, SoftDeleteMixin, Base):
    __tablename__ = "users"

    firebase_uid: Mapped[str] = mapped_column(
        String(128), unique=True, index=True, nullable=False
    )
    email: Mapped[str] = mapped_column(String(255), unique=True, index=True, nullable=False)
    display_name: Mapped[str | None] = mapped_column(String(255), nullable=True)
    avatar_url: Mapped[str | None] = mapped_column(String(1024), nullable=True)

    plan: Mapped[UserPlan] = mapped_column(
        Enum(UserPlan, name="user_plan"), default=UserPlan.free, nullable=False
    )
    # Free trial credits; each storyboard generation consumes one.
    credits: Mapped[int] = mapped_column(Integer, default=3, nullable=False)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True, nullable=False)

    storyboards: Mapped[list["Storyboard"]] = relationship(  # noqa: F821
        back_populates="user", cascade="all, delete-orphan"
    )

    def __repr__(self) -> str:  # pragma: no cover
        return f"<User {self.email} plan={self.plan.value}>"
