#!/bin/bash

APP_DIR=$PWD
cd $APP_DIR/cdk

kubectl delete pods --all
kubectl delete deployment --all
kubectl delete services --all

aws ecr delete-repository --repository-name todo-app --force

cdk destroy --force

STG_BUCKET=$(aws cloudformation describe-stacks --stack-name CDKToolkit --query "Stacks[0].Outputs[?OutputKey=='BucketName'].OutputValue")

CNT=0
for val in $STG_BUCKET; 
do
    if [ $CNT == 1 ]; then 
        BUCKET_NAME=${val//[\"]/}
        echo "Cleaning bucket $BUCKET_NAME"
        aws s3 ls s3://$BUCKET_NAME 
        aws s3 rm s3://$BUCKET_NAME --recursive
    fi
    let CNT=CNT+1
done


aws cloudformation delete-stack --stack-name CDKToolkit


