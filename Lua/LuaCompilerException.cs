using JetBrains.Annotations;

namespace Lua
{
    public class LuaCompilerException : LuaException
    {
        [StringFormatMethod( "format" )]
        internal LuaCompilerException( string fileName, int line, string format, params object[] args )
            : base( string.Format( "{0}(line {1}): {2}", fileName ?? "null", line, string.Format( format, args ) ) )
        {

        }
    }
}
