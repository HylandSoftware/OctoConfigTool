using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using OctoConfig.Core;
using OctoConfig.Core.Secrets;
using OctoConfig.Tests.TestFixture;
using Xunit;

namespace OctoConfig.Tests
{
	public class SecretsManagerTests
	{
		[Theory, AppAutoData]
		public async Task NoSecretsDoesNotUseFactory([Frozen] Mock<ISecretProviderFactory> mockFact, [Frozen]  Mock<ISecretProvider> mockProv, SecretsMananger sut)
		{
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();

			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();
			await sut.ReplaceSecrets(new List<SecretVariable>() { new SecretVariable("hello", "world") }).ConfigureAwait(false);
			mockProv.Verify(v => v.GetSecret(It.IsAny<string>()), Times.Never);
		}

		[Theory, AppAutoData]
		public async Task SecretDoesUseFactory([Frozen] Mock<ISecretProviderFactory> mockFact, [Frozen]  Mock<ISecretProvider> mockProv, SecretsMananger sut)
		{
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult(String.Empty));

			await sut.ReplaceSecrets(new List<SecretVariable>() { new SecretVariable("hello", "#{world}") }).ConfigureAwait(false);
			mockProv.Verify(v => v.GetSecret(It.IsAny<string>()), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task ThreeSecretsUsesFactoryThreeTimes([Frozen] Mock<ISecretProviderFactory> mockFact, [Frozen]  Mock<ISecretProvider> mockProv, SecretsMananger sut)
		{
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult(String.Empty));
			await sut.ReplaceSecrets(new List<SecretVariable>() { new SecretVariable("hello", "#{world} #{double} #{trouble}") }).ConfigureAwait(false);
			mockProv.Verify(v => v.GetSecret(It.IsAny<string>()), Times.Exactly(3));
		}

		[Theory, AppAutoData]
		public async Task SecretsAreReplaced([Frozen] Mock<ISecretProviderFactory> mockFact, [Frozen] Mock<ISecretProvider> mockProv, SecretsMananger sut)
		{
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult("SECRET"));
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			var secrets = new List<SecretVariable>() { new SecretVariable("hello", "#{world}") };
			await sut.ReplaceSecrets(secrets).ConfigureAwait(false);
			secrets.Single().Value.Should().Be("SECRET");
		}

		[Theory]
		[InlineAppAutoData("#{var1} #{var2}", "AB AB", "AB")]
		[InlineAppAutoData("[ #{var1}, #{var2} ]", "[ AB, AB ]", "AB")]
		[InlineAppAutoData("[ { #{var1}, #{var2} } ]", "[ { AB, AB } ]", "AB")]
		[InlineAppAutoData("[ { #{var1}, #{var2} }, { #{var1}, #{var2} } ]", "[ { AB, AB }, { AB, AB } ]", "AB")]
		[InlineAppAutoData("[ { BAD }, { #{var1}, #{var2} }, { #{var1}, #{var2} } ]", "[ { BAD }, { AB, AB }, { AB, AB } ]", "AB")]
		public async Task MultipleSecretsAreReplaced(string variableText, string expectedText, string replace,
			[Frozen] Mock<ISecretProviderFactory> mockFact, [Frozen] Mock<ISecretProvider> mockProv, SecretsMananger sut)
		{
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Verifiable();
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult(replace));
			var secrets = new List<SecretVariable>() { new SecretVariable("hello", variableText) };
			await sut.ReplaceSecrets(secrets).ConfigureAwait(false);
			secrets.Single().Value.Should().Be(expectedText);
		}

		[Theory, AppAutoData]
		public async Task ReplacedVariableIsMarkedSecret([Frozen] Mock<ISecretProviderFactory> mockFact, [Frozen] Mock<ISecretProvider> mockProv, SecretsMananger sut)
		{
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult("SECRET"));
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			var secrets = new List<SecretVariable>() { new SecretVariable("hello", "#{world}") };
			await sut.ReplaceSecrets(secrets).ConfigureAwait(false);
			secrets.Single().IsSecret.Should().BeTrue();
		}

		[Theory, AppAutoData]
		public async Task NonReplacedVariableIsNotMarkedSecret([Frozen] Mock<ISecretProviderFactory> mockFact, [Frozen] Mock<ISecretProvider> mockProv, SecretsMananger sut)
		{
			mockProv.Setup(v => v.GetSecret(It.IsAny<string>())).Returns(Task.FromResult("SECRET"));
			mockFact.Setup(f => f.Create(It.IsAny<string>())).Returns(mockProv.Object);
			var secrets = new List<SecretVariable>() { new SecretVariable("hello", "world") };
			await sut.ReplaceSecrets(secrets).ConfigureAwait(false);
			secrets.Single().IsSecret.Should().BeFalse();
		}
	}
}
