using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Xunit;
using Xunit.Sdk;

namespace OctoConfig.Tests.TestFixture
{
	/// <summary>
	/// Works similar to the built in AutoDataAttribute but adds some
	/// app specific customizations to the fixture.
	/// </summary>
	[DataDiscoverer("AutoFixture.Xunit2.NoPreDiscoveryDataDiscoverer", "AutoFixture.Xunit2")]
	public class AppAutoDataAttribute : AutoDataAttribute
	{
		public static IFixture Create()
		{
			var fixture = new Fixture();
			fixture.Customize(new AutoMoqCustomization
			{
				ConfigureMembers = true
			});
			fixture.Customize(new FakeFileSystemCustomization());
			return fixture;
		}

		public AppAutoDataAttribute() : base(Create)
		{
		}
	}

	/// <summary>
	/// Works similarly to xUnit's InlineDataAttribute
	/// but allows you to add more parameters to the test method than what is defined in the attribute.
	/// Any parameters that cannot be resolved by the inline values in the attribute are
	/// resolved using AutoFixture with our app specific customizations.
	/// </summary>
	/// <example>
	/// <code>
	/// [Theory]
	/// [InlineAppAutoData("")]
	/// [InlineAppAutData((object) null)]
	/// public void ShouldThrowArgumentException(string email, UserService sut)
	/// {
	///		...
	/// }
	/// </code>
	/// </example>
	[DataDiscoverer("AutoFixture.Xunit2.NoPreDiscoveryDataDiscoverer", "AutoFixture.Xunit2")]
	public class InlineAppAutoDataAttribute : CompositeDataAttribute
	{
		public InlineAppAutoDataAttribute(params object[] values)
			: base(new InlineDataAttribute(values), new AppAutoDataAttribute())
		{
		}
	}
}
