using Cake.Core;
using Cake.Core.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Commands;
using OctoConfig.Core.DependencySetup;

namespace OctoConfig.Cake
{
	[CakeAliasCategory("OctoConfig")]
	public static class CakeAliases
	{
		[CakeMethodAlias]
		public static void ValidateConfig(this ICakeContext context, ValidateArgs args)
		{
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<ValidateLibraryCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void UploadJson(this ICakeContext context, JsonReplacementArgs args)
		{
			args.VariableType = VariableType.JsonConversion;
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<UploadLibraryCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void UploadEnvironmentVariables(this ICakeContext context, EnvironmentVarArgs args)
		{
			args.VariableType = VariableType.Environment;
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<UploadLibraryCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void UploadEnvironmentVariablesGlob(this ICakeContext context, EnvironmentVarGlobArgs args)
		{
			args.VariableType = VariableType.EnvironmentGlob;
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<UploadLibraryCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void ClearLibrarySet(this ICakeContext context, ClearVariableSetArgs args)
		{
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<ClearVariableSetCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void UploadLibrarySet(this ICakeContext context, LibraryTargetArgs args)
		{
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<UploadLibraryCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void UploadTenant(this ICakeContext context, TenantTargetArgs args)
		{
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<UploadTenantCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void UploadProject(this ICakeContext context, UploadProjectArgs args)
		{
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<UploadProjectCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void ValidateTenantConfig(this ICakeContext context, ValidateTenantArgs args)
		{
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<ValidateTenantCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void ClearTenantConfig(this ICakeContext context, ClearTenantArgs args)
		{
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<ClearTenantCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}

		[CakeMethodAlias]
		public static void ClearProjectConfig(this ICakeContext context, ClearProjectArgs args)
		{
			DependencyConfig.Setup(args, context).GetAwaiter().GetResult();
			var cmd = DependencyConfig.Container.GetService<ClearProjectCommand>();
			cmd.Execute().GetAwaiter().GetResult();
		}
	}
}
