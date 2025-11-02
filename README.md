# Model validation with data annotations

we will use DataAnnotations to apply model validation.  Both controllers have one endpoint that returns identical 
data from the same database. We will create validation rules using .NET DataAnnotations.


### Packages: 
* dotnet add package Bogus
* dotnet add package Dapper
* dotnet add package Microsoft.Data.Sqlite
* dotnet add package Microsoft.EntityFrameworkCore.Sqlite
* dotnet add package Scalar.AspNetCore
* dotnet add package FluentValidation.AspNetCore
* Build succeeded in 1.7s


### Folder Structure: 
Controllers
Data: contexts
Models
Repositories
Services

### program.cs check-list:
before building the app (builder.build()): 
CORS : origin, header, methods
Controllers.'
ORMs services. (Dapper, EF, ... )
IoCs.

### Features
see commits for more information:
1- Custome Validations
2- Complex validation rules with FluentValidation
