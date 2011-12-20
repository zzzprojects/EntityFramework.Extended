using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace EntityFramework.Reflection
{
    /// <summary>
    /// A class for reflection helper methods
    /// </summary>
  public static class ReflectionHelper
  {
    private static readonly Type _stringType = typeof(string);
    private static readonly Type _byteArrayType = typeof(byte[]);
    private static readonly Type _nullableType = typeof(Nullable<>);
    private static readonly Type _genericCollectionType = typeof(ICollection<>);
    private static readonly Type _collectionType = typeof(ICollection);
    private static readonly Type _genericDictionaryType = typeof(IDictionary<,>);
    private static readonly Type _dictionaryType = typeof(IDictionary);

    /// <summary>
    /// Extracts the property name from a property expression.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g. p => p.PropertyName)</param>
    /// <returns>The name of the property.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyExpression"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the expression is:<br/>
    ///     Not a <see cref="MemberExpression"/><br/>
    ///     The <see cref="MemberExpression"/> does not represent a property.<br/>
    ///     Or, the property is static.
    /// </exception>
    public static string ExtractPropertyName<TValue>(Expression<Func<TValue>> propertyExpression)
    {
      if (propertyExpression == null)
        throw new ArgumentNullException("propertyExpression");

      return ExtractPropertyName(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Extracts the property name from a property expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="propertyExpression">The property expression (e.g. p =&gt; p.PropertyName)</param>
    /// <returns>
    /// The name of the property.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyExpression"/> is null.</exception>
    ///   
    /// <exception cref="ArgumentException">Thrown when the expression is:<br/>
    /// Not a <see cref="MemberExpression"/><br/>
    /// The <see cref="MemberExpression"/> does not represent a property.<br/>
    /// Or, the property is static.
    ///   </exception>
    public static string ExtractPropertyName<TSource, TValue>(Expression<Func<TSource, TValue>> propertyExpression)
    {
      if (propertyExpression == null)
        throw new ArgumentNullException("propertyExpression");

      return ExtractPropertyName(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Extracts the property name from a property expression.
    /// </summary>
    /// <param name="memberExpression">The member expression</param>
    /// <returns>
    /// The name of the property.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="memberExpression"/> is null.</exception>
    ///   
    /// <exception cref="ArgumentException">Thrown when the expression is:<br/>
    /// Not a <see cref="MemberExpression"/><br/>
    /// The <see cref="MemberExpression"/> does not represent a property.<br/>
    /// Or, the property is static.
    ///   </exception>
    public static string ExtractPropertyName(MemberExpression memberExpression)
    {
      var property = ExtractPropertyInfo(memberExpression);

      var getMethod = property.GetGetMethod(true);
      if (getMethod.IsStatic)
        throw new ArgumentException("The referenced property is a static property.", "memberExpression");

      return memberExpression.Member.Name;
    }

    /// <summary>
    /// Extracts the <see cref="PropertyInfo"/> from a property expression.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="propertyExpression">The property expression.</param>
    /// <returns>The <see cref="PropertyInfo"/> from the expression.</returns>
    public static PropertyInfo ExtractPropertyInfo<TValue>(Expression<Func<TValue>> propertyExpression)
    {
      if (propertyExpression == null)
        throw new ArgumentNullException("propertyExpression");

      return ExtractPropertyInfo(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Extracts the <see cref="PropertyInfo"/> from a property expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="propertyExpression">The property expression.</param>
    /// <returns>The <see cref="PropertyInfo"/> from the expression.</returns>
    public static PropertyInfo ExtractPropertyInfo<TSource, TValue>(Expression<Func<TSource, TValue>> propertyExpression)
    {
      if (propertyExpression == null)
        throw new ArgumentNullException("propertyExpression");

      return ExtractPropertyInfo(propertyExpression.Body as MemberExpression);
    }

    /// <summary>
    /// Extracts the property info.
    /// </summary>
    /// <param name="memberExpression">The member expression.</param>
    /// <returns></returns>
    public static PropertyInfo ExtractPropertyInfo(MemberExpression memberExpression)
    {
      if (memberExpression == null)
        throw new ArgumentException("The expression is not a member access expression.", "memberExpression");

      var property = memberExpression.Member as PropertyInfo;
      if (property == null)
        throw new ArgumentException("The member access expression does not access a property.", "memberExpression");
      
      return property;
    }

    /// <summary>
    /// Gets the underlying type dealing with <see cref="Nullable"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Returns a type dealing with <see cref="Nullable"/>.</returns>
    public static Type GetUnderlyingType(this Type type)
    {
      Type t = type;
      bool isNullable = t.IsGenericType && (t.GetGenericTypeDefinition() == _nullableType);
      if (isNullable)
        return Nullable.GetUnderlyingType(t);

      return t;
    }

    /// <summary>
    /// Determines whether the specified type is a collection.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified type is collection; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsCollection(this Type type)
    {
      return type.GetInterfaces()
        .Any(x =>
          (x.IsGenericType && x.GetGenericTypeDefinition() == _genericCollectionType)
          || x.GetType() == _collectionType);
    }

    /// <summary>
    /// Determines whether the specified type is a dictionary.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    ///   <c>true</c> if the specified type is dictionary; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDictionary(this Type type)
    {
      return type.GetInterfaces()
        .Any(x =>
          (x.IsGenericType && x.GetGenericTypeDefinition() == _genericDictionaryType)
          || x.GetType() == _dictionaryType);
    }

    /// <summary>
    /// Attempts to coerce a value of one type into
    /// a value of a different type.
    /// </summary>
    /// <param name="desiredType">
    /// Type to which the value should be coerced.
    /// </param>
    /// <param name="valueType">
    /// Original type of the value.
    /// </param>
    /// <param name="value">
    /// The value to coerce.
    /// </param>
    /// <remarks>
    /// <para>
    /// If the desired type is a primitive type or Decimal, 
    /// empty string and null values will result in a 0 
    /// or equivalent.
    /// </para>
    /// <para>
    /// If the desired type is a <see cref="Nullable"/> type, empty string
    /// and null values will result in a null result.
    /// </para>
    /// <para>
    /// If the desired type is an <c>enum</c> the value's ToString()
    /// result is parsed to convert into the <c>enum</c> value.
    /// </para>
    /// </remarks>
    public static object CoerceValue(Type desiredType, Type valueType, object value)
    {
      // types match, just copy value
      if (desiredType.Equals(valueType))
        return value;

      bool isNullable = desiredType.IsGenericType && (desiredType.GetGenericTypeDefinition() == _nullableType);
      if (isNullable)
      {
        if (value == null)
          return null;
        if (valueType.Equals(_stringType) && Convert.ToString(value) == String.Empty)
          return null;
      }

      desiredType = GetUnderlyingType(desiredType);

      if ((desiredType.IsPrimitive || desiredType.Equals(typeof(decimal)))
          && valueType.Equals(_stringType)
          && String.IsNullOrEmpty((string)value))
        return 0;

      if (value == null)
        return null;

      // types don't match, try to convert
      if (desiredType.Equals(typeof(Guid)))
        return new Guid(value.ToString());

      if (desiredType.IsEnum && valueType.Equals(_stringType))
        return Enum.Parse(desiredType, value.ToString(), true);

      bool isBinary = desiredType.IsArray && desiredType.Equals(_byteArrayType);

      if (isBinary && valueType.Equals(_stringType))
      {
        byte[] bytes = Convert.FromBase64String((string)value);
        return bytes;
      }

      isBinary = valueType.IsArray && valueType.Equals(_byteArrayType);

      if (isBinary && desiredType.Equals(_stringType))
      {
        byte[] bytes = (byte[])value;
        return Convert.ToBase64String(bytes);
      }

      try
      {
        if (desiredType.Equals(_stringType))
          return value.ToString();

        return Convert.ChangeType(value, desiredType, Thread.CurrentThread.CurrentCulture);
      }
      catch
      {
#if !SILVERLIGHT
        TypeConverter converter = TypeDescriptor.GetConverter(desiredType);
        if (converter != null && converter.CanConvertFrom(valueType))
          return converter.ConvertFrom(value);
#endif
        throw;
      }
    }
  }
}
