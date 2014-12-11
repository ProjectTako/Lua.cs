using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;

namespace Lua.Compiler.Parselets.Statements
{
    class BreakParselet : IStatementParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = true;
            return new BreakExpression( token );
        }
    }
}
