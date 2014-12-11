using Lua.Compiler.Expressions;
using System.Globalization;

namespace Lua.Compiler.Parselets
{
    class NumberParselet : IPrefixParselet
    {
        public Expression Parse( Parser parser, Token token )
        {
            double value = double.Parse( token.Contents, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture );
            return new NumberExpression( token, value );
        }
    }
}
