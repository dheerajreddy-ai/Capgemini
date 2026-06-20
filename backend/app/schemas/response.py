"""Generic success envelope.

All successful responses share the shape `{"success": true, "data": ...}` so
the mobile client has one predictable contract for every endpoint (errors use
the mirror shape defined in core.exceptions).
"""

from __future__ import annotations

from typing import Generic, TypeVar

from pydantic import BaseModel

T = TypeVar("T")


class Success(BaseModel, Generic[T]):
    success: bool = True
    data: T
