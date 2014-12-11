using System.Collections.Generic;
using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets.Statements
{
    class ScopeParselet : IStatementParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = false;

            var statements = new List<Expression>();

            while( !parser.Match( TokenType.End ) )
                statements.Add( parser.ParseStatement() );

            parser.Take( TokenType.End );
            return new ScopeExpression( statements );
        }
    }
}
