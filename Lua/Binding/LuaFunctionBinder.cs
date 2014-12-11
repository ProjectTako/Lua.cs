using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lua.Binding
{
    public static partial class LuaFunctionBinder
    {
        private static TFunc BindImpl<TFunc, TReturn>( string moduleName, string methodName, MethodBase method, bool instanceFunction, BindCallFactory callFactory )
        {
            // TODO: clean up everything below this line

            var arguments = GetArguments( method, instanceFunction ).ToArray();

            var errorPrefix = string.Format( "{0}{1}{2}: ", moduleName ?? "", string.IsNullOrEmpty( moduleName ) ? "" : ".", methodName );

            var parameters = new List<ParameterExpression>
            {
                Expression.Parameter(typeof(LuaState), "state"),
                Expression.Parameter(typeof(LuaValue[]), "arguments")
            };

            if( instanceFunction )
                parameters.Insert( 1, Expression.Parameter( typeof( LuaValue ), "instance" ) );

            var argumentsParam = parameters[instanceFunction ? 2 : 1];

            Func<int, Expression> argumentIndex = i => Expression.ArrayIndex( argumentsParam, Expression.Constant( i ) );

            var statements = new List<Expression>();
            var returnLabel = Expression.Label( typeof( TReturn ) );

            // skip type checks if using params LuaValue[]
            var skipTypeChecks = arguments.Any( a => a.Index < 0 && a.Type == typeof( LuaValue[] ) );

            if( !skipTypeChecks )
            {
                // argument count check
                var argLength = Expression.Condition( Expression.Equal( argumentsParam, Expression.Constant( null ) ), Expression.Constant( 0 ), Expression.PropertyOrField( argumentsParam, "Length" ) );
                var requiredArgLength = arguments.Count( a => a.Index >= 0 );
                var argLengthError = string.Format( "{0}must be called with {1} argument{2}", errorPrefix, requiredArgLength, requiredArgLength != 1 ? "s" : "" );

                statements.Add( Expression.IfThen( Expression.NotEqual( argLength, Expression.Constant( requiredArgLength ) ), Throw( argLengthError ) ) );

                // argument type checks
                for( var i = 0; i < arguments.Length; i++ )
                {
                    var arg = arguments[i];

                    // TODO: attribute to verify type of UserData

                    if( arg.Index < 0 || arg.Type == typeof( LuaValue ) )
                        continue;

                    statements.Add( TypeCheck( errorPrefix, i + 1, argumentIndex( arg.Index ), arg.Type ) );
                }
            }

            // call
            var callArgs = new List<Expression>();

            foreach( var arg in arguments )
            {
                if( arg.Type == typeof( LuaState ) )
                {
                    callArgs.Add( parameters[0] );
                    continue;
                }

                if( arg.Type == typeof( LuaValue ) )
                {
                    if( instanceFunction && arg.Info.Attribute<LuaInstanceAttribute>() != null )
                    {
                        callArgs.Add( parameters[1] );
                    }
                    else
                    {
                        callArgs.Add( argumentIndex( arg.Index ) );
                    }

                    continue;
                }

                if( arg.Index < 0 && arg.Type == typeof( LuaValue[] ) ) // params LuaValue[]
                {
                    callArgs.Add( argumentsParam );
                    continue;
                }

                callArgs.Add( ConvertArgument( argumentIndex( arg.Index ), arg.Type ) );
            }

            statements.Add( callFactory( parameters, callArgs, returnLabel ) );

            // end / default return
            statements.Add( Expression.Label( returnLabel, Expression.Constant( LuaValue.Nil ) ) );

            var block = Expression.Block( statements );
            return Expression.Lambda<TFunc>( block, parameters ).Compile();
        }

        /// <summary>
        /// Creates the function call and return statements needed for a function binding.
        /// </summary>
        private delegate Expression BindCallFactory( List<ParameterExpression> parameters, List<Expression> arguments, LabelTarget returnLabel );

        /// <summary>
        /// Creates the function call and return statements for normal function calls. Should be used through BindCallFactory.
        /// </summary>
        private static Expression BindFunctionCall( MethodInfo method, Type instanceType, bool instanceFunction, List<ParameterExpression> parameters, IEnumerable<Expression> arguments, LabelTarget returnLabel )
        {
            var returnType = method.ReturnType;

            Expression callExpr;

            if( instanceFunction && instanceType != null )
            {
                // instance functions store the instance in UserData
                var userData = Expression.Convert( Expression.PropertyOrField( parameters[1], "UserData" ), instanceType );
                callExpr = Expression.Call( userData, method, arguments );
            }
            else
            {
                callExpr = Expression.Call( method, arguments );
            }

            if( returnType != typeof( void ) )
            {
                var variables = new List<ParameterExpression>();
                var expressions = new List<Expression>();
                Expression result;

                if( returnType == typeof( LuaValue ) )
                {
                    result = callExpr;
                }
                else if( BasicTypes.Contains( returnType ) )
                {
                    result = Expression.Convert( callExpr, typeof( LuaValue ) );
                }
                else if( NumberTypes.Contains( returnType ) )
                {
                    result = Expression.Convert( Expression.Convert( callExpr, typeof( double ) ), typeof( LuaValue ) );
                }
                else
                {
                    throw new LuaBindingException( BindingError.UnsupportedReturnType, returnType );
                }

                expressions.Add( Expression.Return( returnLabel, result ) );

                callExpr = Expression.Block( variables, expressions );
            }

            return callExpr;
        }

        /// <summary>
        /// Creates the function call and return statements for a constructor function. Should be used through BindCallFactory.
        /// </summary>
        private static Expression BindConstructorCall( ConstructorInfo constructor, IEnumerable<Expression> arguments, LabelTarget returnLabel )
        {
            return Expression.Return( returnLabel, Expression.New( constructor, arguments ) );
        }

        /// <summary>
        /// Creates a type check statement for the given argument.
        /// </summary>
        private static Expression TypeCheck( string errorPrefix, int index, Expression argument, Type type )
        {
            string expectedTypeName;
            Expression condition;

            LuaValueType[] mondTypes;
            LuaClassAttribute mondClass;

            if( TypeCheckMap.TryGetValue( type, out mondTypes ) )
            {
                expectedTypeName = mondTypes[0].GetName();

                condition = Expression.NotEqual( Expression.PropertyOrField( argument, "Type" ), Expression.Constant( mondTypes[0] ) );

                for( var i = 1; i < mondTypes.Length; i++ )
                {
                    condition = Expression.AndAlso( condition, Expression.NotEqual( Expression.PropertyOrField( argument, "Type" ), Expression.Constant( mondTypes[i] ) ) );
                }
            }
            else if( ( mondClass = type.Attribute<LuaClassAttribute>() ) != null )
            {
                expectedTypeName = mondClass.Name ?? type.Name;

                var argIsNotObject = Expression.NotEqual( Expression.PropertyOrField( argument, "Type" ), Expression.Constant( LuaValueType.Object ) );
                var argIsWrongClass = Expression.Not( Expression.TypeIs( Expression.PropertyOrField( argument, "UserData" ), type ) );
                condition = Expression.OrElse( argIsNotObject, argIsWrongClass );
            }
            else
            {
                throw new LuaBindingException( BindingError.UnsupportedType, type );
            }

            var error = string.Format( "{0}argument {1} must be of type {2}", errorPrefix, index, expectedTypeName );
            return Expression.IfThen( condition, Throw( error ) );
        }

        /// <summary>
        /// Creates an expression that converts an argument from LuaValue into the target type.
        /// </summary>
        private static Expression ConvertArgument( Expression argument, Type type )
        {
            if( BasicTypes.Contains( type ) )
            {
                argument = Expression.Convert( argument, type );
            }
            else if( NumberTypes.Contains( type ) )
            {
                argument = Expression.Convert( Expression.Convert( argument, typeof( double ) ), type );
            }
            else if( type.Attribute<LuaClassAttribute>() != null )
            {
                argument = Expression.Convert( Expression.PropertyOrField( argument, "UserData" ), type );
            }
            else
            {
                throw new LuaBindingException( BindingError.UnsupportedType, type );
            }

            return argument;
        }

        /// <summary>
        /// Creates a statement that throws a LuaRuntimeException.
        /// </summary>
        private static Expression Throw( string message )
        {
            var constructor = typeof( LuaRuntimeException ).GetConstructor( new[] { typeof( string ), typeof( bool ) } );

            if( constructor == null )
                throw new LuaBindingException( "Could not find LuaRuntimeException constructor" );

            return Expression.Throw( Expression.New( constructor, Expression.Constant( message ), Expression.Constant( false ) ) );
        }

        private class FunctionArgument
        {
            public readonly int Index;
            public readonly ParameterInfo Info;

            public Type Type { get { return Info.ParameterType; } }

            public FunctionArgument( int index, ParameterInfo info )
            {
                Index = index;
                Info = info;
            }
        }

        private static IEnumerable<FunctionArgument> GetArguments( MethodBase method, bool instanceFunction )
        {
            var parameters = method.GetParameters();
            var index = 0;

            for( var i = 0; i < parameters.Length; i++ )
            {
                var p = parameters[i];

                var skip = ( p.ParameterType == typeof( LuaState ) ) ||
                           ( instanceFunction && p.ParameterType == typeof( LuaValue ) && p.Attribute<LuaInstanceAttribute>() != null ) ||
                           ( i == parameters.Length - 1 && p.ParameterType == typeof( LuaValue[] ) && p.Attribute<ParamArrayAttribute>() != null );

                yield return new FunctionArgument( skip ? -1 : index, p );

                if( !skip )
                    index++;
            }
        }
    }
}
