using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OctoConfig.Core.Octopus;
using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Repositories.Async;
using Xunit;
using Xunit.Extensions;

namespace OctoConfig.Tests
{
	public static class IOctopusAsyncRepositoryExtensionsTests
	{
		public class ValidateRolesTests
		{
			[Theory]
			[MemberData(nameof(FoundData))]
			public async Task FoundRoleShouldNotThrow(List<string> server, List<string> passedIn)
			{
				var machineMock = new Mock<IMachineRoleRepository>();
				machineMock.Setup(m => m.GetAllRoleNames()).Returns(Task.FromResult(server));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.MachineRoles).Returns(machineMock.Object);
				await mock.Object.ValidateRoles(passedIn).ConfigureAwait(false);
			}

			[Theory]
			[MemberData(nameof(MissingData))]
			public void MissingRoleShouldNotThrow(List<string> server, List<string> passedIn, string missing)
			{
				var machineMock = new Mock<IMachineRoleRepository>();
				machineMock.Setup(m => m.GetAllRoleNames()).Returns(Task.FromResult(server));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.MachineRoles).Returns(machineMock.Object);
				Func<Task> test = () => mock.Object.ValidateRoles(passedIn);
				test.Should().Throw<ArgumentException>().WithMessage($"Unable to find a machine role with the name '{missing}'");
			}

			public static IEnumerable<object[]> FoundData => new[]
					{
					new object[] { new List<string>() { "api" }, new List<string>() { "api" } },
					new object[] { new List<string>() { "api", "web" }, new List<string>() { "api", "web" } },
					new object[] { new List<string>() { "web", "redis", "api" }, new List<string>() { "redis" } }
				};

			public static IEnumerable<object[]> MissingData => new[]
			{
					new object[] { new List<string>() { "web" }, new List<string>() { "api" }, "api" },
					new object[] { new List<string>(), new List<string>() { "api" }, "api" },
					new object[] { new List<string>() { "web", "api" }, new List<string>() { "redis" }, "redis" }
				};
		}

		public class ValidateEnvironmentTests
		{
			[Theory]
			[InlineData("QA")]
			public async Task FoundEnvironmentShouldNotThrow(string environment)
			{
				var environmentMock = new Mock<IEnvironmentRepository>();
				environmentMock.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult(new EnvironmentResource()));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.Environments).Returns(environmentMock.Object);
				await mock.Object.ValidateEnvironment(environment).ConfigureAwait(false);
			}

			[Theory]
			[InlineData("QA")]
			public void MissingEnvironmentShouldNotThrow(string environment)
			{
				var environmentMock = new Mock<IEnvironmentRepository>();
				environmentMock.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult<EnvironmentResource>(null));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.Environments).Returns(environmentMock.Object);
				Func<Task> test = () => mock.Object.ValidateEnvironment(environment);
				test.Should().Throw<ArgumentException>().WithMessage($"Unable to find an environment with the name '{environment}'");
			}
		}

		public class ValidateLibraryTests
		{
			[Theory]
			[InlineData("Shared")]
			public async Task FoundLibraryShouldNotThrow(string library)
			{
				var libraryMock = new Mock<ILibraryVariableSetRepository>();
				libraryMock.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult(new LibraryVariableSetResource()));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.LibraryVariableSets).Returns(libraryMock.Object);
				await mock.Object.ValidateLibrary(library).ConfigureAwait(false);
			}

			[Theory]
			[InlineData("QA")]
			public void MissingLibraryShouldNotThrow(string library)
			{
				var libraryMock = new Mock<ILibraryVariableSetRepository>();
				libraryMock.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult<LibraryVariableSetResource>(null));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.LibraryVariableSets).Returns(libraryMock.Object);
				Func<Task> test = () => mock.Object.ValidateLibrary(library);
				test.Should().Throw<ArgumentException>().WithMessage($"Unable to find a library with the name '{library}'");
			}
		}

		public class ValidateProjectTests
		{
			[Theory]
			[InlineData("QA")]
			public async Task FoundProjectShouldNotThrow(string project)
			{
				var projectMock = new Mock<IProjectRepository>();
				projectMock.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult(new ProjectResource()));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.Projects).Returns(projectMock.Object);
				await mock.Object.ValidateProject(project).ConfigureAwait(false);
			}

			[Theory]
			[InlineData("QA")]
			public void MissingProjectShouldNotThrow(string project)
			{
				var projectMock = new Mock<IProjectRepository>();
				projectMock.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult<ProjectResource>(null));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.Projects).Returns(projectMock.Object);
				Func<Task> test = () => mock.Object.ValidateProject(project);
				test.Should().Throw<ArgumentException>().WithMessage($"Unable to find a project with the name '{project}'");
			}
		}

		public class ValidateTenantTests
		{
			[Theory]
			[InlineData("QA")]
			public async Task FoundRoleShouldNotThrow(string tenant)
			{
				var tenantMock = new Mock<ITenantRepository>();
				tenantMock.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult(new TenantResource()));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.Tenants).Returns(tenantMock.Object);
				await mock.Object.ValidateTenant(tenant).ConfigureAwait(false);
			}

			[Theory]
			[InlineData("QA")]
			public void MissingRoleShouldNotThrow(string tenant)
			{
				var tenantMock = new Mock<ITenantRepository>();
				tenantMock.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(Task.FromResult<TenantResource>(null));
				var mock = new Mock<IOctopusAsyncRepository>();
				mock.Setup(or => or.Tenants).Returns(tenantMock.Object);
				Func<Task> test = () => mock.Object.ValidateTenant(tenant);
				test.Should().Throw<ArgumentException>().WithMessage($"Unable to find a tenant with the name '{tenant}'");
			}
		}
	}
}
