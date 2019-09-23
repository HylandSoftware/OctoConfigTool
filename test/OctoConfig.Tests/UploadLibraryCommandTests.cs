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
using OctoConfig.Core.Converter;
using OctoConfig.Core.Octopus;
using OctoConfig.Core.Secrets;
using OctoConfig.Tests.TestFixture;
using Xunit;

namespace OctoConfig.Tests
{
	public static class UploadLibraryCommandTests
	{
		public class Constructor
		{
			[Theory, AppAutoData]
			public void ConstructorGuardClauses(IFixture fixture)
			{
				var assertion = new GuardClauseAssertion(fixture);
				assertion.Verify(typeof(UploadLibraryCommand).GetConstructors());
			}
		}

		public class Execute
		{
			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ApplyIsSetToTrue(string json, [Frozen] Mock<ILibraryManager> mockLibraryManager,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] LibraryTargetArgs args, UploadLibraryCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockLibraryManager.Verify(m => m.UpdateVars(It.IsAny<List<SecretVariable>>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task CommandArgsArePassed(string json, [Frozen] Mock<ILibraryManager> mockLibraryManager,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] LibraryTargetArgs args, UploadLibraryCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockLibraryManager.Verify(m => m.UpdateVars(It.IsAny<List<SecretVariable>>(), args.Library, args.Environments, args.OctoRoles, true), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task SecretsAreReplaced(string json, [Frozen] Mock<ISecretsMananger> mockSecret, [Frozen] MockFileSystem mockFileSystem,
				[Frozen] LibraryTargetArgs args, UploadLibraryCommand sut)
			{
				args.VaultRoleId = "RoleId";
				args.VaultSecretId = "SecretId";
				args.VaultUri = "Uri";
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockSecret.Verify(m => m.ReplaceSecrets(It.Is<List<SecretVariable>>(l => l.Count == 1), args), Times.Once);
			}

			[Theory, AppAutoData]
			public async Task CorrectValuesUsed(string json, [Frozen] Mock<ILibraryManager> mockLibraryManager,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] Mock<IVariableConverter> mockVariableConverter,
				[Frozen] LibraryTargetArgs args, [Frozen] List<SecretVariable> secrets, UploadLibraryCommand sut)
			{
				mockVariableConverter.Setup(m => m.Convert(json)).Returns(secrets);
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockLibraryManager.Verify(m => m.UpdateVars(secrets, It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), true), Times.Once);
			}
		}
	}
}
