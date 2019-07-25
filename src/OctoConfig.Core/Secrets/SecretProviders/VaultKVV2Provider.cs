using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.DependencySetup;
using OctoConfig.Core.Secrets.Vault;
using VaultSharp;

namespace OctoConfig.Core.Secrets
{
	public class VaultKVV2Provider : ISecretProvider
	{
		private readonly string _mountPoint = null;
		private readonly IVaultClientFactory _vaultClientFactory;
		private readonly ILogger _logger;

		public VaultKVV2Provider(FileArgsBase args, IVaultClientFactory vaultClientFactory, ILogger logger)
		{
			if (args == null)
			{
				throw new ArgumentNullException(nameof(args));
			}

			if (!String.IsNullOrEmpty(args.MountPoint))
			{
				_mountPoint = args.MountPoint;
			}
			_vaultClientFactory = vaultClientFactory ?? throw new ArgumentNullException(nameof(vaultClientFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Gets a secret from Vault, where the secret data is formatted like the following:
		/// {
		///		"data":<SECRET>"
		///	}
		/// </summary>
		/// <param name="path">The path in the /v2/secret section of Vault to pull from</param>
		/// <returns>The value of the secret</returns>
		public async Task<string> GetSecret(string path)
		{
			try
			{
				var kv2Secret = await _vaultClientFactory.GetClient().V1.Secrets.KeyValue.V2
								.ReadSecretAsync(path, mountPoint: _mountPoint ?? "secret")
								.ConfigureAwait(false);
				return kv2Secret.Data.Data["value"].ToString();
			}
			catch (VaultSharp.Core.VaultApiException ex)
			{
				var s = new StringBuilder();
				s.Append("Encountered a Vault API Exception on secret '").Append(path).AppendLine("'");
				s.Append("HTTP Error Code '").Append(ex.HttpStatusCode).AppendLine("'");
				s.AppendLine("API Errors: ");
				foreach (var item in ex?.ApiErrors ?? Enumerable.Empty<string>())
				{
					s.Append("\t").Append(item).AppendLine(" ");
				}
				s.AppendLine();
				s.AppendLine("API Warnings: ");
				foreach (var item in ex?.ApiWarnings ?? Enumerable.Empty<string>())
				{
					s.Append("\t").Append(item).AppendLine(" ");
				}
				s.AppendLine();
				s.Append("Exception:").AppendLine(ex.ToString());
				_logger.Error(s.ToString());
				throw;
			}
		}
	}
}
