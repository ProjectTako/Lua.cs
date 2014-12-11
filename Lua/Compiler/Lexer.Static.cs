using System;
using System.Collections;
using System.Collections.Generic;

namespace Lua.Compiler
{
    partial class Lexer
    {
        private static OperatorDictionary _operators;
        private static Dictionary<string, TokenType> _keywords;
        private static HashSet<char> _hexChars;

        static Lexer()
        {
            _operators = new OperatorDictionary
            {
                { "...", TokenType.Ellipsis },
                { "<=", TokenType.LessThanOrEqual },
                { ">=", TokenType.GreaterThanOrEqual },
                { "==", TokenType.Equals },
                { "~=", TokenType.NotEquals },
                { "..", TokenType.Concat },
                { "::", TokenType.DoubleColon },
                { "+", TokenType.Addition },
                { "-", TokenType.Subtraction },
                { "*", TokenType.Multiplication },
                { "/", TokenType.Division },
                { "^", TokenType.Exponent },
                { "%", TokenType.Modulo },
                { "<", TokenType.LessThan },
                { ">", TokenType.GreaterThan },
                { "#", TokenType.Hash },
                { "[", TokenType.LeftBracket },
                { "]", TokenType.RightBracket },
                { "{", TokenType.LeftBrace },
                { "}", TokenType.RightBrace },
                { "(", TokenType.LeftParen },
                { ")", TokenType.RightParen },
                { "=", TokenType.Assign },
                { ":", TokenType.Colon },
                { ",", TokenType.Comma },
                { ";", TokenType.Semicolon },
                { ".", TokenType.Dot },
            };

            _keywords = new Dictionary<string, TokenType>
            {
                { "break", TokenType.Break },
                { "goto", TokenType.Goto },
                { "do", TokenType.Do },
                { "end", TokenType.End },
                { "while", TokenType.While },
                { "repeat", TokenType.Repeat },
                { "until", TokenType.Until },
                { "if", TokenType.If },
                { "then", TokenType.Then },
                { "elseif", TokenType.ElseIf },
                { "else", TokenType.Else },
                { "for", TokenType.For },
                { "in", TokenType.In },
                { "function", TokenType.Function },
                { "local", TokenType.Local },
                { "nil", TokenType.Nil },
                { "true", TokenType.True },
                { "false", TokenType.False },
                { "not", TokenType.Not },
                { "and", TokenType.And },
                { "or", TokenType.Or },
                { "return", TokenType.Return },
            };

            _hexChars = new HashSet<char>
            {
                'a', 'b', 'c', 'd', 'e', 'f',
                'A', 'B', 'C', 'D', 'E', 'F',
            };
        }

        class OperatorDictionary : IEnumerable<object>
        {
            private readonly GenericComparer<Tuple<string, TokenType>> _comparer;
            private Dictionary<char, List<Tuple<string, TokenType>>> _operatorDictionary;

            public OperatorDictionary()
            {
                _comparer = new GenericComparer<Tuple<string, TokenType>>( ( a, b ) => b.Item1.Length - a.Item1.Length );
                _operatorDictionary = new Dictionary<char, List<Tuple<string, TokenType>>>();
            }

            public void Add( string op, TokenType type )
            {
                List<Tuple<string, TokenType>> list;
                if( !_operatorDictionary.TryGetValue( op[0], out list ) )
                {
                    list = new List<Tuple<string, TokenType>>();
                    _operatorDictionary.Add( op[0], list );
                }

                list.Add( Tuple.Create( op, type ) );
                list.Sort( _comparer );
            }

            public IEnumerable<Tuple<string, TokenType>> Lookup( char ch )
            {
                List<Tuple<string, TokenType>> list;
                if( !_operatorDictionary.TryGetValue( ch, out list ) )
                    return null;

                return list;
            }

            public IEnumerator<object> GetEnumerator()
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
