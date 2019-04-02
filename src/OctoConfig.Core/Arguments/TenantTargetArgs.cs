using CommandLine;

namespace OctoConfig.Core.Arguments
{
	[Verb("upload-project", HelpText = "Convert json file to environment variables and upload them to Octopus")]
	public class TenantTargetArgs : FileArgsBase
	{
		[Option('t', "tenant", Required = true, HelpText = "The Octopus tenant to attach variables to")]
		public string TenantName { get; set; }

		[Option('p', "project", Required = true, HelpText = "The Octopus project to match tenant variables with")]
		public string ProjectName { get; set; }
	}
}
