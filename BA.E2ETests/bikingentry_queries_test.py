#!/bin/python3

from datetime import datetime, timezone
from random import Random
from bikingentry_help import insert_entry
import json

def test_page_one(api):
    count = 10
    for i in range(count):
        insert_entry(api)
    pagesize = 3

    gr = api.get(api.bikingentryurl, params = {'pagesize': pagesize, 'page': 1 })

    assert gr.status_code == 200
    ans = gr.json()
    assert len(ans) == pagesize


def test_pages(api):
    count = 10
    for i in range(count):
        insert_entry(api)
    pagesize = 3

    foundids = set()
    page = 1
    while True:
        gr = api.get(api.bikingentryurl, params = {'pagesize': pagesize, 'page': page })
        assert gr.status_code == 200
        ans = gr.json()
        if len(ans) == 0:
            break
        for entry in ans:
            assert entry['id'] not in foundids
            foundids.add(entry['id'])
        page += 1
    assert len(foundids) >= count


def test_filter_startTime(api):
    count = 10
    
    starttime = getServerFriendlyNowDateTime()
    for i in range(count):
        insert_entry(api, startTime=starttime)
    filterstr = 'startTime eq "' + starttime + '"'

    gr = api.get(api.bikingentryurl, params = {'filter': filterstr })

    assert gr.status_code == 200
    ans = gr.json()
    assert len(ans) == count 

    for entry in ans:
        assert entry['startTime'] == starttime


def test_filter_distance(api):
    count = 10
    
    distance = generateUniqueDistance()
    for i in range(count):
        insert_entry(api, distanceMeters=distance)
    # TODO Use str.format
    filterstr = 'distanceMeters eq ' + str(distance)

    gr = api.get(api.bikingentryurl, params = {'filter': filterstr })

    assert gr.status_code == 200
    ans = gr.json()
    assert len(ans) == count 
    for entry in ans:
        assert entry['distanceMeters'] == distance


def test_filter_and_paging(api):
    count = 10
    
    distance = generateUniqueDistance()
    starttime = getServerFriendlyNowDateTime()
    for i in range(count):
        insert_entry(api, distanceMeters=distance, startTime=starttime)
    # TODO Use str.format
    filterstr = '(distanceMeters eq ' + str(distance) + ') and (startTime eq "' + starttime + '")'

    psize = 3
    gr = api.get(api.bikingentryurl, params = {'filter': filterstr, 'page':1, 'pagesize': psize })

    assert gr.status_code == 200
    ans = gr.json()
    assert len(ans) == psize 
    for entry in ans:
        assert entry['distanceMeters'] == distance
        assert entry['startTime'] == starttime


def getServerFriendlyNowDateTime():
    """ Avoids microseconds as datetime storing differs. """
    d = datetime.utcnow()
    return datetime(
        d.year,
        d.month,
        d.day,
        d.hour,
        d.minute,
        d.second,
        tzinfo=timezone.utc
    ).isoformat()


def generateUniqueDistance():
    return int(datetime.now().strftime('%d%f'))



# TODO Write rand generated test on large scope of input.
