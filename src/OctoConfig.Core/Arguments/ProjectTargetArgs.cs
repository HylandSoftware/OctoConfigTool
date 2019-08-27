using CommandLine;

namespace OctoConfig.Core.Arguments
{
	public class ProjectTargetArgs : FileArgsBase, IProjectArgsBase
	{
		[Option('p', "project", Required = true, HelpText = "The Octopus project to match variables with")]
		public string ProjectName { get; set; }
	}
}
