"""Firebase Admin integration.

Verifies the ID token the mobile app obtains after Google/Apple sign-in. The
Admin SDK checks the token's signature, expiry, audience, and issuer against
Google's public keys, so a forged or expired token is rejected before it ever
reaches our database.

The service account is loaded from an env var (JSON string), never a file
committed to the repo.
"""

from __future__ import annotations

import json
import threading

import firebase_admin
from firebase_admin import auth as firebase_auth
from firebase_admin import credentials

from app.core.config import get_settings
from app.core.exceptions import AuthError
from app.core.logging import get_logger

log = get_logger(__name__)

_init_lock = threading.Lock()
_initialized = False


def _ensure_initialized() -> None:
    """Initialize the default Firebase app exactly once (thread-safe)."""
    global _initialized
    if _initialized:
        return
    with _init_lock:
        if _initialized:
            return
        settings = get_settings()
        if not settings.FIREBASE_CREDENTIALS_JSON:
            raise AuthError(
                "Auth is not configured on the server.",
                error_code="auth_not_configured",
            )
        try:
            cred_dict = json.loads(settings.FIREBASE_CREDENTIALS_JSON)
            cred = credentials.Certificate(cred_dict)
            firebase_admin.initialize_app(cred)
            _initialized = True
            log.info("firebase_initialized", project_id=cred_dict.get("project_id"))
        except (json.JSONDecodeError, ValueError) as exc:
            log.error("firebase_init_failed", error=str(exc))
            raise AuthError(
                "Auth is misconfigured on the server.",
                error_code="auth_not_configured",
            ) from exc


class FirebaseUser:
    """Normalized identity extracted from a verified Firebase token."""

    def __init__(self, claims: dict):
        self.uid: str = claims["uid"]
        self.email: str | None = claims.get("email")
        self.email_verified: bool = bool(claims.get("email_verified", False))
        self.name: str | None = claims.get("name")
        self.picture: str | None = claims.get("picture")


def verify_id_token(id_token: str) -> FirebaseUser:
    """Verify a Firebase ID token and return the identity, or raise AuthError."""
    _ensure_initialized()
    try:
        claims = firebase_auth.verify_id_token(id_token, check_revoked=False)
    except firebase_auth.ExpiredIdTokenError as exc:
        raise AuthError("Your session has expired. Please sign in again.") from exc
    except firebase_auth.RevokedIdTokenError as exc:
        raise AuthError("Your session was revoked. Please sign in again.") from exc
    except (firebase_auth.InvalidIdTokenError, ValueError) as exc:
        raise AuthError("Invalid authentication token.") from exc

    if not claims.get("email"):
        raise AuthError("This account has no email address attached.")

    return FirebaseUser(claims)
