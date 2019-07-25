using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using OctoConfig.Core.Secrets;
using OctoConfig.Core.Secrets.Vault;
using OctoConfig.Tests.TestFixture;
using VaultSharp;
using VaultSharp.V1;
using VaultSharp.V1.Commons;
using VaultSharp.V1.SecretsEngines;
using VaultSharp.V1.SecretsEngines.KeyValue;
using VaultSharp.V1.SecretsEngines.KeyValue.V2;
using Xunit;

namespace OctoConfig.Tests
{
	public static class VaultKV2ProviderTests
	{
		public class GetSecret
		{
			[Theory, InlineAppAutoData]
			public async Task ExceptionIsRethrown([Frozen] Mock<IVaultClientFactory> mockFact,
				string path, VaultKVV2Provider sut)
			{
				mockFact.Setup(m => m.GetClient()).Returns(() => throw new VaultSharp.Core.VaultApiException("exceptions are dumb"));

				Func<Task<string>> s = () => sut.GetSecret(path);
				await s.Should().ThrowExactlyAsync<VaultSharp.Core.VaultApiException>().ConfigureAwait(false);
			}

			[Theory, InlineAppAutoData]
			public async Task ExceptionWithWarnAndErrorIsRethrown([Frozen] Mock<IVaultClientFactory> mockFact,
				string path, VaultKVV2Provider sut)
			{
				var messageDict = new Dictionary<string, IEnumerable<string>>() { { "warnings", new List<string>() { "warning" } }, { "errors", new List<string>() { "error" } } };
				  mockFact.Setup(m => m.GetClient()).Returns(() => throw new VaultSharp.Core.VaultApiException(HttpStatusCode.OK, JsonConvert.SerializeObject(messageDict)));

				Func<Task<string>> s = () => sut.GetSecret(path);
				await s.Should().ThrowExactlyAsync<VaultSharp.Core.VaultApiException>().ConfigureAwait(false);
			}

			[Theory, InlineAppAutoData]
			public async Task PullsSecretUsingCorrectValue([Frozen] Mock<IVaultClientFactory> mockFact, Mock<IVaultClientV1> mockV1,
				Mock<ISecretsEngine> mockSecrets, Mock<IKeyValueSecretsEngine> mockKV, Mock<IVaultClient> mockClient,
				string path, VaultKVV2Provider sut)
			{
				var mockKVV2 = new Mock<IKeyValueSecretsEngineV2>();
				mockKVV2.Setup(m => m.ReadSecretAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>()))
					.Returns<string, int?, string, string>((_, __, ___, ____) => Task.FromResult(new Secret<SecretData>() { Data = new SecretData() { Data = new Dictionary<string, object>() { { "value", "secret" } } } }));
				mockKVV2.CallBase = false;
				mockKV.Setup(m => m.V2).Returns(() => mockKVV2.Object);
				mockSecrets.Setup(m => m.KeyValue).Returns(() => mockKV.Object);
				mockV1.Setup(m => m.Secrets).Returns(() => mockSecrets.Object);
				mockClient.Setup(m => m.V1).Returns(() => mockV1.Object);
				mockFact.Setup(m => m.GetClient()).Returns(() => mockClient.Object);

				var actual = await sut.GetSecret(path).ConfigureAwait(false);
				actual.Should().Be("secret");
			}
		}
	}
}
