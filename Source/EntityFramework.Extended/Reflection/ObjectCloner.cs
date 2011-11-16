using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EntityFramework.Reflection
{
  /// <summary>
  /// A class to support cloning, which creates a new instance of a class with the same value as an existing instance.
  /// </summary>
  public class ObjectCloner
  {
    // key is orginal object hashcode, value is cloned copy
    private readonly Dictionary<int, object> _objectReferences;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectCloner"/> class.
    /// </summary>
    public ObjectCloner()
    {
      _objectReferences = new Dictionary<int, object>();
    }

    /// <summary>
    /// Creates a new object that is a copy of the source instance.
    /// </summary>
    /// <param name="source">The source object to copy.</param>
    /// <returns>A new object that is a copy of the source instance.</returns>
    public object Clone(object source)
    {
      _objectReferences.Clear();
      return CloneInstance(source);
    }

    private object CloneInstance(object source)
    {
      object target;

      // check if this object has already been cloned 
      // using RuntimeHelpers.GetHashCode to get object identity
      int hashCode = RuntimeHelpers.GetHashCode(source);
      if (_objectReferences.TryGetValue(hashCode, out target))
        return target;

#if !SILVERLIGHT
      // using ICloneable if available
      if (source is ICloneable)
      {
        target = ((ICloneable)source).Clone();

        // keep track of cloned instances
        _objectReferences.Add(hashCode, target);
        return target;
      }
#endif

      var sourceType = source.GetType();
      var sourceAccessor = TypeAccessor.GetAccessor(sourceType);

      target = sourceAccessor.Create();
      // keep track of cloned instances
      _objectReferences.Add(hashCode, target);

      var sourceProperties = sourceAccessor.GetProperties(BindingFlags.Public | BindingFlags.Instance);
      foreach (IMemberAccessor sourceProperty in sourceProperties)
      {
        if (!sourceProperty.HasGetter)
          continue;

        var originalValue = sourceProperty.GetValue(source);
        if (originalValue == null)
          continue;

        var propertyType = sourceProperty.MemberType.GetUnderlyingType();

        if (propertyType.IsArray)
          CloneArray(sourceProperty, source, target);
        else if (originalValue is IDictionary)
          CloneDictionary(sourceProperty, originalValue, target);
        else if (originalValue is IList)
          CloneCollection(sourceProperty, originalValue, target);
        else if (!propertyType.IsValueType && propertyType != typeof(string))
          CloneObject(sourceProperty, originalValue, target);
        else if (sourceProperty.HasSetter)
          sourceProperty.SetValue(target, originalValue);
      }

      return target;
    }

    private void CloneArray(IMemberAccessor accessor, object originalValue, object target)
    {
      var valueType = originalValue.GetType();
      var sourceList = originalValue as IList;
      if (sourceList == null)
        return;

      var targetValue = Activator.CreateInstance(valueType, sourceList.Count);
      var targetList = targetValue as IList;
      if (targetList == null)
        return;

      for (int i = 0; i < sourceList.Count; i++)
        targetList[i] = CloneInstance(sourceList[i]);

      accessor.SetValue(target, targetList);
    }

    private void CloneCollection(IMemberAccessor accessor, object originalValue, object target)
    {
      // support readonly collections
      object targetValue = accessor.HasSetter
        ? CreateTargetValue(accessor, originalValue, target)
        : accessor.GetValue(target);

      if (targetValue == null)
        return;

      var sourceList = originalValue as IEnumerable;
      if (sourceList == null)
        return;

      // must support IList
      var targetList = targetValue as IList;
      if (targetList == null)
        return;

      foreach (var sourceItem in sourceList)
      {
        var cloneItem = CloneInstance(sourceItem);
        targetList.Add(cloneItem);
      }

      if (!accessor.HasSetter)
        return;

      accessor.SetValue(target, targetValue);
    }

    private void CloneDictionary(IMemberAccessor accessor, object originalValue, object target)
    {
      // support readonly dictionary
      object targetValue = accessor.HasSetter
        ? CreateTargetValue(accessor, originalValue, target)
        : accessor.GetValue(target);

      if (targetValue == null)
        return;

      // must support IDictionary
      var sourceList = originalValue as IDictionary;
      if (sourceList == null)
        return;

      var targetList = targetValue as IDictionary;
      if (targetList == null)
        return;

      var e = sourceList.GetEnumerator();
      while (e.MoveNext())
      {
        var cloneItem = CloneInstance(e.Value);
        targetList.Add(e.Key, cloneItem);
      }

      if (!accessor.HasSetter)
        return;

      accessor.SetValue(target, targetValue);
    }

    private void CloneObject(IMemberAccessor accessor, object originalValue, object target)
    {
      if (!accessor.HasSetter)
        return;

      object value = CloneInstance(originalValue);
      accessor.SetValue(target, value);
    }

    private object CreateTargetValue(IMemberAccessor accessor, object originalValue, object target)
    {
      var valueType = originalValue.GetType();
      object targetValue;

      // check if this object has already been cloned 
      // using RuntimeHelpers.GetHashCode to get object identity
      int hashCode = RuntimeHelpers.GetHashCode(originalValue);
      if (_objectReferences.TryGetValue(hashCode, out targetValue))
      {
        accessor.SetValue(target, targetValue);
        return null;
      }

      targetValue = LateBinder.CreateInstance(valueType);
      // keep track of cloned instances
      _objectReferences.Add(hashCode, targetValue);
      return targetValue;
    }
  }
}