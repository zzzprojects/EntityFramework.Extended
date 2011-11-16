using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace EntityFramework.Reflection
{
  internal delegate object LateBoundMethod(object target, object[] arguments);
  internal delegate object LateBoundGet(object target);
  internal delegate void LateBoundSet(object target, object value);
  internal delegate object LateBoundConstructor();

  internal static class DelegateFactory
  {
    private static DynamicMethod CreateDynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
    {
#if SILVERLIGHT
      return new DynamicMethod(name, returnType, parameterTypes);
#else
      return !owner.IsInterface
        ? new DynamicMethod(name, returnType, parameterTypes, owner, true)
        : new DynamicMethod(name, returnType, parameterTypes, owner.Assembly.ManifestModule, true);
#endif
    }

    public static LateBoundMethod CreateMethod(MethodInfo methodInfo)
    {
      if (methodInfo == null)
        throw new ArgumentNullException("methodInfo");

#if SILVERLIGHT
      // parameters to execute
      var instanceParameter = Expression.Parameter(typeof(object), "instance");
      var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

      // build parameter list
      var parameterExpressions = new List<Expression>();
      var paramInfos = methodInfo.GetParameters();
      for (int i = 0; i < paramInfos.Length; i++)
      {
        // (Ti)parameters[i]
        var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
        
        Type parameterType = paramInfos[i].ParameterType;
        if (parameterType.IsByRef)
          parameterType = parameterType.GetElementType();
        
        var valueCast = Expression.Convert(valueObj, parameterType);

        parameterExpressions.Add(valueCast);
      }

      // non-instance for static method, or ((TInstance)instance)
      var instanceCast = methodInfo.IsStatic ? null :
          Expression.Convert(instanceParameter, methodInfo.ReflectedType);

      // static invoke or ((TInstance)instance).Method
      var methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);

      // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
      if (methodCall.Type == typeof(void))
      {
        var lambda = Expression.Lambda<Action<object, object[]>>(
                methodCall, instanceParameter, parametersParameter);

        Action<object, object[]> execute = lambda.Compile();
        return (instance, parameters) =>
        {
          execute(instance, parameters);
          return null;
        };
      }
      else
      {
        var castMethodCall = Expression.Convert(methodCall, typeof(object));
        var lambda = Expression.Lambda<LateBoundMethod>(
            castMethodCall, instanceParameter, parametersParameter);

        return lambda.Compile();
      }
#else
      DynamicMethod dynamicMethod = CreateDynamicMethod(
        "Dynamic" + methodInfo.Name, 
        typeof(object), 
        new[] { typeof(object), typeof(object[]) }, 
        methodInfo.DeclaringType);

      ILGenerator il = dynamicMethod.GetILGenerator();
      ParameterInfo[] ps = methodInfo.GetParameters();

      var paramTypes = new Type[ps.Length];
      for (int i = 0; i < paramTypes.Length; i++)
      {
        if (ps[i].ParameterType.IsByRef)
          paramTypes[i] = ps[i].ParameterType.GetElementType();
        else
          paramTypes[i] = ps[i].ParameterType;
      }

      var locals = new LocalBuilder[paramTypes.Length];
      for (int i = 0; i < paramTypes.Length; i++)
        locals[i] = il.DeclareLocal(paramTypes[i], true);

      for (int i = 0; i < paramTypes.Length; i++)
      {
        il.Emit(OpCodes.Ldarg_1);
        il.FastInt(i);
        il.Emit(OpCodes.Ldelem_Ref);
        il.UnboxIfNeeded(paramTypes[i]);
        il.Emit(OpCodes.Stloc, locals[i]);
      }

      if (!methodInfo.IsStatic)
        il.Emit(OpCodes.Ldarg_0);

      for (int i = 0; i < paramTypes.Length; i++)
      {
        if (ps[i].ParameterType.IsByRef)
          il.Emit(OpCodes.Ldloca_S, locals[i]);
        else
          il.Emit(OpCodes.Ldloc, locals[i]);
      }

      if (methodInfo.IsStatic)
        il.EmitCall(OpCodes.Call, methodInfo, null);
      else
        il.EmitCall(OpCodes.Callvirt, methodInfo, null);

      if (methodInfo.ReturnType == typeof(void))
        il.Emit(OpCodes.Ldnull);
      else
        il.BoxIfNeeded(methodInfo.ReturnType);

      for (int i = 0; i < paramTypes.Length; i++)
      {
        if (!ps[i].ParameterType.IsByRef)
          continue;

        il.Emit(OpCodes.Ldarg_1);
        il.FastInt(i);
        il.Emit(OpCodes.Ldloc, locals[i]);
        if (locals[i].LocalType.IsValueType)
          il.Emit(OpCodes.Box, locals[i].LocalType);
        il.Emit(OpCodes.Stelem_Ref);
      }

      il.Emit(OpCodes.Ret);
      return (LateBoundMethod)dynamicMethod.CreateDelegate(typeof(LateBoundMethod)); ;
#endif
    }

    public static LateBoundConstructor CreateConstructor(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

#if SILVERLIGHT
      var constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
      var instanceCreate = Expression.New(constructorInfo);

      UnaryExpression instanceCreateCast = type.IsValueType
        ? Expression.Convert(instanceCreate, typeof(object))
        : Expression.TypeAs(instanceCreate, typeof(object));

      var lambda = Expression.Lambda<LateBoundConstructor>(instanceCreateCast);

      return lambda.Compile();
#else
      DynamicMethod dynamicMethod = CreateDynamicMethod("Create" + type.FullName, typeof(object), Type.EmptyTypes, type);
      dynamicMethod.InitLocals = true;
      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (type.IsValueType)
      {
        generator.DeclareLocal(type);
        generator.Emit(OpCodes.Ldloc_0);
        generator.Emit(OpCodes.Box, type);
      }
      else
      {
        ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
        if (constructorInfo == null)
          throw new InvalidOperationException(string.Format("Could not get constructor for {0}.", type));

        generator.Emit(OpCodes.Newobj, constructorInfo);
      }

      generator.Return();

      return (LateBoundConstructor)dynamicMethod.CreateDelegate(typeof(LateBoundConstructor));
#endif
    }

    public static LateBoundGet CreateGet(PropertyInfo propertyInfo)
    {
      if (propertyInfo == null)
        throw new ArgumentNullException("propertyInfo");
      if (!propertyInfo.CanRead)
        return null;

#if SILVERLIGHT
      var instance = Expression.Parameter(typeof(object), "instance");
      var declaringType = propertyInfo.DeclaringType;
      var getMethod = propertyInfo.GetGetMethod(true);

      UnaryExpression instanceCast;
      if (getMethod.IsStatic)
        instanceCast = null;
      else if (declaringType.IsValueType)
        instanceCast = Expression.Convert(instance, declaringType);
      else
        instanceCast = Expression.TypeAs(instance, declaringType);

      var call = Expression.Call(instanceCast, getMethod);
      var valueCast = Expression.TypeAs(call, typeof(object));

      var lambda = Expression.Lambda<LateBoundGet>(valueCast, instance);
      return lambda.Compile();
#else
      MethodInfo methodInfo = propertyInfo.GetGetMethod(true);
      if (methodInfo == null)
        return null;

      DynamicMethod dynamicMethod = CreateDynamicMethod(
        "Get" + propertyInfo.Name, 
        typeof(object), 
        new[] { typeof(object) }, 
        propertyInfo.DeclaringType);

      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!methodInfo.IsStatic)
        generator.PushInstance(propertyInfo.DeclaringType);

      generator.CallMethod(methodInfo);
      generator.BoxIfNeeded(propertyInfo.PropertyType);
      generator.Return();

      return (LateBoundGet)dynamicMethod.CreateDelegate(typeof(LateBoundGet));
#endif
    }

    public static LateBoundGet CreateGet(FieldInfo fieldInfo)
    {
      if (fieldInfo == null)
        throw new ArgumentNullException("fieldInfo");

#if SILVERLIGHT
      var instance = Expression.Parameter(typeof(object), "instance");
      var declaringType = fieldInfo.ReflectedType;

      // value as T is slightly faster than (T)value, so if it's not a value type, use that
      UnaryExpression instanceCast;
      if (fieldInfo.IsStatic)
        instanceCast = null;
      else if (declaringType.IsValueType)
        instanceCast = Expression.Convert(instance, declaringType);
      else
        instanceCast = Expression.TypeAs(instance, declaringType);

      var fieldAccess = Expression.Field(instanceCast, fieldInfo);
      var valueCast = Expression.TypeAs(fieldAccess, typeof(object));

      var lambda = Expression.Lambda<LateBoundGet>(valueCast, instance);
      return lambda.Compile();
#else
      DynamicMethod dynamicMethod = CreateDynamicMethod(
        "Get" + fieldInfo.Name, 
        typeof(object), 
        new[] { typeof(object) }, 
        fieldInfo.DeclaringType);

      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (fieldInfo.IsStatic)
        generator.Emit(OpCodes.Ldsfld, fieldInfo);
      else
        generator.PushInstance(fieldInfo.DeclaringType);

      generator.Emit(OpCodes.Ldfld, fieldInfo);
      generator.BoxIfNeeded(fieldInfo.FieldType);
      generator.Return();

      return (LateBoundGet)dynamicMethod.CreateDelegate(typeof(LateBoundGet));
#endif
    }

    public static LateBoundSet CreateSet(PropertyInfo propertyInfo)
    {
      if (propertyInfo == null)
        throw new ArgumentNullException("propertyInfo");
      if (!propertyInfo.CanWrite)
        return null;

#if SILVERLIGHT
      var instance = Expression.Parameter(typeof(object), "instance");
      var value = Expression.Parameter(typeof(object), "value");

      Type declaringType = propertyInfo.DeclaringType;
      Type propertyType = propertyInfo.PropertyType;
      var setMethod = propertyInfo.GetSetMethod(true);

      // value as T is slightly faster than (T)value, so if it's not a value type, use that
      UnaryExpression instanceCast;
      if (setMethod.IsStatic)
        instanceCast = null;
      else if (declaringType.IsValueType)
        instanceCast = Expression.Convert(instance, declaringType);
      else
        instanceCast = Expression.TypeAs(instance, declaringType);

      UnaryExpression valueCast;
      if (propertyType.IsValueType)
        valueCast = Expression.Convert(value, propertyType);
      else
        valueCast = Expression.TypeAs(value, propertyType);

      var call = Expression.Call(instanceCast, setMethod, valueCast);
      var parameters = new ParameterExpression[] { instance, value };

      var lambda = Expression.Lambda<LateBoundSet>(call, parameters);
      return lambda.Compile();
#else
      MethodInfo methodInfo = propertyInfo.GetSetMethod(true);
      if (methodInfo == null)
        return null;

      DynamicMethod dynamicMethod = CreateDynamicMethod(
        "Set" + propertyInfo.Name, 
        null, 
        new[] { typeof(object), typeof(object) }, 
        propertyInfo.DeclaringType);

      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (!methodInfo.IsStatic)
        generator.PushInstance(propertyInfo.DeclaringType);

      generator.Emit(OpCodes.Ldarg_1);
      generator.UnboxIfNeeded(propertyInfo.PropertyType);
      generator.CallMethod(methodInfo);
      generator.Return();

      return (LateBoundSet)dynamicMethod.CreateDelegate(typeof(LateBoundSet));
#endif
    }

    public static LateBoundSet CreateSet(FieldInfo fieldInfo)
    {
      if (fieldInfo == null)
        throw new ArgumentNullException("fieldInfo");

#if SILVERLIGHT
      return fieldInfo.SetValue;
#else
      DynamicMethod dynamicMethod = CreateDynamicMethod(
        "Set" + fieldInfo.Name, 
        null, 
        new[] { typeof(object), typeof(object) }, 
        fieldInfo.DeclaringType);

      ILGenerator generator = dynamicMethod.GetILGenerator();

      if (fieldInfo.IsStatic)
        generator.Emit(OpCodes.Ldsfld, fieldInfo);
      else
        generator.PushInstance(fieldInfo.DeclaringType);

      generator.Emit(OpCodes.Ldarg_1);
      generator.UnboxIfNeeded(fieldInfo.FieldType);
      generator.Emit(OpCodes.Stfld, fieldInfo);
      generator.Return();

      return (LateBoundSet)dynamicMethod.CreateDelegate(typeof(LateBoundSet));
#endif
    }
  }
}
