using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.DependencySetup;

namespace OctoConfig.Core.Converter
{
	public class VariableConverter
	{
		internal string _separator;
		private readonly FileArgsBase _args;
		private const string _globVariableName = "ConcatEnvironmentVars";
		private readonly ILogger _logger;

		public VariableConverter(FileArgsBase args, ILogger logger)
		{
			_args = args ?? throw new ArgumentNullException(nameof(args));
			switch (args.VariableType)
			{
				case VariableType.Environment:
				case VariableType.EnvironmentGlob:
					_separator = "__";
					break;
				case VariableType.JsonConversion:
					_separator = ":";
					break;
				default:
					throw new ArgumentException($"Unknown enum vaule of {args.VariableType}", nameof(args.VariableType));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		private List<SecretVariable> flatten(string strJson)
		{
			var ret = new List<SecretVariable>();
			foreach (var obj in JsonConvert.DeserializeObject(strJson) as JObject)
			{
				if (obj.Value.HasValues)
				{
					ret.AddRange(explore(obj.Value));
				}
				else
				{
					ret.Add(new SecretVariable(obj.Key, stringify((JToken) obj.Value)));
				}
			}
			return ret;
		}

		private List<SecretVariable> explore(JToken obj)
		{
			var ret = new List<SecretVariable>();
			if (_args.MergeArrays && obj is Newtonsoft.Json.Linq.JArray arr)
			{
				var value = stringify(arr);
				ret.Add(new SecretVariable(obj.Path, value));
			}
			else if (obj.HasValues)
			{
				foreach (var child in obj.Children())
				{
					ret.AddRange(explore(child));
				}
			}
			else
			{
				var varName = obj.Path;
				ret.Add(new SecretVariable(varName, stringify(obj)));
			}
			return ret;
		}

		private string stringify(JToken obj)
		{
			if (obj is Newtonsoft.Json.Linq.JArray arr)
			{
				return arr.ToString();
			}
			else
			{
				return obj.ToObject<string>();
			}
		}

		/// <summary>
		/// Turns a json formatted string into list of secret variables
		/// All are marked NOT secret and just have name and value
		/// </summary>
		/// <param name="json">The json string to convert</param>
		/// <returns>A list of SecretVariable containing name and value</returns>
		public List<SecretVariable> Convert(string json)
		{
			var ret = flatten(json);

			ret.ForEach(
				x =>
				{
					// replace brackets and periods with the separator
					x.Name = x.Name.Replace("[", _separator)
						.Replace("]", _separator)
						.Replace(".", _separator)
					// netwonsoft stores array items like Parent[INDEX].Child
					// the above can create a double separator on '].', make sure to replace it with just a single one
						.Replace(_separator + _separator, _separator)
					// newtonsoft adds a trailing ] or ., make sure to trim it off
						.Trim(_separator[0]);
					x.Name = String.Concat(_args.Prefix, x.Name);
					if (String.IsNullOrEmpty(x.Value))
					{
						_logger.Warning($"Key '{x.Name}' has no matching value");
					}
				});
			if(_args.VariableType == VariableType.EnvironmentGlob)
			{
				return new List<SecretVariable>() { new SecretVariable(_globVariableName, String.Join(",", ret)) { IsSecret = true } };
			}
			return ret;
		}
	}
}
