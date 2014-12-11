namespace Lua.Compiler.Expressions
{
    class IdentifierExpression : Expression, IStorableExpression
    {
        public string Name { get; private set; }

        public IdentifierExpression( Token token )
            : base( token.FileName, token.Line )
        {
            Name = token.Contents;
        }

        public override void Print( IndentTextWriter writer )
        {
            writer.WriteIndent();
            writer.WriteLine( "identifier: {0}", Name );
        }

        public override int Compile( FunctionContext context )
        {
            context.Line( FileName, Line );

            var stack = 0;
            var identifier = context.Identifier( Name );

            if( !context.Compiler.Options.UseImplicitGlobals && identifier == null )
                throw new LuaCompilerException( FileName, Line, CompilerError.UndefinedIdentifier, Name );

            if( identifier == null )
            {
                stack += context.LoadGlobal();
                stack += context.LoadField( context.String( Name ) );
            }
            else
            {
                stack += context.Load( identifier );
            }

            CheckStack( stack, 1 );
            return stack;
        }

        public int CompileStore( FunctionContext context )
        {
            var stack = 0;
            var identifier = context.Identifier( Name );

            if( !context.Compiler.Options.UseImplicitGlobals && identifier == null )
                throw new LuaCompilerException( FileName, Line, CompilerError.UndefinedIdentifier, Name );

            if( identifier == null )
            {
                stack += context.LoadGlobal();
                stack += context.StoreField( context.String( Name ) );
            }
            else
            {
                if( identifier.IsReadOnly )
                    throw new LuaCompilerException( FileName, Line, CompilerError.CantModifyReadonlyVar, Name );

                stack += context.Store( identifier );
            }

            return stack;
        }

        public override Expression Simplify()
        {
            return this;
        }
    }
}
