"""
  Pytests fixtures. Auto discoverable (no need to import).
"""
import pytest
from context import session as csession



@pytest.fixture
def api(session, request):
    testname = request.node.name
    session.login(testname)
    return session


@pytest.fixture
def session():
    return csession()
    

