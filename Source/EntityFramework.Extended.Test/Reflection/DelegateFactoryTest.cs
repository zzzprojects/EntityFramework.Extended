using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using EntityFramework.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EntityFramework.Test.Reflection
{
  [TestClass]
  public class DelegateFactoryTest
  {
    [TestMethod]
    public void CreateGetProperty()
    {
      TestClass.IsStatic = true;
      TestClass.StaticName = "CreateGetProperty";
      var testClass = new TestClass
      {
        Age = 21,
        FirstName = "John",
        Internal = "test",
        LastName = "Doe",
        PrivateGet = "Private Get"
      };

      var properties = typeof(TestClass).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
      Stopwatch watch = Stopwatch.StartNew();
      foreach (var propertyInfo in properties)
      {
        var d = DelegateFactory.CreateGet(propertyInfo);
        Assert.IsNotNull(d);

        var value = d(testClass);
        Assert.IsNotNull(value);

        Console.WriteLine("Property: {0} Value: {1}", propertyInfo.Name, value);
      }
      watch.Stop();
      Console.WriteLine("Time: {0}ms", watch.ElapsedMilliseconds);
    }

    [TestMethod]
    public void CreateGetPropertyValueType()
    {
      StructureClass.IsStatic = true;
      StructureClass.StaticName = "CreateGetProperty";
      var testClass = new StructureClass
      {
        Age = 21,
        FirstName = "John",
        Internal = "test",
        LastName = "Doe",
        PrivateGet = "Private Get"
      };
      testClass.Defaults();

      var properties = typeof(StructureClass).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
      Stopwatch watch = Stopwatch.StartNew();
      foreach (var propertyInfo in properties)
      {
        var d = DelegateFactory.CreateGet(propertyInfo);
        Assert.IsNotNull(d);

        var value = d(testClass);
        Assert.IsNotNull(value);

        Console.WriteLine("Property: {0} Value: {1}", propertyInfo.Name, value);
      }
      watch.Stop();
      Console.WriteLine("Time: {0}ms", watch.ElapsedMilliseconds);
    }
    
    [TestMethod]
    public void CreateGetField()
    {
      TestClass.IsStatic = true;
      TestClass.StaticName = "CreateGetProperty";
      var testClass = new TestClass
      {
        Age = 21,
        FirstName = "John",
        Internal = "test",
        LastName = "Doe",
        PrivateGet = "Private Get"
      };

      var fieldInfos = typeof(TestClass).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
      Stopwatch watch = Stopwatch.StartNew();
      foreach (var fieldInfo in fieldInfos)
      {
        var d = DelegateFactory.CreateGet(fieldInfo);
        Assert.IsNotNull(d);

        var value = d(testClass);
        Assert.IsNotNull(value);

        Console.WriteLine("Field: {0} Value: {1}", fieldInfo.Name, value);
      }
      watch.Stop();
      Console.WriteLine("Time: {0}ms", watch.ElapsedMilliseconds);

    }

    [TestMethod]
    public void CreateGetFieldStructure()
    {
      StructureClass.IsStatic = true;
      StructureClass.StaticName = "CreateGetProperty";
      var testClass = new StructureClass
      {
        Age = 21,
        FirstName = "John",
        Internal = "test",
        LastName = "Doe",
        PrivateGet = "Private Get"
      };
      testClass.Defaults();

      var fieldInfos = typeof(StructureClass).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
      Stopwatch watch = Stopwatch.StartNew();
      foreach (var fieldInfo in fieldInfos)
      {
        var d = DelegateFactory.CreateGet(fieldInfo);
        Assert.IsNotNull(d);

        var value = d(testClass);
        Assert.IsNotNull(value);

        Console.WriteLine("Field: {0} Value: {1}", fieldInfo.Name, value);
      }
      watch.Stop();
      Console.WriteLine("Time: {0}ms", watch.ElapsedMilliseconds);
    }


    [TestMethod]
    public void CreateSetProperty()
    {
      var testClass = new TestClass();
      var properties = typeof(TestClass).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
      Stopwatch watch = Stopwatch.StartNew();
      foreach (var propertyInfo in properties)
      {
        var d = DelegateFactory.CreateSet(propertyInfo);
        Assert.IsNotNull(d);

        object value = null;
        switch (propertyInfo.Name)
        {
          case "LastName":
            value = "Doe";
            break;
          case "Age":
            value = 21;
            break;
          case "PrivateSet":
            value = "Private Set";
            break;
          case "PrivateGet":
            value = "Private Get";
            break;
          case "Internal":
            value = "test";
            break;
          case "IsStatic":
            value = true;
            break;
          case "StaticName":
            value = "CreateSetProperty";
            break;
          case "FirstName":
            value = "John";
            break;
        }

        d(testClass, value);
        Console.WriteLine("Property: {0} Value: {1}", propertyInfo.Name, value);
      }
      watch.Stop();
      Console.WriteLine("Time: {0}ms", watch.ElapsedMilliseconds);

      Assert.AreEqual("Doe", testClass.LastName);
      Assert.AreEqual(21, testClass.Age);
      Assert.AreEqual("Private Set", testClass.PrivateSet);
      Assert.AreEqual("test", testClass.Internal);
      Assert.AreEqual(true, TestClass.IsStatic);
      Assert.AreEqual("CreateSetProperty", TestClass.StaticName);
      Assert.AreEqual("John", testClass.FirstName);
    }

    [TestMethod]
    public void CreateSetField()
    {
      TestClass.fieldStaticKey = "field test key";

      var testClass = new TestClass();
      var fieldInfos = typeof(TestClass).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
      Stopwatch watch = Stopwatch.StartNew();
      foreach (var fieldInfo in fieldInfos)
      {

        object value = null;
        switch (fieldInfo.Name)
        {
          case "fieldAge":
            value = 21;
            break;
          case "fieldIsStatic":
            value = true;
            break;
          case "fieldStaticKey":
            value = "Static";
            break;
          case "fieldName":
            value = "John Doe";
            break;
          default:
            continue;
        }

        var d = DelegateFactory.CreateSet(fieldInfo);
        Assert.IsNotNull(d);

        d(testClass, value);

        Console.WriteLine("Field: {0} Value: {1}", fieldInfo.Name, value);
      }
      watch.Stop();
      Console.WriteLine("Time: {0}ms", watch.ElapsedMilliseconds);

      Assert.IsNotNull(testClass);
    }


    [TestMethod]
    public void CreateConstructorClass()
    {
      Type type = typeof(TestClass);
      Stopwatch watch = Stopwatch.StartNew();
      LateBoundConstructor c = DelegateFactory.CreateConstructor(type);
      watch.Stop();
      Console.WriteLine("Time: {0}ms", watch.ElapsedMilliseconds);

      Assert.IsNotNull(c);

      object o = c.Invoke();
      Assert.IsNotNull(o);
      Assert.IsTrue(o.GetType() == type);
    }

    [TestMethod]
    public void CreateConstructorInternal()
    {
      Type type = typeof(InternalClass);
      Stopwatch watch = Stopwatch.StartNew();
      LateBoundConstructor c = DelegateFactory.CreateConstructor(type);
      watch.Stop();
      Console.WriteLine("Time: {0}ms", watch.ElapsedMilliseconds);

      Assert.IsNotNull(c);

      object o = c.Invoke();
      Assert.IsNotNull(o);
      Assert.IsTrue(o.GetType() == type);

    }

    [TestMethod]
    public void PublicMethods()
    {
      var test = new TestClass();
      Type t = typeof (TestClass);
      
      var m = t.GetMethod("BaseOpen");
      Assert.IsNotNull(m);
      var d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      var parameters = new object[] { "Test" };
      var r = d.Invoke(test, parameters);
      Assert.IsNull(r);

      m = t.GetMethod("PublicOpen1");
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test" };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);

      m = t.GetMethod("PublicStaticOpen1");
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test" };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);


      m = t.GetMethod("PublicOpen2");
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test", 2 };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);


      m = t.GetMethod("PublicOpenReturn");
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test", 2 };
      r = d.Invoke(test, parameters);
      Assert.IsNotNull(r);

#if !SILVERLIGHT
      m = t.GetMethod("PublicOpenOut");
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test", null };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);
      Assert.AreEqual("1234", parameters[1]);

      m = t.GetMethod("PublicOpenOutReturn");
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] {"test", null};
      r = d.Invoke(test, parameters);
      Assert.IsNotNull(r);
      Assert.AreEqual("1234", parameters[1]);


      m = t.GetMethod("PublicOpenRef");
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test", "" };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);
      Assert.AreEqual("1234", parameters[1]);


      m = t.GetMethod("PublicOpenRefReturn");
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test", "" };
      r = d.Invoke(test, parameters);
      Assert.IsNotNull(r);
      Assert.AreEqual("1234", parameters[1]);
#endif

      m = t.GetMethod("PublicOpenGeneric");
      Assert.IsNotNull(m);
      m = m.MakeGenericMethod(typeof (string));
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test" };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);

      m = t.GetMethod("PublicOpenReturnGeneric");
      Assert.IsNotNull(m);
      m = m.MakeGenericMethod(typeof(string));
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test", 1 };
      r = d.Invoke(test, parameters);
      Assert.IsNotNull(r);
      
    }

    [TestMethod]
    public void PrivateMethods()
    {
      BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

      var test = new TestClass();
      Type t = typeof(TestClass);

      var m = t.GetMethod("PrivateOpen1", flags);
      Assert.IsNotNull(m);
      var d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      var parameters = new object[] { "Test" };
      var r = d.Invoke(test, parameters);
      Assert.IsNull(r);

      m = t.GetMethod("PrivateStaticOpen1", flags);
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test" };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);


      m = t.GetMethod("PrivateOpen2", flags);
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test", 2 };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);


      m = t.GetMethod("PrivateOpenReturn", flags);
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test", 2 };
      r = d.Invoke(test, parameters);
      Assert.IsNotNull(r);

#if !SILVERLIGHT
      m = t.GetMethod("PrivateOpenOut", flags);
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "Test", null };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);
      Assert.AreEqual("1234", parameters[1]);


      m = t.GetMethod("PrivateOpenOutReturn", flags);
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test", null };
      r = d.Invoke(test, parameters);
      Assert.IsNotNull(r);
      Assert.AreEqual("1234", parameters[1]);


      m = t.GetMethod("PrivateOpenRef", flags);
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test", "" };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);
      Assert.AreEqual("1234", parameters[1]);


      m = t.GetMethod("PrivateOpenRefReturn", flags);
      Assert.IsNotNull(m);
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test", "" };
      r = d.Invoke(test, parameters);
      Assert.IsNotNull(r);
      Assert.AreEqual("1234", parameters[1]);
