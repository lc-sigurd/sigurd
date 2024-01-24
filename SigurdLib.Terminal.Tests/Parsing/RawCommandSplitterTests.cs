using System.Linq;
using Sigurd.Terminal.Parsing;

namespace SigurdLib.Terminal.Tests.Parsing;

public class RawCommandSplitterTests
{
    private void AssertSplitTextEquals(string textToSplit, string[] splitText)
    {
        Assert.Equal(splitText, RawCommandSplitter.Split(textToSplit).ToArray());
    }

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
}
