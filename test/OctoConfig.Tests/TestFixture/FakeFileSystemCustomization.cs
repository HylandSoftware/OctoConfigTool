using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AutoFixture;

namespace OctoConfig.Tests.TestFixture
{
	public class FakeFileSystemCustomization : ICustomization
	{
		public void Customize(IFixture fixture)
		{
			fixture.Register<MockFileSystem>(() => new MockFileSystem());
			fixture.Register<IFileSystem>(() => fixture.Create<MockFileSystem>());
		}
	}
}
