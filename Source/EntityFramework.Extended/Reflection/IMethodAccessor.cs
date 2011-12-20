using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntityFramework.Reflection
{
    /// <summary>
    /// An interface for method accessor
    /// </summary>
  public interface IMethodAccessor
  {
    /// <summary>
    /// Gets the method info.
    /// </summary>
    MethodInfo MethodInfo { get; }
    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    /// <value>The name of the member.</value>
    string Name { get; }
    /// <summary>
    /// Invokes the method on the specified instance.
    /// </summary>
    /// <param name="instance">The object on which to invoke the method. If a method is static, this argument is ignored.</param>
    /// <param name="arguments">An argument list for the invoked method.</param>
    /// <returns>An object containing the return value of the invoked method.</returns>
    object Invoke(object instance, params object[] arguments);
  }
}
