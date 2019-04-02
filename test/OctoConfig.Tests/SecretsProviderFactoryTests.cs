using System;
using FluentAssertions;
using Moq;
using OctoConfig.Core.Secrets;
using Xunit;

namespace OctoConfig.Tests
{
	public class SecretsProviderFactoryTests
	{
		[Theory, InlineData("")]
		public void CreatingSecretProviderUsesServiceProvider(string i)
		{
			var mock = new Mock<IServiceProvider>();
			mock.Setup(s => s.GetService(It.IsAny<Type>())).Verifiable();

			var sut = new SecretProviderFactory(mock.Object);
			sut.Create(i);
			mock.Verify(s => s.GetService(It.IsAny<Type>()), Times.Once);
		}

		[Theory, InlineData("tihnspith")]
		public void UnsupportedProviderCodeThrows(string i)
		{
			var mock = Mock.Of<IServiceProvider>();

			var sut = new SecretProviderFactory(mock);
			Action act = () => sut.Create(i);
			act.Should().Throw<NotSupportedException>();
		}

		[Theory, InlineData("")]
		public void NoPrefixCreatesV1VaultProvider(string i)
		{
			var mock = new Mock<IServiceProvider>();
			mock.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(VaultProvider)))).Verifiable();

			var sut = new SecretProviderFactory(mock.Object);
			sut.Create(i);
			mock.Verify(s => s.GetService(It.Is<Type>(t => t == typeof(VaultProvider))), Times.Once);
		}

		[Theory, InlineData("VaultKVV2")]
		public void VaultKVV2CreatesV2VaultProvider(string i)
		{
			var mock = new Mock<IServiceProvider>();
			mock.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(VaultKVV2Provider)))).Verifiable();

			var sut = new SecretProviderFactory(mock.Object);
			sut.Create(i);
			mock.Verify(s => s.GetService(It.Is<Type>(t => t == typeof(VaultKVV2Provider))), Times.Once);
		}

		[Theory, InlineData("VaultKVV1")]
		public void VaultKVV1CreatesV1VaultProvider(string i)
		{
			var mock = new Mock<IServiceProvider>();
			mock.Setup(s => s.GetService(It.Is<Type>(t => t == typeof(VaultProvider)))).Verifiable();

			var sut = new SecretProviderFactory(mock.Object);
			sut.Create(i);
			mock.Verify(s => s.GetService(It.Is<Type>(t => t == typeof(VaultProvider))), Times.Once);
		}
	}
}
