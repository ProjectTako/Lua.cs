using System;
using JetBrains.Annotations;

namespace Lua.Binding
{
    public class LuaBindingException : Exception
    {
        [StringFormatMethod( "format" )]
        internal LuaBindingException( string format, params object[] args )
            : base( string.Format( format, args ) )
        {

        }
    }
}
