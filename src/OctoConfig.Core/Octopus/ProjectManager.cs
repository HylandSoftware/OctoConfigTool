using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.DependencySetup;
using Octopus.Client;

namespace OctoConfig.Core.Octopus
{
	public interface IProjectManager
	{
		Task CreateProjectVariables(List<SecretVariable> vars);
	}

	public class ProjectManager : IProjectManager
	{
		private readonly TenantTargetArgs _args;
		private readonly IOctopusAsyncRepository _octopusRepository;
		private readonly ILogger _logger;

		public ProjectManager(TenantTargetArgs args, IOctopusAsyncRepository octopusRepository, ILogger logger)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_octopusRepository = octopusRepository ?? throw new ArgumentNullException(nameof(octopusRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task CreateProjectVariables(List<SecretVariable> vars)
		{
			var project = await _octopusRepository.ValidateProject(_args.ProjectName).ConfigureAwait(false);
			var before = project.Templates.Count;
			foreach (var variable in vars)
			{
				var isSensitive = project.Templates.Any(t => t.Name.Equals(variable.Name) && t.IsSensitive());
				if (variable.IsSecret || isSensitive)
				{
					project.AddOrUpdateSensitiveTemplate(variable.Name, variable.Name);
				}
				else
				{
					project.AddOrUpdateSingleLineTextTemplate(variable.Name, variable.Name, "PLACEHOLDER_VALUE", variable.Name);
				}
			}
			var after = project.Templates.Count;
			_logger.Information($"Created {after - before} variable templates in project {_args.ProjectName}");
			await _octopusRepository.Projects.Modify(project).ConfigureAwait(false);
		}
	}
}
