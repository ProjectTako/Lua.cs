using System.IO;
using System.Text;

namespace Lua
{
    public sealed partial class LuaValue
    {
        /// <summary>
        /// Serialize the value to a string.
        /// </summary>
        public string Serialize()
        {
            var stringBuilder = new StringBuilder();

            using( var stringWriter = new StringWriter( stringBuilder ) )
            using( var writer = new IndentTextWriter( stringWriter ) )
            {
                SerializeImpl( writer, 0 );
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Serialize the value to a TextWriter.
        /// </summary>
        public void Serialize( TextWriter textWriter )
        {
            using( var writer = new IndentTextWriter( textWriter ) )
            {
                SerializeImpl( writer, 0 );
            }
        }

        private bool SerializeImpl( IndentTextWriter writer, int depth )
        {
            if( depth >= 64 )
            {
                writer.Write( "< max depth reached >" );
                return false;
            }

            bool first = true;

            switch( Type )
            {
                case LuaValueType.True:
                    writer.Write( "true" );
                    break;

                case LuaValueType.False:
                    writer.Write( "false" );
                    break;

                case LuaValueType.Object:
                    writer.WriteLine( "{" );
                    writer.Indent++;

                    foreach( var objValue in ObjectValue.Values )
                    {
                        if( first )
                        {
                            writer.WriteIndent();
                            first = false;
                        }
                        else
                        {
                            writer.Write( "," );
                            writer.WriteLine();
                            writer.WriteIndent();
                        }

                        if( !objValue.Key.SerializeImpl( writer, depth + 1 ) )
                            break;

                        writer.Write( ": " );

                        if( !objValue.Value.SerializeImpl( writer, depth + 1 ) )
                            break;
                    }

                    writer.WriteLine();
                    writer.Indent--;
                    writer.WriteIndent();
                    writer.Write( "}" );
                    break;

                case LuaValueType.Array:
                    writer.WriteLine( "[" );
                    writer.Indent++;

                    foreach( var arrValue in ArrayValue )
                    {
                        if( first )
                        {
                            writer.WriteIndent();
                            first = false;
                        }
                        else
                        {
                            writer.Write( "," );
                            writer.WriteLine();
                            writer.WriteIndent();
                        }

                        if( !arrValue.SerializeImpl( writer, depth + 1 ) )
                            break;
                    }

                    writer.WriteLine();
                    writer.Indent--;
                    writer.WriteIndent();
                    writer.Write( "]" );
                    break;

                case LuaValueType.Number:
                    writer.Write( "{0:R}", _numberValue );
                    break;

                case LuaValueType.String:
                    SerializeString( writer, _stringValue );
                    break;

                default:
                    writer.Write( Type.GetName() );
                    break;
            }

            return true;
        }

        private static void SerializeString( TextWriter writer, string value )
        {
            writer.Write( '"' );

            foreach( var c in value )
            {
                switch( c )
                {
                    case '\\':
                        writer.Write( "\\\\" );
                        break;

                    case '\"':
                        writer.Write( "\\\"" );
                        break;

                    case '\b':
                        writer.Write( "\\b" );
                        break;

                    case '\f':
                        writer.Write( "\\f" );
                        break;

                    case '\n':
                        writer.Write( "\\n" );
                        break;

                    case '\r':
                        writer.Write( "\\r" );
                        break;

                    case '\t':
                        writer.Write( "\\t" );
                        break;

                    default:
                        writer.Write( c );
                        break;
                }
            }

            writer.Write( '"' );
        }
    }
}
