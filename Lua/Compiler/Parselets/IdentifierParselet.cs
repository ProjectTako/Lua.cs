using System.Collections.Generic;
using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;
using Lua.Compiler.Parselets.Statements;

namespace Lua.Compiler.Parselets
{
    class IdentifierParselet : IPrefixParselet
    {
        public Expression Parse( Parser parser, Token token )
        {
            return new IdentifierExpression( token );
        }
    }
}
