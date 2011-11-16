using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EntityFramework.Reflection
{
  /// <summary>
  /// An accessor class for <see cref="MethodInfo"/>.
  /// </summary>
  [DebuggerDisplay("Name: {Name}")]
  public class MethodAccessor : IMethodAccessor
  {
    private readonly MethodInfo _methodInfo;
    private readonly string _name;
    private readonly Lazy<LateBoundMethod> _lateBoundMethod;

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodAccessor"/> class.
    /// </summary>
    /// <param name="methodInfo">The method info.</param>
    public MethodAccessor(MethodInfo methodInfo)
    {
      _methodInfo = methodInfo;
      _name = methodInfo.Name;
      _lateBoundMethod = new Lazy<LateBoundMethod>(() => DelegateFactory.CreateMethod(_methodInfo));
    }

    /// <summary>
    /// Gets the method info.
    /// </summary>
    public MethodInfo MethodInfo
    {
      get { return _methodInfo; }
    }

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    /// <value>
    /// The name of the member.
    /// </value>
    public string Name
    {
      get { return _name; }
    }

    /// <summary>
    /// Invokes the method on the specified instance.
    /// </summary>
    /// <param name="instance">The object on which to invoke the method. If a method is static, this argument is ignored.</param>
    /// <param name="arguments">An argument list for the invoked method.</param>
    /// <returns>
    /// An object containing the return value of the invoked method.
    /// </returns>
    public object Invoke(object instance, params object[] arguments)
    {
      return _lateBoundMethod.Value.Invoke(instance, arguments);
    }

    internal static int GetKey(string name, IEnumerable<Type> parameterTypes)
    {
      unchecked
      {
        int result = (name != null ? name.GetHashCode() : 0);
        result = parameterTypes.Aggregate(result,
          (r, p) => (r * 397) ^ (p != null ? p.GetHashCode() : 0));

        return result;
      }
    }
  }
}
