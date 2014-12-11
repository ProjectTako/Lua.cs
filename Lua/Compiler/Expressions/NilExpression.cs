namespace Lua.Compiler.Expressions
{
    class NilExpression : Expression, IConstantExpression
    {
        public NilExpression( Token token )
            : base( token.FileName, token.Line )
        {

        }

        public override void Print( IndentTextWriter writer )
        {
            writer.WriteIndent();
            writer.WriteLine( "null" );
        }

        public override int Compile( FunctionContext context )
        {
            context.Line( FileName, Line );

            return context.LoadNil();
        }

        public override Expression Simplify()
        {
            return this;
        }

        public LuaValue GetValue()
        {
            return LuaValue.Nil;
        }
    }
}
