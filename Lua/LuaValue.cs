using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Lua.VirtualMachine;

namespace Lua
{
    [StructLayout( LayoutKind.Explicit )]
    public sealed partial class LuaValue : IEquatable<LuaValue>
    {
        public static readonly LuaValue Nil = new LuaValue( LuaValueType.Nil );
        public static readonly LuaValue True = new LuaValue( LuaValueType.True );
        public static readonly LuaValue False = new LuaValue( LuaValueType.False );

        [FieldOffset( 0 )]
        public readonly LuaValueType Type;

        [FieldOffset( 8 )]
        private readonly double _numberValue;

        [FieldOffset( 16 )]
        internal readonly VirtualMachine.Table ObjectValue;

        [FieldOffset( 16 )]
        internal readonly List<LuaValue> ArrayValue;

        [FieldOffset( 16 )]
        private readonly string _stringValue;

        [FieldOffset( 16 )]
        internal readonly Closure FunctionValue;

        /// <summary>
        /// Construct a new LuaValue. Should only be used for Object or Array.
        /// </summary>
        public LuaValue( LuaValueType type )
        {
            Type = type;

            switch( type )
            {
                case LuaValueType.Nil:
                case LuaValueType.True:
                case LuaValueType.False:
                    break;

                case LuaValueType.Object:
                    ObjectValue = new VirtualMachine.Table();
                    break;

                case LuaValueType.Array:
                    ArrayValue = new List<LuaValue>();
                    break;

                default:
                    throw new LuaException( "Incorrect LuaValue constructor use" );
            }
        }

        /// <summary>
        /// Construct a new Object LuaValue and attach a LuaState to it. Should be used if using metamethods.
        /// </summary>
        public LuaValue( LuaState state )
        {
            Type = LuaValueType.Object;
            ObjectValue = new VirtualMachine.Table();
            ObjectValue.State = state;
        }

        /// <summary>
        /// Construct a new Number LuaValue with the specified value.
        /// </summary>
        public LuaValue( double value )
        {
            Type = LuaValueType.Number;
            _numberValue = value;
        }

        /// <summary>
        /// Construct a new String LuaValue with the specified value.
        /// </summary>
        public LuaValue( string value )
        {
            Type = LuaValueType.String;
            _stringValue = value;
        }

        /// <summary>
        /// Construct a new Function LuaValue with the specified value.
        /// </summary>
        public LuaValue( LuaFunction function )
        {
            Type = LuaValueType.Function;
            FunctionValue = new Closure( function );
        }

        /// <summary>
        /// Construct a new Function LuaValue with the specified value. Instance functions will
        /// bind themselves to their parent object when being retrieved.
        /// </summary>
        public LuaValue( LuaInstanceFunction function )
        {
            Type = LuaValueType.Function;
            FunctionValue = new Closure( function );
        }

        internal LuaValue( Closure closure )
        {
            Type = LuaValueType.Function;
            FunctionValue = closure;
        }

