using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EntityFramework.Reflection;

namespace EntityFramework.Test.Reflection
{
  [TestClass]
  public class ReflectionHelperTest
  {
    [TestMethod]
    public virtual void WhenExtractingNameFromAValidPropertyExpression_ThenPropertyNameReturned()
    {
      var propertyName = ReflectionHelper.ExtractPropertyName(() => this.InstanceProperty);
      Assert.AreEqual("InstanceProperty", propertyName);
    }

    [TestMethod]
    public void WhenExpressionRepresentsAStaticProperty_ThenExceptionThrown()
    {
      ExceptionAssert.Throws<ArgumentException>(() => ReflectionHelper.ExtractPropertyName(() => StaticProperty));
    }

    [TestMethod]
    public void WhenExpressionIsNull_ThenAnExceptionIsThrown()
    {
      ExceptionAssert.Throws<ArgumentNullException>(() => ReflectionHelper.ExtractPropertyName<int>(null));
    }

    [TestMethod]
    public void WhenExpressionRepresentsANonMemberAccessExpression_ThenAnExceptionIsThrown()
    {
      ExceptionAssert.Throws<ArgumentException>(
          () => ReflectionHelper.ExtractPropertyName(() => this.GetHashCode())
          );

    }

    [TestMethod]
    public void WhenExpressionRepresentsANonPropertyMemberAccessExpression_ThenAnExceptionIsThrown()
    {
      ExceptionAssert.Throws<ArgumentException>(() => ReflectionHelper.ExtractPropertyName(() => this.InstanceField));
    }

    public static int StaticProperty { get; set; }
    public int InstanceProperty { get; set; }
    public int InstanceField;
    public static int SetOnlyStaticProperty { set { } }

  }
}
