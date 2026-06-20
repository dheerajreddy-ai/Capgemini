"""Shared rate limiter.

Defined in its own module so routes and `main` import the *same* limiter
instance. Keying by client IP gives a first line of defense against abuse and
runaway cost (every generation call hits paid AI APIs). For multi-instance
deployments at scale this can be pointed at Redis via `storage_uri`.
"""

from __future__ import annotations

from slowapi import Limiter
from slowapi.util import get_remote_address

from app.core.config import get_settings

settings = get_settings()

limiter = Limiter(
    key_func=get_remote_address,
    default_limits=[f"{settings.RATE_LIMIT_REQUESTS_PER_MINUTE}/minute"],
    headers_enabled=True,
)
