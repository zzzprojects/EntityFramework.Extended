using System;
using System.Reflection.Emit;
using System.Reflection;

namespace EntityFramework.Reflection
{
    internal static class ILGeneratorExtensions
    {
        public static void PushInstance(this ILGenerator generator, Type type)
        {
            generator.Emit(OpCodes.Ldarg_0);
            if (type.IsValueType)
                generator.Emit(OpCodes.Unbox, type);
            else
                generator.Emit(OpCodes.Castclass, type);
        }

        public static void BoxIfNeeded(this ILGenerator generator, Type type)
        {
            if (type.IsValueType)
                generator.Emit(OpCodes.Box, type);
            else
                generator.Emit(OpCodes.Castclass, type);
        }

        public static void UnboxIfNeeded(this ILGenerator generator, Type type)
        {
            if (type.IsValueType)
                generator.Emit(OpCodes.Unbox_Any, type);
            else
                generator.Emit(OpCodes.Castclass, type);
        }

        public static void CallMethod(this ILGenerator generator, MethodInfo methodInfo)
        {
            if (methodInfo.IsFinal || !methodInfo.IsVirtual)
                generator.Emit(OpCodes.Call, methodInfo);
            else
                generator.Emit(OpCodes.Callvirt, methodInfo);
        }

        public static void Return(this ILGenerator generator)
        {
            generator.Emit(OpCodes.Ret);
        }

        public static void FastInt(this ILGenerator il, int value)
        {
          switch (value)
          {
            case -1:
              il.Emit(OpCodes.Ldc_I4_M1);
              return;
            case 0:
              il.Emit(OpCodes.Ldc_I4_0);
              return;
            case 1:
              il.Emit(OpCodes.Ldc_I4_1);
              return;
            case 2:
              il.Emit(OpCodes.Ldc_I4_2);
              return;
            case 3:
              il.Emit(OpCodes.Ldc_I4_3);
              return;
            case 4:
              il.Emit(OpCodes.Ldc_I4_4);
              return;
            case 5:
              il.Emit(OpCodes.Ldc_I4_5);
              return;
            case 6:
              il.Emit(OpCodes.Ldc_I4_6);
              return;
            case 7:
              il.Emit(OpCodes.Ldc_I4_7);
              return;
            case 8:
              il.Emit(OpCodes.Ldc_I4_8);
              return;
          }

          if (value > -129 && value < 128)
          {
            il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
          }
          else
          {
            il.Emit(OpCodes.Ldc_I4, value);
          }
        }
    }
}
