using System.Collections.Generic;
using Lua.Compiler.Parselets;
using Lua.Compiler.Parselets.Statements;

namespace Lua.Compiler
{
    partial class Parser
    {
        private static Dictionary<TokenType, IPrefixParselet> _prefixParselets;
        private static Dictionary<TokenType, IInfixParselet> _infixParselets;
        private static Dictionary<TokenType, IStatementParselet> _statementParselets;

        static Parser()
        {
            _prefixParselets = new Dictionary<TokenType, IPrefixParselet>();
            _infixParselets = new Dictionary<TokenType, IInfixParselet>();
            _statementParselets = new Dictionary<TokenType, IStatementParselet>();

            // leaves
            RegisterPrefix( TokenType.Number, new NumberParselet() );
            RegisterPrefix( TokenType.String, new StringParselet() );
            RegisterPrefix( TokenType.Identifier, new IdentifierParselet() );

            RegisterPrefix( TokenType.Nil, new NilParselet() );
            RegisterPrefix( TokenType.True, new BoolParselet( true ) );
            RegisterPrefix( TokenType.False, new BoolParselet( false ) );

            // math operations
            RegisterInfix( TokenType.Addition, new BinaryOperatorParselet( (int)PrecedenceValue.Addition, false ) );
            RegisterInfix( TokenType.Subtraction, new BinaryOperatorParselet( (int)PrecedenceValue.Addition, false ) );
            RegisterInfix( TokenType.Multiplication, new BinaryOperatorParselet( (int)PrecedenceValue.Multiplication, false ) );
            RegisterInfix( TokenType.Division, new BinaryOperatorParselet( (int)PrecedenceValue.Multiplication, false ) );
            RegisterInfix( TokenType.Modulo, new BinaryOperatorParselet( (int)PrecedenceValue.Multiplication, false ) );
            RegisterInfix( TokenType.Exponent, new BinaryOperatorParselet( (int)PrecedenceValue.Multiplication, false ) );
            RegisterPrefix( TokenType.Subtraction, new PrefixOperatorParselet( (int)PrecedenceValue.Prefix ) );

            // conditional operations
            RegisterPrefix( TokenType.Not, new PrefixOperatorParselet( (int)PrecedenceValue.Prefix ) );
            RegisterInfix( TokenType.And, new BinaryOperatorParselet( (int)PrecedenceValue.ConditionalAnd, false ) );
            RegisterInfix( TokenType.Or, new BinaryOperatorParselet( (int)PrecedenceValue.ConditionalOr, false ) );

            // relational operations
            RegisterInfix( TokenType.Equals, new BinaryOperatorParselet( (int)PrecedenceValue.Equality, false ) );
            RegisterInfix( TokenType.NotEquals, new BinaryOperatorParselet( (int)PrecedenceValue.Equality, false ) );
            RegisterInfix( TokenType.GreaterThan, new BinaryOperatorParselet( (int)PrecedenceValue.Relational, false ) );
            RegisterInfix( TokenType.GreaterThanOrEqual, new BinaryOperatorParselet( (int)PrecedenceValue.Relational, false ) );
            RegisterInfix( TokenType.LessThan, new BinaryOperatorParselet( (int)PrecedenceValue.Relational, false ) );
            RegisterInfix( TokenType.LessThanOrEqual, new BinaryOperatorParselet( (int)PrecedenceValue.Relational, false ) );
            RegisterInfix( TokenType.Concat, new BinaryOperatorParselet( (int)PrecedenceValue.Relational, false ) );

            // assignment
            RegisterInfix( TokenType.Assign, new BinaryOperatorParselet( (int)PrecedenceValue.Assign, true ) );

            // other expression stuff
            RegisterPrefix( TokenType.LeftParen, new GroupParselet() );
            RegisterInfix( TokenType.LeftParen, new CallParselet() );
            RegisterInfix( TokenType.Dot, new FieldParselet() );
            RegisterInfix( TokenType.Colon, new FieldParselet() );
            RegisterInfix( TokenType.LeftBracket, new IndexerParselet() );
            RegisterPrefix( TokenType.LeftBrace, new TableParselet() );
            RegisterPrefix( TokenType.Function, new FunctionParselet() );

            // statements
            //RegisterStatement( TokenType.Goto, new GotoParselet() );
            RegisterStatement( TokenType.Semicolon, new SemicolonParselet() );
            RegisterStatement( TokenType.Do, new ScopeParselet() );
            RegisterStatement( TokenType.Function, new FunctionParselet() );
            RegisterStatement( TokenType.Return, new ReturnParselet() );
            RegisterStatement( TokenType.Break, new BreakParselet() );
            RegisterStatement( TokenType.Local, new LocalParselet() );
            RegisterStatement( TokenType.If, new IfParselet() );
            RegisterStatement( TokenType.While, new WhileParselet() );
            RegisterStatement( TokenType.Repeat, new RepeatUntilParselet() );
            RegisterStatement( TokenType.For, new ForParselet() );
        }

        static void RegisterPrefix( TokenType type, IPrefixParselet parselet )
        {
            _prefixParselets.Add( type, parselet );
        }

        static void RegisterInfix( TokenType type, IInfixParselet parselet )
        {
            _infixParselets.Add( type, parselet );
        }

        static void RegisterStatement( TokenType type, IStatementParselet parselet )
        {
            _statementParselets.Add( type, parselet );
        }
    }
}
