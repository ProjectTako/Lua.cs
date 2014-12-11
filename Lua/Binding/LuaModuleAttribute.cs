using System;

namespace Lua.Binding
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class LuaModuleAttribute : Attribute
    {
        public readonly string Name;

        public LuaModuleAttribute( string name = null )
        {
            Name = name;
        }
    }
}
