"""Authentication routes — aligned with SuperFit's /auth/sync convention.

Flow:
  1. Mobile signs the user in with Google via Firebase and gets an ID token.
  2. App calls POST /auth/sync with `Authorization: Bearer <id_token>`.
  3. We verify the token, provision the user on first call, return profile.
  4. MUST be called right after every sign-in before hitting any other
     endpoint — all other routes depend on the users row existing in the DB.
"""

from __future__ import annotations

from fastapi import APIRouter, Depends, Request, status
from sqlalchemy.orm import Session

from app.api.v1.dependencies import get_current_user
from app.core.config import get_settings
from app.core.exceptions import AuthError
from app.db.session import get_db
from app.models.user import User
from app.rate_limit import limiter
from app.repositories.user_repo import UserRepository
from app.schemas.response import Success
from app.schemas.user import LoginResponse, UserPublic
from app.services import firebase_service

router = APIRouter(prefix="/auth", tags=["auth"])


@router.post(
    "/sync",
    response_model=Success[LoginResponse],
    status_code=status.HTTP_200_OK,
    summary="Verify Firebase token, provision user on first call",
)
@limiter.limit("20/minute")
def sync(
    request: Request,
    db: Session = Depends(get_db),
) -> Success[LoginResponse]:
    auth_header = request.headers.get("Authorization", "")
    if not auth_header.lower().startswith("bearer "):
        raise AuthError("Missing Bearer token.")
    id_token = auth_header.split(" ", 1)[1].strip()

    identity = firebase_service.verify_id_token(id_token)

    repo = UserRepository(db)
    user, is_new = repo.get_or_create_from_firebase(
        firebase_uid=identity.uid,
        email=identity.email,
        display_name=identity.name,
        avatar_url=identity.picture,
    )

    return Success(
        data=LoginResponse(user=UserPublic.model_validate(user), is_new_user=is_new)
    )


@router.get(
    "/me",
    response_model=Success[UserPublic],
    summary="Return the currently authenticated user",
)
def me(user: User = Depends(get_current_user)) -> Success[UserPublic]:
    return Success(data=UserPublic.model_validate(user))
