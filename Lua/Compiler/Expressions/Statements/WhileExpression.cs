namespace Lua.Compiler.Expressions.Statements
{
    class WhileExpression : Expression, IStatementExpression
    {
        public Expression Condition { get; private set; }
        public BlockExpression Block { get; private set; }

        public WhileExpression( Token token, Expression condition, BlockExpression block )
            : base( token.FileName, token.Line )
        {
            Condition = condition;
            Block = block;
        }

        public override void Print( IndentTextWriter writer )
        {
            writer.WriteIndent();
            writer.WriteLine( "While" );

            writer.WriteIndent();
            writer.WriteLine( "-Condition" );

            writer.Indent += 2;
            Condition.Print( writer );
            writer.Indent -= 2;

            writer.WriteIndent();
            writer.WriteLine( "-Do" );

            writer.Indent += 2;
            Block.Print( writer );
            writer.Indent -= 2;
        }

        public override int Compile( FunctionContext context )
        {
            context.Line( FileName, Line );

            var stack = 0;
            var start = context.MakeLabel( "whileStart" );
            var end = context.MakeLabel( "whileEnd" );

            var boolExpression = Condition as BoolExpression;
            var isInfinite = boolExpression != null && boolExpression.Value;

            stack += context.Bind( start );

            if( !isInfinite )
            {
                stack += Condition.Compile( context );
                stack += context.JumpFalse( end );
            }

            context.PushLoop( start, end );
            stack += Block.Compile( context );
            context.PopLoop();

            stack += context.Jump( start );
            stack += context.Bind( end );

            CheckStack( stack, 0 );
            return stack;
        }

        public override Expression Simplify()
        {
            Condition = Condition.Simplify();
            Block = (BlockExpression)Block.Simplify();

            return this;
        }

        public override void SetParent( Expression parent )
        {
            base.SetParent( parent );

            Condition.SetParent( this );
            Block.SetParent( this );
        }
    }
}
