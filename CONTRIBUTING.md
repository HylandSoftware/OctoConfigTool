# Header

## Adding Secret Providers

* New secret providers should go in the `/src/Secrets/SecretProviders` folder and be in the `OctoConfig.Core.Secrets` namespace. Additionally they must implement `ISecretProvider` for a common provider interface
* Add an entry in `SecretProviderFactory` for your new provider and give it a unique name related to the provider type
* Register the new provider in `DependencyConfig`
* Make sure the necessary configuration options the provider needs are added to `ArgsBase`
* Failing to resolve a secret should bomb out the tool, but put out as much useful information as possible before crashing

## Adding Commands

* Create a new args POCO that extends `ArgsBase` and tag it with a `CommandLine.Verb` attribute
* And config that is only used by this command goes in this class
  * Shared config goes in `ArgsBase`
* Create a new command in the `src/Commands` folder
  * Register it in `DependencyConfig`
* Add the new args to the parser in `Program.Main` for Docker
* Add a Cake binding in `CakeAliases` and make sure it's tagged with the `CakeMethodAlias` attribute
