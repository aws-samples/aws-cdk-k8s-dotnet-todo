#!/bin/bash

APP_DIR=$PWD
echo 'Working Directory- '$APP_DIR

REGION=us-east-1
ACCOUNT_NUMBER=<ENTER_YOUR_ACCOUNT_NUMBER>
EKS_STACK_NAME=EksCdkStack
INSTALL_CDK=true
INSTALL_API=true

echo 'Region - '$REGION' Account Number - '$ACCOUNT_NUMBER

if [ "$INSTALL_CDK" = true ] ; then
    cd $APP_DIR/cdk
    dotnet build src
    cdk synth
    cdk bootstrap
    cdk deploy --require-approval never

    # This section updates the aws kube-config for the newly created cluster
    OUTPUTS=$(aws cloudformation describe-stacks --stack-name $EKS_STACK_NAME --query "Stacks[0].Outputs[0].OutputValue")
    EXEC_SCRIPT=""
    for val in $OUTPUTS; 
    do
        EXEC_SCRIPT=$EXEC_SCRIPT${val//[\"]/}" "
    done

    $($EXEC_SCRIPT)
    echo $EXEC_SCRIPT
fi

if [ "$INSTALL_API" = true ] ; then
    cd $APP_DIR/webapi
    dotnet build

    aws ecr get-login-password --region $REGION | docker login --username AWS --password-stdin $ACCOUNT_NUMBER.dkr.ecr.$REGION.amazonaws.com

    docker build -t todo-app .

    docker tag todo-app:latest $ACCOUNT_NUMBER.dkr.ecr.$REGION.amazonaws.com/todo-app:latest

    docker push $ACCOUNT_NUMBER.dkr.ecr.$REGION.amazonaws.com/todo-app:latest
fi
