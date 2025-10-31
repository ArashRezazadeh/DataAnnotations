# Model validation with data annotations

we will use DataAnnotations to apply model validation. The starter project for this 
chapter is a Web API with two controllers. Both controllers have one endpoint that returns identical 
data from the same database. We will create validation rules using .NET DataAnnotations.


### Packages: 
dotnet add package Bogus
dotnet add package Dapper
dotnet add package Microsoft.Data.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Scalar.AspNetCore

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

--------------------------------------------
