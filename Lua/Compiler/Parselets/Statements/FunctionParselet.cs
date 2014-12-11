using System.Collections.Generic;
using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;

namespace Lua.Compiler.Parselets.Statements
{
    class FunctionParselet : IStatementParselet, IPrefixParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = false;

            string name = null;
            var arguments = new List<string>();
            string otherArgs = null;
            BlockExpression body;

            // optional name
            if( parser.Match( TokenType.Identifier ) )
            {
                name = parser.Take( TokenType.Identifier ).Contents;
            }

            // parse argument list
            parser.Take( TokenType.LeftParen );

            if( !parser.Match( TokenType.RightParen ) )
            {
                while( true )
                {
                    if( parser.MatchAndTake( TokenType.Ellipsis ) )
                    {
                        otherArgs = parser.Take( TokenType.Identifier ).Contents;
                        break;
                    }

                    var identifier = parser.Take( TokenType.Identifier );
                    arguments.Add( identifier.Contents );

                    if( parser.Match( TokenType.RightParen ) )
                        break;

                    parser.Take( TokenType.Comma );
                }
            }

            parser.Take( TokenType.RightParen );
            body = ParseBlock( parser );

            return new FunctionExpression( token, name, arguments, otherArgs, body );
        }

        internal static BlockExpression ParseBlock( Parser parser )
        {
            var statements = new List<Expression>();

            while( !parser.Match( TokenType.End ) )
                statements.Add( parser.ParseStatement() );

            parser.Take( TokenType.End );
            return new BlockExpression( statements );
        }

        public Expression Parse( Parser parser, Token token )
        {
            bool hasTrailingSemicolon;
            return Parse( parser, token, out hasTrailingSemicolon );
        }
    }
}
