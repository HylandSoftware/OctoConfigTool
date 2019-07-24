using System;
using OctoConfig.Core.Arguments;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;

namespace OctoConfig.Core.Secrets.Vault
{
	public interface IVaultClientFactory
	{
		IVaultClient GetClient();
	}

	public class VaultClientFactory : IVaultClientFactory
	{
		private readonly VaultClientSettings _vaultClientSettings;

		public VaultClientFactory(FileArgsBase args)
		{
			if (args == null)
			{
				throw new ArgumentNullException(nameof(args));
			}
			if (String.IsNullOrEmpty(args.VaultRoleId))
			{
				throw new ArgumentException("Vault Rold Id missing", nameof(args.VaultRoleId));
			}
			if (String.IsNullOrEmpty(args.VaultSecretId))
			{
				throw new ArgumentException("Vault Secret Id missing", nameof(args.VaultSecretId));
			}
			if (String.IsNullOrEmpty(args.VaultUri))
			{
				throw new ArgumentException("Vault VaultUri missing", nameof(args.VaultUri));
			}
			IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(args.VaultRoleId, secretId: args.VaultSecretId);
			_vaultClientSettings = new VaultClientSettings(args.VaultUri, authMethod)
			{
				VaultServiceTimeout = TimeSpan.FromMinutes(5)
			};
		}

		public IVaultClient GetClient()
		{
			return new VaultClient(_vaultClientSettings);
		}
	}
}
