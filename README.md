
# Introduction
  
This document describes how to use programme to transforme a XML query into a SQL query.

## Requirements

Install the following IDE:
- [Visual Studio](https://visualstudio.microsoft.com/fr/)

## Steps for Getting Started
- Clone the repository, run the following command :
```bash
git clone https://github.com/Ilhem23/DSLQuery.git
```
- Open project in Visual Studio
- Build the project
- publish the project in local directory

## :arrow_forward: How to Run Programme
- Open a terminal
- Go to a publish directory
- Excecute the test command:
```bash
DSLQuery.exe "pathToXMLTest\file.xml"
```
- Example of command:
 ```bash
DSLQuery.exe "C:\DSLQuery\xml_samples\Step1_FieldSelection.xml"
```
or run the following command to get the help 
 ```bash
DSLQuery.exe help
```
## :no_entry_sign: Note

- you will find the sample XML files you provided in the test document in xml_samples folder : 
![Screenshot](xmlExample.png)

- I found some XML errors in The DSL Query example in all steps the fields node is not closed:
![Screenshot](error.png)


---

author: [Aissaoui Ilhem]
keywords: [DSL, Query, XML, c#, Domain Specific Language]
category: Technical test
...
