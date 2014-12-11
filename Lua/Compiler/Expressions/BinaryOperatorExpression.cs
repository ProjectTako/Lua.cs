using System;
using System.Collections.Generic;

namespace Lua.Compiler.Expressions
{
    class BinaryOperatorExpression : Expression
    {
        public TokenType Operation { get; private set; }
        public Expression Left { get; private set; }
        public Expression Right { get; private set; }

        public BinaryOperatorExpression( Token token, Expression left, Expression right )
            : base( token.FileName, token.Line )
        {
            Operation = token.Type;
            Left = left;
            Right = right;
        }

        public override void Print( IndentTextWriter writer )
        {
            writer.WriteIndent();
            writer.WriteLine( "Operator {0}", Operation );

            writer.Indent++;
            Left.Print( writer );
            Right.Print( writer );
            writer.Indent--;
        }

        public override int Compile( FunctionContext context )
        {
            context.Line( FileName, Line );

            var stack = 0;

            if( IsAssign )
            {
                var storable = Left as IStorableExpression;
                if( storable == null )
                    throw new LuaCompilerException( FileName, Line, CompilerError.LeftSideMustBeStorable );

                var needResult = !( Parent is IBlockExpression );

                stack += Right.Compile( context );

                if( needResult )
                    stack += context.Dup();

                stack += storable.CompileStore( context );

                CheckStack( stack, needResult ? 1 : 0 );
                return stack;
            }

            if( Operation == TokenType.Or )
            {
                var endOr = context.MakeLabel( "endOr" );
                stack += Left.Compile( context );
                stack += context.JumpTruePeek( endOr );
                stack += context.Drop();
                stack += Right.Compile( context );
                stack += context.Bind( endOr );

                CheckStack( stack, 1 );
                return stack;
            }

            if( Operation == TokenType.And )
            {
                var endAnd = context.MakeLabel( "endAnd" );
                stack += Left.Compile( context );
                stack += context.JumpFalsePeek( endAnd );
                stack += context.Drop();
                stack += Right.Compile( context );
                stack += context.Bind( endAnd );

                CheckStack( stack, 1 );
                return stack;
            }

            stack += Right.Compile( context );
            stack += Left.Compile( context );
            stack += context.BinaryOperation( Operation );

            CheckStack( stack, 1 );
            return stack;
        }

        public override Expression Simplify()
        {
            Left = Left.Simplify();
            Right = Right.Simplify();

            Func<double, double, double> simplifyOp;
            if( _simplifyMap.TryGetValue( Operation, out simplifyOp ) )
            {
                var leftNum = Left as NumberExpression;
                var rightNum = Right as NumberExpression;

                if( leftNum != null && rightNum != null )
                {
                    var result = simplifyOp( leftNum.Value, rightNum.Value );
                    var token = new Token( FileName, Line, TokenType.Number, null );
                    return new NumberExpression( token, result );
                }
            }

            return this;
        }

        public override void SetParent( Expression parent )
        {
            base.SetParent( parent );

            Left.SetParent( this );
            Right.SetParent( this );
        }

        public bool IsAssign
        {
            get
            {
                return Operation == TokenType.Assign;
            }
        }

        private static Dictionary<TokenType, Func<double, double, double>> _simplifyMap;

        static BinaryOperatorExpression()
        {
            _simplifyMap = new Dictionary<TokenType, Func<double, double, double>>
            {
                { TokenType.Addition, (x, y) => x + y },
                { TokenType.Subtraction, (x, y) => x - y },
                { TokenType.Multiplication, (x, y) => x * y },
                { TokenType.Division, (x, y) => x / y },
                { TokenType.Modulo, (x, y) => x % y },
                { TokenType.Exponent, (x, y) => Math.Pow(x, y) },
            };
        }
    }
}
