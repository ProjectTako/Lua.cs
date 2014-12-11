using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets
{
    interface IInfixParselet
    {
        int Precedence { get; }

        Expression Parse( Parser parser, Expression left, Token token );
    }
}
