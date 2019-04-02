using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OctoConfig.Core;
using OctoConfig.Core.Secrets;
using Xunit;

namespace OctoConfig.Tests
{
	public class SecretsManagerTests
	{
		[Fact]
		public async Task NoSecretsDoesNotUseFactory()
		{
			var mockFact = new Mock<ISecretProviderFactory>();
			var mockProv = new Mock<ISecretProvider>();
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();

			var mock = new Mock<ISecretProviderFactory>();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();
			var s = new SecretsMananger(mock.Object);
			await s.ReplaceSecrets(new List<SecretVariable>() { new SecretVariable("hello", "world") }).ConfigureAwait(false);
			mockProv.Verify(v => v.GetSecret(It.IsAny<string>()), Times.Never);
		}

		[Fact]
		public async Task SecretDoesUseFactory()
		{
			var mockFact = new Mock<ISecretProviderFactory>();
			var mockProv = new Mock<ISecretProvider>();
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult(String.Empty));

			var s = new SecretsMananger(mockFact.Object);
			await s.ReplaceSecrets(new List<SecretVariable>() { new SecretVariable("hello", "#{world}") }).ConfigureAwait(false);
			mockProv.Verify(v => v.GetSecret(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task ThreeSecretsUsesFactoryThreeTimes()
		{
			var mockFact = new Mock<ISecretProviderFactory>();
			var mockProv = new Mock<ISecretProvider>();
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult(String.Empty));
			var s = new SecretsMananger(mockFact.Object);
			await s.ReplaceSecrets(new List<SecretVariable>() { new SecretVariable("hello", "#{world} #{double} #{trouble}") }).ConfigureAwait(false);
			mockProv.Verify(v => v.GetSecret(It.IsAny<string>()), Times.Exactly(3));
		}

		[Fact]
		public async Task SecretsAreReplaced()
		{
			var mockFact = new Mock<ISecretProviderFactory>();
			var mockProv = new Mock<ISecretProvider>();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult("SECRET"));
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			var s = new SecretsMananger(mockFact.Object);
			var secrets = new List<SecretVariable>() { new SecretVariable("hello", "#{world}") };
			await s.ReplaceSecrets(secrets).ConfigureAwait(false);
			secrets.Single().Value.Should().Be("SECRET");
		}

		[Theory]
		[InlineData("#{var1} #{var2}", "AB AB", "AB")]
		public async Task MultipleSecretsAreReplaced(string variableText, string expectedText, string replace)
		{
			var mockFact = new Mock<ISecretProviderFactory>();
			var mockProv = new Mock<ISecretProvider>();
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult(replace));
			var s = new SecretsMananger(mockFact.Object);
			var secrets = new List<SecretVariable>() { new SecretVariable("hello", variableText) };
			await s.ReplaceSecrets(secrets).ConfigureAwait(false);
			secrets.Single().Value.Should().Be(expectedText);
		}

		[Fact]
		public async Task ReplacedVariableIsMarkedSecret()
		{
			var mockFact = new Mock<ISecretProviderFactory>();
			var mockProv = new Mock<ISecretProvider>();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult("SECRET"));
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			var s = new SecretsMananger(mockFact.Object);
			var secrets = new List<SecretVariable>() { new SecretVariable("hello", "#{world}") };
			await s.ReplaceSecrets(secrets).ConfigureAwait(false);
			secrets.Single().IsSecret.Should().BeTrue();
		}

		[Fact]
		public async Task NonReplacedVariableIsNotMarkedSecret()
		{
			var mockFact = new Mock<ISecretProviderFactory>();
			var mockProv = new Mock<ISecretProvider>();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult("SECRET"));
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			var s = new SecretsMananger(mockFact.Object);
			var secrets = new List<SecretVariable>() { new SecretVariable("hello", "world") };
			await s.ReplaceSecrets(secrets).ConfigureAwait(false);
			secrets.Single().IsSecret.Should().BeFalse();
		}
	}
}
