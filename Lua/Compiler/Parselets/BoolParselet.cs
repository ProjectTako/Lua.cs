using Lua.Compiler.Expressions;

namespace Lua.Compiler.Parselets
{
    class BoolParselet : IPrefixParselet
    {
        private readonly bool _value;

        public BoolParselet( bool value )
        {
            _value = value;
        }

        public Expression Parse( Parser parser, Token token )
        {
            return new BoolExpression( token, _value );
        }
    }
}
