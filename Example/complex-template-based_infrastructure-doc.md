# Infrastructure Documentation
## Resources
### AWS::EC2::Instance
- **AppEC2Instance**

The AWS CloudFormation resource of type "AWS::EC2::Instance" is designed to launch an Amazon Elastic Compute Cloud (EC2) instance. Specifically, this resource initializes an EC2 instance with the t3.micro instance type within the specified subnet ("AppSubnet"). Additionally, it uses the image ID "ami-0abcdef1234567890" and labels the instance as "AppServer".
### AWS::EC2::Subnet
- **AppSubnet**

The AWS::EC2::Subnet resource defined in the CloudFormation template creates a subnet within the specified VPC (AppVPC), which has an IP range of 10.0.1.0/24 and is located in the us-east-1a Availability Zone. It also assigns the tag "Name" with the value "AppSubnet".
### AWS::EC2::VPC
- **AppVPC**

The AWS::EC2::VPC resource in this CloudFormation template creates a virtual private cloud (VPC) with the following characteristics:

- CidrBlock: Assigns IP address range 10.0.0.0/16 for the VPC network.
- EnableDnsSupport: Enables DNS support within the VPC, allowing instances to communicate using hostnames rather than IP addresses.
- EnableDnsHostnames: Enables DNS hostnames for instances in the VPC, providing a simple way of identifying and accessing these instances by name.
- Tags: Attaches a tag with key "Name" and value "AppVPC" to the VPC for easier identification and management within AWS.
### AWS::RDS::DBInstance
- **AppRDSInstance**

The AWS CloudFormation resource "AWS::RDS::DBInstance" configured in this template sets up a MySQL database instance on an Amazon Web Services (AWS) Relational Database Service (RDS). Specifically, it allocates 20GB of storage, uses the db.t2.micro instance class, and provides the specified credentials ("admin" as username and "password1234" as password) to access the database, which is named "AppDB". Additionally, this instance is accessible publicly without requiring association with a VPC security group or subnet.
### AWS::S3::Bucket
- **AppS3Bucket**

This CloudFormation resource, `AWS::S3::Bucket`, named "my-complex-app-bucket", is designed to create an Amazon S3 bucket within AWS. The included `VersioningConfiguration` ensures that all the current and future object versions in this bucket are preserved, allowing for data recovery and point-in-time restore functionality.
