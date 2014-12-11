using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets
{
    class StringParselet : IPrefixParselet
    {
        public Expression Parse( Parser parser, Token token )
        {
            return new StringExpression( token, token.Contents );
        }
    }
}
