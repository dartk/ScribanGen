using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Sample;


[TestClass]
public class Tests {
    
    [TestMethod]
    public void ClassFromScribanTest() {
        Assert.AreEqual(0, ClassFromScriban.Value0);
        Assert.AreEqual(1, ClassFromScriban.Value1);
        Assert.AreEqual(2, ClassFromScriban.Value2);
        Assert.AreEqual(3, ClassFromScriban.Value3);
    }
    
}