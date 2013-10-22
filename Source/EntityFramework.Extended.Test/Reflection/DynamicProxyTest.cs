using System;
using EntityFramework.Reflection;
using NUnit.Framework;

namespace EntityFramework.Test.Reflection
{
  [TestFixture]
  public class DynamicProxyTest
  {
    [Test]
    public void TestMethod()
    {
      var t = new TestWrapper();
      dynamic w = new DynamicProxy(t);

      string r = w.Name;
      Assert.AreEqual("Test", r);

      r = w.Internal;
      Assert.AreEqual("InternalTest", r);

      r = w.Private;
      Assert.AreEqual("PrivateTest", r);

      r = w.EchoPublic("Tester");
      Assert.AreEqual("Public: Tester", r);

      r = w.EchoPrivate("Tester");
      Assert.AreEqual("Private: Tester", r);

      r = w.EchoInternal("Tester");
      Assert.AreEqual("Internal: Tester", r);

      r = w.EchoInternal(null);
      Assert.AreEqual("Internal: ", r);

    }

    [Test]
    public void TestMethodNulls()
    {
      var t = new TestWrapper();
      dynamic w = new DynamicProxy(t);

      string r = w.EchoInternal(null);
      Assert.AreEqual("Internal: ", r);

      r = w.EchoInternal2(1);
      Assert.AreEqual("Int: 1", r);
      r = w.EchoInternal2("Test");
      Assert.AreEqual("String: Test", r);

      r = w.EchoInternal2("Test", "Testing");
      Assert.AreEqual("Name: Test Value: Testing", r);

      r = w.EchoInternal2("Test", null);
      Assert.AreEqual("Name: Test Value: ", r);


      r = w.EchoInternal3("Test", "Testing", null);
      Assert.AreEqual("Name: Test Value: Testing Value2: ", r);

      r = w.EchoInternal3("Test", 1, null);
      Assert.AreEqual("Name: Test Value: 1 Value2: ", r);

    }
  }

  public class TestWrapper
  {
    public TestWrapper()
    {
      Name = "Test";
      Internal = "InternalTest";
      Private = "PrivateTest";
    }

    public string Name { get; set; }
    internal string Internal { get; set; }
    private string Private { get; set; }

    public string EchoPublic(string text)
    {
      return "Public: " + text;
    }
    private string EchoPrivate(string text)
    {
      return "Private: " + text;
    }
    internal string EchoInternal(string text)
    {
      return "Internal: " + text;
    }

    internal string EchoInternal2(int text)
    {
      return "Int: " + text;
    }

    internal string EchoInternal2(string text)
    {
      return "String: " + text;
    }

    internal string EchoInternal2(string name, string value)
    {
      return string.Format("Name: {0} Value: {1}", name, value);
    }
    internal string EchoInternal2(string name, int value)
    {
      return string.Format("Name: {0} Value: {1}", name, value);
    }


    internal string EchoInternal3(string name, string value, string value2 )
    {
      return string.Format("Name: {0} Value: {1} Value2: {2}", name, value, value2);
    }
    internal string EchoInternal3(string name, int value, int? value2)
    {
      return string.Format("Name: {0} Value: {1} Value2: {2}", name, value, value2);
    }

  }
}
