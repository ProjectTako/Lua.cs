using System.Runtime.CompilerServices;
using Lua.VirtualMachine;

[assembly: InternalsVisibleTo( "Lua.Tests" )]

namespace Lua
{
    public delegate LuaValue LuaFunction( LuaState state, params LuaValue[] arguments );
    public delegate LuaValue LuaInstanceFunction( LuaState state, LuaValue instance, params LuaValue[] arguments );

    public class LuaState
    {
        private Machine _machine;

        public LuaState()
        {
            _machine = new Machine( this );
        }

        public LuaValue this[LuaValue index]
        {
            get { return _machine.Global[index]; }
            set { _machine.Global[index] = value; }
        }

        public LuaValue Load( LuaProgram program )
        {
            return _machine.Load( program );
        }

        public LuaValue Call( LuaValue function, params LuaValue[] arguments )
        {
            return _machine.Call( function, arguments );
        }
    }
}
