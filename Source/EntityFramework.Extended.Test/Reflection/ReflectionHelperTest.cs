using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using EntityFramework.Reflection;

namespace EntityFramework.Test.Reflection
{
  
  public class ReflectionHelperTest
  {
    [Fact]
    public virtual void WhenExtractingNameFromAValidPropertyExpression_ThenPropertyNameReturned()
    {
      var propertyName = ReflectionHelper.ExtractPropertyName(() => this.InstanceProperty);
      Assert.Equal("InstanceProperty", propertyName);
    }

    [Fact]
    public void WhenExpressionRepresentsAStaticProperty_ThenExceptionThrown()
    {
      ExceptionAssert.Throws<ArgumentException>(() => ReflectionHelper.ExtractPropertyName(() => StaticProperty));
    }

    [Fact]
    public void WhenExpressionIsNull_ThenAnExceptionIsThrown()
    {
      ExceptionAssert.Throws<ArgumentNullException>(() => ReflectionHelper.ExtractPropertyName<int>(null));
    }

    [Fact]
    public void WhenExpressionRepresentsANonMemberAccessExpression_ThenAnExceptionIsThrown()
    {
      ExceptionAssert.Throws<ArgumentException>(
          () => ReflectionHelper.ExtractPropertyName(() => this.GetHashCode())
          );

    }

    [Fact]
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
