using System;
using FluentAssertions;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Secrets;
using Xunit;


namespace OctoConfig.Tests
{
	public static class VaultKV2ProviderTests
	{
		public class ConstructorTests
		{
			[Theory]
			[InlineData("")]
			[InlineData(null)]
			public void NullOrEmptyRoleIdThrows(string roleId)
			{
				var args = new FileArgsBase() { VaultRoleId = roleId };
				Action test = () => new VaultKVV2Provider(args);
				test.Should().Throw<ArgumentException>();
			}

			[Theory]
			[InlineData("")]
			[InlineData(null)]
			public void NullOrEmptySecretIdThrows(string secretId)
			{
				var args = new FileArgsBase() { VaultSecretId = secretId, VaultRoleId = "a" };
				Action test = () => new VaultKVV2Provider(args);
				test.Should().Throw<ArgumentException>();
			}

			[Theory]
			[InlineData("")]
			[InlineData(null)]
			public void NullOrEmptyUriThrows(string uri)
			{
				var args = new FileArgsBase() { VaultUri = uri, VaultRoleId = "a", VaultSecretId = "b" };
				Action test = () => new VaultKVV2Provider(args);
				test.Should().Throw<ArgumentException>();
			}
		}
	}
}
