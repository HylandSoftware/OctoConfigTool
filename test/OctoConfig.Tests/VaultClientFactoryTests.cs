using System;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Secrets.Vault;
using OctoConfig.Tests.TestFixture;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
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

			[Fact]
			public void NullArgThrows()
			{
				Action test = () => new VaultClientFactory(null);
				test.Should().Throw<ArgumentException>();
			}

			[Theory, AppAutoData]
			public void GoodArgsDoesNotThrow(FileArgsBase args)
			{
				// should not throw
				new VaultClientFactory(args);
			}
		}

		public class GetClient
		{
			[Theory, AppAutoData]
			public void GoodArgsDoesNotThrow(IFixture fixture)
			{
				var args = fixture.Build<FileArgsBase>()
					.With(f => f.VaultUri, "http://localhost/")
					.Create();
				var sut = new VaultClientFactory(args);
				IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(args.VaultRoleId, secretId: args.VaultSecretId);
				var expectedSettings = new VaultClientSettings(args.VaultUri, authMethod)
				{
					VaultServiceTimeout = TimeSpan.FromMinutes(5)
				};
				var client = sut.GetClient();
				client.Settings.Should().BeEquivalentTo(expectedSettings);
			}
		}
	}
}
