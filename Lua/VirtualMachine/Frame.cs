using System;

namespace Lua.VirtualMachine
{
    class Frame
    {
        public readonly int Depth;
        public readonly Frame Previous;
        public LuaValue[] Values;

        public Frame( int depth, Frame previous, int valueCount )
        {
            Depth = depth;
            Previous = previous;
            Values = new LuaValue[valueCount];

            for( var i = 0; i < valueCount; i++ )
            {
                Values[i] = LuaValue.Nil;
            }
        }

        public LuaValue Get( int depth, int index )
        {
            var current = this;

            while( current.Depth > depth )
            {
                current = current.Previous;
            }

            if( index < 0 || index >= current.Values.Length )
                return LuaValue.Nil;

            return current.Values[index];
        }

        public void Set( int depth, int index, LuaValue value )
        {
            var current = this;

            while( current.Depth > depth )
            {
                current = current.Previous;
            }

            if( index < 0 )
                return;

            var values = current.Values;

            if( index >= values.Length )
            {
                var oldLength = values.Length;
                var newLength = index + 1;

                Array.Resize( ref values, newLength );
                current.Values = values;

                for( var i = oldLength; i < newLength; i++ )
                {
                    values[i] = LuaValue.Nil;
                }
            }

            values[index] = value;
        }
    }
}
