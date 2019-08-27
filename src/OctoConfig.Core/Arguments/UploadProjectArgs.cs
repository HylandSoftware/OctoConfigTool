using CommandLine;

namespace OctoConfig.Core.Arguments
{
	[Verb("upload-project-template", HelpText = "Convert json file to environment variables and upload them to Octopus")]
	public sealed class UploadProjectArgs : ProjectTargetArgs
	{
		[Option('c', "clear", Required = false, Default = false, HelpText = "Clears any project variables in the project before uploading new template")]
		public bool ClearProject { get; set; }
	}
}
