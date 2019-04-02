using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Commands;
using OctoConfig.Core.Converter;
using OctoConfig.Core.Octopus;
using OctoConfig.Core.Secrets;
using Octopus.Client;

namespace OctoConfig.Core.DependencySetup
{
	public static class DependencyConfig
	{
		private static readonly IServiceCollection _coll = new ServiceCollection();
		public static IServiceProvider Container;

		public static async Task Setup(ArgsBase args)
		{
			if(String.IsNullOrEmpty(args.OctoUri))
			{
				throw new ArgumentException("Null or empty Octopus Deploy API URI", nameof(args.OctoUri));
			}
			if (String.IsNullOrEmpty(args.ApiKey))
			{
				throw new ArgumentException("Null or empty Octopus Deploy API key", nameof(args.ApiKey));
			}

			addArgs(args);

			var server = new OctopusServerEndpoint(args.OctoUri, args.ApiKey);
			var factory = new OctopusClientFactory();
			_coll.AddSingleton<IOctopusAsyncRepository>(new OctopusAsyncRepository(await factory.CreateAsyncClient(server).ConfigureAwait(false)));

			_coll.AddSingleton<ISecretProviderFactory, SecretProviderFactory>();
			_coll.AddSingleton<ISecretsMananger, SecretsMananger>();
			_coll.AddSingleton<VaultKVV2Provider>();
			_coll.AddSingleton<VaultProvider>();

			_coll.AddSingleton<LibraryManager>();
			_coll.AddSingleton<ProjectManager>();
			_coll.AddSingleton<ProjectClearer>();
			_coll.AddSingleton<TenantClearer>();
			_coll.AddSingleton<TenantManager>();
			_coll.AddSingleton<VariableConverter>();

			_coll.AddSingleton<ValidateLibraryCommand>();
			_coll.AddSingleton<UploadLibraryCommand>();
			_coll.AddSingleton<UploadTenantCommand>();
			_coll.AddSingleton<ClearVariableSetCommand>();
			_coll.AddSingleton<ClearTenantCommand>();
			_coll.AddSingleton<ClearProjectCommand>();
			_coll.AddSingleton<ValidateTenantCommand>();
			Container = _coll.BuildServiceProvider();
		}

		private static void addArgs(ArgsBase args)
		{
			_coll.AddSingleton(args);
			if(args is FileArgsBase fArgs)
			{
				_coll.AddSingleton(fArgs);
			}
			if(args is LibraryTargetArgs lArgs)
			{
				_coll.AddSingleton(lArgs);
			}
			switch (args)
			{
				case ClearVariableSetArgs cArgs:
					_coll.AddSingleton(cArgs);
					break;
				case ValidateArgs libArgs:
					_coll.AddSingleton(libArgs);
					break;
				case ClearProjectArgs cpArgs:
					_coll.AddSingleton(cpArgs);
					break;
				case ClearTenantArgs ctArgs:
					_coll.AddSingleton(ctArgs);
					break;
				case ValidateTenantArgs vtArgs:
					_coll.AddSingleton(vtArgs);
					_coll.AddSingleton<TenantTargetArgs>(vtArgs);
					break;
				case TenantTargetArgs pAgs:
					_coll.AddSingleton(pAgs);
					break;
				default:
					throw new ArgumentException($"Unknown argument type '{args.GetType()}'", nameof(args));
			}
		}
	}
}
