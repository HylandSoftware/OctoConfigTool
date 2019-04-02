using System;

namespace OctoConfig.Core.Secrets
{
	public interface ISecretProviderFactory
	{
		ISecretProvider Create(string forPrefix);
	}

	public class SecretProviderFactory : ISecretProviderFactory
	{
		private const string _vaultKVV2 = "VaultKVV2";
		private const string _vaultKVV1 = "VaultKVV1";
		internal const string Default = "";
		private readonly IServiceProvider _container;

		public SecretProviderFactory(IServiceProvider container)
		{
			_container = container ?? throw new ArgumentNullException(nameof(container));
		}

		public ISecretProvider Create(string forPrefix)
		{
			switch (forPrefix)
			{
				case _vaultKVV1:
				case Default:
					return _container.GetService(typeof(VaultProvider)) as ISecretProvider;
				case _vaultKVV2:
					return _container.GetService(typeof(VaultKVV2Provider)) as ISecretProvider;
				default:
					throw new NotSupportedException($"No supported secret provider for prefix '{forPrefix}'");
			}
		}
	}
}
