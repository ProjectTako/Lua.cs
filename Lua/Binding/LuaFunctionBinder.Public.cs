using System;
using System.Reflection;

namespace Lua.Binding
{
    internal delegate object LuaConstructor( LuaState state, LuaValue instance, params LuaValue[] arguments );

    public static partial class LuaFunctionBinder
    {
        /// <summary>
        /// Generates a LuaFunction binding for the given method.
        /// </summary>
        public static LuaFunction Bind( string moduleName, string methodName, MethodInfo method )
        {
            if( !method.IsStatic )
                throw new LuaBindingException( "Bind only supports static methods" );

            return BindImpl<LuaFunction, LuaValue>( moduleName, methodName, method, false, ( p, a, r ) => BindFunctionCall( method, null, false, p, a, r ) );
        }

        /// <summary>
        /// Generates a LuaInstanceFunction binding for the given method.
        /// </summary>
        internal static LuaInstanceFunction BindInstance( string className, string methodName, MethodInfo method, Type type = null )
        {
            if( className == null )
                throw new ArgumentNullException( "className" );

            if( type == null && !method.IsStatic )
                throw new LuaBindingException( "BindInstance requires a type for non-static methods" );

            return BindImpl<LuaInstanceFunction, LuaValue>( className, methodName, method, true, ( p, a, r ) => BindFunctionCall( method, type, true, p, a, r ) );
        }

        /// <summary>
        /// Generates a LuaConstructor binding for the given constructor.
        /// </summary>
        internal static LuaConstructor BindConstructor( string className, ConstructorInfo constructor )
        {
            if( className == null )
                throw new ArgumentNullException( "className" );

            return BindImpl<LuaConstructor, object>( className, "#ctor", constructor, true, ( p, a, r ) => BindConstructorCall( constructor, a, r ) );
        }
    }
}
