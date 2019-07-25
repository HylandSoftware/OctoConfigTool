using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using OctoConfig.Core.DependencySetup;
using OctoConfig.Core.Octopus;
using OctoConfig.Core.Secrets;

namespace OctoConfig.Core.Commands
{
	public class UploadLibraryCommand
	{
		private readonly LibraryTargetArgs _args;
		private readonly ISecretsMananger _secretsMananger;
		private readonly ILibraryManager _libraryManager;
		private readonly VariableConverter _varConverter;
		private readonly ILogger _logger;

		public UploadLibraryCommand(LibraryTargetArgs args, ISecretsMananger secretsMananger, ILibraryManager libraryMananger, VariableConverter variableConverter, ILogger logger)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_secretsMananger = secretsMananger ?? throw new ArgumentNullException(nameof(secretsMananger));
			_libraryManager = libraryMananger ?? throw new ArgumentNullException(nameof(libraryMananger));
			_varConverter = variableConverter ?? throw new ArgumentNullException(nameof(variableConverter));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Execute()
		{
			var vars = _varConverter.Convert(File.ReadAllText(_args.File));

			await _secretsMananger.ReplaceSecrets(vars).ConfigureAwait(false);
			await _libraryManager.UpdateVars(vars, _args.Library, _args.Environments, _args.OctoRoles, apply: true).ConfigureAwait(false);

			var secretCount = vars.Count(s => s.IsSecret);
			var pub = vars.Count - secretCount;
			_logger.Information($"Found a total of {vars.Count} variables.");
			_logger.Information($"{secretCount} were secrets and {pub} were not");
		}
	}
}
