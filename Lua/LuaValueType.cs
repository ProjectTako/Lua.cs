using System;

namespace Lua
{
    public enum LuaValueType
    {
        Nil, True, False, Object, Array, Number, String, Function
    }

    public static class LuaValueTypeExtensions
    {
        public static string GetName( this LuaValueType type )
        {
            switch( type )
            {
                case LuaValueType.Nil:
                    return "nil";

                case LuaValueType.True:
                case LuaValueType.False:
                    return "bool";

                case LuaValueType.Object:
                    return "object";

                case LuaValueType.Array:
                    return "array";

                case LuaValueType.Number:
                    return "number";

                case LuaValueType.String:
                    return "string";

                case LuaValueType.Function:
                    return "function";

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
