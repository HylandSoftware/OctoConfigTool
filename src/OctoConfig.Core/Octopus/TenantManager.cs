using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.DependencySetup;
using Octopus.Client;
using Octopus.Client.Model;

namespace OctoConfig.Core.Octopus
{
	public interface ITenantManager
	{
		Task CreateTenantVariables(List<SecretVariable> vars, bool apply = true);
	}

	public class TenantManager : ITenantManager
	{
		private readonly TenantTargetArgs _args;
		private readonly IOctopusAsyncRepository _octopusRepository;
		private readonly ILogger _logger;

		public TenantManager(TenantTargetArgs args, IOctopusAsyncRepository octopusRepository, ILogger logger)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_octopusRepository = octopusRepository ?? throw new ArgumentNullException(nameof(octopusRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task CreateTenantVariables(List<SecretVariable> vars, bool apply = true)
		{
			var project = await _octopusRepository.ValidateProject(_args.ProjectName).ConfigureAwait(false);
			var tenant = await _octopusRepository.ValidateTenant(_args.TenantName).ConfigureAwait(false);
			var tenantVars = await _octopusRepository.Tenants.GetVariables(tenant).ConfigureAwait(false);
			if(!tenantVars.ProjectVariables.ContainsKey(project.Id))
			{
				throw new ArgumentException($"Tenant {tenant.Name} is not linked with project {project.Name}");
			}
			var tenentResourceProject = tenantVars.ProjectVariables[project.Id];
			_logger.Information($"{tenentResourceProject.ProjectName} has {tenentResourceProject.Templates.Count} variable templates");

			foreach (var environmentName in _args.Environments)
			{
				var environment = await _octopusRepository.ValidateEnvironment(environmentName).ConfigureAwait(false);
				if(!tenentResourceProject.Variables.ContainsKey(environment.Id))
				{
					throw new ArgumentException($"Tenant {tenant.Name} is not linked with environment {environmentName}");
				}
				if(!apply)
				{
					// don't try and apply tennant vars if not applying them because the project might not have the variables
					break;
				}
				var enviroScopedVars = tenentResourceProject.Variables[environment.Id];
				var before = enviroScopedVars.Count;
				foreach (var variable in vars)
				{
					var template = tenentResourceProject.Templates.SingleOrDefault(tp => tp.Name.Equals(variable.Name, StringComparison.InvariantCultureIgnoreCase));
					if(template == null)
					{
						throw new ArgumentException($"The loaded configuration for tenant '{_args.TenantName}' has variable '{variable.Name}' not found in the project '{_args.ProjectName}'");
					}
					enviroScopedVars[template.Id] = new PropertyValueResource(variable.Value, isSensitive: variable.IsSecret || template.IsSensitive());
				}
				var after = enviroScopedVars.Count;
				_logger.Information($"Created {after - before} variable templates for tenant {_args.TenantName} scoped to environment {environmentName}");
			}
			if(apply)
			{
				await _octopusRepository.Tenants.ModifyVariables(tenant, tenantVars).ConfigureAwait(false);
			}
		}
	}
}
