using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets.Statements
{
    class SemicolonParselet : IStatementParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = false;
            return new EmptyExpression( token );
        }
    }
}
