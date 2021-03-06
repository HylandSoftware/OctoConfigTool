﻿using System;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using Octopus.Client;

namespace OctoConfig.Core.Octopus
{
	public interface IProjectClearer
	{
		Task ClearProjectVariables();
	}
	public class ProjectClearer : IProjectClearer
	{
		private readonly IProjectArgsBase _args;
		private readonly IOctopusAsyncRepository _octopusRepository;

		public ProjectClearer(IProjectArgsBase args, IOctopusAsyncRepository octopusRepository)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_octopusRepository = octopusRepository ?? throw new ArgumentNullException(nameof(octopusRepository));
		}

		public async Task ClearProjectVariables()
		{
			var project = await _octopusRepository.ValidateProject(_args.ProjectName).ConfigureAwait(false);
			if (project == null)
			{
				throw new ArgumentException($"Unable to find a project with the name {_args.ProjectName}");
			}
			project.Clear();
			await _octopusRepository.Projects.Modify(project).ConfigureAwait(false);
		}
	}
}
