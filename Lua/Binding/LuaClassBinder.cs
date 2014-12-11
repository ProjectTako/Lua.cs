using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lua.Binding
{
    public static class LuaClassBinder
    {
        private class ClassBinding
        {
            public readonly LuaConstructor Constructor;
            public readonly Dictionary<string, LuaInstanceFunction> PrototypeFunctions;

            public ClassBinding( LuaConstructor constructor, Dictionary<string, LuaInstanceFunction> prototypeFunctions )
            {
                Constructor = constructor;
                PrototypeFunctions = prototypeFunctions;
            }
        }

        private static Dictionary<Type, ClassBinding> _cache = new Dictionary<Type, ClassBinding>();

        /// <summary>
        /// Generate a class binding for T. Returns the constructor function. The
        /// generated prototype will be locked.
        /// </summary>
        public static LuaFunction Bind<T>( LuaState state = null )
        {
            LuaValue prototype;
            var ctor = Bind<T>( out prototype, state );
            prototype.Lock();
            return ctor;
        }

        /// <summary>
        /// Generate a class binding for T. Returns the constructor function and
        /// sets prototype to the generated prototype.
        /// </summary>
        public static LuaFunction Bind<T>( out LuaValue prototype, LuaState state = null )
        {
            return Bind( typeof( T ), out prototype, state );
        }

        /// <summary>
        /// Generates a class binding for the given type. Returns the constructor
        /// function and sets prototype to the generated prototype.
        /// </summary>
        public static LuaFunction Bind( Type type, out LuaValue prototype, LuaState state = null )
        {
            Dictionary<string, LuaInstanceFunction> functions;
            var constructor = BindImpl( type, out functions );
            var prototypeObj = CopyToObject( functions, state );

            prototype = prototypeObj;

            return ( _, arguments ) =>
            {
                var instance = new LuaValue( _ );
                //instance.UserData = constructor(_, instance, arguments);
                return instance;
            };
        }

        private static LuaValue CopyToObject( Dictionary<string, LuaInstanceFunction> functions, LuaState state )
        {
            var obj = new LuaValue( state );

            foreach( var func in functions )
            {
                obj[func.Key] = new LuaValue( func.Value );
            }

            return obj;
        }

        private static LuaConstructor BindImpl( Type type, out Dictionary<string, LuaInstanceFunction> prototypeFunctions )
        {
            ClassBinding binding;
            if( _cache.TryGetValue( type, out binding ) )
            {
                prototypeFunctions = binding.PrototypeFunctions;
                return binding.Constructor;
            }

            var classAttrib = type.Attribute<LuaClassAttribute>();

            if( classAttrib == null )
                throw new LuaBindingException( BindingError.TypeMissingAttribute, "LuaClass" );

            var className = classAttrib.Name ?? type.Name;

            LuaConstructor constructor = null;
            var functions = new Dictionary<string, LuaInstanceFunction>();

            foreach( var method in type.GetMethods( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var functionAttrib = method.Attribute<LuaFunctionAttribute>();

                if( functionAttrib == null )
                    continue;

                var name = functionAttrib.Name ?? method.Name;

                if( functions.ContainsKey( name ) )
                    throw new LuaBindingException( BindingError.DuplicateDefinition, name );

                functions[name] = LuaFunctionBinder.BindInstance( className, name, method, type );
            }

            foreach( var property in type.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var functionAttrib = property.Attribute<LuaFunctionAttribute>();

                if( functionAttrib == null )
                    continue;

                var name = functionAttrib.Name ?? property.Name;

                var getMethod = property.GetGetMethod();
                var setMethod = property.GetSetMethod();

                if( getMethod != null && getMethod.IsPublic )
                {
                    var getMethodName = "get" + name;

                    if( functions.ContainsKey( getMethodName ) )
                        throw new LuaBindingException( BindingError.DuplicateDefinition, getMethodName );

                    functions[getMethodName] = LuaFunctionBinder.BindInstance( className, name, getMethod, type );
                }

                if( setMethod != null && setMethod.IsPublic )
                {
                    var setMethodName = "set" + name;

                    if( functions.ContainsKey( setMethodName ) )
                        throw new LuaBindingException( BindingError.DuplicateDefinition, setMethodName );

                    functions[setMethodName] = LuaFunctionBinder.BindInstance( className, name, setMethod, type );
                }
            }

            foreach( var ctor in type.GetConstructors( BindingFlags.Public | BindingFlags.Instance ) )
            {
                var constructorAttrib = ctor.Attribute<LuaConstructorAttribute>();

                if( constructorAttrib == null )
                    continue;

                if( constructor != null )
                    throw new LuaBindingException( BindingError.TooManyConstructors );

                constructor = LuaFunctionBinder.BindConstructor( className, ctor );
            }

            if( constructor == null )
                throw new LuaBindingException( BindingError.NotEnoughConstructors );

            binding = new ClassBinding( constructor, functions );
            _cache.Add( type, binding );

            prototypeFunctions = functions;
            return constructor;
        }
    }
}
