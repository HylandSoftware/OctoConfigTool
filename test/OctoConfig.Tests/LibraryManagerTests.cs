using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Moq;
using OctoConfig.Core;
using OctoConfig.Core.Octopus;
using OctoConfig.Tests.TestFixture;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Repositories.Async;
using Xunit;

namespace OctoConfig.Tests
{
	public static class LibraryManagerTests
	{
		public class Constructor
		{
			[Theory, AppAutoData]
			public void ContsructorGuardClauses(IFixture fixture)
			{
				var assertion = new GuardClauseAssertion(fixture);
				assertion.Verify(typeof(LibraryManager).GetConstructors());
			}
		}

		public class ClearLibrarySet
		{
			[Theory, AppAutoData]
			public async Task VariablesShouldBeCleared([Frozen] Mock<IOctopusAsyncRepository> mockRepo, [Frozen] Mock<IVariableSetRepository> mockVar,
				[Frozen] Mock<ILibraryVariableSetRepository> mockLib, LibraryVariableSetResource libResource, VariableSetResource varResource, LibraryManager sut)
			{
				mockLib.Setup(m => m.FindByName(It.Is<string>(s => s.Equals(libResource.Name)), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(Task.FromResult(libResource));
				mockVar.Setup(m => m.Get(It.Is<string>(s => s.Equals(varResource.Id))))
					.Returns(Task.FromResult(varResource));
				mockRepo.Setup(m => m.VariableSets).Returns(mockVar.Object);
				var expected = varResource.Variables.Count;
				var actual = await sut.ClearLibrarySet(libResource.Name).ConfigureAwait(false);
				Assert.Equal(expected, actual);
			}

			[Theory, AppAutoData]
			public async Task EmptyVariablesShoudBePassedToModify([Frozen] Mock<IOctopusAsyncRepository> mockRepo, [Frozen] Mock<IVariableSetRepository> mockVar,
				[Frozen] Mock<ILibraryVariableSetRepository> mockLib, LibraryVariableSetResource libResource, VariableSetResource varResource, LibraryManager sut)
			{
				mockLib.Setup(m => m.FindByName(It.Is<string>(s => s.Equals(libResource.Name)), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(Task.FromResult(libResource));
				mockVar.Setup(m => m.Get(It.Is<string>(s => s.Equals(varResource.Id))))
					.Returns(Task.FromResult(varResource));
				mockRepo.Setup(m => m.VariableSets).Returns(mockVar.Object);
				var expected = varResource.Variables.Count;
				var actual = await sut.ClearLibrarySet(libResource.Name).ConfigureAwait(false);
				Assert.Equal(expected, actual);

				mockVar.Verify(m => m.Modify(It.Is<VariableSetResource>(v => v.Variables.Count == 0)), Times.Once);
			}
		}

		public class UpdateVars
		{
			[Theory, AppAutoData]
			public async Task AllEnvironmentsAreValidated([Frozen] Mock<IMachineRoleRepository> mockRole,
				[Frozen] Mock<IEnvironmentRepository> mockEnv, LibraryVariableSetResource libResource, List<EnvironmentResource> environments,
				List<string> roles, List<SecretVariable> vars, LibraryManager sut)
			{
				mockRole.Setup(m => m.GetAllRoleNames()).Returns(Task.FromResult(roles));
				mockEnv.Setup(m => m.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
					.Returns<string, string, object>((n, _, __) => Task.FromResult(environments.Single(e => e.Name == n)))
					.Verifiable();
				await sut.UpdateVars(vars, libResource.Name, environments.Select(e => e.Name), roles, false).ConfigureAwait(false);

				mockEnv.Verify(m => m.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(environments.Count));
			}

			[Theory, AppAutoData]
			public async Task ModifyNotCalledWhenApplyIsFalse([Frozen] Mock<IMachineRoleRepository> mockRole,
				[Frozen] Mock<IVariableSetRepository> mockVar, LibraryVariableSetResource libResource, List<EnvironmentResource> environments,
				List<string> roles, List<SecretVariable> vars, LibraryManager sut)
			{
				mockRole.Setup(m => m.GetAllRoleNames()).Returns(Task.FromResult(roles));
				await sut.UpdateVars(vars, libResource.Name, environments.Select(e => e.Name), roles, false).ConfigureAwait(false);
				mockVar.Verify(m => m.Modify(It.IsAny<VariableSetResource>()), Times.Never);
			}

			[Theory, AppAutoData]
			public async Task ModifyCalledWhenApplyIsTrue([Frozen] Mock<IMachineRoleRepository> mockRole,
				[Frozen] Mock<IVariableSetRepository> mockVar, LibraryVariableSetResource libResource, List<EnvironmentResource> environments,
				List<string> roles, List<SecretVariable> vars, LibraryManager sut)
			{
				mockRole.Setup(m => m.GetAllRoleNames()).Returns(Task.FromResult(roles));
				await sut.UpdateVars(vars, libResource.Name, environments.Select(e => e.Name), roles, true).ConfigureAwait(false);
				mockVar.Verify(m => m.Modify(It.IsAny<VariableSetResource>()), Times.Once);
			}

			[Theory, AppAutoData]
			public async Task EnvironmentAreAppliedToScope([Frozen] Mock<IMachineRoleRepository> mockRole, [Frozen] Mock<IEnvironmentRepository> mockEnv,
				[Frozen] Mock<IVariableSetRepository> mockVar, LibraryVariableSetResource libResource, List<EnvironmentResource> environments,
				List<string> roles, List<SecretVariable> vars, LibraryManager sut)
			{
				mockEnv.Setup(m => m.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
					.Returns<string, string, object>((n, _, __) => Task.FromResult(environments.Single(e => e.Name == n)));
				mockRole.Setup(m => m.GetAllRoleNames()).Returns(Task.FromResult(roles));

				await sut.UpdateVars(vars, libResource.Name, environments.Select(e => e.Name), roles, true).ConfigureAwait(false);

				mockVar.Verify(m => m.Modify(It.Is<VariableSetResource>(v => v.ScopeValues.Environments.Count == environments.Count)), Times.Once);
			}

			[Theory, AppAutoData]
			public async Task VariablesAreAddedToSet([Frozen] Mock<IMachineRoleRepository> mockRole, [Frozen] Mock<IEnvironmentRepository> mockEnv,
				[Frozen] Mock<IVariableSetRepository> mockVar, LibraryVariableSetResource libResource, List<EnvironmentResource> environments,
				List<string> roles, List<SecretVariable> vars, LibraryManager sut)
			{
				mockEnv.Setup(m => m.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
					.Returns<string, string, object>((n, _, __) => Task.FromResult(environments.Single(e => e.Name == n)));
				mockRole.Setup(m => m.GetAllRoleNames()).Returns(Task.FromResult(roles));

				await sut.UpdateVars(vars, libResource.Name, environments.Select(e => e.Name), roles, true).ConfigureAwait(false);

				mockVar.Verify(m => m.Modify(It.Is<VariableSetResource>(vsr => vars.All(sv => vsr.Variables.Any(vr => vr.Name == sv.Name)))), Times.Once);
			}
		}
	}
}
