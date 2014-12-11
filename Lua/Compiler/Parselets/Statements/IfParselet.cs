using System.Collections.Generic;
using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;

namespace Lua.Compiler.Parselets.Statements
{
    class IfParselet : IStatementParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = false;

            var branches = new List<IfExpression.Branch>();
            IfExpression.Branch elseBranch = null;

            Expression condition = parser.ParseExpession();
            ScopeExpression block = ParseBlock( parser );
            branches.Add( new IfExpression.Branch( condition, block ) );

            while( parser.MatchAndTake( TokenType.ElseIf ) )
            {
                condition = parser.ParseExpession();
                block = ParseBlock( parser );
                branches.Add( new IfExpression.Branch( condition, block ) );
            }

            if( parser.MatchAndTake( TokenType.Else ) )
            {
                condition = parser.ParseExpession();
                block = ParseBlock( parser, false );
                elseBranch = new IfExpression.Branch( condition, block );
            }

            return new IfExpression( token, branches, elseBranch );
        }

        private ScopeExpression ParseBlock( Parser parser, bool matchThen = true )
        {
            var statements = new List<Expression>();

            if( matchThen )
                parser.Take( TokenType.Then );

            while( !parser.Match( TokenType.End ) )
                statements.Add( parser.ParseStatement() );

            parser.Take( TokenType.End );
            return new ScopeExpression( new BlockExpression( statements ) ); ;
        }
    }
}
