## API Gateway Exports
Using these files, you can quickly setup your API Gateway with the appropriate APIs, Headers, and routing to Lambdas

## Pre-reqs
* Make sure you've published all of your Lambdas into AWS account first
* Make sure you've got your AWS Account Number/Identifier

## Steps to Import the API Gateway Files
For each file, do the following:
* Open file and do a find/replace for #ACCOUNT_NUMBER#, with your AWS account number
    * It's import than the ARN for your Lambda matches this file, if you publish your lambda without changing any settings/names the only thing you should have to replace is your AWS Account number, otherwise, replace this entire ARN with your ARN for the Lambda
* Click on Create API
* Ensure REST is selected
* Select Import from Swagger or Open API 3
* Click Select Swagger File
* Navigate to the API file you are importing

## Now Deploy your API
Now that you've imported each API, you need to deploy each one.
For each API:
* Click ono the API, then Resources tab (left side)
* Click on the / (or any other part) of the API Resources
* Right click and select Deploy API, select existing Beta stage (or create new one called Beta)
* Now test your API!