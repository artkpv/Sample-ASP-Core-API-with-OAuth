from datetime import datetime, timezone
from random import Random
import json

randomgenerator = Random()

SaintPetersburgLatLon = [59.9375, 30.308611]

def insert_entry(api, **kargs):
    r = randomgenerator
    someMinDist = 100
    someMaxDist = 42000
    entry = {
        "startTime": getranddatetime(),
        "distanceMeters": r.randint(someMinDist, someMaxDist),
        "latitude": SaintPetersburgLatLon[0],
        "longitude": SaintPetersburgLatLon[1]
    }

    for k in kargs:
        entry[k] = kargs[k]

    entry['durationSeconds'] = _get_rand_duration(entry['distanceMeters'])

    pr = api.post(api.bikingentryurl,
            headers={'Content-Type': 'application/json'},
            data=json.dumps(entry))

    assert pr.status_code == 201
    entry['id'] = pr.json()['id']
    return entry


def _get_rand_duration(distanceMeters):
    someFastPace = 30  # 3 min per km.
    someSlowPace = 100  # 10 min per km.
    randAdjConstant = 10.0
    speed = randomgenerator.randint(someFastPace, someSlowPace) / randAdjConstant
    kilo = 1000.0
    secondsInHour = 3600
    return int(distanceMeters / kilo / speed * secondsInHour)


def getranddatetime():
    r = randomgenerator
    # Numbers are randomly chosen.
    d = datetime(
        2020,
        r.randint(1,12),
        r.randint(1, 28),
        r.randint(0, 23),
        r.randint(0, 59),
        r.randint(0, 59),
        tzinfo=timezone.utc
    )
    return d.isoformat()
