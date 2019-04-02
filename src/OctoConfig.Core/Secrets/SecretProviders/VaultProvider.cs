using System;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;

namespace OctoConfig.Core.Secrets
{
	public class VaultProvider : ISecretProvider
	{
		private readonly IVaultClient _vaultClient;
		private readonly string _mountPoint = null;

		public VaultProvider(FileArgsBase args)
		{
			if (!String.IsNullOrEmpty(args.MountPoint))
			{
				_mountPoint = args.MountPoint;
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
			var vaultClientSettings = new VaultClientSettings(args.VaultUri, authMethod)
			{
				VaultServiceTimeout = TimeSpan.FromMinutes(5)
			};
			_vaultClient = new VaultClient(vaultClientSettings);
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
				var kv2Secret = await _vaultClient.V1.Secrets.KeyValue.V1
								.ReadSecretAsync(path, mountPoint: _mountPoint ?? "secret")
								.ConfigureAwait(false);
				return kv2Secret.Data["value"].ToString();
			}
			catch(VaultSharp.Core.VaultApiException ex)
			{
				Console.Error.WriteLine($"Encountered a Vault API Exception on secret '{path}'");
				Console.Error.WriteLine($"HTTP Error Code '{ex.HttpStatusCode}'");
				Console.Error.Write("API Errors: ");
				foreach(var item in ex.ApiErrors)
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
