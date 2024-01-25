using System;
using System.Linq;
using Sigurd.Terminal.Parsing;

namespace SigurdLib.Terminal.Tests.Parsing;

public class RawCommandSplitterTests
{
    private void AssertSplitTextEquals(string textToSplit, string[] splitText)
    {
        Assert.Equal(splitText, RawCommandSplitter.Split(textToSplit).ToArray());
    }

    private void AssertSplitTextThrows<TException>(string textToSplit) where TException : Exception
    {
        Assert.Throws<TException>(() => RawCommandSplitter.Split(textToSplit).ToArray());
    }

    #region Unquoted Tests

    [Fact]
    public void TestSimpleSplit()
    {
        AssertSplitTextEquals("foo bar baz", ["foo", "bar", "baz"]);
    }

    [Fact]
    public void TestNoSplit()
    {
        AssertSplitTextEquals("foobarbaz", ["foobarbaz"]);
    }

    #endregion

    #region Unquoted Escape Tests

    [Fact]
    public void TestDelimiterEscape()
    {
        AssertSplitTextEquals("foo\\ bar baz", ["foo bar", "baz"]);
    }

    [Fact]
    public void TestSingleQuoteEscape()
    {
        AssertSplitTextEquals("foo\\'bar baz", ["foo'bar", "baz"]);
    }

    [Fact]
    public void TestDoubleQuoteEscape()
    {
        AssertSplitTextEquals("foo\\\"bar baz", ["foo\"bar", "baz"]);
    }

    [Fact]
    public void TestEscapeEscape()
    {
        AssertSplitTextEquals("foo\\\\bar baz", ["foo\\bar", "baz"]);
    }

    [Fact]
    public void TestEndOfFileEscape()
    {
        AssertSplitTextThrows<RawCommandSplitter.RawCommandSyntaxException>(@"foo bar \");
    }

    #endregion

    #region Single Quoted Tests

    [Fact]
    public void TestSimpleSingleQuoted()
    {
        AssertSplitTextEquals("'foo' 'bar' 'baz'", ["foo", "bar", "baz"]);
    }

    [Fact]
    public void TestSingleQuotedConcatenation()
    {
        AssertSplitTextEquals("'foo''bar' baz", ["foobar", "baz"]);
    }

    [Fact]
    public void TestSingleQuotedDelimiter()
    {
        AssertSplitTextEquals("'foo bar'", ["foo bar"]);
    }

    [Fact]
    public void TestSingleQuotedEmptyString()
    {
        AssertSplitTextEquals("foo '' bar", ["foo", "", "bar"]);
    }

    [Fact]
    public void TestMismatchedSingleQuote()
    {
        AssertSplitTextThrows<RawCommandSplitter.RawCommandSyntaxException>("foo ' bar");
    }

    #endregion

    #region Double Quoted Tests

    [Fact]
    public void TestSimpleDoubleQuoted()
    {
        AssertSplitTextEquals("\"foo\" \"bar\" \"baz\"", ["foo", "bar", "baz"]);
    }

    [Fact]
    public void TestDoubleQuotedConcatenation()
    {
        AssertSplitTextEquals("\"foo\"\"bar\" baz", ["foobar", "baz"]);
    }

    [Fact]
    public void TestDoubleQuotedDelimiter()
    {
        AssertSplitTextEquals("\"foo bar\"", ["foo bar"]);
    }

    [Fact]
    public void TestDoubleQuotedEmptyString()
    {
        AssertSplitTextEquals("foo \"\" bar", ["foo", "", "bar"]);
    }

    [Fact]
    public void TestMismatchedDoubleQuote()
    {
        AssertSplitTextThrows<RawCommandSplitter.RawCommandSyntaxException>("foo \" bar");
    }

    #endregion

    #region Composite Tests

    [Fact]
    public void TestCompositeQuotedConcatenation()
    {
        AssertSplitTextEquals("'foo'\"bar\" baz", ["foobar", "baz"]);
    }

    #endregion
}
