"""Storyboard business logic.

Orchestrates the full generate flow:
  1. Credit check (free users get 3; pro gets more)
  2. Create storyboard row (status=processing)
  3. Call Gemini — if it fails, mark storyboard failed + refund credit
  4. Bulk-insert shots
  5. Mark storyboard completed
  6. Deduct one credit

Keeping this in a service (not the route) means it's testable and the route
stays thin.
"""

from __future__ import annotations

from sqlalchemy.orm import Session

from app.core.exceptions import AppError
from app.core.logging import get_logger
from app.models.user import User
from app.repositories.storyboard_repo import StoryboardRepository
from app.schemas.storyboard import GenerateRequest, StoryboardOut
from app.services import gemini_service

log = get_logger(__name__)


class InsufficientCreditsError(AppError):
    status_code = 402
    error_code = "insufficient_credits"
    message = "You have no credits left. Please upgrade your plan."


def generate_storyboard(
    request: GenerateRequest,
    user: User,
    db: Session,
) -> StoryboardOut:
    # 1. Credit gate — free plan requires at least 1 credit.
    if user.credits <= 0:
        raise InsufficientCreditsError()

    repo = StoryboardRepository(db)

    # 2. Persist storyboard shell immediately (status=processing).
    storyboard = repo.create(
        user_id=user.id,
        scene_description=request.scene_description,
        scene_style=request.scene_style,
        location_desc=request.location_desc,
        character_desc=request.character_desc,
    )

    # 3. Call Gemini — roll back status on failure, never deduct credit.
    try:
        result = gemini_service.generate_shot_list(
            scene_description=request.scene_description,
            scene_style=request.scene_style,
            location_desc=request.location_desc,
            character_desc=request.character_desc,
        )
    except Exception as exc:
        repo.mark_failed(storyboard)
        log.error("storyboard_generation_failed", storyboard_id=str(storyboard.id), error=str(exc))
        raise AppError(
            str(exc) if str(exc) else "AI generation failed. Please try again.",
            error_code="generation_failed",
        ) from exc

    # 4. Persist shots.
    shots_data = result.get("shots", [])
    repo.bulk_create_shots(storyboard.id, shots_data)

    # 5. Mark completed.
    repo.mark_completed(
        storyboard,
        title=result.get("title", "Untitled Scene"),
        director_note=result.get("director_note", ""),
    )

    # 6. Deduct credit (only on success).
    user.credits = max(0, user.credits - 1)
    db.flush()

    log.info(
        "storyboard_created",
        storyboard_id=str(storyboard.id),
        shots=len(shots_data),
        credits_remaining=user.credits,
    )

    # Re-fetch with shots eager-loaded for the response.
    storyboard = repo.get_by_id(storyboard.id, user.id)
    return StoryboardOut.model_validate(storyboard)
