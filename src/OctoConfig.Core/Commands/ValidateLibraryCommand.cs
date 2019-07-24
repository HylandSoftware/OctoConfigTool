using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using OctoConfig.Core.Octopus;
using OctoConfig.Core.Secrets;

namespace OctoConfig.Core.Commands
{
	public class ValidateLibraryCommand
	{
		private readonly ValidateArgs _args;
		private readonly ISecretsMananger _secretsMananger;
		private readonly ILibraryManager _libraryManager;
		private readonly VariableConverter _jsonValidator;
		private readonly IFileSystem _fileSystem;

		public ValidateLibraryCommand(ValidateArgs args, ISecretsMananger secretsMananger, ILibraryManager libraryManager,
			VariableConverter jsonValidator, IFileSystem fileSystem)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_secretsMananger = secretsMananger ?? throw new ArgumentNullException(nameof(secretsMananger));
			_libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
			_jsonValidator = jsonValidator ?? throw new ArgumentNullException(nameof(jsonValidator));
			_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		}

		public async Task Execute()
		{
			var vars = _jsonValidator.Convert(_fileSystem.File.ReadAllText(_args.File));

			await _secretsMananger.ReplaceSecrets(vars).ConfigureAwait(false);
			await _libraryManager.UpdateVars(vars, _args.Library, _args.Environments, _args.OctoRoles, apply: false).ConfigureAwait(false);

			var secretCount = vars.Count(s => s.IsSecret);
			var pub = vars.Count - secretCount;
			Console.WriteLine($"Found a total of {vars.Count} variables.");
			Console.WriteLine($"{secretCount} were secrets and {pub} were not");
		}
	}
}
