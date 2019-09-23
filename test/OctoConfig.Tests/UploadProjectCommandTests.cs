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
	public static class UploadProjectCommandTests
	{
		public class Constructor
		{
			[Theory, AppAutoData]
			public void ConstructorGuardClauses(IFixture fixture)
			{
				var assertion = new GuardClauseAssertion(fixture);
				assertion.Verify(typeof(UploadProjectCommand).GetConstructors());
			}
		}

		public class Execute
		{
			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ApplyIsSetToFalse(string json, [Frozen] Mock<IProjectManager> mockProject,
				[Frozen] Mock<IProjectClearer> mockClearer, [Frozen] MockFileSystem mockFileSystem, [Frozen] UploadProjectArgs args, UploadProjectCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockClearer.Verify(m => m.ClearProjectVariables(), Times.Once);
				mockProject.Verify(m => m.CreateProjectVariables(It.IsAny<List<SecretVariable>>(), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task VariablesAreSetCorrectly(string json, [Frozen] Mock<IProjectManager> mockProject,
				[Frozen] Mock<IProjectClearer> mockClearer, [Frozen] MockFileSystem mockFileSystem, [Frozen] UploadProjectArgs args, UploadProjectCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				if (args.ClearProject)
				{
					mockClearer.Verify(m => m.ClearProjectVariables(), Times.Once);
				}
				mockProject.Verify(m => m.CreateProjectVariables(It.Is<List<SecretVariable>>(l => l.Count == 1), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ProjectTemplateDoesNotClear(string json, [Frozen] Mock<IProjectManager> mockProject,
				[Frozen] Mock<IProjectClearer> mockClearer, [Frozen] MockFileSystem mockFileSystem, [Frozen] UploadProjectArgs args, UploadProjectCommand sut)
			{
				args.ClearProject = false;
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockClearer.Verify(m => m.ClearProjectVariables(), Times.Never);
				mockProject.Verify(m => m.CreateProjectVariables(It.IsAny<List<SecretVariable>>(), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ProjectTemplateDoesClear(string json, [Frozen] Mock<IProjectManager> mockProject,
				[Frozen] Mock<IProjectClearer> mockClearer, [Frozen] MockFileSystem mockFileSystem, [Frozen] UploadProjectArgs args, UploadProjectCommand sut)
			{
				args.ClearProject = true;
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockClearer.Verify(m => m.ClearProjectVariables(), Times.Once);
				mockProject.Verify(m => m.CreateProjectVariables(It.IsAny<List<SecretVariable>>(), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task SecretsAreReplaced(string json, [Frozen] Mock<ISecretsMananger> mockSecret, [Frozen] MockFileSystem mockFileSystem,
				[Frozen] UploadProjectArgs args, UploadProjectCommand sut)
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
