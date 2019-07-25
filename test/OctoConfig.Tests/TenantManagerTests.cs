using System;
using System.Collections.Generic;
using Moq;
using OctoConfig.Core;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Octopus;
using Octopus.Client;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using Octopus.Client.Repositories.Async;
using Octopus.Client.Model;
using System.Linq;
using OctoConfig.Tests.TestFixture;
using OctoConfig.Core.DependencySetup;

namespace OctoConfig.Tests
{
	public class TenantManagerTests
	{
		[Theory]
		[InlineAppAutoData("testProj")]
		[InlineAppAutoData("fakeProj")]
		public void MissingProjectThrows(string projectName, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(Mock.Of<IProjectRepository>());
			var args = new TenantTargetArgs() { ProjectName = projectName };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			Func<Task> test = () => sut.CreateTenantVariables(new List<SecretVariable>());
			test.Should().Throw<ArgumentException>().WithMessage($"Unable to find a project with the name '{projectName}'");
		}

		[Theory]
		[InlineAppAutoData("testTen")]
		[InlineAppAutoData("fakeTen")]
		public void MissingTenantThrows(string tenantName, Mock<IOctopusAsyncRepository> octoMoq, Mock<IProjectRepository> projMoq)
		{
			projMoq.Setup(p => p.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
				.Returns(Task.FromResult(new ProjectResource()));
			octoMoq.Setup(o => o.Projects).Returns(projMoq.Object);
			octoMoq.Setup(o => o.Tenants).Returns(Mock.Of<ITenantRepository>());
			var args = new TenantTargetArgs() { TenantName = tenantName };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			Func<Task> test = () => sut.CreateTenantVariables(new List<SecretVariable>());
			test.Should().Throw<ArgumentException>().WithMessage($"Unable to find a tenant with the name '{tenantName}'");
		}

		[Theory]
		[InlineAppAutoData("testEnv")]
		[InlineAppAutoData("fakeEnv")]
		public void MissingEnvironmentThrows(string enviroName, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj("h").Object);
			var tenMoq = mockedTen(null, "h", new TenantVariableResource.Project(""));
			octoMoq.Setup(o => o.Tenants).Returns(tenMoq.Object);
			octoMoq.Setup(o => o.Environments).Returns(Mock.Of<IEnvironmentRepository>());

			var args = new TenantTargetArgs() { Environments = new List<string>() { enviroName } };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			Func<Task> test = () => sut.CreateTenantVariables(new List<SecretVariable>());
			test.Should().Throw<ArgumentException>().WithMessage($"Unable to find an environment with the name '{enviroName}'");
		}

		[Theory]
		[InlineAppAutoData("testTen", "testProj")]
		public void UnlinkedProjectThrows(string tenantName, string projectName, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj(projectName).Object);
			var tenMoq = mockedTen(tenantName, "", null);
			octoMoq.Setup(o => o.Tenants).Returns(tenMoq.Object);
			octoMoq.Setup(o => o.Environments).Returns(Mock.Of<IEnvironmentRepository>());

			var args = new TenantTargetArgs() { TenantName = tenantName, ProjectName = projectName };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			Func<Task> test = () => sut.CreateTenantVariables(new List<SecretVariable>());
			test.Should().Throw<ArgumentException>().WithMessage($"Tenant {tenantName} is not linked with project {projectName}");
		}

		[Theory]
		[InlineAppAutoData("testTen", "testProj", "Qa")]
		public void UnlinkedEnvironmentThrows(string tenantName, string projectName, string environment, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj(projectName).Object);
			var tenMoq = mockedTen(tenantName, projectName, new TenantVariableResource.Project(""));
			octoMoq.Setup(o => o.Tenants).Returns(tenMoq.Object);
			octoMoq.Setup(o => o.Environments).Returns(mockEnv(environment).Object);

