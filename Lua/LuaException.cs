using System;

namespace Lua
{
    public class LuaException : Exception
    {
        internal LuaException( string message )
            : base( message )
        {

        }

        internal LuaException( string message, Exception innerException )
            : base( message, innerException )
        {

        }
    }
}
