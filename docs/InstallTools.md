

![image](https://user-images.githubusercontent.com/17789481/197238565-e3f895d0-6def-4d41-aba2-721d5432b1ef.png)

# Installing prerequisite tools

The following need to be installed

* **Azure CLI** - Azure CLI tooling
* **RegisterBot** - magic tool to automatically set up *all* of your bot settings.
* **Crazor.Templates** - dotnet new templates for creating projects/apps/views 

```cmd
winget install Microsoft.AzureCLI
az login
dotnet tool install -g registerbot
dotnet new install Crazor.Templates
```

