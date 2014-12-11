using System;
using System.Globalization;
using Lua.VirtualMachine;

namespace Lua
{
    public sealed partial class LuaValue
    {
        public static implicit operator LuaValue( bool value )
        {
            return value ? True : False;
        }

        public static implicit operator LuaValue( double value )
        {
            return new LuaValue( value );
        }

        public static implicit operator LuaValue( string value )
        {
            return new LuaValue( value );
        }

        public static implicit operator LuaValue( LuaFunction function )
        {
            return new LuaValue( new Closure( function ) );
        }

        public static implicit operator LuaValue( LuaInstanceFunction function )
        {
            return new LuaValue( new Closure( function ) );
        }

        public static implicit operator bool( LuaValue value )
        {
            switch( value.Type )
            {
                case LuaValueType.Nil:
                case LuaValueType.False:
                    return false;

                case LuaValueType.Number:
                    return !double.IsNaN( value._numberValue );

                default:
                    return true;
            }
        }

        public static bool operator true( LuaValue value )
        {
            return value;
        }

        public static bool operator false( LuaValue value )
        {
            return !value;
        }

        public static implicit operator double( LuaValue value )
        {
            if( value.Type == LuaValueType.Number )
                return value._numberValue;

            throw new LuaRuntimeException( RuntimeError.CantCastTo, value.Type.GetName(), LuaValueType.Number.GetName() );
        }

        public static implicit operator string( LuaValue value )
        {
            return value.ToString();
        }

        public static LuaValue operator +( LuaValue left, LuaValue right )
        {
            if( left.Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( left._numberValue + right._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "addition", left.Type.GetName(), right.Type.GetName() );
        }

        public static LuaValue operator -( LuaValue left, LuaValue right )
        {
            if( left.Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( left._numberValue - right._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "subtraction", left.Type.GetName(), right.Type.GetName() );
        }

        public static LuaValue operator *( LuaValue left, LuaValue right )
        {
            if( left.Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( left._numberValue * right._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "multiplication", left.Type.GetName(), right.Type.GetName() );
        }

        public static LuaValue operator /( LuaValue left, LuaValue right )
        {
            if( left.Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( left._numberValue / right._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "division", left.Type.GetName(), right.Type.GetName() );
        }

        public static LuaValue operator %( LuaValue left, LuaValue right )
        {
            if( left.Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( left._numberValue % right._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "modulo", left.Type.GetName(), right.Type.GetName() );
        }

        public LuaValue Pow( LuaValue right )
        {
            if( Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( Math.Pow( _numberValue, right._numberValue ) );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "exponent", Type.GetName(), right.Type.GetName() );
        }

        public LuaValue LShift( LuaValue right )
        {
            return this << (int)right;
        }

        public LuaValue RShift( LuaValue right )
        {
            return this >> (int)right;
        }

        public static LuaValue operator <<( LuaValue left, int right )
        {
            if( left.Type == LuaValueType.Number )
                return new LuaValue( (int)left._numberValue << right );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "bitwise left shift", left.Type.GetName(), LuaValueType.Number.GetName() );
        }

        public static LuaValue operator >>( LuaValue left, int right )
        {
            if( left.Type == LuaValueType.Number )
                return new LuaValue( (int)left._numberValue >> right );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "bitwise right shift", left.Type.GetName(), LuaValueType.Number.GetName() );
        }

        public static LuaValue operator &( LuaValue left, LuaValue right )
        {
            if( left.Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( (int)left._numberValue & (int)right._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "bitwise and", left.Type.GetName(), right.Type.GetName() );
        }

        public static LuaValue operator |( LuaValue left, LuaValue right )
        {
            if( left.Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( (int)left._numberValue | (int)right._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "bitwise or", left.Type.GetName(), right.Type.GetName() );
        }

        public static LuaValue operator ^( LuaValue left, LuaValue right )
        {
            if( left.Type == LuaValueType.Number && right.Type == LuaValueType.Number )
                return new LuaValue( (int)left._numberValue ^ (int)right._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "bitwise xor", left.Type.GetName(), right.Type.GetName() );
        }

        public static LuaValue operator -( LuaValue value )
        {
            if( value.Type == LuaValueType.Number )
                return new LuaValue( -value._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnType, "negation", value.Type.GetName() );
        }

        public static LuaValue operator ~( LuaValue value )
        {
            if( value.Type == LuaValueType.Number )
                return new LuaValue( ~(int)value._numberValue );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnType, "bitwise not", value.Type.GetName() );
        }

        public static bool operator ==( LuaValue left, LuaValue right )
        {
            if( ReferenceEquals( left, right ) )
                return true;

            if( ReferenceEquals( left, null ) || ReferenceEquals( right, null ) )
                return false;

            return left.Equals( right );
        }

        public static bool operator !=( LuaValue left, LuaValue right )
        {
            return !( left == right );
        }

        public static bool operator >( LuaValue left, LuaValue right )
        {
            switch( left.Type )
            {
                case LuaValueType.Number:
                    if( right.Type != LuaValueType.Number )
                        throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "relational", left.Type.GetName(), right.Type.GetName() );

                    return left._numberValue > right._numberValue;

                case LuaValueType.String:
                    if( right.Type != LuaValueType.String )
                        throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "relational", left.Type.GetName(), right.Type.GetName() );

                    return string.Compare( left._stringValue, right._stringValue, CultureInfo.InvariantCulture, CompareOptions.None ) > 0;

                default:
                    throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "relational", left.Type.GetName(), right.Type.GetName() );
            }
        }

        public static bool operator >=( LuaValue left, LuaValue right )
        {
            return left > right || left == right;
        }

        public static bool operator <( LuaValue left, LuaValue right )
        {
            return !( left >= right );
        }

        public static bool operator <=( LuaValue left, LuaValue right )
        {
            return !( left > right );
        }
    }
}
