using CommandLine;

namespace OctoConfig.Core.Arguments
{
	[Verb("upload-project-template", HelpText = "Upload them to an Octopus project as templates")]
	public sealed class UploadProjectArgs : ProjectTargetArgs
	{
		[Option('c', "clear", Required = false, Default = false, HelpText = "Clears any project variables in the project before uploading new template")]
		public bool ClearProject { get; set; }
	}
}
