# OpenUserSecrets

Adding shortcut on the right click context menu of Project in Solution Explorer.

This Extension will offer access a secret like ASP.NET Core MVC Project has.

> [Safe storage of app secrets in development in ASP.NET Core - access-a-secret
](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2#access-a-secret)


## Motivation

ASP.NET Core project has builtin support for "Manage UserSecrets" when installed "Microsoft.Extensions.Configuration.UserSecrets", however it never appear when installed on .NET Core Console Project.

This Extensions manage UserSecret on .NET Core Console/Generic Host.

## What can do

1. Add "UserSecretsId" PropertyItem entry to the .csproj
1. Create "secret.json" file.
1. Open "secret.json" directly on the Visual Studio.

Status will output to Output Window.

## Q&A

### Why Microsoft.Extensions.Configuration.UserSecrets package not found message shown on output window?
* for .NETCore Console Project, install Microsoft.Extensions.Configuration.UserSecrets to the project.
    * Use PackageReference NuGet management style.
