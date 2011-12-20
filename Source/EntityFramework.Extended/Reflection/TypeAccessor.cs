using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityFramework.Reflection
{
  /// <summary>
  /// A class holding all the accessors for a <see cref="Type"/>.
  /// </summary>
  public class TypeAccessor
  {
    private static readonly ConcurrentDictionary<Type, TypeAccessor> _typeCache = new ConcurrentDictionary<Type, TypeAccessor>();
    private readonly ConcurrentDictionary<string, IMemberAccessor> _memberCache = new ConcurrentDictionary<string, IMemberAccessor>();
    private readonly ConcurrentDictionary<int, IMethodAccessor> _methodCache = new ConcurrentDictionary<int, IMethodAccessor>();
    private readonly ConcurrentDictionary<int, IEnumerable<IMemberAccessor>> _propertyCache = new ConcurrentDictionary<int, IEnumerable<IMemberAccessor>>();

    private readonly Lazy<LateBoundConstructor> _lateBoundConstructor;
    private readonly Type _type;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeAccessor"/> class.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> this accessor is for.</param>
    public TypeAccessor(Type type)
    {
      _type = type;
      _lateBoundConstructor = new Lazy<LateBoundConstructor>(() => DelegateFactory.CreateConstructor(_type));
    }

    /// <summary>
    /// Gets the <see cref="Type"/> this accessor is for.
    /// </summary>
    /// <value>The <see cref="Type"/> this accessor is for.</value>
    public Type Type
    {
      get { return _type; }
    }

    /// <summary>
    /// Creates a new instance of accessors type.
    /// </summary>
    /// <returns>A new instance of accessors type.</returns>
    public object Create()
    {
      var constructor = _lateBoundConstructor.Value;
      if (constructor == null)
        throw new InvalidOperationException(string.Format("Could not find constructor for '{0}'.", Type.Name));

      return constructor.Invoke();
    }


    #region Method
    /// <summary>
    /// Searches for the public property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <returns>
    /// An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <c>null</c>.
    /// </returns>
    public IMethodAccessor FindMethod(string name)
    {
      return FindMethod(name, Type.EmptyTypes);
    }

    /// <summary>
    /// Searches for the public property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <param name="parameterTypes">The parameter argument types.</param>
    /// <returns>
    /// An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <c>null</c>.
    /// </returns>
    public IMethodAccessor FindMethod(string name, params Type[] parameterTypes)
    {
      return FindMethod(name, parameterTypes, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
    }

    /// <summary>
    /// Searches for the specified method, using the specified binding constraints.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <param name="parameterTypes">The parameter argument types.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <c>null</c>.
    /// </returns>
    public IMethodAccessor FindMethod(string name, Type[] parameterTypes, BindingFlags flags)
    {
      int key = MethodAccessor.GetKey(name, parameterTypes);
      return _methodCache.GetOrAdd(key, n => CreateMethodAccessor(name, parameterTypes, flags));
    }

    private IMethodAccessor CreateMethodAccessor(string name, Type[] parameters, BindingFlags flags)
    {
      var info = FindMethod(Type, name, parameters, flags);
      return info == null ? null : CreateAccessor(info);
    }

    private static MethodInfo FindMethod(Type type, string name, Type[] parameterTypes, BindingFlags flags)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      if (name == null)
        throw new ArgumentNullException("name");
      if (parameterTypes == null)
        parameterTypes = Type.EmptyTypes;

      //first try full match
      var methodInfo = type.GetMethod(name, flags, null, CallingConventions.Any, parameterTypes, null);
      if (methodInfo != null)
        return methodInfo;

      // next, get all that match by name
      var methodsByName = type.GetMethods(flags)
        .Where(m => m.Name == name)
        .ToList();

      if (methodsByName.Count == 0)
        return null;

      // if only one matches name, return it
      if (methodsByName.Count == 1)
        return methodsByName.FirstOrDefault();

      // next, get all methods that match param count
      var methodsByParamCount = methodsByName
        .Where(m => m.GetParameters().Length == parameterTypes.Length)
        .ToList();

      // if only one matches with same param count, return it
      if (methodsByParamCount.Count == 1)
        return methodsByParamCount.FirstOrDefault();

      // still no match, make best guess by greatest matching param types
      MethodInfo current = methodsByParamCount.FirstOrDefault();
      int matchCount = 0;

      foreach (var info in methodsByParamCount)
      {
        var paramTypes = info.GetParameters()
          .Select(p => p.ParameterType)
          .ToArray();

        // unsure which way IsAssignableFrom should be checked?
        int count = paramTypes
          .Where((t, i) => t.IsAssignableFrom(parameterTypes[i]))
          .Count();

        if (count <= matchCount)
          continue;

        current = info;
        matchCount = count;
      }

      return current;
    }

    private static IMethodAccessor CreateAccessor(MethodInfo methodInfo)
    {
      return methodInfo == null ? null : new MethodAccessor(methodInfo);
    }
    #endregion

    #region Find
    /// <summary>
    /// Searches for the public property or field with the specified name.
    /// </summary>
    /// <param name="name">The name of the property or field to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <c>null</c>.</returns>
    public IMemberAccessor Find(string name)
    {
      return Find(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Searches for the specified property or field, using the specified binding constraints.
    /// </summary>
    /// <param name="name">The name of the property or field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <c>null</c>.
    /// </returns>
    public IMemberAccessor Find(string name, BindingFlags flags)
    {
      return _memberCache.GetOrAdd(name, n => CreateAccessor(n, flags));
    }

    private IMemberAccessor CreateAccessor(string name, BindingFlags flags)
    {
      // first try property
      PropertyInfo property = FindProperty(Type, name, flags);
      if (property != null)
        return CreateAccessor(property);

      // next try field
      FieldInfo field = FindField(Type, name, flags);
      return field == null ? null : CreateAccessor(field);
    }
    #endregion

    #region Property
    /// <summary>
    /// Searches for the property using a property expression.
    /// </summary>
    /// <typeparam name="T">The object type containing the property specified in the expression.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyExpression"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the expression is:<br/>
    ///     Not a <see cref="MemberExpression"/><br/>
    ///     The <see cref="MemberExpression"/> does not represent a property.<br/>
    ///     Or, the property is static.
    /// </exception>
    public IMemberAccessor FindProperty<T>(Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression == null)
        throw new ArgumentNullException("propertyExpression");

      return FindProperty(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Searches for the property using a property expression.
    /// </summary>
    /// <typeparam name="TSource">The object type containing the property specified in the expression.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g. p =&gt; p.PropertyName)</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyExpression"/> is null.</exception>
    ///   
    /// <exception cref="ArgumentException">Thrown when the expression is:<br/>
    /// Not a <see cref="MemberExpression"/><br/>
    /// The <see cref="MemberExpression"/> does not represent a property.<br/>
    /// Or, the property is static.
    ///   </exception>
    public IMemberAccessor FindProperty<TSource, TValue>(Expression<Func<TSource, TValue>> propertyExpression)
    {
      if (propertyExpression == null)
        throw new ArgumentNullException("propertyExpression");

      return FindProperty(propertyExpression.Body as MemberExpression);
    }

    private IMemberAccessor FindProperty(MemberExpression memberExpression)
    {
      if (memberExpression == null)
      {
        throw new ArgumentException("The expression is not a member access expression.", "memberExpression");
      }

      var property = memberExpression.Member as PropertyInfo;
      if (property == null)
      {
        throw new ArgumentException("The member access expression does not access a property.", "memberExpression");
      }

      var getMethod = property.GetGetMethod(true);
      if (getMethod.IsStatic)
      {
        throw new ArgumentException("The referenced property is a static property.", "memberExpression");
      }

      var accessor = CreateAccessor(property);
      return accessor;
    }

    /// <summary>
    /// Searches for the public property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <returns>An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.</returns>
    public IMemberAccessor FindProperty(string name)
    {
      return FindProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Searches for the specified property, using the specified binding constraints.
    /// </summary>
    /// <param name="name">The name of the property to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.
    /// </returns>
    public IMemberAccessor FindProperty(string name, BindingFlags flags)
    {
      return _memberCache.GetOrAdd(name, n => CreatePropertyAccessor(n, flags));
    }

    /// <summary>
    /// Gets the <see cref="IMemberAccessor"/> for the specified <see cref="PropertyInfo"/>.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> to get the <see cref="IMemberAccessor"/> for.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property.
    /// </returns>
    public IMemberAccessor GetAccessor(PropertyInfo propertyInfo)
    {
      return _memberCache.GetOrAdd(propertyInfo.Name, n => CreateAccessor(propertyInfo));
    }

    /// <summary>
    /// Gets the property member accessors for the Type.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1"/> of <see cref="IMemberAccessor"/> instances for the Type.
    /// </returns>
    public IEnumerable<IMemberAccessor> GetProperties()
    {
      return GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
    }

    /// <summary>
    /// Gets the property member accessors for the Type using the specified flags.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1"/> of <see cref="IMemberAccessor"/> instances for the Type.
    /// </returns>
    public IEnumerable<IMemberAccessor> GetProperties(BindingFlags flags)
    {
      return _propertyCache.GetOrAdd((int)flags, k =>
      {
        var properties = Type.GetProperties(flags);
        return properties.Select(GetAccessor);
      });
    }

    private IMemberAccessor CreatePropertyAccessor(string name, BindingFlags flags)
    {
      var info = FindProperty(Type, name, flags);
      return info == null ? null : CreateAccessor(info);
    }

    private static PropertyInfo FindProperty(Type type, string name, BindingFlags flags)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      if (name == null)
        throw new ArgumentNullException("name");

      // first try GetProperty
      PropertyInfo property = type.GetProperty(name, flags);
      if (property != null)
        return property;

      // if not found, search while ignoring case
      return type.GetProperties(flags).FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static IMemberAccessor CreateAccessor(PropertyInfo propertyInfo)
    {
      return propertyInfo == null ? null : new PropertyAccessor(propertyInfo);
    }
    #endregion

    #region Field
    /// <summary>
    /// Searches for the specified field with the specified name.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <c>null</c>.
    /// </returns>
    public IMemberAccessor FindField(string name)
    {
      return FindField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    /// Searches for the specified field, using the specified binding constraints.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <c>null</c>.
    /// </returns>
    public IMemberAccessor FindField(string name, BindingFlags flags)
    {
      return _memberCache.GetOrAdd(name, n => CreateFieldAccessor(n, flags));
    }

    /// <summary>
    /// Gets the <see cref="IMemberAccessor"/> for the specified <see cref="FieldInfo"/>.
    /// </summary>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> to get the <see cref="IMemberAccessor"/> for.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the field.
    /// </returns>
    public IMemberAccessor GetAccessor(FieldInfo fieldInfo)
    {
      return _memberCache.GetOrAdd(fieldInfo.Name, n => CreateAccessor(fieldInfo));
    }

    private IMemberAccessor CreateFieldAccessor(string name, BindingFlags flags)
    {
      FieldInfo info = FindField(Type, name, flags);
      return info == null ? null : CreateAccessor(info);
    }

    private static FieldInfo FindField(Type type, string name, BindingFlags flags)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      if (name == null)
        throw new ArgumentNullException("name");

      // first try GetField
      var field = type.GetField(name, flags);
      if (field != null)
        return field;

      // if not found, search while ignoring case
      return type.GetFields(flags).FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static IMemberAccessor CreateAccessor(FieldInfo fieldInfo)
    {
      return fieldInfo == null ? null : new FieldAccessor(fieldInfo);
    }
    #endregion

    /// <summary>
    /// Gets the <see cref="TypeAccessor"/> for the specified Type.
    /// </summary>
    /// <typeparam name="T">The Type to get the accessor for.</typeparam>
    /// <returns></returns>
    public static TypeAccessor GetAccessor<T>()
    {
      return GetAccessor(typeof(T));
    }

    /// <summary>
    /// Gets the <see cref="TypeAccessor"/> for the specified Type.
    /// </summary>
    /// <param name="type">The Type to get the accessor for.</param>
    /// <returns></returns>
    public static TypeAccessor GetAccessor(Type type)
    {
      return _typeCache.GetOrAdd(type, t => new TypeAccessor(t));
    }
  }
}
