using System;
using FluentAssertions;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Secrets.Vault;
using Xunit;

namespace OctoConfig.Tests
{
	public static class VaultClientFactoryTests
	{
		public class ConstructorTests
		{
			[Theory]
			[InlineData("")]
			[InlineData(null)]
			public void NullOrEmptyRoleIdThrows(string roleId)
			{
				var args = new FileArgsBase() { VaultRoleId = roleId };
				Action test = () => new VaultClientFactory(args);
				test.Should().Throw<ArgumentException>();
			}

			[Theory]
			[InlineData("")]
			[InlineData(null)]
			public void NullOrEmptySecretIdThrows(string secretId)
			{
				var args = new FileArgsBase() { VaultSecretId = secretId, VaultRoleId = "a" };
				Action test = () => new VaultClientFactory(args);
				test.Should().Throw<ArgumentException>();
			}

			[Theory]
			[InlineData("")]
			[InlineData(null)]
			public void NullOrEmptyUriThrows(string uri)
			{
				var args = new FileArgsBase() { VaultUri = uri, VaultRoleId = "a", VaultSecretId = "b" };
				Action test = () => new VaultClientFactory(args);
				test.Should().Throw<ArgumentException>();
			}
		}
	}
}
