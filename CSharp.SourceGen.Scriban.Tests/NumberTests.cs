using Generated;


namespace CSharp.SourceGen.Scriban.Tests;


public class NumberTests
{
    [Fact]
    public void CheckGeneratedNumbers()
    {
        Assert.Equal(new Number(1, "one"), Number.One);
        Assert.Equal(new Number(2, "two"), Number.Two);
        Assert.Equal(new Number(3, "three"), Number.Three);
    }
}