using System.Linq;
using FluentAssertions;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using Xunit;

namespace OctoConfig.Tests
{
	public class EnvironmentGlobTests
	{
		[Theory]
		[InlineData("{\"a\":\"b\"}", "ConcatEnvironmentVars")]
		[InlineData("{\"a\": { \"b\" : \"c\" } }", "ConcatEnvironmentVars")]
		[InlineData("{'a': { 'b' : [ 'c', 'd' ] } }", "ConcatEnvironmentVars")]
		public void GlobNameShouldBeSame(string json, string expectedName)
		{
			var args = new FileArgsBase() { Prefix = "", MergeArrays = false, VariableType = VariableType.EnvironmentGlob };
			var js = new VariableConverter(args);
			var vars = js.Convert(json);
			vars.Single().Name.Should().Be(expectedName);
		}

		[Theory]
		[InlineData("{\"a\":\"b\"}", "a=b")]
		[InlineData("{\"a\": { \"b\" : \"c\" } }", "a__b=c")]
		[InlineData("{'a': { 'b' : [ 'c', 'd' ] } }", "a__b__0=c,a__b__1=d")]
		public void GlobValuesShouldConcat(string json, string expectedValue)
		{
			var args = new FileArgsBase() { Prefix = "", MergeArrays = false, VariableType = VariableType.EnvironmentGlob };
			var js = new VariableConverter(args);
			var vars = js.Convert(json);
			vars.Single().Value.Should().Be(expectedValue);
		}

		[Theory]
		[InlineData("{\"a\":\"b\"}")]
		[InlineData("{\"a\": { \"b\" : \"c\" } }")]
		[InlineData("{'a': { 'b' : [ 'c', 'd' ] } }")]
		public void GlobShouldBeSecret(string json)
		{
			var args = new FileArgsBase() { Prefix = "", MergeArrays = false, VariableType = VariableType.EnvironmentGlob };
			var js = new VariableConverter(args);
			var vars = js.Convert(json);
			vars.Single().IsSecret.Should().BeTrue();
		}
	}
}
