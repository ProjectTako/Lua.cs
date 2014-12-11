using System.Collections.Generic;
using Lua.Compiler.Expressions;
using Lua.Compiler.Expressions.Statements;
using Lua.Compiler.Parselets.Statements;

namespace Lua.Compiler.Parselets
{
    class GroupParselet : IPrefixParselet
    {
        public Expression Parse( Parser parser, Token token )
        {
            var expression = parser.ParseExpession();
            parser.Take( TokenType.RightParen );
            return expression;
        }
    }
}