			var args = new TenantTargetArgs() { TenantName = tenantName, ProjectName = projectName, Environments = new List<string>() { environment } };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			Func<Task> test = () => sut.CreateTenantVariables(new List<SecretVariable>());
			test.Should().Throw<ArgumentException>().WithMessage($"Tenant {tenantName} is not linked with environment {environment}");
		}

		[Theory]
		[InlineAppAutoData("testTen", "testProj", "Qa")]
		public void UnexpectedTenantConfigThrows(string tenantName, string projectName, string environment, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj(projectName).Object);
			var tenProj = new TenantVariableResource.Project("");
			tenProj.Variables.Add(environment, new Dictionary<string, PropertyValueResource>());
			octoMoq.Setup(o => o.Tenants).Returns(mockedTen(tenantName, projectName, tenProj).Object);
			octoMoq.Setup(o => o.Environments).Returns(mockEnv(environment).Object);

			var args = new TenantTargetArgs() { TenantName = tenantName, ProjectName = projectName, Environments = new List<string>() { environment } };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			Func<Task> test = () => sut.CreateTenantVariables(new List<SecretVariable>() { new SecretVariable("aA", "g") });
			test.Should().Throw<ArgumentException>().WithMessage($"The loaded configuration for tenant '{tenantName}' has variable 'aA' not found in the project '{projectName}'");
		}

		[Theory]
		[InlineAppAutoData("testTen", "testProj", "Qa", "aA", "g")]
		[InlineAppAutoData("testTen", "testProj", "Qa", "varName", "varValue")]
		public async Task TenantvariableShouldBeCreatedToMatchProject(string tenantName, string projectName, string environment, string variableName,
			string variableValue, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj(projectName).Object);
			var tenProj = new TenantVariableResource.Project("");
			tenProj.Variables.Add(environment, new Dictionary<string, PropertyValueResource>());
			tenProj.Templates.Add(new ActionTemplateParameterResource() { Name = variableName, Id = variableName });
			var mockTen = mockedTen(tenantName, projectName, tenProj);
			mockTen.Setup(t => t.ModifyVariables(It.IsAny<TenantResource>(), It.IsAny<TenantVariableResource>()))
				.Returns<TenantResource, TenantVariableResource>((tr, tvr) =>
				{
					tr.Name.Should().Be(tenantName);
					var prj = tvr.ProjectVariables[projectName];
					prj.Variables.Single().Key.Should().Be(environment);
					var env = prj.Variables.Single().Value;
					env.ContainsKey(variableName).Should().BeTrue($"Variable dictionary should contain variable with name {variableName}");
					env[variableName].Value.Should().Be(variableValue, $"Variable dictionary should contain variable with name {variableName}");
					env[variableName].IsSensitive.Should().BeFalse();
					return Task.FromResult(tvr);
				});

			octoMoq.Setup(o => o.Tenants).Returns(mockTen.Object);
			octoMoq.Setup(o => o.Environments).Returns(mockEnv(environment).Object);

			var args = new TenantTargetArgs() { TenantName = tenantName, ProjectName = projectName, Environments = new List<string>() { environment } };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			await sut.CreateTenantVariables(new List<SecretVariable>() { new SecretVariable(variableName, variableValue) }).ConfigureAwait(false);
		}

		[Theory]
		[InlineAppAutoData("testTen", "testProj", "Qa", "aA", "g")]
		[InlineAppAutoData("testTen", "testProj", "Qa", "varName", "varValue")]
		public async Task SensitiveSecretShouldMakeTenantVariableSecret(string tenantName, string projectName, string environment, string variableName,
			string variableValue, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj(projectName).Object);
			var tenProj = new TenantVariableResource.Project("");
			tenProj.Variables.Add(environment, new Dictionary<string, PropertyValueResource>());
			tenProj.Templates.Add(new ActionTemplateParameterResource() { Name = variableName, Id = variableName });
			var mockTen = mockedTen(tenantName, projectName, tenProj);
			mockTen.Setup(t => t.ModifyVariables(It.IsAny<TenantResource>(), It.IsAny<TenantVariableResource>()))
				.Returns<TenantResource, TenantVariableResource>((tr, tvr) =>
				{
					tr.Name.Should().Be(tenantName);
					var prj = tvr.ProjectVariables[projectName];
					prj.Variables.Single().Key.Should().Be(environment);
					var env = prj.Variables.Single().Value;
					env.ContainsKey(variableName).Should().BeTrue($"Variable dictionary should contain variable with name {variableName}");
					env[variableName].IsSensitive.Should().BeTrue();
					env[variableName].SensitiveValue.HasValue.Should().BeTrue();
					env[variableName].SensitiveValue.NewValue.Should().Be(variableValue);
					return Task.FromResult(tvr);
				});

			octoMoq.Setup(o => o.Tenants).Returns(mockTen.Object);
			octoMoq.Setup(o => o.Environments).Returns(mockEnv(environment).Object);

			var args = new TenantTargetArgs() { TenantName = tenantName, ProjectName = projectName, Environments = new List<string>() { environment } };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			await sut.CreateTenantVariables(new List<SecretVariable>() { new SecretVariable(variableName, variableValue) { IsSecret = true } }).ConfigureAwait(false);
		}

		[Theory]
		[InlineAppAutoData("testTen", "testProj", "Qa", "aA", "g")]
		[InlineAppAutoData("testTen", "testProj", "Qa", "varName", "varValue")]
		public async Task SensitiveTemplateShouldMakeTenantVariableSecret(string tenantName, string projectName, string environment, string variableName,
			string variableValue, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj(projectName).Object);
			var tenProj = new TenantVariableResource.Project("");
			tenProj.Variables.Add(environment, new Dictionary<string, PropertyValueResource>());
			tenProj.Templates.Add(new ActionTemplateParameterResource()
				{
					Name = variableName, Id = variableName, DefaultValue = "",
					DisplaySettings = new Dictionary<string, string>() { { "Octopus.ControlType", "Sensitive" } }
				});
			var mockTen = mockedTen(tenantName, projectName, tenProj);
			mockTen.Setup(t => t.ModifyVariables(It.IsAny<TenantResource>(), It.IsAny<TenantVariableResource>()))
				.Returns<TenantResource, TenantVariableResource>((_, tvr) =>
				{
					var prj = tvr.ProjectVariables[projectName];
					var env = prj.Variables.Single().Value;
					env[variableName].IsSensitive.Should().BeTrue();
					env[variableName].SensitiveValue.HasValue.Should().BeTrue();
					env[variableName].SensitiveValue.NewValue.Should().Be(variableValue);
					return Task.FromResult(tvr);
				});

			octoMoq.Setup(o => o.Tenants).Returns(mockTen.Object);
			octoMoq.Setup(o => o.Environments).Returns(mockEnv(environment).Object);

			var args = new TenantTargetArgs() { TenantName = tenantName, ProjectName = projectName, Environments = new List<string>() { environment } };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			await sut.CreateTenantVariables(new List<SecretVariable>() { new SecretVariable(variableName, variableValue) }).ConfigureAwait(false);
		}

		[Theory]
		[InlineAppAutoData("testTen", "testProj")]
		public async Task ApplyFalseDoesntPersistChangesToOctopus(string tenantName, string projectName, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj(projectName).Object);
			var mockTen = mockedTen(tenantName, projectName, new TenantVariableResource.Project(""));
			mockTen.Setup(t => t.ModifyVariables(It.IsAny<TenantResource>(), It.IsAny<TenantVariableResource>()))
				.Verifiable();
			octoMoq.Setup(o => o.Tenants).Returns(mockTen.Object);

			var args = new TenantTargetArgs() { TenantName = tenantName, ProjectName = projectName, Environments = new List<string>() };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			await sut.CreateTenantVariables(new List<SecretVariable>(), apply: false).ConfigureAwait(false);
			mockTen.Verify(t => t.ModifyVariables(It.IsAny<TenantResource>(), It.IsAny<TenantVariableResource>()), Times.Never);
		}

		[Theory]
		[InlineAppAutoData("testTen", "testProj", "Qa", "aA", "g")]
		[InlineAppAutoData("testTen", "testProj", "Qa", "varName", "varValue")]
		public void TenantWithExistingVariableShouldNotThrow(string tenantName, string projectName, string environment, string variableName,
			string variableValue, Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(mockProj(projectName).Object);
			var tenProj = new TenantVariableResource.Project("");
			tenProj.Variables.Add(environment, new Dictionary<string, PropertyValueResource>() { { variableName, new PropertyValueResource(variableValue) } });
			tenProj.Templates.Add(new ActionTemplateParameterResource()
			{
				Name = variableName,
				Id = variableName,
				DefaultValue = "",
				DisplaySettings = new Dictionary<string, string>() { { "Octopus.ControlType", "Sensitive" } }
			});
			var mockTen = mockedTen(tenantName, projectName, tenProj);
			mockTen.Setup(t => t.ModifyVariables(It.IsAny<TenantResource>(), It.IsAny<TenantVariableResource>()))
				.Returns<TenantResource, TenantVariableResource>((_, tvr) => Task.FromResult(tvr));

			octoMoq.Setup(o => o.Tenants).Returns(mockTen.Object);
			octoMoq.Setup(o => o.Environments).Returns(mockEnv(environment).Object);

			var args = new TenantTargetArgs() { TenantName = tenantName, ProjectName = projectName, Environments = new List<string>() { environment } };
			var sut = new TenantManager(args, octoMoq.Object, Mock.Of<ILogger>());

			Func<Task> act = () => sut.CreateTenantVariables(new List<SecretVariable>() { new SecretVariable(variableName, variableValue) });
			act.Should().NotThrow();
		}

		private Mock<ITenantRepository> mockedTen(string tenantName, string projectName, TenantVariableResource.Project obj)
		{
			var tenMoq = new Mock<ITenantRepository>();
			tenMoq.Setup(p => p.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
				.Returns(Task.FromResult(new TenantResource() { Name = tenantName }));
			tenMoq.Setup(t => t.GetVariables(It.IsAny<TenantResource>()))
				.Returns(Task.FromResult(
					new TenantVariableResource()
					{
						ProjectVariables = new Dictionary<string, TenantVariableResource.Project>()
						{ { projectName, obj } }
					}));
			return tenMoq;
		}

		private Mock<IProjectRepository> mockProj(string projectName)
		{
			var projMoq = new Mock<IProjectRepository>();
			projMoq.Setup(p => p.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
				.Returns(Task.FromResult(new ProjectResource() { Name = projectName, Id = projectName }));
			return projMoq;
		}

		private Mock<IEnvironmentRepository> mockEnv(string enviroName)
		{
			var envMoq = new Mock<IEnvironmentRepository>();
			envMoq.Setup(e => e.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
				.Returns(Task.FromResult(new EnvironmentResource() { Id = enviroName }));
			return envMoq;
		}
	}
}
