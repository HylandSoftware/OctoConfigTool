using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Octopus;

namespace OctoConfig.Core.Commands
{
	public class ClearProjectCommand
	{
		private readonly ClearProjectArgs _args;
		private readonly ProjectClearer _projectClearer;

		public ClearProjectCommand(ClearProjectArgs args, ProjectClearer projectClearer)
		{
			_args = args ?? throw new System.ArgumentNullException(nameof(args));
			_projectClearer = projectClearer ?? throw new System.ArgumentNullException(nameof(projectClearer));
		}

		public async Task Execute()
		{
			await _projectClearer.ClearProjectVariables().ConfigureAwait(false);
		}
	}
}
