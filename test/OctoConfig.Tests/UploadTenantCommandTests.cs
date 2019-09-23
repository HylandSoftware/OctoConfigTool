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
	public static class UploadTenantCommandTests
	{
		public class Constructor
		{
			[Theory, AppAutoData]
			public void ConstructorGuardClauses(IFixture fixture)
			{
				var assertion = new GuardClauseAssertion(fixture);
				assertion.Verify(typeof(UploadTenantCommand).GetConstructors());
			}
		}

		public class Execute
		{
			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ApplyIsSetToTrue(string json, [Frozen] Mock<ITenantManager> mockTenantManager,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] UploadTenantArgs args, UploadTenantCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockTenantManager.Verify(m => m.CreateTenantVariables(It.IsAny<List<SecretVariable>>(), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task VariablesAreSetCorrectly(string json, [Frozen] Mock<ITenantManager> mockTenantManager,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] UploadTenantArgs args, UploadTenantCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockTenantManager.Verify(m => m.CreateTenantVariables(It.Is<List<SecretVariable>>(l => l.Count == 1), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ProjectVariablesAreSetCorrectly(string json, [Frozen] Mock<IProjectManager> mockProject,
				[Frozen] Mock<ITenantManager> mockTenantManager, [Frozen] MockFileSystem mockFileSystem, [Frozen] UploadTenantArgs args, UploadTenantCommand sut)
			{
				args.SkipUploadProject = false;
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockProject.Verify(m => m.CreateProjectVariables(It.Is<List<SecretVariable>>(l => l.Count == 1), false), Times.Once);
				mockTenantManager.Verify(m => m.CreateTenantVariables(It.Is<List<SecretVariable>>(l => l.Count == 1), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ProjectVariablesAreNotSet(string json, [Frozen] Mock<IProjectManager> mockProject,
				[Frozen] Mock<ITenantManager> mockTenantManager, [Frozen] MockFileSystem mockFileSystem, [Frozen] UploadTenantArgs args, UploadTenantCommand sut)
			{
				args.SkipUploadProject = true;
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockProject.Verify(m => m.CreateProjectVariables(It.Is<List<SecretVariable>>(l => l.Count == 1), false), Times.Never);
				mockTenantManager.Verify(m => m.CreateTenantVariables(It.Is<List<SecretVariable>>(l => l.Count == 1), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task SecretsAreReplaced(string json, [Frozen] Mock<ISecretsMananger> mockSecret, [Frozen] MockFileSystem mockFileSystem,
				[Frozen] UploadTenantArgs args, UploadTenantCommand sut)
			{
				args.VaultRoleId = "RoleId";
				args.VaultSecretId = "SecretId";
				args.VaultUri = "Uri";
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockSecret.Verify(m => m.ReplaceSecrets(It.Is<List<SecretVariable>>(l => l.Count == 1), args), Times.Once);
			}
		}
	}
}
