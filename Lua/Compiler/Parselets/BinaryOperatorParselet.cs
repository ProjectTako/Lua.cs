using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets
{
    class BinaryOperatorParselet : IInfixParselet
    {
        private readonly int _precedence;
        private readonly bool _isRight;

        public int Precedence { get { return _precedence; } }

        public BinaryOperatorParselet( int precedence, bool isRight )
        {
            _precedence = precedence;
            _isRight = isRight;
        }

        public Expression Parse( Parser parser, Expression left, Token token )
        {
            var right = parser.ParseExpession( Precedence - ( _isRight ? 1 : 0 ) );
            return new BinaryOperatorExpression( token, left, right );
        }
    }
}
