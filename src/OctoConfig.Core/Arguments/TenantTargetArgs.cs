using CommandLine;

namespace OctoConfig.Core.Arguments
{
	[Verb("upload-project", HelpText = "Convert json file to environment variables and upload them to Octopus")]
	public class TenantTargetArgs : ProjectTargetArgs
	{
		[Option('t', "tenant", Required = true, HelpText = "The Octopus tenant to attach variables to")]
		public string TenantName { get; set; }
	}
}
