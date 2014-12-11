using System.Collections.Generic;
using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;

namespace Lua.Compiler.Parselets.Statements
{
    class LocalParselet : IStatementParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = true;

            if( parser.MatchAndTake( TokenType.Function ) )
            {
                var arguments = new List<string>();
                var otherArgs = (string)null;
                var name = parser.Take( TokenType.Identifier ).Contents;
                parser.Take( TokenType.LeftParen );

                while( !parser.Match( TokenType.RightParen ) )
                {
                    if( parser.Match( TokenType.Ellipsis ) )
                    {
                        otherArgs = parser.Take().Contents;
                        break;
                    }

                    arguments.Add( parser.Take( TokenType.Identifier ).Contents );

                    if( !parser.MatchAndTake( TokenType.Comma ) )
                        break;
                }

                parser.Take( TokenType.RightParen );
                var body = FunctionParselet.ParseBlock( parser );
                return new FunctionExpression( token, name, arguments, otherArgs, body, true );
            }

            var identifiers = new List<string>();
            var initializers = new List<Expression>();

            do
            {
                var ident = parser.Take( TokenType.Identifier );
                identifiers.Add( ident.Contents );
            } while( parser.MatchAndTake( TokenType.Comma ) );

            if( parser.MatchAndTake( TokenType.Assign ) )
            {
                do
                {
                    var value = parser.ParseExpession();
                    initializers.Add( value );
                } while( parser.MatchAndTake( TokenType.Comma ) );
            }

            return new LocalExpression( token, identifiers, initializers );
        }
    }
}
