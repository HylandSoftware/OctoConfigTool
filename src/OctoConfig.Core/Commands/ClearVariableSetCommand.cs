using System;
using System.Threading.Tasks;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Octopus;

namespace OctoConfig.Core.Commands
{
	public class ClearVariableSetCommand
	{
		private readonly ClearVariableSetArgs _args;
		private readonly LibraryManager _libraryManager;

		public ClearVariableSetCommand(ClearVariableSetArgs args, LibraryManager libraryManager)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			_libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
		}

		public async Task Execute()
		{
			var removed = await _libraryManager.ClearLibrarySet(_args.Library);
			Console.WriteLine($"Removed {removed} variables");
		}
	}
}
