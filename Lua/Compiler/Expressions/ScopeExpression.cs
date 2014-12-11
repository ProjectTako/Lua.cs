using System.Collections.Generic;

namespace Lua.Compiler.Expressions
{
    class ScopeExpression : BlockExpression
    {
        public ScopeExpression( BlockExpression block )
            : base( block.Statements )
        {

        }

        public ScopeExpression( IList<Expression> statements )
            : base( statements )
        {

        }

        public override int Compile( FunctionContext context )
        {
            context.PushScope();
            var stack = base.Compile( context );
            context.PopScope();

            return stack;
        }
    }
}
