using System;
using JetBrains.Annotations;

namespace Lua
{
    public class LuaRuntimeException : LuaException
    {
        public bool HasStackTrace { get; internal set; }

        public LuaRuntimeException( string message, bool hasStackTrace = false )
            : base( message )
        {
            HasStackTrace = hasStackTrace;
        }

        public LuaRuntimeException( string message, Exception innerException, bool hasStackTrace = false )
            : base( message, innerException )
        {
            HasStackTrace = hasStackTrace;
        }

        [StringFormatMethod( "format" )]
        public LuaRuntimeException( string format, params object[] args )
            : base( string.Format( format, args ) )
        {
            HasStackTrace = false;
        }
    }
}
