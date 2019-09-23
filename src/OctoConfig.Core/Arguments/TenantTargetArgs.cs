using CommandLine;

namespace OctoConfig.Core.Arguments
{
	public class TenantTargetArgs : ProjectTargetArgs
	{
		[Option('t', "tenant", Required = true, HelpText = "The Octopus tenant to attach variables to")]
		public string TenantName { get; set; }
	}
}
