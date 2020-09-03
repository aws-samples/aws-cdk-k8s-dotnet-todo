using Amazon.CDK;
using Amazon.CDK.AWS.EKS;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using ecr = Amazon.CDK.AWS.ECR;
using ec2 = Amazon.CDK.AWS.EC2;
using ssm = Amazon.CDK.AWS.SSM;
using rds = Amazon.CDK.AWS.RDS;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EksCdk
{
    public class EksCdkStack : Amazon.CDK.Stack
    {
        internal EksCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var clusterAdmin = new Role(this, Constants.ADMIN_ROLE, new RoleProps {
                AssumedBy = new AccountRootPrincipal()
            });

            IVpc vpc = new Vpc(this, Constants.VPC_ID, new VpcProps{
                Cidr = Constants.VPC_CIDR
            });            

            var cluster = new Cluster(this, Constants.CLUSTER_ID, new ClusterProps {
                MastersRole = clusterAdmin,
                Version = KubernetesVersion.V1_16,
                KubectlEnabled = true,
                DefaultCapacity = 0,
                Vpc = vpc                
            });
            
            var tags = new Dictionary<string, string>();
            tags.Add("name",Constants.CDK8s);

            var eksEC2sNodeGroup = cluster.AddNodegroup(Constants.CLUSTER_NODE_GRP_ID, new NodegroupOptions{
                InstanceType = new InstanceType(Constants.EC2_INSTANCE_TYPE),
                MinSize = 2,
                Subnets = new SubnetSelection{ Subnets = vpc.PrivateSubnets},
                Tags = tags
            });

            string[] ManagedPolicyArns = GetNodeRoleManagedPolicyARNs();
              foreach(string arn in ManagedPolicyArns){
                eksEC2sNodeGroup.Role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName(arn));
            }

            var repository = new ecr.Repository(this, Constants.ECR_REPOSITORY_ID, new ecr.RepositoryProps{
                RepositoryName = Constants.ECR_REPOSITORY_NAME
            });

            #region Aurora Database 

            var secGrp = new SecurityGroup(this, Constants.DATABASE_SECURITY_GRP, new SecurityGroupProps{ Vpc = vpc });
            var eksSecGrp = ec2.SecurityGroup.FromSecurityGroupId(this, Constants.EKS_SECURITY_GRP, cluster.ClusterSecurityGroupId);            
            secGrp.AddIngressRule(eksSecGrp, ec2.Port.Tcp(3306), description: Constants.EC2_INGRESS_DESCRIPTION);

            var privateSubnets = new List<string>();
            foreach(Subnet subnet in vpc.PrivateSubnets){
                privateSubnets.Add(subnet.SubnetId);
            }

            var dbsubnetGroup = new rds.CfnDBSubnetGroup(this, Constants.AURORA_DB_SUBNET_ID, new rds.CfnDBSubnetGroupProps{
                DbSubnetGroupDescription = Constants.AURORA_DB_SUBNET_DESCRIPTION,
                DbSubnetGroupName = Constants.AURORA_DB_SUBNET_GROUP_NAME,
                SubnetIds = privateSubnets.ToArray()
            });

            List<CfnTag> cfnDbSecurityGroupTag = new List<CfnTag>();
            CfnTag tagName = new CfnTag(){
                Key = "Name", Value = Constants.APP_NAME
            };
            cfnDbSecurityGroupTag.Add(tagName);            

            var dbSecurityGroup = new CfnSecurityGroup(this, Constants.AURORA_CFN_SG_ID,
                new CfnSecurityGroupProps{
                    VpcId = vpc.VpcId,
                    GroupName = Constants.AURORA_GROUP_NAME,
                    GroupDescription = "Access to the RDS",
                    Tags = cfnDbSecurityGroupTag.ToArray()
                }
            );

             var cfnSecurityGroupIngress = new ec2.CfnSecurityGroupIngress(
                this, Constants.AURORA_SG_INGRESS, new ec2.CfnSecurityGroupIngressProps{
                    Description = Constants.AURORA_SG_INGRESS_DESCRIPTION,
                    FromPort = Constants.AURORA_PORT,
                    ToPort = Constants.AURORA_PORT,
                    IpProtocol = Constants.CONTAINER_PROTOCOL,
                    SourceSecurityGroupId =  eksSecGrp.SecurityGroupId,
                    GroupId = dbSecurityGroup.AttrGroupId
            });

            var dbcluster = new rds.CfnDBCluster(this, Constants.AURORA_TODO_DATABASE, 
                new rds.CfnDBClusterProps{
                    Engine = Constants.AURORA_DB_ENGINE,
                    EngineMode = Constants.AURORA_ENGINE_MODE,
                    Port = Constants.AURORA_PORT,
                    MasterUsername = Constants.DB_USER_VALUE,
                    MasterUserPassword = Constants.DB_PASSWORD_VALUE,
                    DbSubnetGroupName = Constants.AURORA_DB_SUBNET_GROUP_NAME, 
                    DatabaseName = Constants.DB_NAME_VALUE,
                    VpcSecurityGroupIds = new string[]{
                        dbSecurityGroup.AttrGroupId
                    }
            });
            dbcluster.DbClusterIdentifier = Constants.AURORA_TODO_DATABASE;
            dbcluster.AddDependsOn(dbsubnetGroup);
            dbcluster.CfnOptions.DeletionPolicy = CfnDeletionPolicy.DELETE;
            #endregion

            #region  SSM
            
            StringBuilder connString = new StringBuilder();
            connString.AppendFormat("server={0}", dbcluster.AttrEndpointAddress);
            connString.AppendFormat(";port={0}", Constants.AURORA_PORT);
            connString.AppendFormat(";database={0}", Constants.DB_NAME_VALUE);
            connString.AppendFormat(";user={0}", Constants.DB_USER_VALUE);
            connString.AppendFormat(";password={0}", Constants.DB_PASSWORD_VALUE);
            
            new ssm.StringParameter(this, "Parameter", new ssm.StringParameterProps {
                Description = "Maintains the Aurora Database Connection String",
                ParameterName = Constants.SSM_DB_CONN_STRING,
                StringValue = connString.ToString(),
                Tier = ssm.ParameterTier.ADVANCED
            });
            
            #endregion
        }

        public static string[] GetNodeRoleManagedPolicyARNs(){ 
            string[] taskDefinitionManagedRoleActions = new string[]{
              "AmazonRDSFullAccess",
              "AmazonSSMFullAccess",
              "AmazonEC2ContainerServiceFullAccess",
              "service-role/AmazonEC2ContainerServiceforEC2Role"
          };
          return taskDefinitionManagedRoleActions;
        }
    }
}
