using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using OctoConfig.Core.DependencySetup;
using OctoConfig.Core.Octopus;
using OctoConfig.Core.Secrets;

namespace OctoConfig.Core.Commands
{
	public class UploadProjectCommand
	{
		private readonly UploadProjectArgs _args;
		private readonly ISecretsMananger _secretsMananger;
		private readonly VariableConverter _varConverter;
		private readonly IProjectManager _projectManager;
		private readonly IProjectClearer _projectClearer;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger _logger;

		public UploadProjectCommand(UploadProjectArgs args, ISecretsMananger secretsMananger, IProjectManager projectManager,
			IProjectClearer projectClearer, VariableConverter variableConverter, IFileSystem fileSystem, ILogger logger)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_secretsMananger = secretsMananger ?? throw new ArgumentNullException(nameof(secretsMananger));
			_varConverter = variableConverter ?? throw new ArgumentNullException(nameof(variableConverter));
			_projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
			_projectClearer = projectClearer ?? throw new ArgumentNullException(nameof(projectClearer));
			_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Execute()
		{
			var vars = _varConverter.Convert(_fileSystem.File.ReadAllText(_args.File));
			if(_args.ClearProject)
			{
				await _projectClearer.ClearProjectVariables();
			}
			await _secretsMananger.ReplaceSecrets(vars).ConfigureAwait(false);
			await _projectManager.CreateProjectVariables(vars, true);

			var secretCount = vars.Count(s => s.IsSecret);
			var pub = vars.Count - secretCount;
			_logger.Information($"Found a total of {vars.Count} variables.");
			_logger.Information($"{secretCount} were secrets and {pub} were not");
		}
	}
}
