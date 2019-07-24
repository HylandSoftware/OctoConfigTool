using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
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

		public ValidateTenantCommand(ValidateTenantArgs args, ISecretsMananger secretsMananger, ITenantManager tenantManager,
			VariableConverter jsonValidator, IFileSystem fileSystem)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_secretsMananger = secretsMananger ?? throw new ArgumentNullException(nameof(secretsMananger));
			_tenantManager = tenantManager ?? throw new ArgumentNullException(nameof(tenantManager));
			_jsonValidator = jsonValidator ?? throw new ArgumentNullException(nameof(jsonValidator));
			_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		}

		public async Task Execute()
		{
			var vars = _jsonValidator.Convert(_fileSystem.File.ReadAllText(_args.File));

			await _secretsMananger.ReplaceSecrets(vars).ConfigureAwait(false);
			await _tenantManager.CreateTenantVariables(vars, apply: false).ConfigureAwait(false);

			var secretCount = vars.Count(s => s.IsSecret);
			var pub = vars.Count - secretCount;
			Console.WriteLine($"Found a total of {vars.Count} variables.");
			Console.WriteLine($"{secretCount} were secrets and {pub} were not");
		}
	}
}
