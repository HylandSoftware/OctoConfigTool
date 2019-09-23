using System.Collections.Generic;
using CommandLine;

namespace OctoConfig.Core.Arguments
{
	public class ArgsBase
	{
		[Option('a', "apikey", Required = true, HelpText = "The Octopus API key to use")]
		public string ApiKey { get; set; }

		[Option('o', "octotUri", Required = true, HelpText = "The Octopus API URI")]
		public string OctoUri { get; set; }

		[Option('v', "verbose", Required = false, Default = false, HelpText = "Max verbosity")]
		public bool Verbose { get; set; }

		[Option('m', "merge", Required = false, Default = false, HelpText = "Forces json arrays to be merged into one variable, rather than generated with idicies")]
		public bool MergeArrays { get; set; }
	}

	public interface IProjectArgsBase
	{
		string ProjectName { get; set; }
	}

	public class ProjectArgsBase : ArgsBase, IProjectArgsBase
	{
		[Option('p', "project", Required = true, HelpText = "The Octopus project to match variables with")]
		public string ProjectName { get; set; }
	}

	public interface IVaultArgs
	{
		string VaultUri { get; set; }

		string VaultRoleId { get; set; }

		string VaultSecretId { get; set; }

		string MountPoint { get; set; }
	}

	public class FileArgsBase : ArgsBase, IVaultArgs
	{
		[Option('f', "file", Required = true, HelpText = "The json file to parse into variables")]
		public string File { get; set; }

		[Option('e', "environments", Required = false, Min = 1, HelpText = "The Octopus Environment to scope variables to")]
		public IEnumerable<string> Environments { get; set; }

		[Option(longName: "vaultUri", HelpText = "The Vault API URI", Required = false)]
		public string VaultUri { get; set; }

		[Option(longName: "vaultRole", HelpText = "The Role ID the app will run as", Required = false)]
		public string VaultRoleId { get; set; }

		[Option(longName: "secret", HelpText = "The Vault Secret ID associated with the Vault Role", Required = false)]
		public string VaultSecretId { get; set; }

		[Option(longName: "mountPoint", Default = "", HelpText = "The base mount point for the Vault secrets engine", Required = false)]
		public string MountPoint { get; set; }

		[Option(longName: "variableType", HelpText = "The type of Octopus variables to convert the json file into", Required = true)]
		public VariableType VariableType { get; set; }

		[Option(longName: "prefix", Required = false, HelpText = "A Prefix to prepend to variables", Default = "")]
		public string Prefix { get; set; }
	}
}
