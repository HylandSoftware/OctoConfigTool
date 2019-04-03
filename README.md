# Octopus Config Tool

[![Build Status](https://travis-ci.org/HylandSoftware/OctoConfigTool.svg?branch=master)](https://travis-ci.org/HylandSoftware/OctoConfigTool)

A tool designed to convert json configuration files into a format usable by Octopus Deploy and upload them to Octopus.
It supports several secrets providers and pulls secrets based on values in the flat configuration files.
Works with tenanted and non-tenanted deployment schemes.

## Usage

The nuget package `OctoConfigTool` and Docker image [`hylandsoftware/octoconfigtool`](https://hub.docker.com/u/hylandsoftware) both use the tool compiled to a .Net Core executable dll called `OctoConfigTool.dll`. All of the command line examples in this document must be prefaced with `dotnet OctoConfigTool.dll` to execute the tool, the .Net runtime handles passing the settings after the dll into the tools `Main` function for parsing.

A full example of calling the tool from a shell looks like this:

```bash
dotnet OctoConfigTool.dll upload-library -f "C:\QA\appsettings.json" --library "API Config" -e QA \
-r api-role -a "API-KEY" -o https://octodeploy/api \
--merge --vaultUri "http://vaultapi:8200" \
--vaultRole "VAULT_PROVIDED_ROLE_GUID" --secret "VAULT_PROVIDED_ROLE_SECRET_GUID" \
--variableType JsonConversion
```

It is also published as a [dotnet tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) to [NuGet](https://nuget.org). So you can install it locally with `dotnet tool install -g OctoConfigTool`.
Once it's installed simply typing `OctoConfigTool` into any shell will execute the tool. Turning the above into this:

```bash
OctoConfigTool upload-library -f "C:\QA\appsettings.json" --library "API Config" -e QA \
-r api-role -a "API-KEY" -o https://octodeploy/api \
--merge --vaultUri "http://vaultapi:8200" \
--vaultRole "VAULT_PROVIDED_ROLE_GUID" --secret "VAULT_PROVIDED_ROLE_SECRET_GUID" \
--variableType JsonConversion
```

### Shared Variables

#### Required for ALL commands

- `-a`, `--apikey`
  - The Octopus API key to use
- `-o`, `--octotUri`
  - The Octopus API URI to upload to

#### Optional for all commands

- `-m`, `--merge`
  - Forces arrays in json file to be merged into one variable, rather than generated with indices
  - Not required
  - Defaults to `false`
- `-v`, `--verbose`
  - Max verbosity
  - Not Required

#### Required for Upload commands

- `-f`, `--file`
  - The json file to parse into variables
- `-r`, `--roles`
  - The Octopus role(s) to scope variables to
  - Separated by a space
  - Applied to all variables
  - e.g. `-r api api-role web-role`
  - At least one required
- `--variableType`
  - The type of Octopus variables to convert the json file into
  - Options are `Environment`, `JsonConversion`, `EnvironmentGlob`
  - `EnvironmentGlob` makes a comma-separated list of variables, and uploads them as a secret using the variable name `ConcatEnvironmentVars`

#### Optional for Upload commands

- `-e`, `--environments`
  - The Octopus Environment(s) to scope variables to
  - Separated by a space
  - Applied to all variables
  - e.g. `-e RC-Green RC-Blue`
- `--vaultUri`
  - The Vault API URI
- `--vaultRole`
  - The Role ID the app will run as
- `--secret`
  - The Vault Secret ID associated with the Vault Role
- `--mountPoint`
  - The Vault mount point for the secrets engine
  - Uses the default Vault mount if not present
- `-p`. `--prefix`
  - A Prefix to prepend to variables
  - Generally only used for environment variables

**Note**: If the json file has any secrets and one of the parameters for Vault is not present, the tool will fail.

### Targeting a Library

Targeting a library has additional arguments

- `-l`, `--library`
  - The Octopus Library to upload variables to
- `r`, `--roles`
  - A list of Octopus roles to scope variables to
  - Not required

#### Uploading as Json replacement variables

```bash
upload-library -f "C:\QA\appsettings.json" --library "API Config" -e QA \
-r api-role -a "API-KEY" -o https://octodeploy/api \
--merge --vaultUri "http://vaultapi:8200" \
--vaultRole "VAULT_PROVIDED_ROLE_GUID" --secret "VAULT_PROVIDED_ROLE_SECRET_GUID" \
--variableType JsonConversion
```

#### Uploading as Environment Variables

```bash
upload-library -f "C:\QA\appsettings.json" -l "API Config" -e QA \
-r api-role -a "API-KEY" -o https://octodeploy/api \
--prefix "API_" --vaultUri "http://vaultapi:8200" \
--vaultRole "VAULT_PROVIDED_ROLE_GUID" --secret "VAULT_PROVIDED_ROLE_SECRET_GUID" \
--variableType JsonConversion --roles api-role web-role
```

#### Uploading as Concatenated Environment Variables

This option creates all the environment variables as normal, but concatenates them into a comma-separated list. This list is then uploaded into one variable that is always marked `secret`
This variable name is hard-coded in the tool as `ConcatEnvironmentVars`.

This command is designed for deploying to Helm since it can be challenging/impossible to get a combined list of environment variables in an Octo deployment exported into the release. This makes it easier since you only need to reference once variable in Octopus to get all of your configuration.

Example:

```bash
upload-library -f "C:\QA\appsettings.json" -l "Test-Var-Set" -e QA \
-r api-role -a "API-KEY" -o https://octodeploy/api \
-p "API_" --vaultUri "http://vaultapi:8200" \
--vaultRole "VAULT_PROVIDED_ROLE_GUID" --secret "VAULT_PROVIDED_ROLE_SECRET_GUID" \
--variableType EnvironmentGlob
```

### Clearing a Library Variable Set

This option deletes **ALL** the variables in the specified set, leaving it existing, but empty.

You need to give a value for the `-f` flag but it is ignored.

Example:

```bash
clear-library -l "Test-Var-Set" -a "API-KEY" -o https://octodeploy/api -f ""
```

### Targeting a Tenant

This command takes the variables from the json file and creates project variable templates in the specified project.
It then takes those created/existing templates and matches with the variable values and uploads them to the specified Tenant.
Tenant variables do not support scoping to Octopus roles so that option is not provided here.
You can scope Tenant variables to an Environment, and the tool will match them correctly, but if the Tenant is not linked to a specified environment for the specified project then it will fail.
Variables that are not secret are created with a default value of `PLACEHOLDER_VALUE`.
Variables that are secret are given no default value.

Targeting a Tenant has two additional arguments

- `-t`, `--tenant`
  - The Octopus tenant to attach variables to
- `-p`, `--project`
  - The Octopus project to match Tenant variables with

Example:

```bash
upload-project --tenant "QA Infra" --project "Deploy API" --variableType Environment \
-a "API-KEY" -p "API_" --vaultUri "http://vaultapi:8200" \
--vaultRole "VAULT_PROVIDED_ROLE_GUID" --secret "VAULT_PROVIDED_ROLE_SECRET_GUID" \
-f "C:\QA\appsettings.json" -e QA RC
```

### Clearing a Project of Variable Templates

Deletes **ALL** the variables templates in the specified project

```bash
clear-project --project "Deploy API" -o https://octodeploy/api -a "API-KEY"
```

### Clearing a Tenants Variables

Deletes **ALL** the variables in the specified tenant

```bash
clear-tenant --tenant "PreProduction" -o https://octodeploy/api -a "API-KEY"
```

## Secrets

Secrets are identified by starting them with `#{` and ending with `}`. In between them is an identifier for the secret and secret provider. The format of this identifier can vary based on the [secret provider](#secret-providers).
For example, the config file could contain the following `#{RC/RedisConnectionString}`.
This tells the tool to use Vault and then identifier is a URI path to the secret, so it calls out to this URL `https://vaultapi:8200/v1/secret/RC/RedisConnectionString` for the secret.

Secrets are uploaded to Octopus as `Sensitive` so they are still stored securely and cannot be read.

The tool does not write or update secrets to any of the supported providers.

### Secret Providers

The tool supports specifying what secret provider a secret is in. All providers use this format for specification:

`#{<ProviderId>:<SecretIdentifier>}`

So a Vault V1 secret would look like this:

`#{VaultKVV1:QA/API/ConnectionString}`

If no provider is specified then it will default to using Vault Key-Value V1.

#### Vault Key/Value V1

[Vault KV V2](https://www.vaultproject.io/docs/secrets/kv/kv-v1.html) engine.
The provider ID is `VaultKVV1`

So a Vault V1 secret would look like this:

`#{VaultKVV1:QA/API/ConnectionString}` or `#{QA/API/ConnectionString}`

The secret itself is expected to be the first and only thing at that path. And the json version should look like the following:

```json
{
  "value":"SECRET_HERE",
}
```

These are accessed by the tool as a dictionary that just grabs the first key/value pair and only uses the value.
Other secrets at the location will be ignored.

#### Vault Key/Value V2

[Vault KV V2](https://www.vaultproject.io/docs/secrets/kv/kv-v2.html) engine.

The provider ID is `VaultKVV2`

So a Vault V1 secret would look like this:

`#{VaultKVV2:QA/API/ConnectionString}`

The secret itself is expected to be the first and only thing at that path. And the json version should look like the following:

```json
"data" : {
    "value":"SECRET_HERE",
}
```

These are accessed by the tool as a dictionary that just grabs the first key/value pair and only uses the value.
Other secrets at the location will be ignored.

## Cake

There are also [Cake](https://cakebuild.net/) bindings for the tool that use the same parameters as the Docker image.

### Cake Library Targets

```csharp
#addin "nuget:?package=OctoLib.Core&version=0.3.1"
#addin "nuget:?package=Octopus.Client&version=5.2.6"
#addin "nuget:?package=VaultSharp&version=0.11.0"
#addin "nuget:?package=Microsoft.Extensions.Primitives&version=2.0.0"
#addin "nuget:?package=Microsoft.Extensions.DependencyInjection.Abstractions&version=2.2.0"
#addin "nuget:?package=Microsoft.Extensions.DependencyInjection&version=2.2.0"
using OctoConfig.Core;

void UploadJson(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string library, string filePath, string prefix)
{
    Information($"Uploading {filePath}");
    UploadLibrarySet(new LibraryTargetArgs(){
        File = filePath,
        Library = library,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri,
        Environments = enviros,
        OctoRoles =  roles,
        VaultUri = vaultUri,
        VaultRoleId = vaultRoleId,
        VaultSecretId = vaultSecretId,
        Prefix = prefix,
        VariableType = VariableType.JsonConversion
    });
}

void UploadEnviro(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string library, string filePath, string prefix)
{
    Information($"Uploading {filePath}");
    UploadLibrarySet(new LibraryTargetArgs(){
        File = filePath,
        Library = library,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri,
        Environments = enviros,
        VaultUri = vaultUri,
        VaultRoleId = vaultRoleId,
        VaultSecretId = vaultSecretId,
        Prefix = prefix,
        VariableType = VariableType.Environment
    });
}

void ValidateLibraryConfig(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string library, string filePath)
{
    Information($"Validating {filePath}");
    ValidateConfig(new ValidateArgs(){
        File = filePath,
        Library = library,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri,
        Environments = enviros,
        OctoRoles =  roles,
        VaultUri = vaultUri,
        VaultRoleId = vaultRoleId,
        VaultSecretId = vaultSecretId,
    });
}

void ClearLibrary(string octoApiUri, string octoApiKey, string library)
{
    Information($"Validating {filePath}");
    ClearLibrarySet(new ClearVariableSetArgs(){
        Library = library,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri
    });
}
```

### Cake Tenant Targets

```csharp
void UploadTenantJson(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string tenant, string project, string filePath, string prefix)
{
    Information($"Uploading {filePath}");
    UploadTenant(new TenantTargetArgs(){
        File = filePath,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri,
        TenantName = tenant,
        ProjectName = project,
        Environments = enviros,
        OctoRoles =  roles,
        VaultUri = vaultUri,
        VaultRoleId = vaultRoleId,
        VaultSecretId = vaultSecretId,
        VariableType = VariableType.JsonConversion
    });
}

void UploadTenantEnviro(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string tenant, string project, string filePath, string prefix)
{
    Information($"Uploading {filePath}");
    UploadTenant(new TenantTargetArgs(){
        File = filePath,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri,
        TenantName = tenant,
        ProjectName = project,
        Environments = enviros,
        OctoRoles =  roles,
        VaultUri = vaultUri,
        VaultRoleId = vaultRoleId,
        VaultSecretId = vaultSecretId,
        VariableType = VariableType.Environment
    });
}

void ValidateTenantConfig(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string tenant, string project, string filePath)
{
    Information($"Validating {filePath}");
    ValidateTenantConfig(new TenantTargetArgs(){
        File = filePath,
        TenantName = tenant,
        ProjectName = project,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri,
        Environments = enviros,
        VaultUri = vaultUri,
        VaultRoleId = vaultRoleId,
        VaultSecretId = vaultSecretId,
        VariableType = VariableType.Environment
    });
}

void ClearTenantConfig(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string tenant, string project, string filePath)
{
    Information($"Validating {filePath}");
    ClearTenantConfig(new TenantTargetArgs(){ ... });
}

void ClearProjectConfig(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string tenant, string project, string filePath)
{
    Information($"Validating {filePath}");
    ClearProjectConfig(new TenantTargetArgs(){ ... });
}
```

### Deprecated Cake Targets

```csharp
#addin "nuget:?package=OctoLib.Core&version=0.3.1"
#addin "nuget:?package=Octopus.Client&version=5.2.6"
#addin "nuget:?package=VaultSharp&version=0.11.0"
#addin "nuget:?package=Microsoft.Extensions.Primitives&version=2.0.0"
#addin "nuget:?package=Microsoft.Extensions.DependencyInjection.Abstractions&version=2.2.0"
#addin "nuget:?package=Microsoft.Extensions.DependencyInjection&version=2.2.0"
using OctoConfig.Core;

void UploadEnviro(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string library, string filePath, string prefix)
{
    Information($"Uploading {filePath}");
    UploadEnvironmentVariables(new EnvironmentVarArgs(){
        File = filePath,
        Library = library,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri,
        Environments = enviros,
        OctoRoles =  roles,
        VaultUri = vaultUri,
        VaultRoleId = vaultRoleId,
        VaultSecretId = vaultSecretId,
        Prefix = prefix
    });
}

void UploadJson(string octoApiUri, string octoApiKey, string vaultUri, string vaultRoleId, string vaultSecretId,
    List<string> enviros, List<string> roles, string library, string filePath)
{
    Information($"Uploading {filePath}");
    UploadJson(new JsonReplacementArgs(){
        File = filePath,
        Library = library,
        ApiKey = octoApiKey,
        OctoUri = octoApiUri,
        Environments = enviros,
        OctoRoles =  roles,
        VaultUri = vaultUri,
        VaultRoleId = vaultRoleId,
        VaultSecretId = vaultSecretId,
    });
}
```
