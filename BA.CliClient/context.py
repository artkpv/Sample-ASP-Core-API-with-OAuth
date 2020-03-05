#!/bin/python3
import requests
import unittest
import os
from urllib.parse import urljoin
from oauthlib.oauth2 import BackendApplicationClient, LegacyApplicationClient
from requests_oauthlib import OAuth2Session


def session():
    abspath = os.path.abspath(__file__)
    dname = os.path.dirname(abspath)
    crt_file_path = os.path.join(dname, "../localhost.crt")
    pem_file_path = os.path.join(dname, "../localhost.pem")
    
    apiurls = os.environ['BIKINGAPP_API_URLS']
    assert apiurls
    apiurl = apiurls.split(';')[0]

    iserverurl = os.environ['BIKINGAPP_ISERVER_URLS']
    assert iserverurl
    iserverurl = iserverurl.split(';')[0]

    client_id = 'ro.client'
    client_secret = '992536EF-F270-4058-80CA-1C89C192FAAB'

    session = OAuth2Session(
        client=LegacyApplicationClient(client_id=client_id, scope="JogAppWebAPI")
    )
    session.verify = crt_file_path
    session.bikingentryurl = urljoin(apiurl, 'api/bikingentry')
    session.identityurl = urljoin(apiurl, 'api/identity')
    session.reporturl = urljoin(apiurl, 'api/weeklyreport')

    def createuser(username):
        assert username
        requesturl = urljoin(iserverurl, '/account/ensureusercreated')
        rp = requests.post(requesturl, params={'username': username}, verify=crt_file_path)
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
            verify=pem_file_path,
            cert=crt_file_path
        )
        session.username = user

    session.login = login

    return session
