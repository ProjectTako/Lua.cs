using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Lua.Compiler;
using Lua.Compiler.Expressions;
using Lua.VirtualMachine;

namespace Lua
{
    public sealed class LuaProgram
    {
        private const uint MagicId = 0xFA57C0DE;
        private const byte FormatVersion = 1;

        internal readonly byte[] Bytecode;
        internal readonly List<LuaValue> Numbers;
        internal readonly List<LuaValue> Strings;
        internal readonly DebugInfo DebugInfo;

        internal LuaProgram( byte[] bytecode, IEnumerable<double> numbers, IEnumerable<string> strings, DebugInfo debugInfo = null )
        {
            Bytecode = bytecode;
            Numbers = numbers.Select( n => new LuaValue( n ) ).ToList();
            Strings = strings.Select( s => new LuaValue( s ) ).ToList();
            DebugInfo = debugInfo;
        }

        /// <summary>
        /// Writes the compiled Lua program to the specified file.
        /// </summary>
        /// <param name="path">File to write the bytecode to.</param>
        public void SaveBytecode( string path )
        {
            using( var fs = File.Open( path, FileMode.Create, FileAccess.Write, FileShare.None ) )
            {
                SaveBytecode( fs );
            }
        }

        /// <summary>
        /// Writes the compiled Lua program to the specified stream.
        /// </summary>
        /// <param name="output">Stream to write to.</param>
        public void SaveBytecode( Stream output )
        {
            using( var writer = new BinaryWriter( output, Encoding.UTF8, true ) )
            {
                writer.Write( MagicId );
                writer.Write( FormatVersion );
                writer.Write( DebugInfo != null );
                writer.Write( Strings.Count );

                foreach( var str in Strings )
                    writer.Write( str.ToString() );

                writer.Write( Numbers.Count );

                foreach( var num in Numbers )
                    writer.Write( (double)num );

                writer.Write( Bytecode.Length );
                writer.Write( Bytecode, 0, Bytecode.Length );

                if( DebugInfo != null )
                {
                    writer.Write( DebugInfo.Functions.Count );

                    foreach( var function in DebugInfo.Functions )
                    {
                        writer.Write( function.Address );
                        writer.Write( function.Name );
                    }

                    writer.Write( DebugInfo.Lines.Count );

                    foreach( var line in DebugInfo.Lines )
                    {
                        writer.Write( line.Address );
                        writer.Write( line.FileName );
                        writer.Write( line.LineNumber );
                    }
                }

                writer.Flush();
            }
        }

        /// <summary>
        /// Load a Lua source code from a file and return the compiled program.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="options">Compiler options.</param>
        public static LuaProgram FromFile( string path, LuaCompilerOptions options = null )
        {
            var source = File.ReadAllText( path, Encoding.UTF8 );
            var name = Path.GetFileName( path );
            return Compile( source, name, options );
        }

        /// <summary>
        /// Loads Lua bytecode from the specified file.
        /// </summary>
        /// <param name="path">The file to load.</param>
        public static LuaProgram LoadBytecode( string path )
        {
            using( var fs = File.Open( path, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            {
                return LoadBytecode( fs );
            }
        }

        /// <summary>
        /// Loads Lua bytecode from the specified stream.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        public static LuaProgram LoadBytecode( Stream stream )
        {
            using( var reader = new BinaryReader( stream, Encoding.UTF8, true ) )
            {
                if( reader.ReadUInt32() != MagicId )
                    throw new NotSupportedException( "Input is not valid." );

                byte version;
                if( ( version = reader.ReadByte() ) != FormatVersion )
                    throw new NotSupportedException( string.Format( "Wrong version. Expected 0x{0:X2}, got 0x{1:X2}.", FormatVersion, version ) );

                var hasDebugInfo = reader.ReadBoolean();

                var stringCount = reader.ReadInt32();
                var strings = new List<string>( stringCount );

                for( var i = 0; i < stringCount; ++i )
                    strings.Add( reader.ReadString() );

                var numberCount = reader.ReadInt32();
                var numbers = new List<double>( numberCount );

                for( var i = 0; i < numberCount; ++i )
                    numbers.Add( reader.ReadDouble() );

                var bytecodeLength = reader.ReadInt32();
                var bytecode = reader.ReadBytes( bytecodeLength );

                var debugInfo = (DebugInfo)null;
                if( hasDebugInfo )
                {
                    var functionCount = reader.ReadInt32();
                    var functions = new List<DebugInfo.Function>( functionCount );

                    for( var i = 0; i < functionCount; ++i )
                    {
                        var address = reader.ReadInt32();
                        var name = reader.ReadInt32();
                        var function = new DebugInfo.Function( address, name );
                        functions.Add( function );
                    }

                    var lineCount = reader.ReadInt32();
                    var lines = new List<DebugInfo.Line>( lineCount );

                    for( var i = 0; i < lineCount; ++i )
                    {
                        var address = reader.ReadInt32();
                        var fileName = reader.ReadInt32();
                        var lineNumber = reader.ReadInt32();
                        var line = new DebugInfo.Line( address, fileName, lineNumber );
                        lines.Add( line );
                    }

                    debugInfo = new DebugInfo( functions, lines );
                }

                return new LuaProgram( bytecode, numbers, strings, debugInfo );
            }
        }

        /// <summary>
        /// Compile a Lua program from a stream of characters.
        /// </summary>
        /// <param name="source">Source code to compile</param>
        /// <param name="fileName">Optional file name to use in errors</param>
        /// <param name="options">Compiler options</param>
        public static LuaProgram Compile( IEnumerable<char> source, string fileName = null, LuaCompilerOptions options = null )
        {
            var lexer = new Lexer( source, fileName );
            var parser = new Parser( lexer );
            return CompileImpl( parser.ParseAll(), options );
        }

        /// <summary>
        /// Compiles statements from an infinite stream of characters.
        /// This should only be useful when implementing REPLs.
        /// </summary>
        /// <param name="source">Source code to compile</param>
        /// <param name="fileName">Optional file name to use in errors</param>
        /// <param name="options">Compiler options</param>
        public static IEnumerable<LuaProgram> CompileStatements( IEnumerable<char> source, string fileName = null, LuaCompilerOptions options = null )
        {
            var lexer = new Lexer( source, fileName );
            var parser = new Parser( lexer );

            while( true )
            {
                var expression = new BlockExpression( new[]
                {
                    parser.ParseStatement()
                } );

                yield return CompileImpl( expression, options ); ;
            }
        }

        private static LuaProgram CompileImpl( Expression expression, LuaCompilerOptions options )
        {
            options = options ?? new LuaCompilerOptions();

            expression.SetParent( null );
            expression.Simplify();

            using( var writer = new IndentTextWriter( Console.Out, " " ) )
                expression.Print( writer );

            var compiler = new ExpressionCompiler( options );
            return compiler.Compile( expression );
        }
    }
}
