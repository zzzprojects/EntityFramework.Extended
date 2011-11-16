using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
#if !SILVERLIGHT
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace EntityFramework.Reflection
{
  /// <summary>
  /// Copy data from a source into a target object by copying public property values.
  /// </summary>
  /// <remarks></remarks>
  public static class ObjectCopier
  {
    /// <summary>
    /// Creates a new object that is a copy of the source instance.
    /// </summary>
    /// <typeparam name="T">The type of the object to clone.</typeparam>
    /// <param name="source">The source object to copy.</param>
    /// <returns>A new object that is a copy of the source instance.</returns>
    public static T Clone<T>(T source)
    {
      var cloner = new ObjectCloner();
      return (T)cloner.Clone(source);
    }

    /// <summary>
    /// Creates a new object that is a copy of the source instance.
    /// </summary>
    /// <param name="source">The source object to copy.</param>
    /// <returns>A new object that is a copy of the source instance.</returns>
    public static object Clone(object source)
    {
      var cloner = new ObjectCloner();
      return cloner.Clone(source);
    }

    #region Copy Object To Object
    /// <summary>
    /// Copies values from the source into the properties of the target.
    /// </summary>
    /// <param name="source">An object containing the source values.</param>
    /// <param name="target">An object with properties to be set from the source.</param>
    /// <remarks>
    /// The property names and types of the source object must match the property names and types
    /// on the target object. Source properties may not be indexed. 
    /// Target properties may not be readonly or indexed.
    /// </remarks>
    public static void Copy(object source, object target)
    {
      Copy(source, target, false, new string[0]);
    }

    /// <summary>
    /// Copies values from the source into the properties of the target.
    /// </summary>
    /// <param name="source">An object containing the source values.</param>
    /// <param name="target">An object with properties to be set from the source.</param>
    /// <param name="ignoreList">A list of property names to ignore. 
    /// These properties will not be set on the target object.</param>
    /// <remarks>
    /// The property names and types of the source object must match the property names and types
    /// on the target object. Source properties may not be indexed. 
    /// Target properties may not be readonly or indexed.
    /// </remarks>
    public static void Copy(object source, object target, params string[] ignoreList)
    {
      Copy(source, target, false, ignoreList);
    }

    /// <summary>
    /// Copies values from the source into the properties of the target.
    /// </summary>
    /// <param name="source">An object containing the source values.</param>
    /// <param name="target">An object with properties to be set from the source.</param>
    /// <param name="ignoreList">A list of property names to ignore. 
    /// These properties will not be set on the target object.</param>
    /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
    /// <remarks>
    /// <para>
    /// The property names and types of the source object must match the property names and types
    /// on the target object. Source properties may not be indexed. 
    /// Target properties may not be readonly or indexed.
    /// </para><para>
    /// Properties to copy are determined based on the source object. Any properties
    /// on the source object marked with the <see cref="BrowsableAttribute"/> equal
    /// to false are ignored.
    /// </para>
    /// </remarks>
    public static void Copy(object source, object target, bool suppressExceptions, params string[] ignoreList)
    {
      var ignore = new HashSet<string>(ignoreList ?? new string[0]);
      Copy(source, target, suppressExceptions, ignore.Contains);
    }

    /// <summary>
    /// Copies values from the source into the properties of the target.
    /// </summary>
    /// <param name="source">An object containing the source values.</param>
    /// <param name="target">An object with properties to be set from the source.</param>
    /// <param name="propertyFilter">A delegate to determine if the property name should be ignored. 
    /// When the delegate returns true, the property will not be set in the target.</param>
    /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
    /// <remarks>
    /// <para>
    /// The property names and types of the source object must match the property names and types
    /// on the target object. Source properties may not be indexed. 
    /// Target properties may not be readonly or indexed.
    /// </para><para>
    /// Properties to copy are determined based on the source object. Any properties
    /// on the source object marked with the <see cref="BrowsableAttribute"/> equal
    /// to false are ignored.
    /// </para>
    /// </remarks>
    public static void Copy(object source, object target, bool suppressExceptions, Func<string, bool> propertyFilter)
    {
      if (source == null)
        throw new ArgumentNullException("source", "Source object can not be Null.");
      if (target == null)
        throw new ArgumentNullException("target", "Target object can not be Null.");

      var sourceAccessor = TypeAccessor.GetAccessor(source.GetType());
      var targetAccessor = TypeAccessor.GetAccessor(target.GetType());
      var sourceProperties = sourceAccessor.GetProperties();

      foreach (var sourceProperty in sourceProperties)
      {
        if (propertyFilter != null && propertyFilter(sourceProperty.Name))
          continue;

        try
        {
          var targetProperty = targetAccessor.FindProperty(sourceProperty.Name);
          if (targetProperty == null)
            continue;

          object value = sourceProperty.GetValue(source);
          SetValueWithCoercion(target, targetProperty, value);
        }
        catch (Exception ex)
        {
          Debug.WriteLine(string.Format("Property '{0}' copy failed.", sourceProperty.Name));
          if (!suppressExceptions)
            throw new InvalidOperationException(
                string.Format("Property '{0}' copy failed.", sourceProperty.Name), ex);
        }
      }
    }

    #endregion

    #region Copy Object to IDictionary<string, object>
    /// <summary>
    /// Copies values from the source into the target <see cref="IDictionary"/>.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target <see cref="IDictionary"/>.</param>
    public static void Copy(object source, IDictionary<string, object> target)
    {
      Copy(source, target, false, new string[0]);
    }

    /// <summary>
    /// Copies values from the source into the target <see cref="IDictionary"/>.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target <see cref="IDictionary"/>.</param>
    /// <param name="ignoreList">A list of property names to ignore. 
    /// These properties will not be added to the targeted <see cref="IDictionary"/>.</param>
    public static void Copy(object source, IDictionary<string, object> target, params string[] ignoreList)
    {
      Copy(source, target, false, ignoreList);
    }

    /// <summary>
    /// Copies values from the source into the target <see cref="IDictionary"/>.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target <see cref="IDictionary"/>.</param>
    /// <param name="ignoreList">A list of property names to ignore. 
    /// These properties will not be added to the targeted <see cref="IDictionary"/>.</param>
    /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
    public static void Copy(object source, IDictionary<string, object> target, bool suppressExceptions, params string[] ignoreList)
    {
      var ignore = new HashSet<string>(ignoreList ?? new string[0]);
      Copy(source, target, suppressExceptions, ignore.Contains);
    }

    /// <summary>
    /// Copies values from the source into the target <see cref="IDictionary"/>.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target <see cref="IDictionary"/>.</param>
    /// <param name="propertyFilter">A delegate to determine if the property name should be ignored. 
    /// When the delegate returns true, the property will not be added to the targeted <see cref="IDictionary"/>.</param>
    /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
    public static void Copy(object source, IDictionary<string, object> target, bool suppressExceptions, Func<string, bool> propertyFilter)
    {
      if (source == null)
        throw new ArgumentNullException("source", "Source object can not be Null.");
      if (target == null)
        throw new ArgumentNullException("target", "Target object can not be Null.");

      var sourceAccessor = TypeAccessor.GetAccessor(source.GetType());
      var sourceProperties = sourceAccessor.GetProperties();

      foreach (var property in sourceProperties)
      {
        if (propertyFilter != null && propertyFilter(property.Name))
          continue;

        try
        {
          object value = property.GetValue(source);
          target.Add(property.Name, value);
        }
        catch (Exception ex)
        {
          Debug.WriteLine(string.Format("Property '{0}' copy failed.", property.Name));
          if (!suppressExceptions)
            throw new ArgumentException(
                String.Format("Property '{0}' copy failed.", property.Name), ex);
        }
      }
    }
    #endregion

#if !SILVERLIGHT
    #region Copy From NameValueCollection to Object
    /// <summary>
    /// Copies values from the <see cref="NameValueCollection"/> into the properties of the target.
    /// </summary>
    /// <param name="source">The <see cref="NameValueCollection"/> source.</param>
    /// <param name="target">The target object.</param>
    public static void Copy(NameValueCollection source, object target)
    {
      Copy(source, target, false, new string[0]);
    }

    /// <summary>
    /// Copies values from the <see cref="NameValueCollection"/> into the properties of the target.
    /// </summary>
    /// <param name="source">The <see cref="NameValueCollection"/> source.</param>
    /// <param name="target">The target object.</param>
    /// <param name="ignoreList">A list of property names to ignore. 
    /// These properties will not be set on the target object.</param>
    public static void Copy(NameValueCollection source, object target, params string[] ignoreList)
    {
      Copy(source, target, false, ignoreList);
    }

    /// <summary>
    /// Copies values from the <see cref="NameValueCollection"/> into the properties of the target.
    /// </summary>
    /// <param name="source">The <see cref="NameValueCollection"/> source.</param>
    /// <param name="target">The target object.</param>
    /// <param name="ignoreList">A list of property names to ignore. 
    /// These properties will not be set on the target object.</param>
    /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
    public static void Copy(NameValueCollection source, object target, bool suppressExceptions, params string[] ignoreList)
    {
      var newSource = new Dictionary<string, object>();
      for (int i = 0; i < source.Count; i++)
        if (!string.IsNullOrEmpty(source.Keys[i]))
          newSource.Add(source.Keys[i], source[i]);

      Copy(newSource, target, suppressExceptions, ignoreList);
    }

    /// <summary>
    /// Copies values from the <see cref="NameValueCollection"/> into the properties of the target.
    /// </summary>
    /// <param name="source">The <see cref="NameValueCollection"/> source.</param>
    /// <param name="target">The target object.</param>
    /// <param name="propertyFilter">A delegate to determine if the property name should be ignored. 
    /// When the delegate returns true, the property will not be set in the target.</param>
    /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
    public static void Copy(NameValueCollection source, object target, bool suppressExceptions, Func<string, bool> propertyFilter)
    {
      var newSource = new Dictionary<string, object>();
      for (int i = 0; i < source.Count; i++)
        if (!string.IsNullOrEmpty(source.Keys[i]))
          newSource.Add(source.Keys[i], source[i]);

      Copy(newSource, target, suppressExceptions, propertyFilter);
    }
    #endregion
#endif

    #region Copy IDictionary<string, object> to Object
    /// <summary>
    /// Copies values from the <see cref="IDictionary"/> into the properties of the target.
    /// </summary>
    /// <param name="source">The <see cref="IDictionary"/> source.</param>
    /// <param name="target">The target object.</param>
    public static void Copy(IDictionary<string, object> source, object target)
    {
      Copy(source, target, false, new string[0]);
    }

    /// <summary>
    /// Copies values from the <see cref="IDictionary"/> into the properties of the target.
    /// </summary>
    /// <param name="source">The <see cref="IDictionary"/> source.</param>
    /// <param name="target">The target object.</param>
    /// <param name="ignoreList">A list of property names to ignore. 
    /// These properties will not be set on the target object.</param>
    public static void Copy(IDictionary<string, object> source, object target, params string[] ignoreList)
    {
      Copy(source, target, false, ignoreList);
    }

    /// <summary>
    /// Copies values from the <see cref="IDictionary"/> into the properties of the target.
    /// </summary>
    /// <param name="source">The <see cref="IDictionary"/> source.</param>
    /// <param name="target">The target object.</param>
    /// <param name="ignoreList">A list of property names to ignore. 
    /// These properties will not be set on the target object.</param>
    /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
    public static void Copy(IDictionary<string, object> source, object target, bool suppressExceptions, params string[] ignoreList)
    {
      var ignore = new HashSet<string>(ignoreList ?? new string[0]);
      Copy(source, target, suppressExceptions, ignore.Contains);
    }

    /// <summary>
    /// Copies values from the <see cref="IDictionary"/> into the properties of the target.
    /// </summary>
    /// <param name="source">The <see cref="IDictionary"/> source.</param>
    /// <param name="target">The target object.</param>
    /// <param name="propertyFilter">A delegate to determine if the property name should be ignored. 
    /// When the delegate returns true, the property will not be set in the target.</param>
    /// <param name="suppressExceptions">If <see langword="true" />, any exceptions will be suppressed.</param>
    public static void Copy(IDictionary<string, object> source, object target, bool suppressExceptions, Func<string, bool> propertyFilter)
    {
      if (source == null)
        throw new ArgumentNullException("source", "Source object can not be Null.");
      if (target == null)
        throw new ArgumentNullException("target", "Target object can not be Null.");

      var targetAccessor = TypeAccessor.GetAccessor(target.GetType());

      foreach (var item in source)
      {
        if (propertyFilter != null && propertyFilter(item.Key))
          continue;

        try
        {
          var targetProperty = targetAccessor.FindProperty(item.Key);
          if (targetProperty == null)
            continue;

          SetValueWithCoercion(target, targetProperty, item.Value);
        }
        catch (Exception ex)
        {
          Debug.WriteLine(string.Format("Property '{0}' copy failed.", item.Key));
          if (!suppressExceptions)
            throw new ArgumentException(string.Format("Property '{0}' copy failed.", item.Key), ex);
        }
      }
    }
    #endregion

    private static void SetValueWithCoercion(object target, IMemberAccessor accessor, object value)
    {
      if (value == null)
        return;

      Type pType = accessor.MemberType;
      Type vType = ReflectionHelper.GetUnderlyingType(value.GetType());
      object v = ReflectionHelper.CoerceValue(pType, vType, value);
      if (v != null)
        accessor.SetValue(target, v);
    }
  }
}
