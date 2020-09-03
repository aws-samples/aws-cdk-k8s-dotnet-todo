# Build and Deploy .Net Core WebAPI Container to Amazon EKS using CDK & cdk8s



 In this blog, we will leverage the development capabilities of [CDK for Kubernetes](https://cdk8s.io/) also known as cdk8s along with defining cloud infrastructure as code using [AWS Cloud Development Kit (AWS CDK)](https://docs.aws.amazon.com/cdk/latest/guide/home.html) which provisions it through AWS CloudFormation. 

cdk8s allows us to define Kubernetes apps and components using familiar languages. cdk8s is an open-source software development framework for defining Kubernetes applications and reusable abstractions using familiar programming languages and rich object-oriented APIs. cdk8s apps synthesize into standard Kubernetes manifests which can be applied to any Kubernetes cluster.  cdk8s lets you define applications using Typescript, JavaScript, and Python. In this blog we will use Python.

The AWS CDK is an open source software development framework to model and provision your cloud application resources using familiar programming languages, including TypeScript, JavaScript, Python, C# and Java. For the solution in this blog, we will use C# for the infrastructure code. Let’s get started!

At a high-level, we will go through the following:

1. Create a simple TODO, Microsoft .NET Core Web API application and integrate with Amazon Aurora Serverless database, AWS SDK Package like SSM into it.
2. Use AWS CDK to define the Infrastructure resources required for the application.
3. Use cdk8s to define, deploy and run the application within the created Kubernetes cluster (Created above by CDK)
4. Use [Elastic Kubernetes Service](https://aws.amazon.com/eks/), [Elastic Container Registry (ECR)](https://aws.amazon.com/ecr/), [Amazon Systems Manager (SSM)](https://aws.amazon.com/systems-manager/) (maintains the Aurora DB Credentials).
5. [Amazon Aurora Database (Serverless)](https://aws.amazon.com/rds/aurora/) is used as the backend.


The creation of above infrastructure on your account would result in charges beyond free tier. Please see below Pricing section for each individual services’ specific details. Make sure to clean up the built infrastructure to avoid any recurring cost.

![Alt text](blog/architecture.png?raw=true "Title")

The [Github source code](https://github.com/aws-samples/aws-cdk-k8s-dotnet-todo) includes a “cdk8s” folder where the .NET application (docker container WebAPI in ECR) will be deployed and run in the Kubernetes cluster. “cdk” folder with Microsoft .NET Core based AWS Cloud Development Kit (CDK) solution to build the infrastructure. This solution constructs the AWS infrastructure where the “webapi” (.NET Core Web api) is packaged, built as an artifact and pushed to AWS ECR. The provided .NET project leverages AWS SDK, Mysql data packages to connect to MySQL and interact with Amazon Aurora database. The exposed Web API endpoint performs HTTP calls (GET & POST) to add/retrieve TODOs. The end user can use any http get/put tool like curl or UI tools like Google Chrome ARC Rest Client or POSTMAN to validate the changes.

## Overview of the AWS services used in this solution

* [Amazon Aurora](https://aws.amazon.com/rds/aurora/), a MySQL and PostgreSQL-compatible relational database is used as the backend for the purpose of this project.
* [Amazon Elastic Kubernetes Service](https://aws.amazon.com/eks/) is a fully managed Kubernetes service. EKS runs upstream Kubernetes and is certified Kubernetes conformant so you can leverage all benefits of open source tooling from the community. You can also easily migrate any standard Kubernetes application to EKS without needing to refactor your code.In this example we use cdk8s to deploy the K8s services and pods The code is provided as part of the solution.
* [Amazon Elastic Container Registry,](https://aws.amazon.com/ecr/) the AWS provided Docker container registry is used and integrated with ECS, simplifying the development to production workflow.

## Prerequisites

We will use Docker Containers to deploy the Microsoft .NET Web API. The following are required to setup your development environment:

1. [Python >=3.7](https://www.python.org/downloads/release/python-377/)
2. [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/cliv2-migration.html)
3. [.NET Core](https://dotnet.microsoft.com/download/dotnet-core/3.1):
    1. Web API application was built using Microsoft .NET core 3.1
    2. Please refer Microsoft Documentation for installation.
4. [Docker](https://www.docker.com/)
    1. [Install Docker](https://www.docker.com/products/docker-desktop) based on your OS.
    2. Make sure the docker daemon is running
5. [Kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
6. [AWS CDK >= 1.58.0](https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html#getting_started_install)
7. [AWS cdk8s](https://github.com/awslabs/cdk8s/blob/master/docs/getting-started/python.md)
8.  Additionally, we use AWS SDK, MySql data packages for the Microsoft .NET project and have added them as nuget packages to the solution. In this example, we use Mysql data to connect to MySql/Aurora database and also AWS SDK Systems Manager to connect to Amazon Systems Manager.

## Walk-through of the Solution

We will use the following steps to provision the infrastructure (and services) and deploy the application:

**High level steps for deploying the solution**

1. Download/Clone the git solution

1. Code has installation and cleanup scripts. In “run_infra.sh”(installation script), provide your AWS account number where the solution needs to be deployed.

1. Provide your account number in “main.py” (within “cdk8s”) folder, where the cdk8s Kubernetes deployment needs to be done. This could be the same account number in above step

1. Run the installation scripts

1. Review and Validate the application

1. Cleanup (using the cleanup scripts) the solution


**Detailed steps are provided below**

### 1. Clone the sample code from the [GitHub](https://github.com/aws-samples/aws-cdk-k8s-dotnet-todo) location.

`$ git clone https://github.com/aws-samples/aws-cdk-k8s-dotnet-todo`

The git source provided above has a  “cdk”, “webapi” and a “cdk8s” folder. “webapi” has the necessary .NET Web API solution. We will use the AWS CDK commands to build the infrastructure and deploy the webapi into EKS. cdk8s code provided (using Python language) defines our kubernetes chart which creates a webservice (k8s Service and Deployment). 

Once the code is downloaded, please take a moment to see how CDK provides a simpler implementation for spinning up an infrastructure using C# code. You may use [Visual Studio Code](https://aws.amazon.com/visualstudiocode/) or your favorite choice of IDE to open the folder aws-cdk-k8s-dotnet-todo).
Open the file “/aws-cdk-k8s-dotnet-todo/cdk8/main.py”. Code below (provided a snippet from the github solution) spins up a VPC for the required Cidr and number of availability zones. Another snippet (below) creates a kubernetes chart and creates a webservice.

NOTE: Make sure to replace <YOUR_ACCOUNT_NUMBER> with your AWS account number (where you are trying to deploy/run this application).

main.py is called by “cdk8s.yaml” when cdk8s synth is invoked (by run_cdk8s.sh“). Windows users may have to change the name to ”main.py“ instead of ”.\main.py“ in the cdk8s.yaml

```
Open the file “/aws-cdk-k8s-dotnet-todo/cdk8/main.py”. 
Code below (provided a snippet from the github solution) creates a Chart and creates a webservice.

#!/usr/bin/env python
from constructs import Construct
from cdk8s import App, Chart
from imports import k8s
from webservice import webservice
class MyChart(Chart):
    def __init__(self, scope: Construct, ns: str):
        super().__init__(scope, ns)
        # define resources here
        webservice(self, 'todo-app', image='<YOUR_ACCOUNT_NUMBER>.dkr.ecr.us-east-1.amazonaws.com/todo-app:latest', replicas=1)
```


Open the file “/aws-cdk-k8s-dotnet-todo/cdk/src/EksCdk/EksCdkStack.cs”. Below snippet creates a kubernetes chart and creates a webservice.

```

// This sample snippet creates the EKS Cluster
var cluster = new Cluster(this, Constants.CLUSTER_ID, new ClusterProps {
        MastersRole = clusterAdmin,
        Version = KubernetesVersion.V1_16,
        KubectlEnabled = true,
        DefaultCapacity = 0,
        DefaultCapacityType = DefaultCapacityType.NODEGROUP,
        Vpc = vpc                
    });
```

### 2. Build the CDK source code and deploy the AWS CloudFormation stacks.

Scripts provided

* `run_infra.sh`
* `run_cdks.sh`
* `cleanup.sh.  -  NOTE. This will clean up the entire infrastructure. This is needed only when we need to cleanup/destroy the infrastructure created by this blog `

Provided “run_infra.sh” script/bash file as part of the code base folder, Make sure to replace <YOUR_ACCOUNT_NUMBER> with your AWS account number (where you are trying to deploy/run this application). This will create the CDK infrastructure and pushes the WebAPI into the ECR. Additionally the script registers the kube update config for the newly created cluster. 

If you would like to perform these steps you can do these manual steps as below

**Step 1**: Steps to build CDK

* `$ cd aws-cdk-k8s-dotnet-todo\cdk`
* `$ dotnet build src`
* `$ cdk synth`
* `$ cdk bootstrap`
* `$ cdk deploy --require-approval never`


The above CLI will produce output similar to below. Copy and execute this in the command line

Below provided below is a sample only:

EksCdkStack.cdkeksConfigCommand415D5239 = aws eks update-kubeconfig --name cdkeksDB67CD5C-34ca1ef8aef7463c80c3517cc12737da --region $REGION —role-arn arn:aws:iam::$ACCOUNT_NUMBER:role/EksCdkStack-AdminRole38563C57-57FLB39DWVJR

**Step 2:** Steps to Build and push WebAPI into ECR (todo-app ECR repository created as part of above CDK infrastructure)


* `$ cd aws-cdk-k8s-dotnet-todo\cdk`
* `$ dotnet build `

* `$ aws ecr get-login-password --region $REGION | docker login --username AWS --password-stdin $ACCOUNT_NUMBER.dkr.ecr.$REGION.amazonaws.com`
* `$ docker build -t todo-app .`
* `$ docker tag todo-app:latest $ACCOUNT_NUMBER.dkr.ecr.$REGION.amazonaws.com/todo-app:latest`
* `$ docker push $ACCOUNT_NUMBER.dkr.ecr.$REGION.amazonaws.com/todo-app:latest`


Make sure to update your region and account number above

**Step 3:** Steps to create kubernetes service and pods using cdk8s


* `$ cd aws-cdk-k8s-dotnet-todo\cdk8s`
* `$ pip install pipenv`
* `$ cdk8s import`
* `$ pipenv install`
* `$ pipenv install`
* `$ cdk8s synth`
* `$ kubectl apply -f dist/cdk8s.k8s.yaml`


After this is run, review the list/cdk8s.k8s.yaml.  cdk8s created k8s yamls that are needed for deploying, loading the image from the ECR. A sample is provided below.

The generated yaml has a service, deployment


```
apiVersion: v1
kind: Service
metadata:
  name: cdk8s-todo-app-service-4b26805b
....
....
....
---
apiVersion: apps/v1
kind: Deployment
metadata:
.
.
.
    spec:
      containers:
        - image: REDACTED.dkr.ecr.us-east-1.amazonaws.com/todo-app:latest
          name: app
          ports:
            - containerPort: 8080
```


Once the kubernetes objects are created, you can see the created pods and services like below. NOTE This could take sometime to start the ELB cluster with the deployment

* `$ kubectl get pods`
* `$ kubectl get services`



### 3. Stack Verification

The .NET code provided(cdk/src/EksCdk/Program.cs) creates the EksCdkStack as coded. Based on the name provided, a CloudFormation stack is built. You will be able to see this new stack in AWS Console > CloudFormation.

Stack creation creates close to 44 resources including a VPC. Some of them are provided here for your reference. Review some of the components the CDK will be creating.

```


    AWS::EC2::EIP                         | eks-vpc/PublicSubnet2/EIP
    AWS::EC2::VPC                         | eks-vpc
    AWS::EC2::InternetGateway             | eks-vpc/IGW
    AWS::EC2::VPCGatewayAttachment        | eks-vpc/VPCGW
    AWS::EC2::Subnet                      | eks-vpc/PrivateSubnet1/Subnet
    AWS::EC2::Subnet                      | eks-vpc/PublicSubnet2/Subnet
    AWS::EC2::Subnet                      | eks-vpc/PublicSubnet1/Subnet
    AWS::EC2::SecurityGroup               | cdk-eks/ControlPlaneSecurityGroup
    AWS::RDS::DBCluster                   | Database
    AWS::EC2::SecurityGroup               | db-sg
    AWS::RDS::DBInstance                  | Database/Instance1
    AWS::RDS::DBInstance                  | Database/Instance2
```

At the end of this step, you will create the Amazon Aurora DB table and the EKS Cluster exposed with a Classic LoadBalancer where the .NET Core Web API is deployed & exposed to the outside world. The output of the stack returns the following:

* HealthCheckUrl – http://<your_ALB>.us-east-1.elb.amazonaws.com/api/values
* Web ApiUrl – http://<your_ALB>.us-east-1.elb.amazonaws.com/api/todo

![Alt text](blog/testing_put.png?raw=true "Title")

Once the above CloudFormation stack is created successfully, take a moment to identify the major components. Here is the infrastructure you’d have created —

* Infrastructure containing VPC, Public & Private Subnet, Route Tables, Internet Gateway, NAT Gateway, Public Load Balancer, EKS Cluster.
* Other AWS Services – ECR, Amazon Aurora Database Serverless, Systems Manager, CloudWatch Logs.

Using CDK constructs, we have built the above infrastructure and integrated the solution with a Public Load Balancer. The output of this stack will give the API URLs for health check and API validation. As you notice by defining the solution using CDK, you were able to:

* Use object-oriented techniques to create a model of your system
* Organize your project into logical modules
* Code completion within your IDE

Let’s test the TODO API using any REST API tools, like Postman, Chrome extension ARC or RestMan.

* GET – Open browser and you can hit the Web ApiUrl to see the data.
    *  http://<your_ALB>.us-east-1.elb.amazonaws.com/api/todo
* POST – Create a sample –

Set Headers as “Content-type” & “application/json”
Sample request:
` {`
` "Task": "Deploying WebAPI in K8s",`
` "Status": "WIP"`
` }`


### Troubleshooting

* Issues with running the installation/shell script
    * Windows users - Shell scripts by default opens in a new window and closes once done. To see the execution you can paste the contents in a windows CMD and run them
    * If you are deploying through the provided installation/cleanup scripts, make sure to have “chmod +x <file_name>.sh” or “chmod +777 <file_name>.sh” (similar to elevate the execution permission of the scripts)
    * Linux Users - Permission issues could arise if you are not running as root user. you may have to “sudo su“ 
* Error retrieving pods/services/kubectl 
    * Make sure to update your kube config (This is an output from “cdk” deply in “run_infra.sh”)
        * aws eks update-kubeconfig —name <cluster-name>
        * Make sure to review your "~/.kube/config"
        * https://docs.aws.amazon.com/eks/latest/userguide/create-kubeconfig.html
* Check if local Docker is running. Optionally enable Kubernetes
    * Additional handy commands
    * Docker > Preferences > Kubernetes > "Enable Kubernetes" > Apply & Restart
    * kubectl version
    * kubectl config view 
    * kubectl config get-contexts
    * kubectl config use-context kubernetes
    * kubectl config use-context docker-for-desktop
* If you get unAuthorized error -  kubectl get pods error: You must be logged in to the server (Unauthorized)

            https://aws.amazon.com/premiumsupport/knowledge-center/eks-api-server-unauthorized-error/

*  Redeployment. If you are trying to remove and reinstall manually, 
    * Make sure to delete CDK staging directory if you are trying to delete and reinstall the stack. Your directory could be like below    cdktoolkit-stagingbucket-guid

    * Make sure to delete the SSM parameter "/Database/Config/AuroraConnectionString"
* Where can i see the load balancer 
    * $ kubectl get svc.   - This command can provide the LB url
    * Optionally in AWS Console > EC2 > LoadBalancer
* My application is not loading or running when I hit the LB url
    * We use Aurora Serverless Database. The DB may take time to initialize for the first time
        * Check if AWS Console > RDS > “eks-cdk-aurora-database” is running
        * Select “Query Editor” select the database, enter the credentials. This is provided in SSM “Database/Config/AuroraConnectionString”
        * run “select * from ToDos"
    * Check if the pods are running
        * kubectl get pods
        * kubectl describe pod <pod_name>
        * kubectl exec -t -i <pod_name> bash
* My CDK8s deployment failed
    * Make sure you have the prerequisites versions. ex: pipenv, python, kubectl
    * Check your kubectl pods, services (“ex: kubectl get pods, kubectl get src”) to make sure you are able to connect and view the deployment
    * kube config issues - Open AWS Console> CloudFormation> EksCdkStack > Output. Select the aws update kubeconfig command from the console and run that in your command line (note this is done automatically by the installation run_infra script)
    * Windows users check for your reference of “main.py” in cdk8s.yaml (within “cdk8s” folder)
        * language: python
            app: pipenv run ./main.py
            imports:
              - k8s

### Pricing

* EKS - https://aws.amazon.com/eks/pricing/
* Aurora RDS - https://aws.amazon.com/rds/aurora/pricing/
* Classic LoadBalancer - https://aws.amazon.com/elasticloadbalancing/pricing/
* EC2 - https://aws.amazon.com/ec2/pricing/

### 4. Code Cleanup

Run the “cleanup.sh” to delete the created infrastructure

If you would like to do this manually, make sure the following resources are deleted before performing the delete/destroy:

* Stop the kubernetes services, deployment, pods
* Contents of the S3 files are deleted.
* In AWS Console, look for “CDKToolkit” stack
* Go to “Resources” tab, select the s3 bucket
* Select all the contents & delete the contents manually

Above can be ran by below CLIs also


* `$ cd aws-cdk-k8s-dotnet-todo\cdk8s`
* `$ kubectl delete pods --all`
* `$ kubectl delete services --all`
* `$ aws ecr delete-repository --repository-name todo-app --force`
* `$ cdk destroy --force`
* `$ aws cloudformation delete-stack --stack-name CDKToolkit`

## Conclusion

As you can see, we were able to onboard an ASP.NET Core Web API application and integrate it with various AWS Services. The post walked through deploying Microsoft .NET Core application code as containers with infrastructure as code using CDK and deploy the Kubernetes services, pods using CDK8s. To try out more CDK8s examples we encourage you try [various examples](https://github.com/awslabs/cdk8s/blob/master/examples/README.md), additionally for architecture as code with applications on AWS, check out [patterns](https://docs.aws.amazon.com/prescriptive-guidance/latest/patterns/deploy-kubernetes-resources-and-packages-using-amazon-eks-and-a-helm-chart-repository-in-amazon-s3.html), [AWS EKS Architecture](https://aws.amazon.com/quickstart/architecture/amazon-eks/) 
We encourage you to try this example and see for yourself how this overall application design works within AWS. Then, it will just be a matter of replacing your current applications (Web API, MVC, or other Microsoft .NET core application), package them as Docker containers and let the Amazon EKS manage the application efficiently.
If you have any questions/feedback about this blog please provide your comments below!

## References

* [CDK for Kubernetes](https://github.com/awslabs/cdk8s/blob/master/README.md#getting-started)
* [AWS Cloud Development Kit (CDK)](https://docs.aws.amazon.com/cdk/latest/guide/home.html)
* [AWS CDK .NET API Reference](https://docs.aws.amazon.com/cdk/api/latest/dotnet/api/index.html)
* [Microsoft .Net Core](https://dotnet.microsoft.com/download/dotnet-core/2.2)
* [Windows on AWS](https://aws.amazon.com/windows)
* [Docker Containers](https://www.docker.com/resources/what-container)
* [.NET WebAPI with AWS CDK Example](https://aws.amazon.com/blogs/developer/developing-a-microsoft-net-core-web-api-application-with-aurora-database-using-aws-cdk/)


## License

This library is licensed under the MIT-0 License. See the LICENSE file.
