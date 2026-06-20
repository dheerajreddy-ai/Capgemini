"""Background image generation pipeline.

Called as a FastAPI BackgroundTask after storyboard generation returns.
Runs in the same process (no broker needed) — suitable for Phase 3.
Phase 5+ can swap this for Celery if throughput demands it.

Flow per storyboard:
  1. Mark storyboard.images_status = 'generating'
  2. For each shot with a prompt and no image_url:
       a. Call nano_banana_service.generate_image(prompt) → bytes | None
       b. If bytes: call storage_service.upload(...) → url | None
       c. If url: update shot.image_url in DB
  3. Mark storyboard.images_status = 'done' (or 'failed' on unrecoverable error)

Uses its own DB session (BackgroundTasks run outside the request's session scope).
"""

from __future__ import annotations

import uuid

from app.core.logging import get_logger
from app.db.session import SessionLocal
from app.models.storyboard import ImagesStatus
from app.repositories.storyboard_repo import StoryboardRepository
from app.services import nano_banana_service, storage_service

log = get_logger(__name__)


def generate_images_for_storyboard(storyboard_id: uuid.UUID, user_id: uuid.UUID) -> None:
    """Entry point for FastAPI BackgroundTask."""
    db = SessionLocal()
    try:
        repo = StoryboardRepository(db)
        board = repo.get_by_id(storyboard_id, user_id)
        if not board:
            log.error("image_pipeline_storyboard_missing", storyboard_id=str(storyboard_id))
            return

        repo.mark_images_generating(board)

        failed_count = 0
        success_count = 0

        for shot in board.shots:
            if not shot.prompt or shot.image_url:
                continue  # skip shots without prompts or already have images

            try:
                image_bytes = nano_banana_service.generate_image(shot.prompt)

                if image_bytes is None:
                    # Demo mode — no key configured, leave image_url null.
                    log.info("image_pipeline_demo_skip", shot_id=str(shot.id))
                    continue

                cdn_url = storage_service.upload(image_bytes, storyboard_id, shot.id)

                if cdn_url:
                    repo.update_shot_image_url(shot, cdn_url)
                    success_count += 1
                    log.info(
                        "image_pipeline_shot_done",
                        shot_id=str(shot.id),
                        shot_number=shot.shot_number,
                    )

            except Exception as exc:
                failed_count += 1
                log.error(
                    "image_pipeline_shot_failed",
                    shot_id=str(shot.id),
                    shot_number=shot.shot_number,
                    error=str(exc),
                )

        if failed_count > 0 and success_count == 0:
            repo.mark_images_failed(board)
        else:
            repo.mark_images_done(board)

        log.info(
            "image_pipeline_complete",
            storyboard_id=str(storyboard_id),
            success=success_count,
            failed=failed_count,
        )

    except Exception as exc:
        log.error(
            "image_pipeline_unhandled_error",
            storyboard_id=str(storyboard_id),
            error=str(exc),
        )
        try:
            repo = StoryboardRepository(db)
            board = repo.get_by_id(storyboard_id, user_id)
            if board:
                repo.mark_images_failed(board)
        except Exception:
            pass
    finally:
        db.close()
