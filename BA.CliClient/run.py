#!/bin/python3

import requests
from oauthlib.oauth2 import BackendApplicationClient, LegacyApplicationClient
from requests_oauthlib import OAuth2Session


def main():
    while True:
        action = get_action()
        if not action:
            print('exit')
            return
        action()


def get_action():
    action_type = read_action_type()
    if is_exit(action_type):
        return None
    action = construct_action(action_type)
    return action or get_action()


def read_action_type():
    while True:
        print("Choose action:")
        print(str.join('\n', (k + ": " + action_types[k] for k in action_types)))
        entered = input().strip()
        if entered in action_types:
            return entered


def construct_action(action_type):
    assert action_type in constructors
    return construct_action_with(constructors[action_type])


action_types = 
{
    "0": "Exit",
    "1": "Get biking entries",
    "2": "Enter biking entry",
    "3": "Modify biking entry",
    "4": "Delete biking entry",
    "5": "Get report"
}

constructors = 
{
    "0": lambda : None ,
}

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



if __name__ == '__main__':
    main()
