# AppCenterBuilder
This console application was created to remotely build an AppCenter application and get build reports.
It will:
* Receive list of branches for the app and build them
* Print the following information to console output
  * < branch name > build < completed/failed > in < number > seconds. Link to build logs: < link >

## Technologies
* .NET Core (C#)
* NuGet Packages used:
  * CommandLineParser
  * Newtonsoft.Json
  * System.Configuration.ConfigurationManager

## Usage
Application will need input parameters either command line or via app.config:
* BaseUrl (Default value = https://api.appcenter.ms/)
* AppName
* OwnerName
* ApiKeyName (Default value = X-API-Token)
* Token
* Debug
  * Build mode = True or False
* Timeout (Default value 600)
  * Timeout for build run (in seconds)
* Sleep (Default value 20)
  * Sleep for build finishing check (in seconds)

![image](https://user-images.githubusercontent.com/61601755/172160768-9e7f60a0-40ae-4fe4-88f1-b68b5ff3a48b.png)

## Example result
![image](https://user-images.githubusercontent.com/61601755/172160734-37550d59-3794-4011-9e3c-c5d595f48346.png)

## Methods
Some methods can be used independantly - they are described in the IBuildReporter interface:
* RunBuildAsync
* ReportBuildResultAsync
* IsBuildFinishedAsync
* GetBranchesAsync
