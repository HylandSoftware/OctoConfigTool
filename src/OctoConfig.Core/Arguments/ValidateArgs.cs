using CommandLine;

namespace OctoConfig.Core.Arguments
{
	[Verb("validate-json", HelpText = "Validates that the json is valid and any referenced secrets exist")]
	public sealed class ValidateArgs : LibraryTargetArgs { }

	[Verb("validate-tenant", HelpText = "Validates that the json is valid and any referenced secrets, environments, or projects, exist")]
	public sealed class ValidateTenantArgs : TenantTargetArgs { }
}
