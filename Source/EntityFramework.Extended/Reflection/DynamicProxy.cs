using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace EntityFramework.Reflection
{
  /// <summary>
  /// A class used to wrap an object as a <see cref="T:System.Dynamic.DynamicObject"/> allowing access to internal and private members.
  /// </summary>
  public class DynamicProxy : DynamicObject
  {
    private readonly object _wrapped;
    private readonly TypeAccessor _typeAccessor;

    private const BindingFlags _flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicProxy"/> class.
    /// </summary>
    /// <param name="wrapped">The wrapped object.</param>
    public DynamicProxy(object wrapped)
      : this(wrapped, true)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicProxy"/> class.
    /// </summary>
    /// <param name="wrapped">The wrapped object.</param>
    /// <param name="safeMode">if set to <c>true</c>, return null when name not found.</param>
    public DynamicProxy(object wrapped, bool safeMode)
    {
      if (wrapped == null)
        throw new ArgumentNullException("wrapped");

      _wrapped = wrapped;
      SafeMode = safeMode;

      Type type = _wrapped.GetType();
      _typeAccessor = TypeAccessor.GetAccessor(type);
    }

    /// <summary>
    /// Gets the object where access is wrapped from.
    /// </summary>
    public object Wrapped
    {
      get { return _wrapped; }
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to return null when name is not found.
    /// </summary>
    /// <value>
    ///   <c>true</c> to return null when name not found; otherwise, <c>false</c> to throw an exception.
    /// </value>
    public bool SafeMode { get; set; }

    /// <summary>
    /// Provides the implementation for operations that invoke a member. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as calling a method.
    /// </summary>
    /// <param name="binder">Provides information about the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleMethod". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
    /// <param name="args">The arguments that are passed to the object member during the invoke operation. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, args[0] is equal to 100.</param>
    /// <param name="result">The result of the member invocation.</param>
    /// <returns>
    /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
    /// </returns>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
      result = null;
      
      var types = args.Select(a => a == null ? typeof(object) : a.GetType()).ToArray();
      var method = _typeAccessor.FindMethod(binder.Name, types, _flags);

      if (method == null)
        return SafeMode;

      var value = method.Invoke(_wrapped, args);
      if (value == null)
        return true;

      result = new DynamicProxy(value, SafeMode);
      return true;
    }

    /// <summary>
    /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
    /// </summary>
    /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
    /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
    /// <returns>
    /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
    /// </returns>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      result = null;

      var memeber = _typeAccessor.Find(binder.Name, _flags);
      if (memeber == null)
        return SafeMode;

      if (!memeber.HasGetter)
        return SafeMode;

      var value = memeber.GetValue(_wrapped);
      if (value == null)
        return true;

      result = new DynamicProxy(value, SafeMode);      
      return true;
    }

    /// <summary>
    /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
    /// </summary>
    /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
    /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
    /// <returns>
    /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
    /// </returns>
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
      var memeber = _typeAccessor.Find(binder.Name, _flags);
      if (memeber == null)
        return false;

      if (!memeber.HasSetter)
        return false;

      memeber.SetValue(_wrapped, value);
      return true;
    }

    /// <summary>
    /// Provides implementation for type conversion operations. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations that convert an object from one type to another.
    /// </summary>
    /// <param name="binder">Provides information about the conversion operation. The binder.Type property provides the type to which the object must be converted. For example, for the statement (String)sampleObject in C# (CType(sampleObject, Type) in Visual Basic), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Type returns the <see cref="T:System.String"/> type. The binder.Explicit property provides information about the kind of conversion that occurs. It returns true for explicit conversion and false for implicit conversion.</param>
    /// <param name="result">The result of the type conversion operation.</param>
    /// <returns>
    /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
    /// </returns>
    public override bool TryConvert(ConvertBinder binder, out object result)
    {
      result = null;
      if (_wrapped == null || !binder.Type.IsAssignableFrom(_wrapped.GetType()))
        return false;

      result = _wrapped;
      return true;
    }
  }
}
