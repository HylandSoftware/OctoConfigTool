using System;
using System.Threading.Tasks;
using Cake.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.DependencySetup;
using OctoConfig.Tests.TestFixture;
using Xunit;

namespace OctoConfig.Tests
{
	public class DependencyConfigTests
	{
		public class UnkonwnArgs : ArgsBase { }

		[Theory, AppAutoData]
		public async Task CakeLoggerUsedWhenPresent(Mock<ICakeContext> mockCake, Mock<IServiceCollection> mockColl, ClearVariableSetArgs args)
		{
			await DependencyConfig.Setup(args, mockCake.Object, mockColl.Object, false).ConfigureAwait(false);
			mockCake.Verify(m => m.Log, Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ILogger) && ((CakeLoggerAbstraction) s.ImplementationInstance)._cakeLogger != null)), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task CakeLoggerNotUsedWhenMissing(Mock<IServiceCollection> mockColl, ClearVariableSetArgs args)
		{
			await DependencyConfig.Setup(args, null, mockColl.Object, false).ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ILogger) && ((CakeLoggerAbstraction) s.ImplementationInstance)._cakeLogger == null)), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task SerilogUsedWhenCakeMissing(Mock<IServiceCollection> mockColl, ClearVariableSetArgs args)
		{
			await DependencyConfig.Setup(args, null, mockColl.Object, false).ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ILogger) && ((CakeLoggerAbstraction) s.ImplementationInstance)._serilogLogger != null)), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task ClearVariableSetArgsAddedAndSubTypes(Mock<IServiceCollection> mockColl, ClearVariableSetArgs args)
		{
			await DependencyConfig.Setup(args, null, mockColl.Object, false).ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ClearVariableSetArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(LibraryTargetArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(FileArgsBase))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ArgsBase))), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task ValidateArgsAddedAndSubTypes(Mock<IServiceCollection> mockColl, ValidateArgs args)
		{
			await DependencyConfig.Setup(args, null, mockColl.Object, false).ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ValidateArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(LibraryTargetArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(FileArgsBase))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ArgsBase))), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task ClearProjectArgsAddedAndSubTypes(Mock<IServiceCollection> mockColl, ClearProjectArgs args)
		{
			await DependencyConfig.Setup(args, null, mockColl.Object, false).ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ClearProjectArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ArgsBase))), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task ClearTenantArgsAddedAndSubTypes(Mock<IServiceCollection> mockColl, ClearTenantArgs args)
		{
			await DependencyConfig.Setup(args, null, mockColl.Object, false).ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ClearTenantArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ArgsBase))), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task ValidateTenantArgsAddedAndSubTypes(Mock<IServiceCollection> mockColl, ValidateTenantArgs args)
		{
			await DependencyConfig.Setup(args, null, mockColl.Object, false).ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ValidateTenantArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(TenantTargetArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(IProjectArgsBase))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(FileArgsBase))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ArgsBase))), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task TenantTargetArgsAddedAndSubTypes(Mock<IServiceCollection> mockColl, UploadTenantArgs args)
		{
			await DependencyConfig.Setup(args, null, mockColl.Object, false).ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(UploadTenantArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(TenantTargetArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(IProjectArgsBase))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(FileArgsBase))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ArgsBase))), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task LibraryTargetArgsAddedAndThrows(Mock<IServiceCollection> mockColl, LibraryTargetArgs args)
		{
			Func<Task> sut = () => DependencyConfig.Setup(args, null, mockColl.Object, false);
			await sut.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(LibraryTargetArgs))), Times.Once);
			mockColl.Verify(m => m.Add(It.Is<ServiceDescriptor>(s => s.ServiceType == typeof(ArgsBase))), Times.Once);
		}

		[Theory, AppAutoData]
		public async Task UnknownTypeThrows(Mock<IServiceCollection> mockColl, UnkonwnArgs args)
		{
			Func<Task> sut = () => DependencyConfig.Setup(args, null, mockColl.Object, false);
			await sut.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
		}

		[Theory]
		[InlineAppAutoData("")]
		[InlineAppAutoData(null)]
		public async Task NullOrEmptyOctoUriThrows(string uri, Mock<IServiceCollection> mockColl, ArgsBase args)
		{
			args.OctoUri = uri;
			Func<Task> sut = () => DependencyConfig.Setup(args, null, mockColl.Object, false);
			await sut.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
		}

		[Theory]
		[InlineAppAutoData("")]
		[InlineAppAutoData(null)]
		public async Task NullOrEmptyApiKeyThrows(string apiKey, Mock<IServiceCollection> mockColl, ArgsBase args)
		{
			args.ApiKey = apiKey;
			Func<Task> sut = () => DependencyConfig.Setup(args, null, mockColl.Object, false);
			await sut.Should().ThrowAsync<ArgumentException>().ConfigureAwait(false);
		}
	}
}
