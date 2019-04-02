using System.Threading.Tasks;

namespace OctoConfig.Core.Secrets
{
	public interface ISecretProvider
	{
		/// <summary>
		/// Gets a secret from backing secret store, where the secret data is formatted like the following:
		/// <param name="path">The URL path the secret is at</param>
		/// <returns>The value of the secret</returns>
		Task<string> GetSecret(string path);
	}
}
