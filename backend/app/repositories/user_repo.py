"""Data-access layer for users.

Repositories isolate all SQLAlchemy queries behind plain method calls, so
services and routes never touch the ORM directly. This keeps business logic
testable and makes a future storage swap a one-file change. All access goes
through the ORM (parameterized) — never raw string SQL — so injection is
structurally impossible.
"""

from __future__ import annotations

from sqlalchemy import select
from sqlalchemy.orm import Session

from app.models.user import User


class UserRepository:
    def __init__(self, db: Session):
        self.db = db

    def get_by_firebase_uid(self, firebase_uid: str) -> User | None:
        stmt = select(User).where(
            User.firebase_uid == firebase_uid, User.deleted_at.is_(None)
        )
        return self.db.scalar(stmt)

    def get_by_email(self, email: str) -> User | None:
        stmt = select(User).where(User.email == email, User.deleted_at.is_(None))
        return self.db.scalar(stmt)

    def create(
        self,
        *,
        firebase_uid: str,
        email: str,
        display_name: str | None = None,
        avatar_url: str | None = None,
    ) -> User:
        user = User(
            firebase_uid=firebase_uid,
            email=email,
            display_name=display_name,
            avatar_url=avatar_url,
        )
        self.db.add(user)
        self.db.flush()  # populate server defaults (id, credits) without committing
        return user

    def get_or_create_from_firebase(
        self,
        *,
        firebase_uid: str,
        email: str,
        display_name: str | None,
        avatar_url: str | None,
    ) -> tuple[User, bool]:
        """Return (user, is_new). Idempotent — safe to call on every login."""
        existing = self.get_by_firebase_uid(firebase_uid)
        if existing:
            # Keep profile fields fresh from the identity provider.
            existing.display_name = display_name or existing.display_name
            existing.avatar_url = avatar_url or existing.avatar_url
            return existing, False

        user = self.create(
            firebase_uid=firebase_uid,
            email=email,
            display_name=display_name,
            avatar_url=avatar_url,
        )
        return user, True
