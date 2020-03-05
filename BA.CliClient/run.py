#!/bin/python3

import requests
import os
from oauthlib.oauth2 import BackendApplicationClient, LegacyApplicationClient
from requests_oauthlib import OAuth2Session
from context import session as contextsession


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
            if username:
                self._session.login(username, password)
            else:
                self._session.login()
        return self._session

    def _create_session(self):
        return  contextsession()


if __name__ == '__main__':
    main()
