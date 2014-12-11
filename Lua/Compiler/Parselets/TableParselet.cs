using System.Collections.Generic;
using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets
{
    class TableParselet : IPrefixParselet
    {
        public Expression Parse( Parser parser, Token token )
        {
            var values = new List<KeyValuePair<Expression, Expression>>();

            while( !parser.Match( TokenType.RightBrace ) )
            {
                Expression key = null, value = null;

                if( parser.MatchAndTake( TokenType.LeftBracket ) )
                {
                    key = parser.ParseExpession();
                    parser.Take( TokenType.RightBracket );
                }
                else if( parser.Match( TokenType.Identifier ) )
                {
                    var keyToken = parser.Take();
                    key = new StringExpression( keyToken, keyToken.Contents );
                }
                else
                {
                    var errorToken = parser.Take();
                    throw new LuaCompilerException( errorToken.FileName, errorToken.Line, CompilerError.ExpectedButFound2, TokenType.Identifier, TokenType.LeftBracket, errorToken );
                }

                parser.Take( TokenType.Assign );
                value = parser.ParseExpession();

                values.Add( new KeyValuePair<Expression, Expression>( key, value ) );

                if( !parser.Match( TokenType.Comma ) )
                    break;

                parser.Take( TokenType.Comma );
            }

            parser.Take( TokenType.RightBrace );

            return new ObjectExpression( token, values );
        }
    }
}