        /// <summary>
        /// Get or set values in the Object or Array or its' prototype.
        /// </summary>
        public LuaValue this[LuaValue index]
        {
            get
            {
                if( Type == LuaValueType.Array && index.Type == LuaValueType.Number )
                {
                    var n = (int)index._numberValue;

                    if( n < 0 || n >= ArrayValue.Count )
                        throw new LuaRuntimeException( RuntimeError.IndexOutOfBounds );

                    return ArrayValue[n];
                }

                LuaValue indexValue;
                if( Type == LuaValueType.Object )
                {
                    if( ObjectValue.Values.TryGetValue( index, out indexValue ) )
                        return CheckWrapInstanceNative( indexValue );
                }

                return Nil;
            }
            set
            {
                if( index == null )
                    throw new ArgumentNullException( "index" );

                if( value == null )
                    throw new ArgumentNullException( "value" );

                if( Type == LuaValueType.Array && index.Type == LuaValueType.Number )
                {
                    var n = (int)index._numberValue;

                    if( n < 0 || n >= ArrayValue.Count )
                        throw new LuaRuntimeException( RuntimeError.IndexOutOfBounds );

                    ArrayValue[n] = value;
                    return;
                }

                if( Type == LuaValueType.Object && ObjectValue.Values.ContainsKey( index ) )
                {
                    if( !ObjectValue.Locked )
                        ObjectValue.Values[index] = value;

                    return;
                }

                if( Type != LuaValueType.Object )
                    throw new LuaRuntimeException( RuntimeError.CantCreateField, Type.GetName() );

                if( ObjectValue.Locked )
                    return;

                ObjectValue.Values[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user data value of an Object.
        /// </summary>
        //public object UserData
        //{
        //    get
        //    {
        //        if (Type != LuaValueType.Object)
        //            throw new LuaRuntimeException("UserData is only available on Objects");

        //        return ObjectValue.UserData;
        //    }
        //    set
        //    {
        //        if (Type != LuaValueType.Object)
        //            throw new LuaRuntimeException("UserData is only available on Objects");

        //        ObjectValue.UserData = value;
        //    }
        //}

        /// <summary>
        /// Locks an Object to prevent modification from scripts. All prototypes should be locked.
        /// </summary>
        public void Lock()
        {
            if( Type != LuaValueType.Object )
                throw new LuaRuntimeException( "Attempt to lock non-object" );

            ObjectValue.Locked = true;
        }

        public bool Contains( LuaValue search )
        {
            if( Type == LuaValueType.String && search.Type == LuaValueType.String )
                return _stringValue.Contains( search._stringValue );

            if( Type == LuaValueType.Object )
            {
                if( ObjectValue.Values.ContainsKey( search ) )
                    return true;

                return false;
            }

            if( Type == LuaValueType.Array )
                return ArrayValue.Contains( search );

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "in", Type, search.Type.GetName() );
        }

        public LuaValue Concat( LuaValue right )
        {
            if( Type == LuaValueType.String && right.Type == LuaValueType.String )
                return _stringValue + right._stringValue;

            throw new LuaRuntimeException( RuntimeError.CantUseOperatorOnTypes, "concat", Type.GetName(), right.Type.GetName() );
        }

        public bool Equals( LuaValue other )
        {
            if( ReferenceEquals( other, null ) )
                return false;

            switch( Type )
            {
                case LuaValueType.Object:
                    {
                        if( ReferenceEquals( ObjectValue, other.ObjectValue ) )
                            return true;

                        return false;
                    }

                case LuaValueType.Array:
                    return ReferenceEquals( ArrayValue, other.ArrayValue );

                case LuaValueType.Number:
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    return _numberValue == other._numberValue;

                case LuaValueType.String:
                    return _stringValue == other._stringValue;

                case LuaValueType.Function:
                    return ReferenceEquals( FunctionValue, other.FunctionValue );

                default:
                    return Type == other.Type;
            }
        }

        public override bool Equals( object other )
        {
            if( ReferenceEquals( other, null ) )
                return false;

            if( !( other is LuaValue ) )
                return false;

            return Equals( (LuaValue)other );
        }

        public override int GetHashCode()
        {
            switch( Type )
            {
                case LuaValueType.Nil:
                    return int.MinValue;

                case LuaValueType.True:
                    return 1;

                case LuaValueType.False:
                    return 0;

                case LuaValueType.Object:
                    return ObjectValue.GetHashCode();

                case LuaValueType.Array:
                    return ArrayValue.GetHashCode();

                case LuaValueType.Number:
                    return _numberValue.GetHashCode();

                case LuaValueType.String:
                    return _stringValue.GetHashCode();

                case LuaValueType.Function:
                    return FunctionValue.GetHashCode();
            }

            throw new NotSupportedException();
        }

        public override string ToString()
        {
            switch( Type )
            {
                case LuaValueType.True:
                    return "true";
                case LuaValueType.False:
                    return "false";
                case LuaValueType.Object:
                    return "object";
                case LuaValueType.Number:
                    return string.Format( "{0:R}", _numberValue );
                case LuaValueType.String:
                    return _stringValue;
                default:
                    return Type.GetName();
            }
        }

        private LuaValue CheckWrapInstanceNative( LuaValue value )
        {
            if( value.Type != LuaValueType.Function || value.FunctionValue.Type != ClosureType.InstanceNative )
                return value;

            var func = value.FunctionValue.InstanceNativeFunction;
            var inst = this;
            return new LuaValue( ( state, args ) => func( state, inst, args ) );
        }
    }
}
