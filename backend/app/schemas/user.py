"""User API schemas (Pydantic v2)."""

from __future__ import annotations

import uuid
from datetime import datetime

from pydantic import BaseModel, ConfigDict, EmailStr

from app.models.user import UserPlan


class UserPublic(BaseModel):
    """Safe representation of a user returned to clients."""

    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    email: EmailStr
    display_name: str | None
    avatar_url: str | None
    plan: UserPlan
    credits: int
    created_at: datetime


class LoginResponse(BaseModel):
    """Returned after a Firebase token is verified."""

    user: UserPublic
    is_new_user: bool
