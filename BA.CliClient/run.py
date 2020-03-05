#!/bin/python3

from context import session as contextsession
from datetime import datetime, timezone
import json


def main():
    context = ClientContext()
    while True:
        actiontype = choose_action()
        if actiontype is ExitAction:
            print('Demo exit')
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
        self.session = self.context.get_session_by_user_pass

    def run(self):
        print("No action")


class ExitAction(Action):
    description = "Exit"


class GetBikingAction(Action):
    description = "Get biking entries"

    def run(self):
        api = self.session()
        page = int(input("Page: ") or 0)
        pagesize = int(input("Page size: ") or 0)
        gparams = {}
        if page and pagesize:
            gparams['page'] = page
            gparams['pagesize'] = pagesize
        filter_ = input("Filter: ").strip() or None 
        if filter_:
            gparams['filter'] = filter_

        gr = api.get(api.bikingentryurl, params=gparams)

        assert gr.status_code == 200

        entries = gr.json()

        print("Found {} entries".format(len(entries)))

        for i, entry in enumerate(entries):
            print("Entry 1:".format(i+1))
            print_dict(entry)


class RecordBikingAction(Action):
    description = "Record biking entry"

    def run(self):
        api = self.session()

        entry = {
            "startTime": None,
            "distanceMeters": int(input('Distance in meters: ')),
            "durationSeconds": int(input('Duration in seconds: ')), 
            "latitude": 51.48,
            "longitude": 0.0
        }

        pr = api.post(api.bikingentryurl,
                headers={'Content-Type': 'application/json'},
                data=json.dumps(entry))
        
        assert pr.status_code == 201

        print("Entry recorded: " + str(pr.json()))


class ModifyBikingAction(Action):
    description = "Modify biking entry"

    def run(self):
        api = self.session()

        entryid = int(input("Enter entry id: "))

        gr = api.get(api.bikingentryurl + '/' + str(entryid))
        if gr.status_code != 200:
            print("Entry not found")
            return
        entry = gr.json()

        print("Found entry:")
        print_dict(entry)

        entry['distanceMeters'] = int(input('Distance in meters: ') or entry['distanceMeters'])
        entry["durationSeconds"] = int(input('Duration in seconds: ') or entry["durationSeconds"])
        entry["latitude"] = int(input('Latitude: ') or entry["latitude"])
        entry["longitude"] = int(input('Longitude: ') or entry["longitude"]) 

        pr = api.put(api.bikingentryurl + '/' + str(entryid),
                headers={'Content-Type': 'application/json'},
                data=json.dumps(entry))

        assert pr.status_code == 204

        print("Entry modified")



class DeleteBikingAction(Action):
    description = "Delete biking entry"

    def run(self):
        api = self.session()
        entryid = int(input("Enter entry id: "))
        dr = api.delete(api.bikingentryurl + '/' + str(entryid))
        if 200 <= dr.status_code <= 299:
            print("Entry deleted")
        else:
            print("Entry was not deleted ({} status)".format(dr.status_code))


class GetWeeklyReportAction(Action):
    description = "Weekly report"

    def run(self):
        api = self.session()

        print("Generating reports...")

        adminContext = ClientContext()
        admapi = adminContext.get_session_by_user_pass("administrator", "Pass123$")
        generaterequerst = admapi.post(admapi.reporturl, params = {'userId': api.username })
        assert generaterequerst.status_code == 200, \
            "Failed: {} {}".format(generaterequerst.status_code, str(generaterequerst))

        gr = api.get(api.reporturl)
        assert gr.status_code == 200
        reports = gr.json()
        print("{} reports found".format(reports))
        for r in reports:
            print_dict(r)


class ClientContext(object):
    def __init__(self):
        self._session = None

    def get_session_by_user_pass(self, username=None, password=None):
        if self._session is None:
            username = username or input("Enter username: ")
            password = password or input("Enter password: ")
            self._session = self._create_session()
            if username:
                self._session.login(username, password)
            else:
                self._session.login()
        return self._session

    def _create_session(self):
        return  contextsession()


def print_dict(entry):
    def getprefix(s):
        if isinstance(s, str):
            prefixlen = 100
            if len(s) > prefixlen:
                return s[:prefixlen] + "..."
        return s

    for k in entry:
        print("\t{} {}".format(k, getprefix(entry[k])))



if __name__ == '__main__':
    main()
