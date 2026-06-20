"""Application entrypoint — wires the FastAPI app together.

Responsibilities kept deliberately thin: configure logging, build the app,
attach middleware (order matters), register routers and error handlers. All
real logic lives in services/repositories.
"""

from __future__ import annotations

from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from slowapi.errors import RateLimitExceeded

from app import __version__
from app.api.v1.router import api_router
from app.core.config import get_settings
from app.core.exceptions import RateLimitError, register_exception_handlers
from app.core.logging import configure_logging, get_logger
from app.core.middleware import RequestContextMiddleware, SecurityHeadersMiddleware
from app.rate_limit import limiter

configure_logging()
log = get_logger(__name__)
settings = get_settings()


@asynccontextmanager
async def lifespan(_: FastAPI):
    log.info(
        "app_starting",
        environment=settings.ENVIRONMENT,
        version=__version__,
    )
    yield
    log.info("app_shutdown")


def create_app() -> FastAPI:
    app = FastAPI(
        title=settings.APP_NAME,
        version=__version__,
        # Hide interactive docs in production to reduce surface area.
        docs_url=None if settings.is_production else "/docs",
        redoc_url=None if settings.is_production else "/redoc",
        openapi_url=None if settings.is_production else "/openapi.json",
        lifespan=lifespan,
    )

    # ── Rate limiting ─────────────────────────────────────────
    app.state.limiter = limiter

    async def _rate_limit_handler(request, exc: RateLimitExceeded):
        # Translate slowapi's exception into our standard error envelope.
        from app.core.exceptions import _envelope
        from fastapi.responses import JSONResponse

        return JSONResponse(
            status_code=RateLimitError.status_code,
            content=_envelope(RateLimitError.error_code, RateLimitError.message),
        )

    app.add_exception_handler(RateLimitExceeded, _rate_limit_handler)

    # ── Middleware (executed bottom-up on the way in) ─────────
    app.add_middleware(SecurityHeadersMiddleware)
    app.add_middleware(RequestContextMiddleware)
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.cors_origin_list or ["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
        expose_headers=["X-Request-ID"],
    )

    # ── Errors + routes ───────────────────────────────────────
    register_exception_handlers(app)
    app.include_router(api_router, prefix=settings.API_V1_PREFIX)

    @app.get("/", include_in_schema=False)
    def root() -> dict:
        return {"service": settings.APP_NAME, "version": __version__, "docs": "/docs"}

    return app


app = create_app()
