using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Lua.Compiler;

namespace Lua.VirtualMachine
{
    partial class Machine
    {
        private readonly LuaState _state;
        public LuaValue Global;

        public Machine( LuaState state )
            : this()
        {
            _state = state;
            Global = new LuaValue( LuaValueType.Object );
        }

        public LuaValue Load( LuaProgram program )
        {
            if( program == null )
                throw new ArgumentNullException( "program" );

            var function = new LuaValue( new Closure( program, 0, null, null ) );
            return Call( function );
        }

        public LuaValue Call( LuaValue function, params LuaValue[] arguments )
        {
            //if (function.Type == LuaValueType.Object)
            //{
            //    // insert "this" value into argument array
            //    Array.Resize(ref arguments, arguments.Length + 1);
            //    Array.Copy(arguments, 0, arguments, 1, arguments.Length - 1);
            //    arguments[0] = function;

            //    LuaValue result;
            //    if (function.ObjectValue.TryDispatch("__call", out result, arguments))
            //        return result;
            //}

            if( function.Type != LuaValueType.Function )
                throw new LuaRuntimeException( RuntimeError.ValueNotCallable, function.Type.GetName() );

            var closure = function.FunctionValue;

            switch( closure.Type )
            {
                case ClosureType.Lua:
                    var argFrame = closure.Arguments;
                    if( argFrame == null )
                        argFrame = new Frame( 0, null, arguments.Length );
                    else
                        argFrame = new Frame( argFrame.Depth + 1, argFrame, arguments.Length );

                    for( var i = 0; i < arguments.Length; i++ )
                    {
                        argFrame.Values[i] = arguments[i];
                    }

                    PushCall( new ReturnAddress( closure.Program, closure.Address, argFrame ) );
                    PushLocal( closure.Locals );
                    break;

                case ClosureType.Native:
                    return closure.NativeFunction( _state, arguments );

                default:
                    throw new NotSupportedException();
            }

            return Run();
        }

        public LuaValue Run()
        {
            var functionAddress = PeekCall();
            var program = functionAddress.Program;
            var code = program.Bytecode;

            var initialCallDepth = _callStackSize - 1;
            var initialLocalDepth = _localStackSize;
            var initialEvalDepth = _evalStackSize;

            var ip = functionAddress.Address;
            var errorIp = 0;

            var args = functionAddress.Arguments;
            Frame locals = null;

            try
            {
                while( true )
                {
                    errorIp = ip;

                    /*if (program.DebugInfo != null)
                    {
                        var line = program.DebugInfo.FindLine(errorIp);
                        if (line.HasValue)
                            Console.WriteLine("{0:X4} {1} line {2}: {3}", errorIp, program.Strings[line.Value.FileName], line.Value.LineNumber, (InstructionType)code[ip]);
                        else
                            Console.WriteLine("{0:X4}: {1}", errorIp, (InstructionType)code[ip]);
                    }*/

                    switch( code[ip++] )
                    {
                        #region Stack Manipulation
                        case (int)InstructionType.Dup:
                            {
                                Push( Peek() );
                                break;
                            }

                        case (int)InstructionType.Drop:
                            {
                                Pop();
                                break;
                            }

                        case (int)InstructionType.Swap:
                            {
                                var value1 = Pop();
                                var value2 = Pop();
                                Push( value1 );
                                Push( value2 );
                                break;
                            }
                        #endregion

                        #region Constants

                        case (int)InstructionType.LdNil:
                            {
                                Push( LuaValue.Nil );
                                break;
                            }

                        case (int)InstructionType.LdTrue:
                            {
                                Push( LuaValue.True );
                                break;
                            }

                        case (int)InstructionType.LdFalse:
                            {
                                Push( LuaValue.False );
                                break;
                            }

                        case (int)InstructionType.LdNum:
                            {
                                var numId = ReadInt32( code, ref ip );
                                Push( program.Numbers[numId] );
                                break;
                            }

                        case (int)InstructionType.LdStr:
                            {
                                var strId = ReadInt32( code, ref ip );
                                Push( program.Strings[strId] );
                                break;
                            }

                        case (int)InstructionType.LdGlobal:
                            {
                                Push( Global );
                                break;
                            }
                        #endregion

                        #region Storables
                        case (int)InstructionType.LdLocF:
                            {
                                var index = ReadInt32( code, ref ip );
                                Push( locals.Values[index] );
                                break;
                            }

                        case (int)InstructionType.StLocF:
                            {
                                var index = ReadInt32( code, ref ip );
                                locals.Values[index] = Pop();
                                break;
                            }

                        case (int)InstructionType.LdLoc:
                            {
                                var depth = ReadInt32( code, ref ip );
                                var index = ReadInt32( code, ref ip );

                                if( depth < 0 )
                                    Push( args.Get( -depth, index ) );
                                else
                                    Push( locals.Get( depth, index ) );

                                break;
                            }

                        case (int)InstructionType.StLoc:
                            {
                                var depth = ReadInt32( code, ref ip );
                                var index = ReadInt32( code, ref ip );

                                if( depth < 0 )
                                    args.Set( -depth, index, Pop() );
                                else
                                    locals.Set( depth, index, Pop() );

                                break;
                            }

                        case (int)InstructionType.LdFld:
                            {
                                var obj = Pop();
                                Push( obj[program.Strings[ReadInt32( code, ref ip )]] );
                                break;
                            }

                        case (int)InstructionType.StFld:
                            {
                                var obj = Pop();
                                var value = Pop();

                                obj[program.Strings[ReadInt32( code, ref ip )]] = value;
                                break;
                            }

                        case (int)InstructionType.LdArr:
                            {
                                var index = Pop();
                                var array = Pop();
                                Push( array[index] );
                                break;
                            }

                        case (int)InstructionType.StArr:
                            {
                                var index = Pop();
                                var array = Pop();
                                var value = Pop();
                                array[index] = value;
                                break;
                            }

                        case (int)InstructionType.Concat:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left.Concat( right ) );
                                break;
                            }
                        #endregion

                        #region Object Creation
                        case (int)InstructionType.NewObject:
                            {
                                var obj = new LuaValue( _state );
                                var count = ReadInt32( code, ref ip );

                                for( var i = 0; i < count; ++i )
                                {
                                    //var key = Pop();
                                    //var value = Pop();
                                    obj.ObjectValue.Values.Add( Pop(), Pop() );
                                }

                                Push( obj );
                                break;
                            }

                        case (int)InstructionType.NewArray:
                            {
                                var count = ReadInt32( code, ref ip );
                                var array = new LuaValue( LuaValueType.Array );

                                for( var i = 0; i < count; i++ )
                                {
                                    array.ArrayValue.Add( default( LuaValue ) );
                                }

                                for( var i = count - 1; i >= 0; i-- )
                                {
                                    array.ArrayValue[i] = Pop();
                                }

                                Push( array );
                                break;
                            }
                        #endregion

                        #region Math
                        case (int)InstructionType.Add:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left + right );
                                break;
                            }

