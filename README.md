# Payment Bank API

##Functionality

### Public

Payment Bank API v1.0 supports a list of operations:
* returns a list of all customer accounts
* returns a list of all accounts for a particular customer
* returns an extended customer information
* creates a new account for a customer


### Private
Payment Bank API v1.0 for internal purposes supports a list of operations:
- returns a list of all transactions for all accounts
- creates a new transaction
- returns a list of all transactions for a specified account 

## CI/CD pipeline
### Continious integration
Azure Devops has been chosen for CI purposes. kivlev/PaymentBank project contains a single pipeline being triggered for any commit to any branch.
The pipeline leverages Microsoft hosted agents to build, test, publish build artifacts and deploy activities.
Visual studio task for Azure Devops is used to build and pack both Account and Transaction services, and to build unit test project.
Unit tests are run by dotnet core task with output format trx. Code coverage is defined and collected in a cobertura format. *.trx file and *.cobertura.xml file are published as the pipeline artifacts.

Both of them are visible in any of pipline execution panels:
<img src="doc/images/PipelineSummary.JPG" width="90%"></img>

The pipeline test panel gives an update regarding success rate of unit tests:
<img src="doc/images/PipelineTest.JPG" width="90%"></img>

There is an extensive report for unit tests coverage in on the Code Coverage blade. All the application classes are available for the detailed inspection.
<img src="doc/images/PipelineCoverage.JPG" width="90%"></img>
<img src="doc/images/PipelineCoverageDetails.JPG" width="90%"></img>

### Continious deployment
There are two deploy step defined at the end of the pipeline, which are triggered only for master branch. Every merge of Pull Request leads to a deployment of Account and Transaction services
to the corresponding Web Applications in Azure Dev Environment (Test/Acceptance/Production environments are not defined and not deployed yet for a sake of cost savings). Disclaimer: Web Applications are configured to use
Free Plan which limits their availability.

Some test text
<img src="doc/images/AccountServiceSwagger.JPG" width="90%"></img>

Another test text
<img src="doc/images/TransactionServiceSwagger.JPG" width="90%"></img>