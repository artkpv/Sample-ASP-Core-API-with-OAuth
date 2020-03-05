#!/bin/python3

from datetime import datetime, timezone
from random import Random
from bikingentry_help import insert_entry
import json

def test_generates_reports(session):
    session.login("administrator")
    count = 3
    for i in range(count):
        insert_entry(session)

    pr = session.post(session.reporturl, params = {'userId': session.username })

    assert pr.status_code == 200
    ans = pr.json()
    num = int(ans)
    assert num > 0


def test_filter_startTime(session):
    session.login("administrator")
    count = 10
    starttime = getServerFriendlyNowDateTime()
    for i in range(count):
        insert_entry(session, startTime=starttime)

    pr = session.post(session.reporturl, params = {'userId': session.username })

    filterstr = 'date ge "' + starttime + '"'
    gr = session.get(session.reporturl, params = {'filter': filterstr , 'page':1, 'pagesize': 3 })

    assert gr.status_code == 200
    ans = gr.json()
    print(ans)
    assert len(ans) > 0


def getServerFriendlyNowDateTime():
    """ Avoids microseconds as datetime storing differs. """
    d = datetime.now(timezone.utc)
    return datetime(
        d.year,
        d.month,
        d.day,
        d.hour,
        d.minute,
        d.second,
        tzinfo=d.tzinfo
    ).isoformat()

