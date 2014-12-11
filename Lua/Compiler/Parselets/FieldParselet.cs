using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets
{
    class FieldParselet : IInfixParselet
    {
        public int Precedence { get { return (int)PrecedenceValue.Postfix; } }

        public Expression Parse( Parser parser, Expression left, Token token )
        {
            if( token.Type == TokenType.Colon )
                throw new System.NotImplementedException();

            var identifier = parser.Take( TokenType.Identifier );
            return new FieldExpression( identifier, left );
        }
    }
}
