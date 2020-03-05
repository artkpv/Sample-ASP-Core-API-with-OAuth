"""
  Pytests fixtures. Auto discoverable (no need to import).
"""
import pytest
import requests
import unittest
import os
from urllib.parse import urljoin


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

    apiurls = os.environ['BIKINGAPP_API_URLS']
    assert apiurls
    apiurl = apiurls.split(';')[0]
    session.bikingentryurl = urljoin(apiurl, 'api/bikingentry')
    session.identityurl = urljoin(apiurl, 'api/identity')
    session.reporturl = urljoin(apiurl, 'api/weeklyreport')

    iserverurl = os.environ['BIKINGAPP_ISERVER_URLS']
    assert iserverurl
    iserverurl = iserverurl.split(';')[0]

    def createuser(username):
        assert username
        requesturl = urljoin(iserverurl, '/account/ensureusercreated')
        rp = requests.post(requesturl, params={'username': username}, verify=session.verify)
        assert rp.status_code == 204

    session.createuser = createuser

    def login(user="administrator", password="Pass123$"):
        createuser(user)
        tokenurl = urljoin(iserverurl,'/connect/token')
        session.fetch_token(
            token_url=tokenurl,
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

