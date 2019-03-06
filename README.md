# OpenDKP - AWS Lambdas

OpenDKP was developed over the course of several months as a passion project of mine. My primary goal was exposure to Amazon Web Services, as many components as I could. It started off as a simple AngularJS ap hosted in an [S3](https://aws.amazon.com/s3/) bucket, then grew to becoming a full fledged application with middle tier and backend provided by [API Gateway](https://aws.amazon.com/api-gateway/) and [Lambdas](https://aws.amazon.com/lambda/). 

I've always been a huge fan of .NET and this was also a great opportunity to work with [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/) within the AWS environment.

## Disclaimer
Please do not leverage the source code you find in this repository as a standard or best practice. This was primarily a learning experience for integration into AWS and the overall design is piece meal over a long period of time. Given the opportunity to rebuild, as many engineers would say, I'd do things totally different!

Much refactoring is needed and any contributions are much appreciated

## AWS Components
The following AWS components are required to be setup within your AWS account in order for these Lambdas to function appropriately

* [API Gateway](https://aws.amazon.com/api-gateway/) - You'll need to setup APIs pointing to each lambda
* [RDS](https://aws.amazon.com/rds/) - I used a MySQL Micro RDS instance, the dbContext is within this source code for the structure of the DB
* [IAM](https://aws.amazon.com/rds/) - You'll need the appropriate Identity & Access Management roles setup
* [Cognito](https://aws.amazon.com/cognito/) - Cognito handles our Users & Authorization
    * Both UserPool & Federated Identity Providers will have to be setup
    * DKP_ADMIN usergroup needs to be created as part of the UserPool for Administrators
    * DKP_ADMIN must have a valid IAM role assigned to it for lambda execution

## Installation

### Clone repo

``` bash
# clone the repo
$ git clone https://github.com/Moncleared/OpenDKPLambdas.git
```
Open the OpenDKPLambdas.sln file with Visual Studio 201X

I personally use AWS Toolkit for Visual Studio 2017, this allows you to right click and publish Lambdas directly from VS. Alternatively, the ideal situation would be to create a build pipeline that upon commit builds and publishes lambdas for you.

## Usage
Each Project represents one or more lambdas that should be published

## Documentation
Documentation will be developed and provided over time.

## Contributing

If you are interested in contributing back to this project, feel free to create pull requests. They will be reviwed and merged accordingly.

## Creators

**Moncleared (aka Moncs)**

* <https://github.com/Moncleared>

## Community

Get updates on CoreUI's development and chat with the project maintainers and community members.

- Join us on [Discord](https://discord.gg/WguFyYJ).
- Check us out at [OpenDKP](http://opendkp.com/).

## Support OpenDKP Development

If you would like to show your support, you can with  [PayPal](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2R3B5A3LJ5LBC&source=url), however, it is absolutely not required.
