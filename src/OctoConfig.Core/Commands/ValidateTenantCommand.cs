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
	public class ValidateTenantCommand
	{
		private readonly ValidateTenantArgs _args;
		private readonly ISecretsMananger _secretsMananger;
		private readonly ITenantManager _tenantManager;
		private readonly VariableConverter _jsonValidator;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger _logger;

		public ValidateTenantCommand(ValidateTenantArgs args, ISecretsMananger secretsMananger, ITenantManager tenantManager,
			VariableConverter jsonValidator, IFileSystem fileSystem, ILogger logger)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_secretsMananger = secretsMananger ?? throw new ArgumentNullException(nameof(secretsMananger));
			_tenantManager = tenantManager ?? throw new ArgumentNullException(nameof(tenantManager));
			_jsonValidator = jsonValidator ?? throw new ArgumentNullException(nameof(jsonValidator));
			_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Execute()
		{
			var vars = _jsonValidator.Convert(_fileSystem.File.ReadAllText(_args.File));

			await _secretsMananger.ReplaceSecrets(vars, _args).ConfigureAwait(false);
			await _tenantManager.CreateTenantVariables(vars, apply: false).ConfigureAwait(false);

			var secretCount = vars.Count(s => s.IsSecret);
			var pub = vars.Count - secretCount;
			_logger.Information($"Found a total of {vars.Count} variables.");
			_logger.Information($"{secretCount} were secrets and {pub} were not");
		}
	}
}