                        case (int)InstructionType.Sub:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left - right );
                                break;
                            }

                        case (int)InstructionType.Mul:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left * right );
                                break;
                            }

                        case (int)InstructionType.Div:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left / right );
                                break;
                            }

                        case (int)InstructionType.Mod:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left % right );
                                break;
                            }

                        case (int)InstructionType.Exp:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left.Pow( right ) );
                                break;
                            }

                        case (int)InstructionType.Neg:
                            {
                                Push( -Pop() );
                                break;
                            }
                        #endregion

                        #region Logic
                        case (int)InstructionType.Eq:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left == right );
                                break;
                            }

                        case (int)InstructionType.Neq:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left != right );
                                break;
                            }

                        case (int)InstructionType.Gt:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left > right );
                                break;
                            }

                        case (int)InstructionType.Gte:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left >= right );
                                break;
                            }

                        case (int)InstructionType.Lt:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left < right );
                                break;
                            }

                        case (int)InstructionType.Lte:
                            {
                                var left = Pop();
                                var right = Pop();
                                Push( left <= right );
                                break;
                            }

                        case (int)InstructionType.Not:
                            {
                                Push( !Pop() );
                                break;
                            }
                        #endregion

                        #region Functions
                        case (int)InstructionType.Closure:
                            {
                                var address = ReadInt32( code, ref ip );
                                Push( new LuaValue( new Closure( program, address, args, locals ) ) );
                                break;
                            }

                        case (int)InstructionType.Call:
                            {
                                var argCount = ReadInt32( code, ref ip );

                                var function = Pop();

                                List<LuaValue> unpackedArgs = null;

                                var returnAddress = ip;

                                //if (function.Type == LuaValueType.Object)
                                //{
                                //    LuaValue[] argArr;

                                //    if (unpackedArgs == null)
                                //    {
                                //        argArr = new LuaValue[argCount + 1];

                                //        for (var i = argCount; i >= 1; i--)
                                //        {
                                //            argArr[i] = Pop();
                                //        }

                                //        argArr[0] = function;
                                //    }
                                //    else
                                //    {
                                //        unpackedArgs.Insert(0, function);
                                //        argArr = unpackedArgs.ToArray();
                                //    }

                                //    LuaValue result;
                                //    if (function.ObjectValue.TryDispatch("__call", out result, argArr))
                                //    {
                                //        Push(result);
                                //        break;
                                //    }
                                //}

                                if( function.Type != LuaValueType.Function )
                                    throw new LuaRuntimeException( RuntimeError.ValueNotCallable, function.Type.GetName() );

                                var closure = function.FunctionValue;

                                var argFrame = function.FunctionValue.Arguments;
                                var argFrameCount = unpackedArgs == null ? argCount : unpackedArgs.Count;

                                if( argFrame == null )
                                    argFrame = new Frame( 1, null, argFrameCount );
                                else
                                    argFrame = new Frame( argFrame.Depth + 1, argFrame, argFrameCount );

                                // copy arguments into frame
                                if( unpackedArgs == null )
                                {
                                    for( var i = argFrameCount - 1; i >= 0; i-- )
                                    {
                                        argFrame.Values[i] = Pop();
                                    }
                                }
                                else
                                {
                                    for( var i = 0; i < argFrameCount; i++ )
                                    {
                                        argFrame.Values[i] = unpackedArgs[i];
                                    }
                                }

                                switch( closure.Type )
                                {
                                    case ClosureType.Lua:
                                        PushCall( new ReturnAddress( program, returnAddress, argFrame ) );
                                        PushLocal( closure.Locals );

                                        program = closure.Program;
                                        code = program.Bytecode;
                                        ip = closure.Address;
                                        args = argFrame;
                                        locals = closure.Locals;
                                        break;

                                    case ClosureType.Native:
                                        var result = closure.NativeFunction( _state, argFrame.Values );
                                        Push( result );
                                        break;

                                    default:
                                        throw new LuaRuntimeException( RuntimeError.UnhandledClosureType );
                                }

                                break;
                            }

                        case (int)InstructionType.TailCall:
                            {
                                var argCount = ReadInt32( code, ref ip );
                                var address = ReadInt32( code, ref ip );
                                var unpackCount = code[ip++];

                                List<LuaValue> unpackedArgs = null;

                                if( unpackCount > 0 )
                                    unpackedArgs = UnpackArgs( code, ref ip, argCount, unpackCount );

                                var returnAddress = PopCall();
                                var argFrame = returnAddress.Arguments;
                                int last;

                                // copy arguments into frame
                                if( unpackedArgs == null )
                                {
                                    last = argCount;

                                    // make sure the array is the right size
                                    argFrame.Set( argFrame.Depth, last - 1, LuaValue.Nil );

                                    for( var i = last - 1; i >= 0; i-- )
                                    {
                                        argFrame.Values[i] = Pop();
                                    }
                                }
                                else
                                {
                                    last = unpackedArgs.Count;

                                    // make sure the array is the right size
                                    argFrame.Set( argFrame.Depth, last - 1, LuaValue.Nil );

                                    for( var i = last - 1; i >= 0; i-- )
                                    {
                                        argFrame.Values[i] = unpackedArgs[i];
                                    }
                                }

                                // clear other arguments
                                for( var i = last; i < argFrame.Values.Length; i++ )
                                {
                                    argFrame.Values[i] = LuaValue.Nil;
                                }

                                // get rid of old locals
                                PushLocal( PopLocal().Previous );

                                PushCall( new ReturnAddress( returnAddress.Program, returnAddress.Address, argFrame ) );

                                ip = address;
                                break;
                            }

                        case (int)InstructionType.Enter:
                            {
                                var localCount = ReadInt32( code, ref ip );

                                var frame = PopLocal();
                                frame = new Frame( frame != null ? frame.Depth + 1 : 0, frame, localCount );

                                PushLocal( frame );
                                locals = frame;
                                break;
                            }

                        case (int)InstructionType.Ret:
                            {
                                var returnAddress = PopCall();
                                PopLocal();

                                program = returnAddress.Program;
                                code = program.Bytecode;
                                ip = returnAddress.Address;

                                args = _callStackSize > 0 ? PeekCall().Arguments : null;
                                locals = _localStackSize > 0 ? PeekLocal() : null;

                                if( _callStackSize == initialCallDepth )
                                    return Pop();

                                break;
                            }

                        case (int)InstructionType.VarArgs:
                            {
                                var fixedCount = ReadInt32( code, ref ip );
                                var varArgs = new LuaValue( LuaValueType.Array );

                                for( var i = fixedCount; i < args.Values.Length; i++ )
                                {
                                    varArgs.ArrayValue.Add( args.Values[i] );
                                }

                                args.Set( args.Depth, fixedCount, varArgs );
                                break;
                            }

                        case (int)InstructionType.JmpTable:
                            {
                                var start = ReadInt32( code, ref ip );
                                var count = ReadInt32( code, ref ip );

                                var endIp = ip + count * 4;

                                var value = Pop();
                                if( value.Type == LuaValueType.Number )
                                {
                                    var number = (double)value;
                                    var numberInt = (int)number;

                                    if( number >= start && number < start + count &&
                                        Math.Abs( number - numberInt ) <= double.Epsilon )
                                    {
                                        ip += ( numberInt - start ) * 4;
                                        ip = ReadInt32( code, ref ip );
                                        break;
                                    }
                                }

                                ip = endIp;
                                break;
                            }
                        #endregion

                        #region Branching
                        case (int)InstructionType.Jmp:
                            {
                                var address = ReadInt32( code, ref ip );
                                ip = address;
                                break;
                            }

                        case (int)InstructionType.JmpTrueP:
                            {
                                var address = ReadInt32( code, ref ip );

                                if( Peek() )
                                    ip = address;

                                break;
                            }

                        case (int)InstructionType.JmpFalseP:
                            {
                                var address = ReadInt32( code, ref ip );

                                if( !Peek() )
                                    ip = address;

                                break;
                            }

                        case (int)InstructionType.JmpTrue:
                            {
                                var address = ReadInt32( code, ref ip );

                                if( Pop() )
                                    ip = address;

                                break;
                            }

                        case (int)InstructionType.JmpFalse:
                            {
                                var address = ReadInt32( code, ref ip );

                                if( !Pop() )
                                    ip = address;

                                break;
                            }
                        #endregion

                        default:
                            throw new LuaRuntimeException( RuntimeError.UnhandledOpcode );
                    }
                }
            }
            catch( Exception e )
            {
                var errorBuilder = new StringBuilder();

                errorBuilder.AppendLine( e.Message.Trim() );

                var runtimeException = e as LuaRuntimeException;
                if( runtimeException != null && runtimeException.HasStackTrace )
                    errorBuilder.AppendLine( "[... native ...]" );
                else
                    errorBuilder.AppendLine();

                errorBuilder.AppendLine( GetAddressDebugInfo( program, errorIp ) );

                while( _callStackSize > initialCallDepth + 1 )
                {
                    var returnAddress = PopCall();

                    errorBuilder.AppendLine( GetAddressDebugInfo( returnAddress.Program, returnAddress.Address ) );
                }

                PopCall();

                while( _localStackSize > initialLocalDepth )
                {
                    PopLocal();
                }

                while( _evalStackSize > initialEvalDepth )
                {
                    Pop();
                }

                throw new LuaRuntimeException( errorBuilder.ToString(), e, true );
            }
        }

        private List<LuaValue> UnpackArgs( byte[] code, ref int ip, int argCount, int unpackCount )
        {
            var unpackIndices = new List<int>( unpackCount );

            for( var i = 0; i < unpackCount; i++ )
            {
                unpackIndices.Add( ReadInt32( code, ref ip ) );
            }

            var unpackedArgs = new List<LuaValue>( argCount + unpackCount * 16 );
            var argIndex = 0;
            var unpackIndex = 0;

            for( var i = argCount - 1; i >= 0; i-- )
            {
                var value = Pop();

                if( unpackIndex < unpackIndices.Count && i == unpackIndices[unpackIndex] )
                {
                    unpackIndex++;

                    var start = argIndex;
                    var count = 0;

                    foreach( var unpackedValue in value.Enumerate( _state ) )
                    {
                        unpackedArgs.Add( unpackedValue );
                        argIndex++;
                        count++;
                    }

                    unpackedArgs.Reverse( start, count );

                    continue;
                }

                unpackedArgs.Add( value );
                argIndex++;
            }

            unpackedArgs.Reverse();
            return unpackedArgs;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static int ReadInt32( byte[] buffer, ref int offset )
        {
            return buffer[offset++] << 0 |
                   buffer[offset++] << 8 |
                   buffer[offset++] << 16 |
                   buffer[offset++] << 24;
        }

        private static string GetAddressDebugInfo( LuaProgram program, int address )
        {
            if( program.DebugInfo != null )
            {
                var func = program.DebugInfo.FindFunction( address );
                var line = program.DebugInfo.FindLine( address );

                if( func.HasValue && line.HasValue )
                {
                    var prefix = "";
                    var funcName = program.Strings[func.Value.Name];
                    var fileName = program.Strings[line.Value.FileName];

                    if( !string.IsNullOrEmpty( funcName ) )
                        prefix = string.Format( "at {0} ", funcName );

                    return string.Format( "{0}in {1}: line {2}", prefix, fileName, line.Value.LineNumber );
                }
            }

            return address.ToString( "X8" );
        }
    }
}
