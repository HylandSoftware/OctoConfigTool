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
	public class UploadTenantCommand
	{
		private readonly TenantTargetArgs _args;
		private readonly ISecretsMananger _secretsMananger;
		private readonly VariableConverter _varConverter;
		private readonly IProjectManager _projectManager;
		private readonly ITenantManager _tenantManager;
		private readonly IFileSystem _fileSystem;

		public UploadTenantCommand(TenantTargetArgs args, ISecretsMananger secretsMananger, IProjectManager projectManager,
			ITenantManager tenantManager, VariableConverter variableConverter, IFileSystem fileSystem)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_secretsMananger = secretsMananger ?? throw new ArgumentNullException(nameof(secretsMananger));
			_varConverter = variableConverter ?? throw new ArgumentNullException(nameof(variableConverter));
			_projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
			_tenantManager = tenantManager ?? throw new ArgumentNullException(nameof(tenantManager));
			_fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
		}

		public async Task Execute()
		{
			var vars = _varConverter.Convert(_fileSystem.File.ReadAllText(_args.File));

			await _secretsMananger.ReplaceSecrets(vars).ConfigureAwait(false);
			await _projectManager.CreateProjectVariables(vars);
			await _tenantManager.CreateTenantVariables(vars);

			var secretCount = vars.Count(s => s.IsSecret);
			var pub = vars.Count - secretCount;
			Console.WriteLine($"Found a total of {vars.Count} variables.");
			Console.WriteLine($"{secretCount} were secrets and {pub} were not");
		}
	}
}
