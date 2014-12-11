using System.Collections.Generic;
using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;

namespace Lua.Compiler.Parselets.Statements
{
    class RepeatUntilParselet : IStatementParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = true;

            var block = ParseBlock( parser );

            parser.Take( TokenType.Until );

            var condition = parser.ParseExpession();

            return new RepeatUntilExpression( token, block, condition );
        }

        private ScopeExpression ParseBlock( Parser parser )
        {
            var statements = new List<Expression>();

            while( !parser.Match( TokenType.Until ) )
                statements.Add( parser.ParseStatement() );

            return new ScopeExpression( new BlockExpression( statements ) );
        }
    }
}
