#!/usr/bin/env python
from constructs import Construct
from cdk8s import App, Chart

from imports import k8s
from webservice import WebService

class MyChart(Chart):
    def __init__(self, scope: Construct, ns: str):
        super().__init__(scope, ns)

        # define resources here
        WebService(self, 'todo-app', image='<REPLACE_YOUR_ACCOUNT_NUMBER>.dkr.ecr.us-east-1.amazonaws.com/todo-app:latest', replicas=1)

app = App()
MyChart(app, "cdk8s")

app.synth()
