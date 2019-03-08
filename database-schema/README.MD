## OpenDKP Database Structure
This folder contains a .sql file which represents the structure of the OpenDKP schema. You can use this file to Import into a newly created RDS instance if you choose. You can also use the Entity Framework to create the DB using the dbContext already found within OpenDKPShared folder. Which way you decide to do is up to you!

## General Steps for AWS RDS Instance Setup

* Create a Micro instance for RDS
    * The settings you select are completely up to you (backup style, failover, etc)
        * These selects have an impact on the price your AWS Account will be charged, please research this ahead of time
* Ensure your Security Group and VPC allow for Inbound/Outbound connections appropriately
    * Essentially, you'll need your Lambdas to connect to the RDS instance as well as potentially MySQL Workbench for anyone who wants to look at the database directly
    * Security Groups have inbound and outbound rules, make sure both are open for the RDS port 3309
* Once the database is up and running, test a connection to it from your local computer using any of your preferred methods (ssh or MySQL Workbench etc)
    * if connection fails or is rejected, review your username/password + security group setup
* Once connection is successful:
    * Create a new database called opendkp
    * Import the .sql file in this directory
    * Verify your tables now exist properly