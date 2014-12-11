using System;

namespace Lua.Binding
{
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false )]
    public class LuaFunctionAttribute : Attribute
    {
        public readonly string Name;

        public LuaFunctionAttribute( string name = null )
        {
            Name = name;
        }
    }
}
