#!/bin/python3

from datetime import datetime, timezone
from random import Random
from bikingentry_help import insert_entry
import json

def test_post(api):
    entry = {
        "startTime": "2020-02-22T18:19:20Z",
        "distanceMeters": 1000,
        "durationSeconds": 300,
        "latitude": 50.30,
        "longitude": 0.01
    }

    pr = api.post(api.bikingentryurl,
            headers={'Content-Type': 'application/json'},
            data=json.dumps(entry))
    
    assert pr.status_code == 201
    ans = pr.json()
    assert ans
    assert 'id' in ans
    assert isinstance(ans['id'], int)
    assert ans['id'] > 0


def test_get(api):
    entry = insert_entry(api)

    gr = api.get(api.bikingentryurl + '/' + str(entry['id']) )

    assert gr.status_code == 200
    persisted = gr.json()
    assert persisted
    assert persisted['id'] == entry['id']
    for k in entry:
        assert k in persisted
        assert persisted[k] == entry[k]


def test_put(api):
    entry = insert_entry(api)

    entry['startTime'] = datetime(2020, 1, 1, tzinfo=timezone.utc).isoformat()

    pr = api.put(api.bikingentryurl + '/' + str(entry['id']),
            headers={'Content-Type': 'application/json'},
            data=json.dumps(entry))

    assert pr.status_code == 204
    gr = api.get(api.bikingentryurl + '/' + str(entry['id']))
    persisted = gr.json()
    assert persisted
    assert persisted['id'] == entry['id']
    assert persisted['startTime'] == entry['startTime']


def test_delete(api):
    entry = insert_entry(api)

    dr = api.delete(api.bikingentryurl + '/' + str(entry['id']))

    assert dr.status_code == 200
    gr = api.get(api.bikingentryurl + '/' + str(entry['id']))
    assert gr.status_code == 404

