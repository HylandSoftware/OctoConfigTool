using System;
using System.Linq;
using AutoFixture.Xunit2;
using Cake.Core.Diagnostics;
using Moq;
using OctoConfig.Core.DependencySetup;
using OctoConfig.Tests.TestFixture;
using Xunit;

namespace OctoConfig.Tests
{
	public static class CakeLoggerAbstractionTests
	{
		public class Verbose
		{
			[Theory]
			[AppAutoData]
			public void VerboseLoggerCalledIfPresent([Frozen] Mock<Serilog.ILogger> mockLogger, [Frozen] Mock<ICakeLog> mockCake,
				CakeLoggerAbstraction sut, string message)
			{
				sut.Verbose(message);
				mockLogger.Verify(m => m.Verbose(It.Is<string>(s => s == message)), Times.Once);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Verbose),
					It.Is<LogLevel>(l => l == LogLevel.Verbose),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullSerilogDoesntThrow(Mock<ICakeLog> mockCake, string message)
			{
				var sut = new CakeLoggerAbstraction(mockCake.Object, null);
				sut.Verbose(message);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Verbose),
					It.Is<LogLevel>(l => l == LogLevel.Verbose),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullCakeDoesntThrow(Mock<Serilog.ILogger> mockLogger, string message)
			{
				var sut = new CakeLoggerAbstraction(null, mockLogger.Object);
				sut.Verbose(message);
				mockLogger.Verify(m => m.Verbose(It.Is<string>(s => s == message)), Times.Once);
			}
		}

		public class Debug
		{
			[Theory]
			[AppAutoData]
			public void DebugLoggerCalledIfPresent([Frozen] Mock<Serilog.ILogger> mockLogger, [Frozen] Mock<ICakeLog> mockCake,
				CakeLoggerAbstraction sut, string message)
			{
				sut.Debug(message);
				mockLogger.Verify(m => m.Debug(It.Is<string>(s => s == message)), Times.Once);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Diagnostic),
					It.Is<LogLevel>(l => l == LogLevel.Debug),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullSerilogDoesntThrow(Mock<ICakeLog> mockCake, string message)
			{
				var sut = new CakeLoggerAbstraction(mockCake.Object, null);
				sut.Debug(message);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Diagnostic),
					It.Is<LogLevel>(l => l == LogLevel.Debug),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullCakeDoesntThrow(Mock<Serilog.ILogger> mockLogger, string message)
			{
				var sut = new CakeLoggerAbstraction(null, mockLogger.Object);
				sut.Debug(message);
				mockLogger.Verify(m => m.Debug(It.Is<string>(s => s == message)), Times.Once);
			}
		}

		public class Information
		{
			[Theory]
			[AppAutoData]
			public void InformationLoggerCalledIfPresent([Frozen] Mock<Serilog.ILogger> mockLogger, [Frozen] Mock<ICakeLog> mockCake,
				CakeLoggerAbstraction sut, string message)
			{
				sut.Information(message);
				mockLogger.Verify(m => m.Information(It.Is<string>(s => s == message)), Times.Once);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Normal),
					It.Is<LogLevel>(l => l == LogLevel.Information),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullSerilogDoesntThrow(Mock<ICakeLog> mockCake, string message)
			{
				var sut = new CakeLoggerAbstraction(mockCake.Object, null);
				sut.Information(message);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Normal),
					It.Is<LogLevel>(l => l == LogLevel.Information),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullCakeDoesntThrow(Mock<Serilog.ILogger> mockLogger, string message)
			{
				var sut = new CakeLoggerAbstraction(null, mockLogger.Object);
				sut.Information(message);
				mockLogger.Verify(m => m.Information(It.Is<string>(s => s == message)), Times.Once);
			}
		}

		public class Warning
		{
			[Theory]
			[AppAutoData]
			public void WarningLoggerCalledIfPresent([Frozen] Mock<Serilog.ILogger> mockLogger, [Frozen] Mock<ICakeLog> mockCake,
				CakeLoggerAbstraction sut, string message)
			{
				sut.Warning(message);
				mockLogger.Verify(m => m.Warning(It.Is<string>(s => s == message)), Times.Once);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Minimal),
					It.Is<LogLevel>(l => l == LogLevel.Warning),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullSerilogDoesntThrow(Mock<ICakeLog> mockCake, string message)
			{
				var sut = new CakeLoggerAbstraction(mockCake.Object, null);
				sut.Error(message);
				sut.Warning(message);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Minimal),
					It.Is<LogLevel>(l => l == LogLevel.Warning),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullCakeDoesntThrow(Mock<Serilog.ILogger> mockLogger, string message)
			{
				var sut = new CakeLoggerAbstraction(null, mockLogger.Object);
				sut.Warning(message);
				mockLogger.Verify(m => m.Warning(It.Is<string>(s => s == message)), Times.Once);
			}
		}

		public class Error
		{
			[Theory]
			[AppAutoData]
			public void ErrorLoggerCalledIfPresent([Frozen] Mock<Serilog.ILogger> mockLogger, [Frozen] Mock<ICakeLog> mockCake,
				CakeLoggerAbstraction sut, string message)
			{
				sut.Error(message);
				mockLogger.Verify(m => m.Error(It.Is<string>(s => s == message)), Times.Once);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Quiet),
					It.Is<LogLevel>(l => l == LogLevel.Error),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullSerilogDoesntThrow(Mock<ICakeLog> mockCake, string message)
			{
				var sut = new CakeLoggerAbstraction(mockCake.Object, null);
				sut.Error(message);
				mockCake.Verify(m => m.Write(
					It.Is<Verbosity>(v => v == Verbosity.Quiet),
					It.Is<LogLevel>(l => l == LogLevel.Error),
					It.Is<string>(s => s == "{0}"),
					It.Is<object[]>(o => o.Contains(message))), Times.Once);
			}

			[Theory]
			[AppAutoData]
			public void NullCakeDoesntThrow(Mock<Serilog.ILogger> mockLogger, string message)
			{
				var sut = new CakeLoggerAbstraction(null, mockLogger.Object);
				sut.Error(message);
				mockLogger.Verify(m => m.Error(It.Is<string>(s => s == message)), Times.Once);
			}
		}
	}
}
