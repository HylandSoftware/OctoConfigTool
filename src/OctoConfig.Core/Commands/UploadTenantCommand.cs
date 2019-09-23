using System;
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
	public class UploadTenantCommand
	{
		private readonly UploadTenantArgs _args;
		private readonly ISecretsMananger _secretsMananger;
		private readonly VariableConverter _varConverter;
		private readonly IProjectManager _projectManager;
		private readonly ITenantManager _tenantManager;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger _logger;

		public UploadTenantCommand(UploadTenantArgs args, ISecretsMananger secretsMananger, IProjectManager projectManager,
			ITenantManager tenantManager, VariableConverter variableConverter, IFileSystem fileSystem, ILogger logger)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_secretsMananger = secretsMananger ?? throw new ArgumentNullException(nameof(secretsMananger));
			_varConverter = variableConverter ?? throw new ArgumentNullException(nameof(variableConverter));
			_projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
			_tenantManager = tenantManager ?? throw new ArgumentNullException(nameof(tenantManager));
			_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Execute()
		{
			var vars = _varConverter.Convert(_fileSystem.File.ReadAllText(_args.File));
			await _secretsMananger.ReplaceSecrets(vars, _args).ConfigureAwait(false);
			if (!_args.SkipUploadProject)
			{
				await _projectManager.CreateProjectVariables(vars).ConfigureAwait(false);
			}
			await _tenantManager.CreateTenantVariables(vars).ConfigureAwait(false);

			var secretCount = vars.Count(s => s.IsSecret);
			var pub = vars.Count - secretCount;
			_logger.Information($"Found a total of {vars.Count} variables.");
			_logger.Information($"{secretCount} were secrets and {pub} were not");
		}
	}
}
