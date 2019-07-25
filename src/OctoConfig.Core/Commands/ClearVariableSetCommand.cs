using System;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.DependencySetup;
using OctoConfig.Core.Octopus;

namespace OctoConfig.Core.Commands
{
	public class ClearVariableSetCommand
	{
		private readonly ClearVariableSetArgs _args;
		private readonly ILibraryManager _libraryManager;
		private readonly ILogger _logger;

		public ClearVariableSetCommand(ClearVariableSetArgs args, ILibraryManager libraryManager, ILogger logger)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Execute()
		{
			var removed = await _libraryManager.ClearLibrarySet(_args.Library).ConfigureAwait(false);
			_logger.Information($"Removed {removed} variables");
		}
	}
}
