using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Lua.Binding
{
    public static class LuaModuleBinder
    {
        private class ModuleBinding
        {
            public readonly Dictionary<string, LuaFunction> Functions;

            public ModuleBinding( Dictionary<string, LuaFunction> functions )
            {
                Functions = functions;
            }
        }

        private static Dictionary<Type, ModuleBinding> _cache = new Dictionary<Type, ModuleBinding>();

        /// <summary>
        /// Generates module bindings for T. Returns an object containing the bound methods.
        /// </summary>
        /// <param name="state">Optional state to bind to. Only required if you plan on using metamethods.</param>
        public static LuaValue Bind<T>( LuaState state = null )
        {
            return Bind( typeof( T ), state );
        }

        /// <summary>
        /// Generates module bindings for a type. Returns an object containing the bound methods.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="state">Optional state to bind to. Only required if you plan on using metamethods.</param>
        public static LuaValue Bind( Type type, LuaState state = null )
        {
            return CopyToObject( BindImpl( type ), state );
        }

        /// <summary>
        /// Generates module bindings for T. Returns a dictionary containing the bindings.
        /// </summary>
        public static ReadOnlyDictionary<string, LuaFunction> BindFunctions<T>()
        {
            return new ReadOnlyDictionary<string, LuaFunction>( BindImpl( typeof( T ) ) );
        }

        /// <summary>
        /// Generates module bindings for a type. Returns a dictionary containing the bindings.
        /// </summary>
        public static ReadOnlyDictionary<string, LuaFunction> BindFunctions( Type type )
        {
            return new ReadOnlyDictionary<string, LuaFunction>( BindImpl( type ) );
        }

        private static LuaValue CopyToObject( Dictionary<string, LuaFunction> functions, LuaState state )
        {
            var obj = new LuaValue( state );

            foreach( var func in functions )
            {
                obj[func.Key] = func.Value;
            }

            return obj;
        }

        private static Dictionary<string, LuaFunction> BindImpl( Type type )
        {
            ModuleBinding binding;
            if( _cache.TryGetValue( type, out binding ) )
            {
                return binding.Functions;
            }

            var moduleAttrib = type.Attribute<LuaModuleAttribute>();

            if( moduleAttrib == null )
                throw new LuaBindingException( BindingError.TypeMissingAttribute, "LuaModule" );

            var moduleName = moduleAttrib.Name ?? type.Name;

            var result = new Dictionary<string, LuaFunction>();

            foreach( var method in type.GetMethods( BindingFlags.Public | BindingFlags.Static ) )
            {
                var functionAttrib = method.Attribute<LuaFunctionAttribute>();

                if( functionAttrib == null )
                    continue;

                var name = functionAttrib.Name ?? method.Name;

                if( result.ContainsKey( name ) )
                    throw new LuaBindingException( BindingError.DuplicateDefinition, name );

                result[name] = LuaFunctionBinder.Bind( moduleName, name, method );
            }

            foreach( var property in type.GetProperties( BindingFlags.Public | BindingFlags.Static ) )
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

                    if( result.ContainsKey( getMethodName ) )
                        throw new LuaBindingException( BindingError.DuplicateDefinition, getMethodName );

                    result[getMethodName] = LuaFunctionBinder.Bind( moduleName, name, getMethod );
                }

                if( setMethod != null && setMethod.IsPublic )
                {
                    var setMethodName = "set" + name;

                    if( result.ContainsKey( setMethodName ) )
                        throw new LuaBindingException( BindingError.DuplicateDefinition, setMethodName );

                    result[setMethodName] = LuaFunctionBinder.Bind( moduleName, name, setMethod );
                }
            }

            binding = new ModuleBinding( result );
            _cache.Add( type, binding );

            return result;
        }
    }
}
