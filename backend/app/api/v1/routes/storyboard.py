"""Storyboard routes.

POST /storyboard/generate   → call Gemini, save, return full storyboard
GET  /storyboard/history    → paginated list of user's past storyboards
GET  /storyboard/{id}       → single storyboard with all shots
DELETE /storyboard/{id}     → soft delete
"""

from __future__ import annotations

import uuid

from fastapi import APIRouter, BackgroundTasks, Depends, Query, Request, status
from sqlalchemy.orm import Session

from app.api.v1.dependencies import get_current_user
from app.core.exceptions import NotFoundError
from app.db.session import get_db
from app.models.user import User
from app.rate_limit import limiter
from app.repositories.storyboard_repo import StoryboardRepository
from app.schemas.response import Success
from app.schemas.storyboard import GenerateRequest, StoryboardOut, StoryboardSummary
from app.services import image_pipeline, storyboard_service

router = APIRouter(prefix="/storyboard", tags=["storyboard"])


@router.post(
    "/generate",
    response_model=Success[StoryboardOut],
    status_code=status.HTTP_201_CREATED,
    summary="Generate a storyboard from a scene description",
)
@limiter.limit("10/minute")
def generate(
    request: Request,
    body: GenerateRequest,
    background_tasks: BackgroundTasks,
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
) -> Success[StoryboardOut]:
    result = storyboard_service.generate_storyboard(body, user, db)
    # Kick off image generation after the response is sent.
    background_tasks.add_task(
        image_pipeline.generate_images_for_storyboard,
        result.id,
        user.id,
    )
    return Success(data=result)


@router.get(
    "/history",
    response_model=Success[list[StoryboardSummary]],
    summary="List the authenticated user's past storyboards",
)
def history(
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
    limit: int = Query(default=20, ge=1, le=50),
    offset: int = Query(default=0, ge=0),
) -> Success[list[StoryboardSummary]]:
    repo = StoryboardRepository(db)
    boards = repo.list_by_user(user.id, limit=limit, offset=offset)
    summaries = [
        StoryboardSummary(
            id=b.id,
            title=b.title,
            scene_style=b.scene_style,
            status=b.status,
            shot_count=len(b.shots),
            created_at=b.created_at,
        )
        for b in boards
    ]
    return Success(data=summaries)


@router.get(
    "/{storyboard_id}",
    response_model=Success[StoryboardOut],
    summary="Get a single storyboard with all its shots",
)
def get_one(
    storyboard_id: uuid.UUID,
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
) -> Success[StoryboardOut]:
    repo = StoryboardRepository(db)
    board = repo.get_by_id(storyboard_id, user.id)
    if not board:
        raise NotFoundError("Storyboard not found.")
    return Success(data=StoryboardOut.model_validate(board))


@router.delete(
    "/{storyboard_id}",
    response_model=Success[dict],
    summary="Soft-delete a storyboard",
)
def delete(
    storyboard_id: uuid.UUID,
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
) -> Success[dict]:
    repo = StoryboardRepository(db)
    board = repo.get_by_id(storyboard_id, user.id)
    if not board:
        raise NotFoundError("Storyboard not found.")
    repo.soft_delete(board)
    return Success(data={"deleted": True})
