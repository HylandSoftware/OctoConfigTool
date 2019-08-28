using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Octopus;

namespace OctoConfig.Core.Commands
{
	public class ClearProjectCommand
	{
		private readonly IProjectClearer _projectClearer;

		public ClearProjectCommand(IProjectClearer projectClearer)
		{
			_projectClearer = projectClearer ?? throw new System.ArgumentNullException(nameof(projectClearer));
		}

		public async Task Execute()
		{
			await _projectClearer.ClearProjectVariables().ConfigureAwait(false);
		}
	}
}
