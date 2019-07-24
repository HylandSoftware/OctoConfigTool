using System;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Secrets.Vault;
using VaultSharp;

namespace OctoConfig.Core.Secrets
{
	public class VaultProvider : ISecretProvider
	{
		private readonly string _mountPoint = null;
		private readonly IVaultClientFactory _vaultClientFactory;

		public VaultProvider(FileArgsBase args, IVaultClientFactory vaultClientFactory)
		{
			if (args == null)
			{
				throw new ArgumentNullException(nameof(args));
			}
			if (!String.IsNullOrEmpty(args.MountPoint))
			{
				_mountPoint = args.MountPoint;
			}
			_vaultClientFactory = vaultClientFactory;
		}

		/// <summary>
		/// Gets a secret from Vault, where the secret data is formatted like the following:
		/// {
		///		"data":<SECRET>"
		///	}
		/// </summary>
		/// <param name="path">The path in the /v1/secret section of Vault to pull from</param>
		/// <returns>The value of the secret</returns>
		public async Task<string> GetSecret(string path)
		{
			try
			{
				var kv2Secret = await _vaultClientFactory.GetClient().V1.Secrets.KeyValue.V1
								.ReadSecretAsync(path, mountPoint: _mountPoint ?? "secret")
								.ConfigureAwait(false);
				return kv2Secret.Data["value"].ToString();
			}
			catch(VaultSharp.Core.VaultApiException ex)
			{
				Console.Error.WriteLine($"Encountered a Vault API Exception on secret '{path}'");
				Console.Error.WriteLine($"HTTP Error Code '{ex.HttpStatusCode}'");
				Console.Error.Write("API Errors: ");
				foreach(var item in ex.ApiErrors ?? Enumerable.Empty<string>())
				{
					Console.Error.Write(item + " ");
				}
				Console.Error.WriteLine();
				Console.Error.Write("API Warnings: ");
				foreach (var item in ex.ApiWarnings ?? Enumerable.Empty<string>())
				{
					Console.Error.Write(item + " ");
				}
				Console.Error.WriteLine();
				Console.Error.WriteLine("Exception:" + ex.ToString());
				throw;
			}
		}
	}
}
