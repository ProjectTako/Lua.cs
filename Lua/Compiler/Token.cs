namespace Lua.Compiler
{
    enum TokenType
    {
        Identifier,
        String,
        Number,
        Break,
        Goto,
        Do,
        End,
        While,
        Repeat,
        Until,
        If,
        Then,
        ElseIf,
        Else,
        For,
        In,
        Function,
        Local,
        Return,
        Nil,
        False,
        True,
        And,
        Or,
        Not,
        Semicolon,
        Assign,
        Equals,
        NotEquals,
        Comma,
        Dot,
        Colon,
        Ellipsis,
        Concat,
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Exponent,
        Modulo,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Hash,
        DoubleColon,
        LeftBracket,
        RightBracket,
        LeftParen,
        RightParen,
        LeftBrace,
        RightBrace,
        Eof
    }

    class Token
    {
        public readonly string FileName;
        public readonly int Line;
        public readonly TokenType Type;
        public readonly string Contents;

        public Token( string fileName, int line, TokenType type, string contents )
        {
            FileName = fileName;
            Line = line;
            Type = type;
            Contents = contents;
        }

        public override string ToString()
        {
            switch( Type )
            {
                case TokenType.Identifier:
                case TokenType.Number:
                case TokenType.String:
                    var contentsStr = Contents;
                    if( contentsStr.Length > 16 )
                        contentsStr = contentsStr.Substring( 0, 13 ) + "...";

                    return string.Format( "{0}('{1}')", Type, contentsStr );

                default:
                    return Type.ToString();
            }
        }
    }
}
