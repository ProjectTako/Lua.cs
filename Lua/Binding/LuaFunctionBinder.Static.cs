using System;
using System.Collections.Generic;

namespace Lua.Binding
{
    public static partial class LuaFunctionBinder
    {
        private static readonly Dictionary<Type, LuaValueType[]> TypeCheckMap;
        private static readonly HashSet<Type> BasicTypes;
        private static readonly HashSet<Type> NumberTypes;

        static LuaFunctionBinder()
        {
            TypeCheckMap = new Dictionary<Type, LuaValueType[]>
            {
                { typeof(double),       new [] { LuaValueType.Number } },
                { typeof(float),        new [] { LuaValueType.Number } },
                { typeof(int),          new [] { LuaValueType.Number } },
                { typeof(uint),         new [] { LuaValueType.Number } },
                { typeof(short),        new [] { LuaValueType.Number } },
                { typeof(ushort),       new [] { LuaValueType.Number } },
                { typeof(sbyte),        new [] { LuaValueType.Number } },
                { typeof(byte),         new [] { LuaValueType.Number } },

                { typeof(string),       new [] { LuaValueType.String } },

                { typeof(bool),         new [] { LuaValueType.True, LuaValueType.False } }
            };

            // types with a direct conversion to/from LuaValue
            BasicTypes = new HashSet<Type>
            {
                typeof(double),
                typeof(string),
                typeof(bool)
            };

            // types that can be casted to/from double
            NumberTypes = new HashSet<Type>
            {
                typeof(float),
                typeof(int),
                typeof(uint),
                typeof(short),
                typeof(ushort),
                typeof(sbyte),
                typeof(byte),
            };
        }
    }
}
