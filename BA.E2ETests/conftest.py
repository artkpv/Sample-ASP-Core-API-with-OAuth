"""
  Pytests fixtures. Auto discoverable (no need to import).
"""
import pytest
import requests
import unittest
import os


from oauthlib.oauth2 import BackendApplicationClient, LegacyApplicationClient
from requests_oauthlib import OAuth2Session


@pytest.fixture
def api(session, request):
    testname = request.node.name
    session.login(testname)
    return session


@pytest.fixture
def session():
    abspath = os.path.abspath(__file__)
    dname = os.path.dirname(abspath)
    os.chdir(dname)

    client_id = 'ro.client'
    client_secret = '992536EF-F270-4058-80CA-1C89C192FAAB'

    session = OAuth2Session(
        client=LegacyApplicationClient(client_id=client_id, scope="JogAppWebAPI")
    )

    session.verify = "../localhost.crt" 
    baseurl = 'https://localhost:5000/api'
    iserverurl = 'https://localhost:5002'
    session.bikingentryurl = baseurl + '/bikingentry'
    session.identityurl = baseurl + '/identity'
    session.reporturl = baseurl + '/weeklyreport'

    def createuser(username):
        assert username
        rp = requests.post(iserverurl + '/account/ensureusercreated', params={'username': username}, verify=session.verify)
        assert rp.status_code == 204

    session.createuser = createuser

    def login(user="administrator", password="Pass123$"):
        createuser(user)
        session.fetch_token(
            token_url=iserverurl + '/connect/token',
            client_id=client_id,
            client_secret=client_secret,
            username=user,
            password=password,
            verify="../localhost.pem",
            cert="../localhost.crt"
        )
        session.username = user

    session.login = login

    return session

