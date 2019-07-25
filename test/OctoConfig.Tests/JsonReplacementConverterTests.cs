using System.Linq;
using FluentAssertions;
using Moq;
using OctoConfig.Core;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using OctoConfig.Core.DependencySetup;
using Xunit;

namespace OctoConfig.Tests
{
	public class JsonReplacementConverterTests
	{
		[Fact]
		public void BasicJsonTest()
		{
			var js = new VariableConverter(new JsonReplacementArgs() { MergeArrays = false, VariableType = VariableType.JsonConversion }, Mock.Of<ILogger>());
			var vars = js.Convert("{\"a\":\"b\"}");
			vars.Single().Name.Should().Be("a");
		}

		[Theory]
		[InlineData("{\"a\":\"b\"}", "a", "b")]
		[InlineData("{\"a\": { \"b\" : \"c\" } }", "a:b", "c")]
		[InlineData("{\"a\": { \"b\" : [ \"c\" ] } }", "a:b:0", "c")]
		public void ColonSeparatorTest(string json, string expectedName, string expectedValue)
		{
			var js = new VariableConverter(new JsonReplacementArgs() { MergeArrays = false, VariableType = VariableType.JsonConversion }, Mock.Of<ILogger>());
			var vars = js.Convert(json);
			vars.Single().Name.Should().Be(expectedName);
			vars.Single().Value.Should().Be(expectedValue);
		}
	}
}
