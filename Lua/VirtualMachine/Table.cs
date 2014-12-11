using System.Collections.Generic;

namespace Lua.VirtualMachine
{
    class Table
    {
        public readonly Dictionary<LuaValue, LuaValue> Values;
        public bool Locked;
        public object UserData;

        private LuaState _dispatcherState;

        public LuaState State
        {
            set
            {
                if( _dispatcherState == null )
                    _dispatcherState = value;
            }
        }

        public Table()
        {
            Values = new Dictionary<LuaValue, LuaValue>();
            Locked = false;
            UserData = null;
        }
    }
}
