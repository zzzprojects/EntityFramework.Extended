using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace EntityFramework.Reflection
{
  /// <summary>
  /// A class for late bound operations on a type.
  /// </summary>
  public static class LateBinder
  {
      /// <summary>
      /// Default Flags for pulic binding.
      /// </summary>
    public const BindingFlags DefaultPublicFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
    /// <summary>
    /// Default Flags for nonpublic binding.
    /// </summary>
    public const BindingFlags DefaultNonPublicFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

    /// <summary>
    /// Searches for the specified method with the specified name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the method in.</param>
    /// <param name="name">The name of the method to find.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>
    /// An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <c>null</c>.
    /// </returns>
    public static IMethodAccessor FindMethod(Type type, string name, params object[] arguments)
    {
      return FindMethod(type, name, DefaultPublicFlags, arguments);
    }

    /// <summary>
    /// Searches for the specified method, using the specified binding constraints.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the method in.</param>
    /// <param name="name">The name of the method to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <param name="arguments">The arguments.</param>
    /// <returns>
    /// An <see cref="IMethodAccessor"/> instance for the method if found; otherwise <c>null</c>.
    /// </returns>
    public static IMethodAccessor FindMethod(Type type, string name, BindingFlags flags, params object[] arguments)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      TypeAccessor typeAccessor = TypeAccessor.GetAccessor(type);

      Type[] types = arguments
        .Select(a => a == null ? typeof(object) : a.GetType())
        .ToArray();

      var methodAccessor = typeAccessor.FindMethod(name, types, flags);

      return methodAccessor;
    }

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
    public static IMemberAccessor FindProperty<T>(Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression == null)
        throw new ArgumentNullException("propertyExpression");

      TypeAccessor typeAccessor = TypeAccessor.GetAccessor(typeof(T));
      return typeAccessor.FindProperty<T>(propertyExpression);
    }

    /// <summary>
    /// Searches for the public property with the specified name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the property in.</param>
    /// <param name="name">The name of the property to find.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.
    /// </returns>
    public static IMemberAccessor FindProperty(Type type, string name)
    {
      return FindProperty(type, name, DefaultPublicFlags);
    }

    /// <summary>
    /// Searches for the specified property, using the specified binding constraints.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the property in.</param>
    /// <param name="name">The name of the property to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property if found; otherwise <c>null</c>.
    /// </returns>
    public static IMemberAccessor FindProperty(Type type, string name, BindingFlags flags)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      Type currentType = type;
      TypeAccessor typeAccessor;
      IMemberAccessor memberAccessor = null;

      // support nested property
      var parts = name.Split('.');
      foreach (var part in parts)
      {
        if (memberAccessor != null)
          currentType = memberAccessor.MemberType;

        typeAccessor = TypeAccessor.GetAccessor(currentType);
        memberAccessor = typeAccessor.FindProperty(part, flags);
      }

      return memberAccessor;
    }

    /// <summary>
    /// Searches for the field with the specified name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the field in.</param>
    /// <param name="name">The name of the field to find.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <c>null</c>.
    /// </returns>
    public static IMemberAccessor FindField(Type type, string name)
    {
      return FindField(type, name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    /// Searches for the field, using the specified binding constraints.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the field in.</param>
    /// <param name="name">The name of the field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the field if found; otherwise <c>null</c>.
    /// </returns>
    public static IMemberAccessor FindField(Type type, string name, BindingFlags flags)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      TypeAccessor typeAccessor = TypeAccessor.GetAccessor(type);
      IMemberAccessor memberAccessor = typeAccessor.FindField(name, flags);

      return memberAccessor;
    }

    /// <summary>
    /// Searches for the property or field with the specified name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the property or field in.</param>
    /// <param name="name">The name of the property or field to find.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <c>null</c>.
    /// </returns>
    public static IMemberAccessor Find(Type type, string name)
    {
      return Find(type, name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    /// <summary>
    /// Searches for the property or field, using the specified binding constraints.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to search for the property or field in.</param>
    /// <param name="name">The name of the property or field to find.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>
    /// An <see cref="IMemberAccessor"/> instance for the property or field if found; otherwise <c>null</c>.
    /// </returns>
    public static IMemberAccessor Find(Type type, string name, BindingFlags flags)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      TypeAccessor typeAccessor = TypeAccessor.GetAccessor(type);
      IMemberAccessor memberAccessor = typeAccessor.Find(name, flags);

      return memberAccessor;
    }

    /// <summary>
    /// Sets the property value with the specified name.
    /// </summary>
    /// <param name="target">The object whose property value will be set.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <remarks>This method supports nested property names. An exmample name would be 'Person.Address.ZipCode'.</remarks>
    public static void SetProperty(object target, string name, object value)
    {
      SetProperty(target, name, value, DefaultPublicFlags);
    }

    /// <summary>
    /// Sets the property value with the specified name.
    /// </summary>
    /// <param name="target">The object whose property value will be set.</param>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <remarks>This method supports nested property names. An exmample name would be 'Person.Address.ZipCode'.</remarks>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    public static void SetProperty(object target, string name, object value, BindingFlags flags)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      Type rootType = target.GetType();
      Type currentType = rootType;
      object currentTarget = target;

      TypeAccessor typeAccessor;
      IMemberAccessor memberAccessor = null;

      // support nested property
      var parts = name.Split('.');
      foreach (var part in parts)
      {
        if (memberAccessor != null)
        {
          currentTarget = memberAccessor.GetValue(currentTarget);
          currentType = memberAccessor.MemberType;
        }

        typeAccessor = TypeAccessor.GetAccessor(currentType);
        memberAccessor = typeAccessor.FindProperty(part, flags);
      }

      if (memberAccessor == null)
        throw new InvalidOperationException(string.Format(
            "Could not find property '{0}' in type '{1}'.", name, rootType.Name));

      memberAccessor.SetValue(currentTarget, value);
    }

    /// <summary>
    /// Sets the field value with the specified name.
    /// </summary>
    /// <param name="target">The object whose field value will be set.</param>
    /// <param name="name">The name of the field to set.</param>
    /// <param name="value">The new value to be set.</param>
    public static void SetField(object target, string name, object value)
    {
      SetField(target, name, value, DefaultPublicFlags);
    }

    /// <summary>
    /// Sets the field value with the specified name.
    /// </summary>
    /// <param name="target">The object whose field value will be set.</param>
    /// <param name="name">The name of the field to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    public static void SetField(object target, string name, object value, BindingFlags flags)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      Type rootType = target.GetType();
      var memberAccessor = FindField(rootType, name, flags);

      if (memberAccessor == null)
        throw new InvalidOperationException(string.Format(
            "Could not find field '{0}' in type '{1}'.", name, rootType.Name));

      memberAccessor.SetValue(target, value);
    }

    /// <summary>
    /// Sets the property or field value with the specified name.
    /// </summary>
    /// <param name="target">The object whose property or field value will be set.</param>
    /// <param name="name">The name of the property or field to set.</param>
    /// <param name="value">The new value to be set.</param>
    public static void Set(object target, string name, object value)
    {
      Set(target, name, value, DefaultPublicFlags);
    }

    /// <summary>
    /// Sets the property or field value with the specified name.
    /// </summary>
    /// <param name="target">The object whose property or field value will be set.</param>
    /// <param name="name">The name of the property or field to set.</param>
    /// <param name="value">The new value to be set.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    public static void Set(object target, string name, object value, BindingFlags flags)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      Type rootType = target.GetType();
      var memberAccessor = Find(rootType, name, flags);

      if (memberAccessor == null)
        throw new InvalidOperationException(string.Format(
            "Could not find a property or field with a name of '{0}' in type '{1}'.", name, rootType.Name));

      memberAccessor.SetValue(target, value);
    }

    /// <summary>
    /// Returns the value of the property with the specified name.
    /// </summary>
    /// <param name="target">The object whose property value will be returned.</param>
    /// <param name="name">The name of the property to read.</param>
    /// <returns>The value of the property.</returns>
    public static object GetProperty(object target, string name)
    {
      return GetProperty(target, name, DefaultPublicFlags);
    }

    /// <summary>
    /// Returns the value of the property with the specified name.
    /// </summary>
    /// <param name="target">The object whose property value will be returned.</param>
    /// <param name="name">The name of the property to read.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>The value of the property.</returns>
    public static object GetProperty(object target, string name, BindingFlags flags)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      Type rootType = target.GetType();
      Type currentType = rootType;
      object currentTarget = target;

      IMemberAccessor memberAccessor = null;

      // support nested property
      var parts = name.Split('.');
      foreach (var part in parts)
      {
        if (memberAccessor != null)
        {
          currentTarget = memberAccessor.GetValue(currentTarget);
          currentType = memberAccessor.MemberType;
        }

        var typeAccessor = TypeAccessor.GetAccessor(currentType);
        memberAccessor = typeAccessor.FindProperty(part, flags);
      }

      if (memberAccessor == null)
        throw new InvalidOperationException(string.Format(
            "Could not find property '{0}' in type '{1}'.", name, rootType.Name));

      return memberAccessor.GetValue(currentTarget);
    }

    /// <summary>
    /// Returns the value of the field with the specified name.
    /// </summary>
    /// <param name="target">The object whose field value will be returned.</param>
    /// <param name="name">The name of the field to read.</param>
    /// <returns>The value of the field.</returns>
    public static object GetField(object target, string name)
    {
      return GetField(target, name, DefaultPublicFlags);
    }

    /// <summary>
    /// Returns the value of the field with the specified name.
    /// </summary>
    /// <param name="target">The object whose field value will be returned.</param>
    /// <param name="name">The name of the field to read.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>The value of the field.</returns>
    public static object GetField(object target, string name, BindingFlags flags)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      Type rootType = target.GetType();
      var memberAccessor = FindField(rootType, name, flags);
      if (memberAccessor == null)
        throw new InvalidOperationException(string.Format(
            "Could not find field '{0}' in type '{1}'.", name, rootType.Name));

      return memberAccessor.GetValue(target);
    }

    /// <summary>
    /// Returns the value of the property or field with the specified name.
    /// </summary>
    /// <param name="target">The object whose property or field value will be returned.</param>
    /// <param name="name">The name of the property or field to read.</param>
    /// <returns>The value of the property or field.</returns>
    public static object Get(object target, string name)
    {
      return Get(target, name, DefaultPublicFlags);
    }

    /// <summary>
    /// Returns the value of the property or field with the specified name.
    /// </summary>
    /// <param name="target">The object whose property or field value will be returned.</param>
    /// <param name="name">The name of the property or field to read.</param>
    /// <param name="flags">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted.</param>
    /// <returns>The value of the property or field.</returns>
    public static object Get(object target, string name, BindingFlags flags)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      Type rootType = target.GetType();
      var memberAccessor = Find(rootType, name, flags);
      if (memberAccessor == null)
        throw new InvalidOperationException(string.Format(
            "Could not find a property or field with a name of '{0}' in type '{1}'.", name, rootType.Name));

      return memberAccessor.GetValue(target);
    }

    /// <summary>
    /// Creates an instance of the specified type.
    /// </summary>
    /// <param name="type">The type to create.</param>
    /// <returns>A new instance of the specified type.</returns>
    public static object CreateInstance(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      var typeAccessor = TypeAccessor.GetAccessor(type);
      if (typeAccessor == null)
        throw new InvalidOperationException(string.Format("Could not find constructor for {0}.", type.Name));

      return typeAccessor.Create();
    }

    public static object InvokeMethod(object target, string name, params object[] arguments)
    {
      if (target == null)
        throw new ArgumentNullException("target");
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      Type rootType = target.GetType();
      var methodAccessor = FindMethod(rootType, name);

      if (methodAccessor == null)
        throw new InvalidOperationException(string.Format(
            "Could not find method '{0}' in type '{1}'.", name, rootType.Name));

      return methodAccessor.Invoke(target, arguments);

    }
  }
}
