using CommandLine;

namespace OctoConfig.Core.Arguments
{
	[Verb("clear-library", HelpText = "Removes ALL the variables in the specified library variable set")]
	public class ClearVariableSetArgs : LibraryTargetArgs
	{
	}

	[Verb("clear-project", HelpText = "Removes ALL the variable templates in the specified project")]
	public class ClearProjectArgs : ProjectArgsBase { }

	[Verb("clear-tenant", HelpText = "Removes ALL the variables in the specified tenant")]
	public class ClearTenantArgs : ArgsBase
	{
		[Option('t', "tenant", Required = true, HelpText = "The Octopus role(s) to scope variables to")]
		public string TenantName { get; set; }
	}
}
