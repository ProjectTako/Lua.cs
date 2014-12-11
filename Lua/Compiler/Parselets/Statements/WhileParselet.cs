using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;

namespace Lua.Compiler.Parselets.Statements
{
    class WhileParselet : IStatementParselet
    {
        public Expression Parse( Parser parser, Token token, out bool trailingSemicolon )
        {
            trailingSemicolon = false;

            var condition = parser.ParseExpession();

            var block = new ScopeExpression( parser.ParseStatementBlock() );
            return new WhileExpression( token, condition, block );
        }
    }
}
