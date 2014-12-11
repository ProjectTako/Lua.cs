using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lua.Compiler.Expressions
{
    class ObjectExpression : Expression
    {
        public ReadOnlyCollection<KeyValuePair<Expression, Expression>> Values { get; private set; }

        public ObjectExpression( Token token, List<KeyValuePair<Expression, Expression>> values )
            : base( token.FileName, token.Line )
        {
            Values = values.AsReadOnly();
        }

        public override void Print( IndentTextWriter writer )
        {
            writer.WriteIndent();
            writer.WriteLine( "Table" );

            foreach( var value in Values )
            {
                writer.WriteIndent();

                if( value.Key is IdentifierExpression )
                    writer.WriteLine( "- {0}", ( value.Key as IdentifierExpression ).Name );
                else if( value.Key is StringExpression )
                    writer.WriteLine( "- {0}", ( value.Key as StringExpression ).Value );
                else
                    writer.WriteLine( "- [{0}]", value.Key.GetType().Name );

                writer.Indent += 2;
                value.Value.Print( writer );
                writer.Indent -= 2;
            }
        }

        public override int Compile( FunctionContext context )
        {
            context.Line( FileName, Line );

            var stack = 0;

            foreach( var value in Values )
            {
                stack += value.Value.Compile( context );
                stack += value.Key.Compile( context );
            }

            stack += context.NewObject( Values.Count );
            System.Console.WriteLine( stack );

            CheckStack( stack, 1 );
            return stack;
        }

        public override Expression Simplify()
        {
            Values = Values
                .Select( v => new KeyValuePair<Expression, Expression>( v.Key.Simplify(), v.Value.Simplify() ) )
                .ToList()
                .AsReadOnly();

            return this;
        }

        public override void SetParent( Expression parent )
        {
            base.SetParent( parent );

            foreach( var value in Values )
            {
                value.Value.SetParent( this );
            }
        }
    }
}
