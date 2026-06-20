"""Authentication routes.

Flow:
  1. Mobile app signs the user in with Google/Apple via Firebase and obtains
     an ID token.
  2. App calls POST /auth/login with `Authorization: Bearer <id_token>`.
  3. We verify the token, provision the user on first login, and return their
     profile. Subsequent calls to any protected route reuse the same Bearer
     token — there is no separate server-issued session to manage.
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
settings = get_settings()


@router.post(
    "/login",
    response_model=Success[LoginResponse],
    status_code=status.HTTP_200_OK,
    summary="Verify a Firebase ID token and provision the user",
)
@limiter.limit("20/minute")
def login(
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
