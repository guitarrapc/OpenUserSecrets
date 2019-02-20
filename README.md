# OpenUserSecrets

## Install

You have 2 choice.

1. Visual Studio > Tools > Extensions and Updates > search `Open UserSecrets`.
1. Go to https://marketplace.visualstudio.com/items?itemName=guitarrapc.OpenUserSecrets and download > install.

## How it works

Visual Studio Extensions to manage UserSecret, generate and open UserSecret json.

The Open User Secret makes it easy to manage UserSecrets and work with your .NET Core project.

Right click ib the Project in Solution Explorer. You will find "Open UserSecret" in the Context menu and UserSecret's secret.json will be open in Visual Studio.

![](docs/usage_contextmenu.png)

![](docs/usage_opensecretjson.png)

For more information about the extension, visit https://github.com/guitarrapc/OpenUserSecrets

## What extension manage

1. If PropertyItem "UserSecretsId" entry is missing in .csproj, add entry with random GUID.
1. If "secret.json" file is missing, create file.
1. Open "secret.json" on the Visual Studio.

> Current Status and secret path will output in Visual Studio OutputWindow.

![](docs/usage_outputwindow.png)

## Motivation

ASP.NET Core project has builtin support for "Manage UserSecrets" when installed "Microsoft.Extensions.Configuration.UserSecrets", however it never appear when installed on .NET Core Console Project.

This Extension will offer access UserSecret to .NET Core Console/Generic project, like ASP.NET Core MVC Project has.

> [Safe storage of app secrets in development in ASP.NET Core - access-a-secret
](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2#access-a-secret)

## Q&A

### Why Microsoft.Extensions.Configuration.UserSecrets package not found message shown on output window?

* for .NETCore Console Project, install Microsoft.Extensions.Configuration.UserSecrets to the project.
    * Use PackageReference NuGet management style.
