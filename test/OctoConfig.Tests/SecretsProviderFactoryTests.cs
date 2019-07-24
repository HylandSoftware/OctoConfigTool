using System;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using OctoConfig.Core.Secrets;
using OctoConfig.Tests.TestFixture;
using Xunit;

namespace OctoConfig.Tests
{
	public static class SecretsProviderFactoryTests
	{
		public class Constructor
		{
			[Theory, AppAutoData]
			public void ContsructorGuardClauses(IFixture fixture)
			{
				var assertion = new GuardClauseAssertion(fixture);
				assertion.Verify(typeof(SecretProviderFactory).GetConstructors());
			}
		}

		public class GetService
		{
			[Theory, InlineAppAutoData("")]
			public void CreatingSecretProviderUsesServiceProvider(string i, [Frozen] Mock<IServiceProvider> mock, SecretProviderFactory sut)
			{
				sut.Create(i);
				mock.Verify(s => s.GetService(It.IsAny<Type>()), Times.Once);
			}

			[Theory, InlineAppAutoData("tihnspith")]
			public void UnsupportedProviderCodeThrows(string i, SecretProviderFactory sut)
			{
				Action act = () => sut.Create(i);
				act.Should().Throw<NotSupportedException>();
			}

			[Theory, InlineAppAutoData("")]
			public void NoPrefixCreatesV1VaultProvider(string i, [Frozen] Mock<IServiceProvider> mock, SecretProviderFactory sut)
			{
				sut.Create(i);
				mock.Verify(s => s.GetService(It.Is<Type>(t => t == typeof(VaultProvider))), Times.Once);
			}

			[Theory, InlineAppAutoData("VaultKVV2")]
			public void VaultKVV2CreatesV2VaultProvider(string i, [Frozen] Mock<IServiceProvider> mock, SecretProviderFactory sut)
			{
				sut.Create(i);
				mock.Verify(s => s.GetService(It.Is<Type>(t => t == typeof(VaultKVV2Provider))), Times.Once);
			}

			[Theory, InlineAppAutoData("VaultKVV1")]
			public void VaultKVV1CreatesV1VaultProvider(string i, [Frozen] Mock<IServiceProvider> mock, SecretProviderFactory sut)
			{
				sut.Create(i);
				mock.Verify(s => s.GetService(It.Is<Type>(t => t == typeof(VaultProvider))), Times.Once);
			}
		}
	}
}
