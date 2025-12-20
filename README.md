# Basic-API-Endpoints-With-Essential-Features

Developed simple API endpoints by utilizing essential ASP.NET framework features.

### vs code Extensions: 
* alexcvzz.vscode-sqlite

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
* dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
* dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.10
* dotnet tool install --global dotnet-ef
* dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0
* dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks --version 9.0.0
* dotnet add package Serilog.AspNetCore
* dotnet add package Serilog.Sinks.Seq

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
* Setting up HTTPS on a custom domain – creating a self-signed certificate with PowerShell
* Setting up ASP.NET Core Identity
* Using cookie authentication in ASP.NET Core Web API
* JWT authentication with Identity
* Implementing policy-based and role-based authentication


### Setting up HTTPS on a custom domain
* Run windows powershell ( administrator )
* $domainName = "dev.webapi-book.com"
* $cert = New-SelfSignedCertificate -DnsName $domainName -CertStoreLocation "cert:\LocalMachine\My" -NotAfter (Get-Date).AddYears(1)
* $pwd = ConvertTo-SecureString -String "ExamplePassword" -Force -AsPlaintext
* $certPath = ".\Certificates"
*  if (!(Test-Path $certPath)) {
 New-Item -ItemType Directory -Force -Path $certPath
 }
*  $path = Join-Path $certPath "dev-webapi-book-cert.pfx"
 Export-PfxCertificate -Cert $cert -FilePath $path -Password $pwd
* $rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store "Root","LocalMachine"
* $rootStore.Open("ReadWrite")
* $rootStore.Add($cert)
* $rootStore.Close()

### Adding Certification
.NET will automatically use the development certificate that you trusted with dotnet dev-certs https --trust.
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001"
      },
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}


* dotnet dev-certs https --check
* dotnet dev-certs https --trust
* you are good to go 

### EF migrations
    dotnet ef migrations add IdentitySchema
    dotnet ef database update
    dotnet run

Note: 
`Before using Identity for the first time, you must delete the SqlliteDB.db file and run 
 dotnet ef database update to create a new database. Otherwise, the Identity tables will not be created.`