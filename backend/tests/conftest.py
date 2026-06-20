"""Test fixtures.

Sets minimal env vars before app import so Settings validates, then exposes a
TestClient. Auth-dependent tests override `get_current_user` rather than minting
real Firebase tokens.
"""

from __future__ import annotations

import os

os.environ.setdefault(
    "DATABASE_URL", "postgresql+psycopg2://test:test@localhost:5432/test"
)
os.environ.setdefault("ENV", "dev")

import pytest
from fastapi.testclient import TestClient


@pytest.fixture(scope="session")
def app():
    from app.main import create_app

    return create_app()


@pytest.fixture
def client(app):
    return TestClient(app, raise_server_exceptions=False)