#endif

      m = t.GetMethod("PrivateOpenGeneric", flags);
      Assert.IsNotNull(m);
      m = m.MakeGenericMethod(typeof(string));
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test" };
      r = d.Invoke(test, parameters);
      Assert.IsNull(r);

      m = t.GetMethod("PrivateOpenReturnGeneric", flags);
      Assert.IsNotNull(m);
      m = m.MakeGenericMethod(typeof(string));
      d = DelegateFactory.CreateMethod(m);
      Assert.IsNotNull(d);
      parameters = new object[] { "test", 1 };
      r = d.Invoke(test, parameters);
      Assert.IsNotNull(r);

    }
  }

  public class TestClassBase
  {
    public string fieldName = "fieldName";
    public string FirstName { get; set; }

    public void BaseOpen(string name)
    {
      Console.WriteLine("Base Open: " + name);
    }

  }

  public class TestClass : TestClassBase
  {
    public TestClass()
    {
      PrivateSet = "Private Set";
    }

    private int fieldAge = 21;
    private static bool fieldIsStatic = true;
    public static string fieldStaticKey = "static field key";

    public string LastName { get; set; }
    public int Age { get; set; }

    public string PrivateSet { get; private set; }
    public string PrivateGet { private get; set; }

    internal string Internal { get; set; }

    public static bool IsStatic { get; set; }
    public static string StaticName { get; set; }

    #region private methods
    private void PrivateOpenGeneric<T>(T name)
    {
      Console.WriteLine("Private Open: " + name);
    }

    private T PrivateOpenReturnGeneric<T>(T name, int count)
    {
      Console.WriteLine("Private Open: {0}, Count: {1}", name, count);
      return name;

    }

    private void PrivateOpen1(string name)
    {
      Console.WriteLine("Private Open: " + name);
    }

    private void PrivateOpen2(string name, int count)
    {
      Console.WriteLine("Private Open: {0}, Count: {1}", name, count);
    }

    private string PrivateOpenReturn(string name, int count)
    {
      Console.WriteLine("Private Open: {0}, Count: {1}", name, count);
      return string.Format("Private Open: {0}, Count: {1}", name, count);
    }

    private void PrivateOpenOut(string name, out string count)
    {
      count = "1234";
      Console.WriteLine("Private Open: {0}, Count: {1}", name, count);
    }

    private string PrivateOpenOutReturn(string name, out string count)
    {
      count = "1234";

      Console.WriteLine("Private Open: {0}, Count: {1}", name, count);
      return string.Format("Private Open: {0}, Count: {1}", name, count);
    }

    private void PrivateOpenRef(string name, ref string count)
    {
      count = "1234";
      Console.WriteLine("Private Open: {0}, Count: {1}", name, count);
    }

    private string PrivateOpenRefReturn(string name, ref string count)
    {
      count = "1234";
      Console.WriteLine("Private Open: {0}, Count: {1}", name, count);
      return string.Format("Private Open: {0}, Count: {1}", name, count);
    }

    private static void PrivateStaticOpen1(string name)
    {
      Console.WriteLine("Private Open: " + name);
    }
    #endregion
    
    #region public methods
    public void PublicOpenGeneric<T>(T name)
    {
      Console.WriteLine("Public Open: " + name);
    }

    public T PublicOpenReturnGeneric<T>(T name, int count)
    {
      Console.WriteLine("Public Open: {0}, Count: {1}", name, count);
      return name;

    }

    public void PublicOpen1(string name)
    {
      Console.WriteLine("Public Open: " + name);
    }

    public void PublicOpen2(string name, int count)
    {
      Console.WriteLine("Public Open: {0}, Count: {1}", name, count);
    }

    public string PublicOpenReturn(string name, int count)
    {
      Console.WriteLine("Public Open: {0}, Count: {1}", name, count);
      return string.Format("Public Open: {0}, Count: {1}", name, count);
    }

    public void PublicOpenOut(string name, out string count)
    {
      count = "1234";
      Console.WriteLine("Public Open: {0}, Count: {1}", name, count);
    }

    public string PublicOpenOutReturn(string name, out string count)
    {
      count = "1234";

      Console.WriteLine("Public Open: {0}, Count: {1}", name, count);
      return string.Format("Public Open: {0}, Count: {1}", name, count);
    }

    public void PublicOpenRef(string name, ref string count)
    {
      count = "1234";
      Console.WriteLine("Public Open: {0}, Count: {1}", name, count);
    }

    public string PublicOpenRefReturn(string name, ref string count)
    {
      count = "1234";
      Console.WriteLine("Public Open: {0}, Count: {1}", name, count);
      return string.Format("Public Open: {0}, Count: {1}", name, count);
    }

    public static void PublicStaticOpen1(string name)
    {
      Console.WriteLine("Public Open: " + name);
    }
    #endregion
  }

  internal class InternalClass
  {
    private string fieldName;
    private int fieldAge;
    private static bool fieldIsStatic;
    private static string fieldStaticKey;

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }

    public string PrivateSet { get; private set; }
    public string PrivateGet { private get; set; }

    internal string Internal { get; set; }

    public static bool IsStatic { get; set; }
    public static string StaticName { get; set; }
  }

  public struct StructureClass
  {

    private string fieldName;
    private int fieldAge;
    private static bool fieldIsStatic = true;
    private static string fieldStaticKey = "static field key";

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }

    private string _privateSet;
    public string PrivateSet
    {
      get { return _privateSet ; }
      private set { _privateSet  = value; }
    }

    private string _privateGet;
    public string PrivateGet
    {
      private get { return _privateGet; }
      set { _privateGet = value; }
    }

    internal string Internal { get; set; }

    public static bool IsStatic { get; set; }
    public static string StaticName { get; set; }

    public void Defaults()
    {
      _privateGet = "Private Get";
      _privateSet = "Private Set";
      fieldAge = 21;
      fieldName = "test field name";
    }
  }
}
