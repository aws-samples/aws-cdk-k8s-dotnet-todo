namespace EksCdk
{
    public class Constants
    {
        public static string APP_NAME = "todo-app";
        public static string ADMIN_ROLE = "AdminRole";

        public static string STACK_PREFIX = "eks-cdk-";
        public static string AURORA_DB_ID = STACK_PREFIX+ "aurora-db-id";
        public static string AURORA_TODO_DATABASE = STACK_PREFIX+ "aurora-database";

        public static string AURORA_CFN_SG_ID = "cfn-db-sg";

        public static string AURORA_GROUP_NAME = Constants.APP_NAME + "-eks-db-sg";

        public static string AURORA_SG_INGRESS = STACK_PREFIX +  "cfn-db-sg-ingress";

        public static string AURORA_SG_INGRESS_DESCRIPTION =  "Ingress 3306";
        public static string AURORA_DB_INSTANCE_IDENTIFIER = "r5.large";
        public static string AURORA_DB_ENGINE_VERSION = "5.6.10a";
        public static string AURORA_DB_ENGINE = "aurora";
        public static string AURORA_ENGINE_MODE = "serverless";
        public static double AURORA_PORT = 3306;
        public static string AURORA_DB_SUBNET_ID = STACK_PREFIX+ "subnet-id";
        public static string AURORA_DB_SUBNET_DESCRIPTION = STACK_PREFIX+ "subnet-desc";
        public static string AURORA_DB_SUBNET_GROUP_NAME = STACK_PREFIX+ "subnet-grp";
        public static string CONTAINER_PROTOCOL = "tcp";
        public static string CDK8s = "cdk8s";
        public static string CLUSTER_ID = "cdk-eks";
        public static string CLUSTER_NODE_GRP_ID = "nodegroup";
        public static string DB_NAME = "/Database/Config/DBName";
        public static string DB_NAME_VALUE = "todo";
        public static string DB_USER_VALUE = "master";
        public static string DB_PASSWORD_VALUE = "netc0re123";
        public static string DB_NAME_ID = STACK_PREFIX+ "db-name-id";
        public static string DATABASE = "Database";
        public static string DATABASE_SECURITY_GRP = "db-sg";
        public static string EKS_SECURITY_GRP = "eks-sg";
        public static string EC2_INSTANCE_TYPE = "t3.large";
        public static string EC2_INGRESS_DESCRIPTION = "EKS cluster Ingress";
        public static string ECR_REPOSITORY_ID = "todo-app-id";
        public static string ECR_REPOSITORY_NAME = "todo-app";
        public static string SSM_DB_CONN_STRING_ID = STACK_PREFIX+ "image-repo-db-conn-string-id";
        public static string SSM_DB_CONN_STRING = "/Database/Config/AuroraConnectionString";

        public static string VPC_ID = "eks-vpc";

        public static string VPC_CIDR = "10.0.0.0/16";
    }
}
