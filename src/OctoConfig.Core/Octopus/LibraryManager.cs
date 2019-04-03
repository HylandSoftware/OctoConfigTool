using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octopus.Client;
using Octopus.Client.Model;

namespace OctoConfig.Core.Octopus
{
	public class LibraryManager
	{
		private readonly IOctopusAsyncRepository _octopusRepository;

		public LibraryManager(IOctopusAsyncRepository octopusRepository)
		{
			_octopusRepository = octopusRepository ?? throw new ArgumentNullException(nameof(octopusRepository));
		}

		/// <summary>
		/// Removes all the variables in a Library Variable set
		/// </summary>
		/// <param name="libraryName">The set to clear</param>
		/// <returns>The number of variables removed</returns>
		public async Task<int> ClearLibrarySet(string libraryName)
		{
			var lib = await _octopusRepository.ValidateLibrary(libraryName).ConfigureAwait(false);
			var set = await _octopusRepository.VariableSets.Get(lib.VariableSetId).ConfigureAwait(false);
			var count = set.Variables.Count;
			set.Variables.Clear();
			await _octopusRepository.VariableSets.Modify(set).ConfigureAwait(false);
			return count;
		}

		public async Task UpdateVars(List<SecretVariable> vars, string libraryName, IEnumerable<string> environments, IEnumerable<string> roles, bool apply)
		{
			var lib = await _octopusRepository.ValidateLibrary(libraryName).ConfigureAwait(false);
			var set = await _octopusRepository.VariableSets.Get(lib.VariableSetId).ConfigureAwait(false);

			var scope = new ScopeSpecification();
			foreach(var environment in environments)
			{
				var enviro = await _octopusRepository.ValidateEnvironment(environment).ConfigureAwait(false);
				if(scope.ContainsKey(ScopeField.Environment))
				{
					scope[ScopeField.Environment].Add(enviro.Id);
				}
				else
				{
					scope.Add(ScopeField.Environment, new ScopeValue(enviro.Id));
				}
			}

			await _octopusRepository.ValidateRoles(roles).ConfigureAwait(false);
			scope.Add(ScopeField.Role, new ScopeValue(roles));
			foreach (var variable in vars)
			{
				set.AddOrUpdateVariableValue(variable.Name, variable.Value, scope, variable.IsSecret);
			}
			if(apply)
			{
				await _octopusRepository.VariableSets.Modify(set).ConfigureAwait(false);
			}
		}
	}
}
