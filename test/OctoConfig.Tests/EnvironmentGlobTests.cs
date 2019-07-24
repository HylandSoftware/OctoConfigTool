using System.Linq;
using AutoFixture;
using FluentAssertions;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using OctoConfig.Tests.TestFixture;
using Xunit;

namespace OctoConfig.Tests
{
	public class EnvironmentGlobTests
	{
		[Theory]
		[InlineAppAutoData("{\"a\":\"b\"}", "ConcatEnvironmentVars")]
		[InlineAppAutoData("{\"a\": { \"b\" : \"c\" } }", "ConcatEnvironmentVars")]
		[InlineAppAutoData("{'a': { 'b' : [ 'c', 'd' ] } }", "ConcatEnvironmentVars")]
		public void GlobNameShouldBeSame(string json, string expectedName, IFixture fixture)
		{
			var args = fixture.Build<FileArgsBase>()
				.With(a => a.Prefix, "")
				.With(a => a.MergeArrays, false)
				.With(a => a.VariableType, VariableType.EnvironmentGlob)
				.Create();
			var js = new VariableConverter(args);
			var vars = js.Convert(json);
			vars.Single().Name.Should().Be(expectedName);
		}

		[Theory]
		[InlineAppAutoData("{\"a\":\"b\"}", "a=b")]
		[InlineAppAutoData("{\"a\": { \"b\" : \"c\" } }", "a__b=c")]
		[InlineAppAutoData("{'a': { 'b' : [ 'c', 'd' ] } }", "a__b__0=c,a__b__1=d")]
		public void GlobValuesShouldConcat(string json, string expectedValue, IFixture fixture)
		{
			var args = fixture.Build<FileArgsBase>()
				.With(a => a.Prefix, "")
				.With(a => a.MergeArrays, false)
				.With(a => a.VariableType, VariableType.EnvironmentGlob)
				.Create();
			var js = new VariableConverter(args);
			var vars = js.Convert(json);
			vars.Single().Value.Should().Be(expectedValue);
		}

		[Theory]
		[InlineAppAutoData("{\"a\":\"b\"}")]
		[InlineAppAutoData("{\"a\": { \"b\" : \"c\" } }")]
		[InlineAppAutoData("{'a': { 'b' : [ 'c', 'd' ] } }")]
		public void GlobShouldBeSecret(string json, IFixture fixture)
		{
			var args = fixture.Build<FileArgsBase>()
				.With(a => a.Prefix, "")
				.With(a => a.MergeArrays, false)
				.With(a => a.VariableType, VariableType.EnvironmentGlob)
				.Create();
			var js = new VariableConverter(args);
			var vars = js.Convert(json);
			vars.Single().IsSecret.Should().BeTrue();
		}
	}
}
