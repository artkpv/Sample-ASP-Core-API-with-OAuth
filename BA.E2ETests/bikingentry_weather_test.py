#!/bin/python3

import json
import time
from datetime import datetime, timezone

def test_weather_condition_on_post(api):
    entry = {
        "startTime": None,
        "distanceMeters": 1000,
        "durationSeconds": 300,
        "latitude": 41.85,
        "longitude": -87.65
    }

    pr = api.post(api.bikingentryurl,
            headers={'Content-Type': 'application/json'},
            data=json.dumps(entry))
    assert pr.status_code == 201
    ans = pr.json()
    myid = ans['id']

    triesNum = 3
    for tryTime in range(triesNum):
        gr = api.get(api.bikingentryurl + '/' + str(myid) )
        assert gr.status_code == 200
        persisted = gr.json()
        if persisted['weather']:
            break
        else:
            time.sleep(1)

    assert 'weather' in persisted
    assert persisted['weather']
