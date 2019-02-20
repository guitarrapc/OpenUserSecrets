# OpenUserSecrets

Adding shortcut on the right click context menu of Project in Solution Explorer.

This Extension will offer access a secret like ASP.NET Core MVC Project has.

> [Safe storage of app secrets in development in ASP.NET Core - access-a-secret
](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2#access-a-secret)


## Motivation

ASP.NET Core project has builtin support for "Manage UserSecrets" when installed "Microsoft.Extensions.Configuration.UserSecrets", however it never appear when installed on .NET Core Console Project.

This Extensions manage UserSecret on .NET Core Console/Generic Host.

## What extension manage

1. If PropertyItem "UserSecretsId" entry is missing in .csproj, add entry with random GUID.
1. If "secret.json" file is missing, create file.
1. Open "secret.json" on the Visual Studio.

Current Status and secret path will output in Visual Studio OutputWindow.

## Q&A

### Why Microsoft.Extensions.Configuration.UserSecrets package not found message shown on output window?
* for .NETCore Console Project, install Microsoft.Extensions.Configuration.UserSecrets to the project.
    * Use PackageReference NuGet management style.
