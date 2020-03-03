#!/bin/python3


def test_authorizes(api):
    api.login()
    gr = api.get(api.identityurl)
    assert gr.status_code == 200
    print(gr.json())

