using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lua.Compiler.Expressions.Statements
{
    class FunctionExpression : Expression, IStatementExpression
    {
        public string Name { get; private set; }
        public ReadOnlyCollection<string> Arguments { get; private set; }
        public string OtherArguments { get; private set; }
        public BlockExpression Block { get; private set; }
        public bool IsLocal { get; private set; }

        public string DebugName { get; private set; }

        public FunctionExpression( Token token, string name, List<string> arguments, string otherArgs, BlockExpression block, bool isLocal = false, string debugName = null )
            : base( token.FileName, token.Line )
        {

            Name = name;
            Arguments = arguments.AsReadOnly();
            OtherArguments = otherArgs;
            Block = block;
            IsLocal = isLocal;
            DebugName = debugName;
        }

        public override void Print( IndentTextWriter writer )
        {
            writer.WriteIndent();
            writer.WriteLine( "Function " + Name );

            writer.WriteIndent();
            writer.WriteLine( "-Arguments: {0}", string.Join( ", ", Arguments ) );

            if( OtherArguments != null )
            {
                writer.WriteIndent();
                writer.WriteLine( "-Other Arguments: {0}", OtherArguments );
            }

            writer.WriteIndent();
            writer.WriteLine( "-Block" );

            writer.Indent += 2;
            Block.Print( writer );
            writer.Indent -= 2;
        }

        public virtual void CompileBody( FunctionContext context )
        {
            var stack = 0;

            stack += context.Bind( context.Label );
            stack += context.Enter();

            if( OtherArguments != null )
                stack += context.VarArgs( Arguments.Count );

            stack += Block.Compile( context );
            stack += context.LoadNil();
            stack += context.Return();

            CheckStack( stack, 0 );
        }

        public override int Compile( FunctionContext context )
        {
            var isStatement = Parent is IBlockExpression;
            var shouldBeGlobal = !IsLocal;

            if( Name == null && isStatement )
                throw new LuaCompilerException( FileName, Line, CompilerError.FunctionNeverUsed );

            IdentifierOperand identifier = null;

            if( Name != null && !shouldBeGlobal )
            {
                if( !context.DefineIdentifier( Name ) )
                    throw new LuaCompilerException( FileName, Line, CompilerError.IdentifierAlreadyDefined, Name );

                identifier = context.Identifier( Name );
            }

            // compile body
            var functionContext = context.MakeFunction( Name ?? DebugName );
            functionContext.Function( functionContext.FullName );
            functionContext.Line( FileName, Line );
            functionContext.PushScope();

            for( var i = 0; i < Arguments.Count; i++ )
            {
                var name = Arguments[i];

                if( !functionContext.DefineArgument( i, name ) )
                    throw new LuaCompilerException( FileName, Line, CompilerError.IdentifierAlreadyDefined, name );
            }

            if( OtherArguments != null && !functionContext.DefineArgument( Arguments.Count, OtherArguments ) )
                throw new LuaCompilerException( FileName, Line, CompilerError.IdentifierAlreadyDefined, OtherArguments );

            CompileBody( functionContext );
            functionContext.PopScope();

            // assign result
            var stack = 0;
            stack += context.Closure( functionContext.Label );

            if( Name != null )
            {
                if( !isStatement ) // statements should leave nothing on the stack
                    stack += context.Dup();

                if( !shouldBeGlobal )
                {
                    stack += context.Store( identifier );
                }
                else
                {
                    stack += context.LoadGlobal();
                    stack += context.StoreField( context.String( Name ) );
                }

                if( isStatement )
                {
                    CheckStack( stack, 0 );
                    return stack;
                }
            }

            CheckStack( stack, 1 );
            return stack;
        }

        public override Expression Simplify()
        {
            Block = (BlockExpression)Block.Simplify();

            return this;
        }

        public override void SetParent( Expression parent )
        {
            base.SetParent( parent );

            Block.SetParent( this );
        }
    }
}
