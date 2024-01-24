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
}
