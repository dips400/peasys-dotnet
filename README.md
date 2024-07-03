# Peasys dotnet

[NuGet](https://www.nuget.org/packages/Peasys)

The official [Peasys](https://dips400.com/library) .NET library, supporting .NET Standard 8.0+

## Installation

Using the [.NET Core command-line interface (CLI) tools](https://learn.microsoft.com/en-us/dotnet/core/tools/):

```sh
dotnet add package Peasys
```

Using the [NuGet Command Line Interface (CLI)](https://learn.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference?tabs=windows).

```sh
nuget install Peasys
```

Using the [Package Manager Console](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell):

```powershell
Install-Package Peasys
```

From within Visual Studio:

1. Open the Solution Explorer.
2. Right-click on a project within your solution.
3. Click on *Manage NuGet Packages...*
4. Click on the *Browse* tab and search for "Peasys".
5. Click on the Peasys package, select the appropriate version in the
   right-tab and click *Install*.

## Documentation

For a comprehensive list of examples, check out the [documentation](https://dips400.com/docs).

## Usage

### License key

Peasys is a tool used along a license that should be found on the [dips400](https://dips400.com) website. This license key is required for the use of the service Peasys.

### Connexion to the server

``` C#
PeaClient conn = new PeaClient("DNS OR IP", PORT, "USERNAME", "PASSWORD", "ID_CLIENT", onlineVersion: true, retrieveStatistics: false);
Console.WriteLine("Status de connexion : " + conn.ConnectionMessage);
```

### Query the DB2

For example, use the `ExecuteCreate` method of the `PeaClient` class in order to create a a new table in the database.

``` C#
PeaCreateResponse createResponse = conn.ExecuteCreate("CREATE TABLE schema_name/table_name (name CHAR(10), age INT)");
Console.WriteLine(createResponse.ReturnedSQLMessage);
Console.WriteLine(createResponse.ReturnedSQLState);
```

### Deconnexion

It is important to always disconnect from the server after you used the connexion.

``` C#
conn.Disconnect();
```

## Support

New features and bug fixes are released on the latest major version of the Peasys .NET client library. If you are on an older major version, we recommend that you upgrade to the latest in order to use the new features and bug fixes including those for security vulnerabilities. Older major versions of the package will continue to be available for use, but will not be receiving any updates.

The library uses [`dotnet-format`](https://github.com/dotnet/format) for code formatting. Code
must be formatted before PRs are submitted. Run the
formatter with:

```sh
dotnet format Peasys.sln
```

For any requests, bug or comments, please open an issue or submit a
pull request.
