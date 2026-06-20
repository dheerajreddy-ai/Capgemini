"""Firebase Admin integration — aligned with SuperFit's pattern.

Verifies the ID token the mobile app obtains after Google Sign-In.
Service-account credential is loaded from FIREBASE_SERVICE_ACCOUNT env var
(raw JSON string) exactly as SuperFit does — Railway-friendly, no files needed.

The private_key \n escaping fix from SuperFit is applied automatically.

Mock-auth support: when ALLOW_MOCK_AUTH=true (dev/test only, never production),
tokens of the form `mock_token_<URL-encoded JSON>` are accepted and decoded
without hitting Firebase. This mirrors SuperFit's test-mode behaviour.
"""

from __future__ import annotations

import json
import threading
import urllib.parse

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
        if not settings.FIREBASE_SERVICE_ACCOUNT:
            raise AuthError(
                "Auth is not configured on this server.",
                error_code="auth_not_configured",
            )
        try:
            cred_dict = json.loads(settings.FIREBASE_SERVICE_ACCOUNT)
            # Railway escapes \n as \\n in env vars — fix the private key.
            if "private_key" in cred_dict:
                cred_dict["private_key"] = cred_dict["private_key"].replace("\\n", "\n")
            cred = credentials.Certificate(cred_dict)
            firebase_admin.initialize_app(cred)
            _initialized = True
            log.info("firebase_initialized", project_id=cred_dict.get("project_id"))
        except (json.JSONDecodeError, ValueError) as exc:
            log.error("firebase_init_failed", error=str(exc))
            raise AuthError(
                "Auth is misconfigured on this server.",
                error_code="auth_not_configured",
            ) from exc


class FirebaseUser:
    """Normalized identity extracted from a verified Firebase token."""

    def __init__(self, uid: str, email: str | None, name: str | None, picture: str | None):
        self.uid = uid
        self.email = email
        self.name = name
        self.picture = picture


def _decode_mock_token(token: str) -> FirebaseUser:
    """Decode a mock_token_<encoded-json> without hitting Firebase."""
    try:
        encoded = token[len("mock_token_"):]
        claims = json.loads(urllib.parse.unquote(encoded))
        return FirebaseUser(
            uid=claims["uid"],
            email=claims.get("email"),
            name=claims.get("name"),
            picture=claims.get("picture"),
        )
    except Exception as exc:
        raise AuthError("Invalid mock token.") from exc


def verify_id_token(id_token: str) -> FirebaseUser:
    """Verify a Firebase ID token and return the identity, or raise AuthError."""
    settings = get_settings()

    # Mock-auth path (dev/test only — blocked in production by allow_mock).
    if id_token.startswith("mock_token_"):
        if not settings.allow_mock:
            raise AuthError("Mock auth is not enabled on this server.")
        log.info("mock_auth_accepted")
        return _decode_mock_token(id_token)

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

    return FirebaseUser(
        uid=claims["uid"],
        email=claims.get("email"),
        name=claims.get("name"),
        picture=claims.get("picture"),
    )
