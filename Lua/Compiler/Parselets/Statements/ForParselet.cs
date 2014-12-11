using System.Collections.Generic;
using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;

namespace Lua.Compiler.Parselets.Statements
{
    class ForParselet : IStatementParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = false;

            var ident = parser.Take( TokenType.Identifier );

            if( parser.MatchAndTake( TokenType.Assign ) )
                return ParseNumericFor( parser, ident );
            else
                return ParseForIn( parser, token );
        }

        private Expression ParseNumericFor( Parser parser, Token token )
        {
            var start = parser.ParseExpession();
            parser.Take( TokenType.Comma );
            var end = parser.ParseExpession();
            Expression step;

            if( parser.MatchAndTake( TokenType.Comma ) )
                step = parser.ParseExpession();
            else
            {
                var next = parser.Peek();
                var stepToken = new Token( next.FileName, next.Line, TokenType.Number, "1" );
                step = new NumberExpression( stepToken, 1 );
            }

            var block = parser.ParseStatementBlock();
            return null;
        }

        private Expression ParseForIn( Parser parser, Token token )
        {
            var names = new List<string>();
            names.Add( token.Contents );

            while( parser.MatchAndTake( TokenType.Comma ) )
                names.Add( parser.Take( TokenType.Identifier ).Contents );

            parser.Take( TokenType.In );
            var interable = parser.ParseExpession();
            var block = parser.ParseStatementBlock();

            return null;
        }
    }
}
