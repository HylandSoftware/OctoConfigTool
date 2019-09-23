using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Commands;
using OctoConfig.Core.DependencySetup;

namespace OctopusConfigTool
{
	public static class Program
	{
		public static async Task<int> Main(string[] cmdArgs)
		{
			return await Parser.Default.ParseArguments<ValidateArgs, ClearVariableSetArgs, LibraryTargetArgs, ValidateTenantArgs, UploadTenantArgs, UploadProjectArgs, ClearProjectArgs, ClearTenantArgs>(cmdArgs)
				.MapResult<ValidateArgs, ClearVariableSetArgs, LibraryTargetArgs, ValidateTenantArgs, UploadTenantArgs, UploadProjectArgs, ClearProjectArgs, ClearTenantArgs, Task<int>>(
				async validateArgs =>
				{
					await DependencyConfig.Setup(validateArgs).ConfigureAwait(false);
					var cmd = DependencyConfig.Container.GetService<ValidateLibraryCommand>();
					await cmd.Execute().ConfigureAwait(false);
					return 0;
				},
				async clearArgs =>
				{
					await DependencyConfig.Setup(clearArgs).ConfigureAwait(false);
					var cmd = DependencyConfig.Container.GetService<ClearVariableSetCommand>();
					await cmd.Execute().ConfigureAwait(false);
					return 0;
				},
				async libArgs =>
				{
					await DependencyConfig.Setup(libArgs).ConfigureAwait(false);
					var cmd = DependencyConfig.Container.GetService<UploadLibraryCommand>();
					await cmd.Execute().ConfigureAwait(false);
					return 0;
				},
				async validateTeanantArgs =>
				{
					await DependencyConfig.Setup(validateTeanantArgs).ConfigureAwait(false);
					var cmd = DependencyConfig.Container.GetService<ValidateTenantCommand>();
					await cmd.Execute().ConfigureAwait(false);
					return 0;
				},
				async uploadTenantArgs =>
				{
					await DependencyConfig.Setup(uploadTenantArgs).ConfigureAwait(false);
					var cmd = DependencyConfig.Container.GetService<UploadTenantCommand>();
					await cmd.Execute().ConfigureAwait(false);
					return 0;
				},
				async uploadProjectArgs =>
				{
					await DependencyConfig.Setup(uploadProjectArgs).ConfigureAwait(false);
					var cmd = DependencyConfig.Container.GetService<UploadProjectCommand>();
					await cmd.Execute().ConfigureAwait(false);
					return 0;
				},
				async clearProjectArgs =>
				{
					await DependencyConfig.Setup(clearProjectArgs).ConfigureAwait(false);
					var cmd = DependencyConfig.Container.GetService<ClearProjectCommand>();
					await cmd.Execute().ConfigureAwait(false);
					return 0;
				},
				async clearTenantArgs =>
				{
					await DependencyConfig.Setup(clearTenantArgs).ConfigureAwait(false);
					var cmd = DependencyConfig.Container.GetService<ClearTenantCommand>();
					await cmd.Execute().ConfigureAwait(false);
					return 0;
				},
				_ => Task.FromResult(1)
			).ConfigureAwait(false);
		}
	}
}
