#!/bin/python3

import requests
import os
from oauthlib.oauth2 import BackendApplicationClient, LegacyApplicationClient
from requests_oauthlib import OAuth2Session


def main():
    context = ClientContext()
    while True:
        actiontype = choose_action()
        if actiontype is ExitAction:
            print('exit')
            return
        print("Action chosen: " + actiontype.description)
        actiontype(context).run()


def choose_action():
    actions = [(i, class_) for i, class_ in enumerate(Action.__subclasses__())]
    
    while True:
        print("\nAvailable actions:")
        for i, c in actions:
            if c.description:
                print(" {}) {}".format(i+1, c.description))
        entered = input("Enter action number: ").strip()
        if entered.isnumeric():
            num = int(entered) - 1
            if 0 <= num < len(actions):
                return actions[num][1]


class Action(object):
    description = "<Unknown>"
    def __init__(self, context):
        self.context = context

    def run(self):
        print("No action")


class ExitAction(Action):
    description = "Exit"


class GetBikingAction(Action):
    description = "Get biking entries"

    def run(self):
        self.context.get_session_by_user_pass()
        print('getting entries')


class RecordBikingAction(Action):
    description = "Record biking entry"


class ModifyBikingAction(Action):
    description = "Modify biking entry"


class DeleteBikingAction(Action):
    description = "Delete biking entry"


class GetWeeklyReportAction(Action):
    description = "Weekly report"


class ClientContext(object):
    def __init__(self):
        self._session = None

    def get_session_by_user_pass(self):
        if self._session is None:
            username = input("Enter username: ")
            password = input("Enter password: ")
            self._session = self._create_session()
            self._session.login(username, password)
        return self._session

    def _create_session(self):
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
