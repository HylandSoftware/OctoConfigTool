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
	public static class ValidateLibraryCommandTests
	{
		public class Constructor
		{
			[Theory, AppAutoData]
			public void ContsructorGuardClauses(IFixture fixture)
			{
				var assertion = new GuardClauseAssertion(fixture);
				assertion.Verify(typeof(ValidateLibraryCommand).GetConstructors());
			}
		}

		public class Execute
		{
			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task ApplyIsSetToFalse(string json, [Frozen] Mock<ISecretsMananger> mockSecret, [Frozen] Mock<ILibraryManager> mockLibrary,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] ValidateArgs args, ValidateLibraryCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockSecret.Verify(m => m.ReplaceSecrets(It.IsAny<List<SecretVariable>>(), args), Times.Once);
				mockLibrary.Verify(m => m.UpdateVars(It.IsAny<List<SecretVariable>>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.Is<bool>(b => !b)), Times.Once);
			}

			[Theory, InlineAppAutoData("{ \"a\":\"b\" }")]
			public async Task VariablesAreSetCorrectly(string json, [Frozen] Mock<ISecretsMananger> mockSecret, [Frozen] Mock<ILibraryManager> mockLibrary,
				[Frozen] MockFileSystem mockFileSystem, [Frozen] ValidateArgs args, ValidateLibraryCommand sut)
			{
				mockFileSystem.AddFile(args.File, new MockFileData(json));
				await sut.Execute().ConfigureAwait(false);
				mockSecret.Verify(m => m.ReplaceSecrets(It.Is<List<SecretVariable>>(l => l.Count == 1), args), Times.Once);
				mockLibrary.Verify(m => m.UpdateVars(It.Is<List<SecretVariable>>(l => l.Count == 1), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.Is<bool>(b => !b)), Times.Once);
			}
		}
	}
}
