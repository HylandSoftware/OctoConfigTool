using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Cake.Core;
using Microsoft.Extensions.DependencyInjection;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Commands;
using OctoConfig.Core.Converter;
using OctoConfig.Core.Octopus;
using OctoConfig.Core.Secrets;
using OctoConfig.Core.Secrets.Vault;
using Octopus.Client;
using Serilog;

namespace OctoConfig.Core.DependencySetup
{
	public static class DependencyConfig
	{
		public static IServiceProvider Container;

		public static async Task Setup(ArgsBase args, ICakeContext context = null, IServiceCollection _coll = null, bool connectToOctopus = true)
		{
			var coll = _coll ?? new ServiceCollection();
			if (String.IsNullOrEmpty(args.OctoUri))
			{
				throw new ArgumentException("Null or empty Octopus Deploy API URI", nameof(args.OctoUri));
			}
			if(String.IsNullOrEmpty(args.ApiKey))
			{
				throw new ArgumentException("Null or empty Octopus Deploy API key", nameof(args.ApiKey));
			}

			addArgs(args, coll);

			if(context != null)
			{
				coll.AddSingleton<ILogger>(new CakeLoggerAbstraction(context.Log, null));
			}
			else
			{
				var serilogLogger = new LoggerConfiguration()
					.WriteTo.Console();
				if(args.Verbose)
				{
					serilogLogger.MinimumLevel.Verbose();
				}
				else
				{
					serilogLogger.MinimumLevel.Information();
				}
				coll.AddSingleton<ILogger>(new CakeLoggerAbstraction(null, serilogLogger.CreateLogger()));
			}

			if(connectToOctopus)
			{
				var server = new OctopusServerEndpoint(args.OctoUri, args.ApiKey);
				var factory = new OctopusClientFactory();
				coll.AddSingleton<IOctopusAsyncRepository>(new OctopusAsyncRepository(await factory.CreateAsyncClient(server).ConfigureAwait(false)));
			}

			coll.AddSingleton<IFileSystem, FileSystem>();

			coll.AddSingleton<IVaultClientFactory, VaultClientFactory>();
			coll.AddSingleton<ISecretProviderFactory, SecretProviderFactory>();
			coll.AddSingleton<ISecretsMananger, SecretsMananger>();
			coll.AddSingleton<VaultKVV2Provider>();
			coll.AddSingleton<VaultProvider>();

			coll.AddSingleton<ILibraryManager, LibraryManager>();
			coll.AddSingleton<IProjectManager, ProjectManager>();
			coll.AddSingleton<IProjectClearer, ProjectClearer>();
			coll.AddSingleton<ITenantClearer, TenantClearer>();
			coll.AddSingleton<ITenantManager, TenantManager>();
			coll.AddSingleton<VariableConverter>();

			coll.AddSingleton<ValidateLibraryCommand>();
			coll.AddSingleton<UploadLibraryCommand>();
			coll.AddSingleton<UploadTenantCommand>();
			coll.AddSingleton<ClearVariableSetCommand>();
			coll.AddSingleton<ClearTenantCommand>();
			coll.AddSingleton<ClearProjectCommand>();
			coll.AddSingleton<ValidateTenantCommand>();
			coll.AddSingleton<UploadProjectCommand>();
			Container = coll.BuildServiceProvider();
		}

		private static void addArgs(ArgsBase args, IServiceCollection coll)
		{
			coll.AddSingleton(args);
			if(args is FileArgsBase fArgs)
			{
				coll.AddSingleton(fArgs);
			}
			if(args is LibraryTargetArgs lArgs)
			{
				coll.AddSingleton(lArgs);
			}
			if (args is ProjectTargetArgs ptArgs)
			{
				coll.AddSingleton(ptArgs);
				coll.AddSingleton<IProjectArgsBase>(new ProjectArgsBase { ProjectName = ptArgs.ProjectName });
			}
			if (args is ProjectArgsBase pbArgs)
			{
				coll.AddSingleton(pbArgs);
			}
			switch (args)
			{
				case ClearVariableSetArgs cArgs:
					coll.AddSingleton(cArgs);
					break;
				case ValidateArgs libArgs:
					coll.AddSingleton(libArgs);
					break;
				case ClearProjectArgs cpArgs:
					coll.AddSingleton(cpArgs);
					break;
				case ClearTenantArgs ctArgs:
					coll.AddSingleton(ctArgs);
					break;
				case ValidateTenantArgs vtArgs:
					coll.AddSingleton(vtArgs);
					coll.AddSingleton<TenantTargetArgs>(vtArgs);
					break;
				case UploadProjectArgs upArgs:
					coll.AddSingleton(upArgs);
					break;
				case TenantTargetArgs pAgs:
					coll.AddSingleton(pAgs);
					break;
				default:
					throw new ArgumentException($"Unknown argument type '{args.GetType()}'", nameof(args));
			}
		}
	}
}
