using System.Collections.Generic;

namespace Lua
{
    public sealed partial class LuaValue
    {
        public bool IsEnumerable
        {
            get
            {
                var hasGetEnumerator = this["getEnumerator"].Type == LuaValueType.Function;
                var hasEnumeratorFunc = this["moveNext"].Type == LuaValueType.Function;

                return hasGetEnumerator || hasEnumeratorFunc;
            }
        }

        public IEnumerable<LuaValue> Enumerate( LuaState state )
        {
            var enumerator = this;
            var moveNext = enumerator["moveNext"];

            if( moveNext.Type != LuaValueType.Function )
            {
                var getEnumerator = this["getEnumerator"];
                if( getEnumerator.Type != LuaValueType.Function )
                    throw new LuaRuntimeException( "Value is not enumerable" );

                enumerator = state.Call( getEnumerator );

                moveNext = enumerator["moveNext"];
                if( moveNext.Type != LuaValueType.Function )
                    throw new LuaRuntimeException( "Value is not enumerable" );
            }

            while( state.Call( moveNext ) )
            {
                yield return enumerator["current"];
            }
        }

        public static LuaValue FromEnumerable( IEnumerable<LuaValue> enumerable )
        {
            var enumerator = enumerable.GetEnumerator();
            var enumerableObj = new LuaValue( LuaValueType.Object );

            enumerableObj["current"] = Nil;

            enumerableObj["moveNext"] = new LuaFunction( ( _, args ) =>
            {
                var success = enumerator.MoveNext();
                enumerableObj["current"] = success ? enumerator.Current : Nil;
                return success;
            } );

            enumerableObj["dispose"] = new LuaFunction( ( _, args ) =>
            {
                enumerator.Dispose();
                return Nil;
            } );

            enumerableObj["getEnumerator"] = new LuaFunction( ( _, args ) => enumerableObj );

            return enumerableObj;
        }
    }
}
