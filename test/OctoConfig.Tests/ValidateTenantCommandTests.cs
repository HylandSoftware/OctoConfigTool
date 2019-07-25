using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Moq;
using OctoConfig.Core;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Commands;
using OctoConfig.Core.Octopus;
using OctoConfig.Core.Secrets;
using OctoConfig.Tests.TestFixture;
using Xunit;

namespace OctoConfig.Tests
{
	public static class ValidateTenantCommandTests
	{
		public class Constructor
		{
			[Theory, AppAutoData]
			public void ContsructorGuardClauses(IFixture fixture)
			{
				var assertion = new GuardClauseAssertion(fixture);
				assertion.Verify(typeof(ValidateTenantCommand).GetConstructors());
			}
		}

		public class Execute
		{
			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ApplyIsSetToFalse(string json, [Frozen] Mock<ISecretsMananger> mockSecret, [Frozen] Mock<ITenantManager> mockTenant,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] ValidateTenantArgs args, ValidateTenantCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockSecret.Verify(m => m.ReplaceSecrets(It.IsAny<List<SecretVariable>>()), Times.Once);
				mockTenant.Verify(m => m.CreateTenantVariables(It.IsAny<List<SecretVariable>>(), It.Is<bool>(b => !b)), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task VariablesAreSetCorrectly(string json, [Frozen] Mock<ISecretsMananger> mockSecret, [Frozen] Mock<ITenantManager> mockTenant,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] ValidateTenantArgs args, ValidateTenantCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockSecret.Verify(m => m.ReplaceSecrets(It.Is<List<SecretVariable>>(l => l.Count == 1)), Times.Once);
				mockTenant.Verify(m => m.CreateTenantVariables(It.Is<List<SecretVariable>>(l => l.Count == 1), It.Is<bool>(b => !b)), Times.Once);
			}
		}
	}
}
