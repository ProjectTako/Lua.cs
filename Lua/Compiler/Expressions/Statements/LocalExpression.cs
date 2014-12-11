using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lua.Compiler.Expressions.Statements
{
    class LocalExpression : Expression, IStatementExpression
    {
        public class Declaration
        {
            public string Name { get; private set; }
            public Expression Initializer { get; private set; }

            public Declaration( string name, Expression initializer )
            {
                Name = name;
                Initializer = initializer;
            }
        }

        public ReadOnlyCollection<Declaration> Declarations { get; private set; }

        public LocalExpression( Token token, List<string> identifiers, List<Expression> values )
            : base( token.FileName, token.Line )
        {
            if( values.Count > identifiers.Count )
                values = values.Take( identifiers.Count ).ToList();

            if( identifiers.Count > values.Count )
            {
                var nullToken = new NilExpression( new Token( token.FileName, -1, TokenType.Number, null ) );
                var difference = identifiers.Count - values.Count;
                values = values.Concat( Enumerable.Repeat( nullToken, difference ) ).ToList();
            }

            Declarations = new ReadOnlyCollection<Declaration>( identifiers.Zip( values, ( name, value ) => new Declaration( name, value ) ).ToList() );
        }

        public override void Print( IndentTextWriter writer )
        {
            writer.WriteIndent();
            writer.WriteLine( "Local" );

            foreach( var declaration in Declarations )
            {
                writer.WriteIndent();
                writer.WriteLine( "-" + declaration.Name + ( declaration.Initializer != null ? " =" : "" ) );

                if( declaration.Initializer != null )
                {
                    writer.Indent += 2;
                    declaration.Initializer.Print( writer );
                    writer.Indent -= 2;
                }
            }
        }

        public override int Compile( FunctionContext context )
        {
            context.Line( FileName, Line );

            var stack = 0;
            var shouldBeGlobal = false;

            foreach( var declaration in Declarations )
            {
                var name = declaration.Name;

                if( !shouldBeGlobal )
                {
                    if( !context.DefineIdentifier( name ) )
                        throw new LuaCompilerException( FileName, Line, CompilerError.IdentifierAlreadyDefined, name );
                }

                if( declaration.Initializer == null )
                    continue;

                if( !shouldBeGlobal )
                {
                    var identifier = context.Identifier( name );

                    stack += declaration.Initializer.Compile( context );
                    stack += context.Store( identifier );
                }
                else
                {
                    stack += declaration.Initializer.Compile( context );
                    stack += context.LoadGlobal();
                    stack += context.StoreField( context.String( name ) );
                }
            }

            CheckStack( stack, 0 );
            return stack;
        }

        public override Expression Simplify()
        {
            Declarations = Declarations
                .Select( d => new Declaration( d.Name, d.Initializer == null ? null : d.Initializer.Simplify() ) )
                .ToList()
                .AsReadOnly();

            return this;
        }

        public override void SetParent( Expression parent )
        {
            base.SetParent( parent );

            foreach( var declaration in Declarations.Where( d => d.Initializer != null ) )
            {
                declaration.Initializer.SetParent( this );
            }
        }
    }
}
