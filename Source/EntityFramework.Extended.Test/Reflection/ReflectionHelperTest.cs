using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using EntityFramework.Reflection;

namespace EntityFramework.Test.Reflection
{
  [TestFixture]
  public class ReflectionHelperTest
  {
    [Test]
    public virtual void WhenExtractingNameFromAValidPropertyExpression_ThenPropertyNameReturned()
    {
      var propertyName = ReflectionHelper.ExtractPropertyName(() => this.InstanceProperty);
      Assert.AreEqual("InstanceProperty", propertyName);
    }

    [Test]
    public void WhenExpressionRepresentsAStaticProperty_ThenExceptionThrown()
    {
      ExceptionAssert.Throws<ArgumentException>(() => ReflectionHelper.ExtractPropertyName(() => StaticProperty));
    }

    [Test]
    public void WhenExpressionIsNull_ThenAnExceptionIsThrown()
    {
      ExceptionAssert.Throws<ArgumentNullException>(() => ReflectionHelper.ExtractPropertyName<int>(null));
    }

    [Test]
    public void WhenExpressionRepresentsANonMemberAccessExpression_ThenAnExceptionIsThrown()
    {
      ExceptionAssert.Throws<ArgumentException>(
          () => ReflectionHelper.ExtractPropertyName(() => this.GetHashCode())
          );

    }

    [Test]
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
