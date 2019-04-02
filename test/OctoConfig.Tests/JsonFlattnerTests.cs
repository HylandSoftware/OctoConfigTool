using System;
using FluentAssertions;
using OctoConfig.Core.Arguments;
using OctoConfig.Core.Converter;
using Xunit;

namespace OctoConfig.Tests
{
	public class JsonFlattnerTests
	{
		[Theory]
		[InlineData("{\"a\":\"b\"}", 1)]
		[InlineData("{\"a\":\"b\", \"c\":\"d\"}", 2)]
		public void BasicJsonParsingTest(string json, int varCount)
		{
			var js = new VariableConverter(new FileArgsBase());
			var vars = js.Convert(json);
			vars.Count.Should().Be(varCount);
		}

		[Theory]
		[InlineData("{ \"list\": [ \"a\" ] }", 1)]
		[InlineData("{ \"list\": [ \"a\", \"b\", \"c\" ] }", 3)]
		[InlineData("{ \"list\": [ ] }", 1)]
		public void ListParsingTest(string json, int varCount)
		{
			var js = new VariableConverter(new FileArgsBase());
			var vars = js.Convert(json);
			vars.Count.Should().Be(varCount);
		}

		[Theory]
		[InlineData("{ \"list\": [ \"a\" ] }", 1)]
		[InlineData("{ \"list\": [ \"a\", \"b\", \"c\" ] }", 1)]
		[InlineData("{ \"list\": [ ] }", 1)]
		[InlineData("{'a': { 'b' : [ 'c', 'd' ] } }", 1)]
		[InlineData("{'a': { 'b' : [ 'c', 'd' ], 'e' : [ 'f' ] } }", 2)]
		public void FlattenArraysTest(string json, int varCount)
		{
			var js = new VariableConverter(new FileArgsBase() { MergeArrays = true });
			var vars = js.Convert(json);
			vars.Count.Should().Be(varCount);
		}

		[Theory]
		[InlineData("{ \"list\": [ \"a ] }")]
		[InlineData("{ \"list\": [ \"a\" \"b\", \"c\" ] }")]
		[InlineData("{ \"list\": { \"a\",, \"b\", \"c\" } }")]
		[InlineData("{ \"list\": [ }")]
		public void InvalidJsonThrowsReaderException(string json)
		{
			var js = new VariableConverter(new FileArgsBase() { MergeArrays = true });
			Action flattenAct = () => js.Convert(json);
			flattenAct.Should().Throw<Newtonsoft.Json.JsonReaderException>();
		}

		[Theory]
		[InlineData("{ \"single\": \"a\"")]
		public void InvalidJsonThrowsSerialException(string json)
		{
			var js = new VariableConverter(new FileArgsBase() { MergeArrays = true });
			Action flattenAct = () => js.Convert(json);
			flattenAct.Should().Throw<Newtonsoft.Json.JsonSerializationException>();
		}
	}
}
