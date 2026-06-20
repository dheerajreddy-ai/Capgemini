"""Import every model here so Alembic autogenerate + Base.metadata see them."""

from app.models.shot import Shot
from app.models.storyboard import Storyboard, StoryboardStatus
from app.models.user import User, UserPlan

__all__ = ["User", "UserPlan", "Storyboard", "StoryboardStatus", "Shot"]
