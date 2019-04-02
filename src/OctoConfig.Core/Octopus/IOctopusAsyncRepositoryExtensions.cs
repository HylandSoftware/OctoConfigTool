using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octopus.Client;
using Octopus.Client.Model;

namespace OctoConfig.Core.Octopus
{
	public static class IOctopusAsyncRepositoryExtensions
	{
		public static async Task ValidateRoles(this IOctopusAsyncRepository octopusRepository, IEnumerable<string> roles)
		{
			var serverRolesList = await octopusRepository.MachineRoles.GetAllRoleNames().ConfigureAwait(false);
			var serverRoles = new HashSet<string>(serverRolesList);
			foreach(var role in roles)
			{
				if (!serverRoles.Contains(role))
				{
					throw new ArgumentException($"Unable to find a machine role with the name '{role}'");
				}
			}
		}

		public static async Task<EnvironmentResource> ValidateEnvironment(this IOctopusAsyncRepository octopusRepository, string environmentName)
		{
			var enviro = await octopusRepository.Environments.FindByName(environmentName).ConfigureAwait(false);
			return enviro ?? throw new ArgumentException($"Unable to find an environment with the name '{environmentName}'");
		}

		public static async Task<LibraryVariableSetResource> ValidateLibrary(this IOctopusAsyncRepository octopusRepository, string libraryName)
		{
			var lib = await octopusRepository.LibraryVariableSets.FindByName(libraryName).ConfigureAwait(false);
			return lib ?? throw new ArgumentException($"Unable to find a library with the name '{libraryName}'");
		}

		public static async Task<ProjectResource> ValidateProject(this IOctopusAsyncRepository octopusRepository, string projectName)
		{
			var project = await octopusRepository.Projects.FindByName(projectName).ConfigureAwait(false);
			return project ?? throw new ArgumentException($"Unable to find a project with the name '{projectName}'");
		}

		public static async Task<TenantResource> ValidateTenant(this IOctopusAsyncRepository octopusRepository, string tenantName)
		{
			var tenant = await octopusRepository.Tenants.FindByName(tenantName).ConfigureAwait(false);
			return tenant ?? throw new ArgumentException($"Unable to find a tenant with the name '{tenantName}'");
		}
	}
}
