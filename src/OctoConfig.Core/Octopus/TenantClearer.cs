using System;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using Octopus.Client;

namespace OctoConfig.Core.Octopus
{
	public interface ITenantClearer
	{
		Task ClearTenantVariables();
	}

	public class TenantClearer : ITenantClearer
	{
		private readonly ClearTenantArgs _args;
		private readonly IOctopusAsyncRepository _octopusRepository;

		public TenantClearer(ClearTenantArgs args, IOctopusAsyncRepository octopusRepository)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_octopusRepository = octopusRepository ?? throw new ArgumentNullException(nameof(octopusRepository));
		}

		public async Task ClearTenantVariables()
		{
			var tenant = await _octopusRepository.ValidateTenant(_args.TenantName).ConfigureAwait(false);
			var tenantVars = await _octopusRepository.Tenants.GetVariables(tenant).ConfigureAwait(false);
			tenantVars.ProjectVariables.Clear();
			await _octopusRepository.Tenants.ModifyVariables(tenant, tenantVars).ConfigureAwait(false);
		}
	}
}
