namespace Lua.Compiler.Expressions.Statements
{
    class BreakExpression : Expression, IStatementExpression
    {
        public BreakExpression( Token token )
            : base( token.FileName, token.Line )
        {

        }

        public override void Print( IndentTextWriter writer )
        {
            writer.WriteIndent();
            writer.WriteLine( "Break" );
        }

        public override int Compile( FunctionContext context )
        {
            context.Line( FileName, Line );

            var target = context.BreakLabel();
            if( target == null )
                throw new LuaCompilerException( FileName, Line, CompilerError.UnresolvedJump );

            return context.Jump( target );
        }

        public override Expression Simplify()
        {
            return this;
        }
    }
}
