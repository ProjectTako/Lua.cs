using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets
{
    class NilParselet : IPrefixParselet
    {
        public Expression Parse( Parser parser, Token token )
        {
            return new NilExpression( token );
        }
    }
}
