using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets.Statements
{
    interface IStatementParselet
    {
        Expression Parse( Parser parser, Token token, out bool trailingSemicolon );
    }
}
