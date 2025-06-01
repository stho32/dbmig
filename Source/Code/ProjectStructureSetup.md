# Project Setup Documentation

This document outlines the steps taken to create the dbmig project structure.

## Project Structure Creation

```powershell
# Create solution file
dotnet new sln
Rename-Item -Path .\Code.sln -NewName dbmig.sln

# Create project directories
mkdir dbmig.BL
mkdir dbmig.BL.Tests
mkdir dbmig.BL.IntegrationTests
mkdir dbmig.Console

# Create Class Library project for Business Logic
cd .\dbmig.BL\
dotnet new classlib
cd ..

# Create Class Library project for Integration Tests
cd .\dbmig.BL.IntegrationTests\
dotnet new classlib
cd ..

# Create Class Library project for Unit Tests
cd .\dbmig.BL.Tests\
dotnet new classlib
cd ..

# Create Console Application
cd .\dbmig.Console\
dotnet new console
cd ..

# Add projects to solution
dotnet sln add .\dbmig.BL\
dotnet sln add .\dbmig.BL.Tests\
dotnet sln add .\dbmig.BL.IntegrationTests\
dotnet sln add .\dbmig.Console\
```

## Adding NuGet Packages and Project References

```powershell
# Add NUnit packages to the test projects
cd .\dbmig.BL.Tests\
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk
cd ..

cd .\dbmig.BL.IntegrationTests\
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk
cd ..

# Add project references
dotnet add .\dbmig.Console\dbmig.Console.csproj reference .\dbmig.BL\dbmig.BL.csproj
dotnet add .\dbmig.BL.Tests\dbmig.BL.Tests.csproj reference .\dbmig.BL\dbmig.BL.csproj
dotnet add .\dbmig.BL.IntegrationTests\dbmig.BL.IntegrationTests.csproj reference .\dbmig.BL\dbmig.BL.csproj

# Create gitignore for .NET projects
dotnet new gitignore
```
