using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Octopus;

namespace OctoConfig.Core.Commands
{
	public class ClearTenantCommand
	{
		private readonly ClearTenantArgs _args;
		private readonly TenantClearer _tenantClearer;

		public ClearTenantCommand(ClearTenantArgs args, TenantClearer tenantClearer)
		{
			_args = args ?? throw new System.ArgumentNullException(nameof(args));
			_tenantClearer = tenantClearer ?? throw new System.ArgumentNullException(nameof(tenantClearer));
		}

		public async Task Execute()
		{
			await _tenantClearer.ClearTenantVariables().ConfigureAwait(false);
		}
	}
}
