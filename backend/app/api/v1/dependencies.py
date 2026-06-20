"""Reusable FastAPI dependencies (auth guard, DB session wiring).

`get_current_user` is the gate placed on every protected route. It:
  1. Extracts the Bearer token from the Authorization header.
  2. Verifies it with Firebase.
  3. Resolves (or lazily provisions) the matching row in our users table.

Routes simply declare `user: User = Depends(get_current_user)` and are
guaranteed an authenticated, active account.
"""

from __future__ import annotations

from fastapi import Depends
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from sqlalchemy.orm import Session

from app.core.exceptions import AuthError, ForbiddenError
from app.db.session import get_db
from app.models.user import User
from app.repositories.user_repo import UserRepository
from app.services import firebase_service

# auto_error=False so we can raise our own typed AuthError envelope.
_bearer = HTTPBearer(auto_error=False)


def get_current_user(
    creds: HTTPAuthorizationCredentials | None = Depends(_bearer),
    db: Session = Depends(get_db),
) -> User:
    if creds is None or not creds.credentials:
        raise AuthError("Missing authentication token.")

    identity = firebase_service.verify_id_token(creds.credentials)

    repo = UserRepository(db)
    user, _ = repo.get_or_create_from_firebase(
        firebase_uid=identity.uid,
        email=identity.email,  # guaranteed present by verify_id_token
        display_name=identity.name,
        avatar_url=identity.picture,
    )

    if not user.is_active:
        raise ForbiddenError("This account has been disabled.")

    return user
