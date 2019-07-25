using System.Linq;
using FluentAssertions;
using Moq;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using OctoConfig.Core.DependencySetup;
using Xunit;

namespace OctoConfig.Tests
{
	public class EnvironmentVariableConverterTests
	{
		[Fact]
		public void BasicEnviroTest()
		{
			var args = new FileArgsBase() { Prefix = "", MergeArrays = false, VariableType = VariableType.Environment };
			var js = new VariableConverter(args, Mock.Of<ILogger>());
			var vars = js.Convert("{\"a\":\"b\"}");
			vars.Single().Name.Should().Be("a");
		}

		[Theory]
		[InlineData("{\"a\":\"b\"}", "a", "b")]
		[InlineData("{\"a\": { \"b\" : \"c\" } }", "a__b", "c")]
		[InlineData("{\"a\": { \"b\" : [ \"c\" ] } }", "a__b__0", "c")]
		public void UnderscoreSeparatorTest(string json, string expectedName, string expectedValue)
		{
			var args = new FileArgsBase() { Prefix = "", MergeArrays = false, VariableType = VariableType.Environment };
			var js = new VariableConverter(args, Mock.Of<ILogger>());
			var vars = js.Convert(json);
			vars.Single().Name.Should().Be(expectedName);
			vars.Single().Value.Should().Be(expectedValue);
		}

		[Theory]
		[InlineData("{\"a\":\"b\"}", "test_", "test_a")]
		[InlineData("{\"a\":\"b\"}", "test", "testa")]
		[InlineData("{\"a\": { \"b\" : \"c\" } }", "test_", "test_a__b")]
		public void PrefixUnderscoreSeparatorTest(string json, string prefix, string expectedName)
		{
			var args = new FileArgsBase() { Prefix = prefix, MergeArrays = false, VariableType = VariableType.Environment };
			var js = new VariableConverter(args, Mock.Of<ILogger>());
			var vars = js.Convert(json);
			vars.Single().Name.Should().Be(expectedName);
		}

		[Theory]
		[InlineData("{\"a\": [ \"b\", \"c\" ] }", "test", "testa")]
		public void ArrayFlattenTest(string json, string prefix, string expectedName)
		{
			var args = new FileArgsBase() { Prefix = prefix, MergeArrays = true };
			var js = new VariableConverter(args, Mock.Of<ILogger>());
			var vars = js.Convert(json);
			vars.Single().Name.Should().Be(expectedName);
		}

		[Theory]
		[InlineData("test", "{\"a\":\"b\", \"c\":\"d\"}")]
		[InlineData("test2", "{ \"list\": [ \"a\", \"b\", \"c\" ] }")]
		public void PrefixTest(string prefix, string json)
		{
			var js = new VariableConverter(new FileArgsBase() { Prefix = prefix, MergeArrays = false }, Mock.Of<ILogger>());
			var vars = js.Convert(json);
			foreach (var expected in vars)
			{
				expected.Name.StartsWith(prefix).Should().Be(true, $"Expected variable name to start with {prefix} but was {expected.Name}");
			}
		}

		[Theory]
		[InlineData("test", "{ \"list\": [ ] }")]
		[InlineData("test2", "{ \"list\": [ \"a\" ] }")]
		[InlineData("test2", "{ \"list\": [ \"a\", \"b\", \"c\" ] }")]
		public void FlattenArrayPrefixTest(string prefix, string json)
		{
			var js = new VariableConverter(new FileArgsBase() { Prefix = prefix, MergeArrays = true }, Mock.Of<ILogger>());
			var vars = js.Convert(json);
			foreach (var expected in vars)
			{
				expected.Name.StartsWith(prefix).Should().Be(true, $"Expected variable name to start with {prefix} but was {expected.Name}");
			}
		}
	}
}
