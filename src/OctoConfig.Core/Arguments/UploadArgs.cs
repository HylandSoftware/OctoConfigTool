using CommandLine;

namespace OctoConfig.Core.Arguments
{
	[Verb("upload-project", HelpText = "Upload them to an Octopus project as templates")]
	public sealed class UploadProjectArgs : ProjectTargetArgs
	{
		[Option('c', "clear", Required = false, Default = false, HelpText = "Clears any project variables in the project before uploading new template")]
		public bool ClearProject { get; set; }
	}

	[Verb("upload-tenant", HelpText = "Convert json file to environment variables and upload them to Octopus tenant")]
	public sealed class UploadTenantArgs : TenantTargetArgs
	{
		[Option(longName: "skip-upload-project", Required = false, Default = false, HelpText = "Does not update project template when uploading tenant variables")]
		public bool SkipUploadProject { get; set; }
	}
}
