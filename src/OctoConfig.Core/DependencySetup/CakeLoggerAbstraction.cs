using Cake.Core;
using Cake.Core.Diagnostics;

namespace OctoConfig.Core.DependencySetup
{
	public interface ILogger
	{
		void Error(string message);
		void Warning(string message);
		void Information(string message);
		void Debug(string message);
		void Verbose(string message);
	}

	public class CakeLoggerAbstraction : ILogger
	{
		internal readonly ICakeLog _cakeLogger;
		internal readonly Serilog.ILogger _serilogLogger;

		public CakeLoggerAbstraction(ICakeLog cakeLogger, Serilog.ILogger logger)
		{
			_cakeLogger = cakeLogger;
			_serilogLogger = logger;
		}

		public void Debug(string message)
		{
			_cakeLogger?.Debug(message);
			_serilogLogger?.Debug(message);
		}

		public void Error(string message)
		{
			_cakeLogger?.Error(message);
			_serilogLogger?.Error(message);
		}

		public void Information(string message)
		{
			_cakeLogger?.Information(message);
			_serilogLogger?.Information(message);
		}

		public void Verbose(string message)
		{
			_cakeLogger?.Verbose(message);
			_serilogLogger?.Verbose(message);
		}

		public void Warning(string message)
		{
			_cakeLogger?.Warning(message);
			_serilogLogger?.Warning(message);
		}
	}
}
