#!/bin/bash

APP_DIR=$PWD
# Install cdk8s
#pip install cdk8s or (pip3) accordingly
cd $APP_DIR/cdk8s
# Delete for Deployment, Services, pods need not be run for the first time

#kubectl delete deployment --all
#kubectl delete services --all
#kubectl delete pods --all

pip install pipenv
cdk8s import
pipenv install
pipenv run cdk8s synth
kubectl apply -f dist/cdk8s.k8s.yaml
kubectl get pods
kubectl get services
