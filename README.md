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
* dotnet add package AutoMapper
* dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
* dotnet add package Microsoft.AspNetCore.JsonPatch
* dotnet add package Microsoft.AspNetCore.Mvc.NewtonSoftJson

### Certificattion (to test locally)
* dotnet dev-certs https --trust


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

### Validations
see commits for more information:
* Custome Validations
* Complex validation rules with FluentValidation

### ORMs
* Dapper
* EntityFrameWork

### Security
* Rejecting HTTP requests with custom ProblemDetails middleware.
* Setting up HTTPS on a custom domain – creating a self-signed certificate with   PowerShell
* Setting up ASP.NET Core Identity
* Using cookie authentication in ASP.NET Core Web API
* JWT authentication with Identity
* Implementing policy-based and role-based authentication
