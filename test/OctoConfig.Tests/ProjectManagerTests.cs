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
using AutoFixture.Xunit2;
using OctoConfig.Core.DependencySetup;

namespace OctoConfig.Tests
{
	public class ProjectManagerTests
	{
		[Theory]
		[InlineAppAutoData("testProj")]
		[InlineAppAutoData("fakeProj")]
		public void MissingProjectThrows(string projectName, [Frozen] Mock<IOctopusAsyncRepository> octoMoq)
		{
			octoMoq.Setup(o => o.Projects).Returns(Mock.Of<IProjectRepository>());
			var args = new ProjectArgsBase() { ProjectName = projectName };
			var sut = new ProjectManager(args, octoMoq.Object, Mock.Of<ILogger>());

			Func<Task> test = () => sut.CreateProjectVariables(new List<SecretVariable>());
			test.Should().Throw<ArgumentException>().WithMessage($"Unable to find a project with the name '{projectName}'");
		}

		[Theory]
		[InlineData("testProj", "aA", "g")]
		[InlineData("testProj", "varName", "varValue")]
		public async Task ProjectTemplatesShouldBeCreatedToMatchVariables(string projectName, string variableName, string variableValue)
		{
			var project = new ProjectResource() { Id = projectName, Name = projectName };
			var octoMoq = new Mock<IOctopusAsyncRepository>();
			var mockProj = this.mockProj(projectName);
			mockProj.Setup(t => t.Modify(It.IsAny<ProjectResource>()))
				.Returns<ProjectResource>(prj =>
				{
					prj.Templates.Count.Should().Be(1);
					prj.Templates.Single().Name.Should().Be(variableName);
					prj.Templates.Single().Label.Should().Be(variableName);
					prj.Templates.Single().HelpText.Should().Be(variableName);
					prj.Templates.Single().DefaultValue.IsSensitive.Should().BeFalse();
					prj.Templates.Single().DefaultValue.Value.Should().Be("PLACEHOLDER_VALUE");
					return Task.FromResult(prj);
				});
			octoMoq.Setup(o => o.Projects).Returns(mockProj.Object);

			var args = new ProjectArgsBase() { ProjectName = projectName };
			var sut = new ProjectManager(args, octoMoq.Object, Mock.Of<ILogger>());

			await sut.CreateProjectVariables(new List<SecretVariable>() { new SecretVariable(variableName, variableValue) }).ConfigureAwait(false);
		}

		[Theory]
		[InlineData("testProj", "aA", "g")]
		[InlineData("testProj", "varName", "varValue")]
		public async Task ProjectTemplatesShouldBeCreatedWithNewDefaultValues(string projectName, string variableName, string variableValue)
		{
			var project = new ProjectResource() { Id = projectName, Name = projectName };
			var octoMoq = new Mock<IOctopusAsyncRepository>();
			var mockProj = this.mockProj(projectName);
			mockProj.Setup(t => t.Modify(It.IsAny<ProjectResource>()))
				.Returns<ProjectResource>(prj =>
				{
					prj.Templates.Count.Should().Be(1);
					prj.Templates.Single().Name.Should().Be(variableName);
					prj.Templates.Single().Label.Should().Be(variableName);
					prj.Templates.Single().HelpText.Should().Be(variableName);
					prj.Templates.Single().DefaultValue.IsSensitive.Should().BeFalse();
					prj.Templates.Single().DefaultValue.Value.Should().Be(variableValue);
					return Task.FromResult(prj);
				});
			octoMoq.Setup(o => o.Projects).Returns(mockProj.Object);

			var args = new ProjectArgsBase() { ProjectName = projectName };
			var sut = new ProjectManager(args, octoMoq.Object, Mock.Of<ILogger>());

			await sut.CreateProjectVariables(new List<SecretVariable>() { new SecretVariable(variableName, variableValue) }, true).ConfigureAwait(false);
		}

		[Theory]
		[InlineData("testProj", "aA", "g")]
		[InlineData("testProj", "varName", "varValue")]
		public async Task SensitiveProjectTemplatesShouldBeCreatedToMatchVariables(string projectName, string variableName, string variableValue)
		{
			var project = new ProjectResource() { Id = projectName, Name = projectName };
			var octoMoq = new Mock<IOctopusAsyncRepository>();
			var mockProj = this.mockProj(projectName);
			mockProj.Setup(t => t.Modify(It.IsAny<ProjectResource>()))
				.Returns<ProjectResource>(prj =>
				{
					prj.Templates.Count.Should().Be(1);
					prj.Templates.Single().Name.Should().Be(variableName);
					prj.Templates.Single().Label.Should().Be(variableName);
					prj.Templates.Single().HelpText.Should().BeNull();
					prj.Templates.Single().DisplaySettings.Single().Value.Should().Be("Sensitive");
					prj.Templates.Single().DefaultValue.Should().BeNull();
					return Task.FromResult(prj);
				});
			octoMoq.Setup(o => o.Projects).Returns(mockProj.Object);

			var args = new ProjectArgsBase() { ProjectName = projectName };
			var sut = new ProjectManager(args, octoMoq.Object, Mock.Of<ILogger>());

			await sut.CreateProjectVariables(new List<SecretVariable>() { new SecretVariable(variableName, variableValue) { IsSecret = true } }).ConfigureAwait(false);
		}

		private Mock<IProjectRepository> mockProj(string projectName)
		{
			var projMoq = new Mock<IProjectRepository>();
			projMoq.Setup(p => p.FindByName(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
				.Returns(Task.FromResult(new ProjectResource() { Name = projectName, Id = projectName }));
			return projMoq;
		}
	}
}
