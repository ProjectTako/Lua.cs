using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets
{
    interface IPrefixParselet
    {
        Expression Parse( Parser parser, Token token );
    }
}
