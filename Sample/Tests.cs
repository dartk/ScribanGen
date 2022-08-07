using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Sample;


[TestClass]
public class Tests {
    
    [TestMethod]
    public void ClassFromScribanTest() {
        Assert.AreEqual(42, GeneratedByScriban.GlobalValue);
    }
    
}