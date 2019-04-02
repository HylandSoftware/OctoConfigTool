using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using OctoConfig.Core.Octopus;
using OctoConfig.Core.Secrets;

namespace OctoConfig.Core.Commands
{
	public class UploadLibraryCommand
	{
		private readonly LibraryTargetArgs _args;
		private readonly ISecretsMananger _secretsMananger;
		private readonly LibraryManager _libraryManager;
		private readonly VariableConverter _varConverter;

		public UploadLibraryCommand(LibraryTargetArgs args, ISecretsMananger secretsMananger, LibraryManager libraryMananger, VariableConverter variableConverter)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_secretsMananger = secretsMananger ?? throw new ArgumentNullException(nameof(secretsMananger));
			_libraryManager = libraryMananger ?? throw new ArgumentNullException(nameof(libraryMananger));
			_varConverter = variableConverter ?? throw new ArgumentNullException(nameof(variableConverter));
		}

		public async Task Execute()
		{
			var vars = _varConverter.Convert(File.ReadAllText(_args.File));

			await _secretsMananger.ReplaceSecrets(vars).ConfigureAwait(false);
			await _libraryManager.UpdateVars(vars, _args.Library, _args.Environments, _args.OctoRoles, apply: true).ConfigureAwait(false);

			var secretCount = vars.Count(s => s.IsSecret);
			var pub = vars.Count - secretCount;
			Console.WriteLine($"Found a total of {vars.Count} variables.");
			Console.WriteLine($"{secretCount} were secrets and {pub} were not");
		}
	}
}
