using System.Collections.Generic;
using CommandLine;

namespace OctoConfig.Core.Arguments
{
	[Verb("upload-library", HelpText = "Removes ALL the variables in the specified library variable set")]
	public class LibraryTargetArgs : FileArgsBase
	{
		[Option('l', "library", Required = true, HelpText = "The Octopus Library to upload variables to")]
		public string Library { get; set; }

		[Option('r', "roles", Required = false, HelpText = "The Octopus role(s) to scope variables to")]
		public IEnumerable<string> OctoRoles { get; set; }
	}
}
