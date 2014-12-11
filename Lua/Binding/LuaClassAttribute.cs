using System;

namespace Lua.Binding
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class LuaClassAttribute : Attribute
    {
        public readonly string Name;

        public LuaClassAttribute( string name = null )
        {
            Name = name;
        }
    }
}
