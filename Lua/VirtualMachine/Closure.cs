namespace Lua.VirtualMachine
{
    enum ClosureType
    {
        Native, InstanceNative, Lua
    }

    class Closure
    {
        public readonly ClosureType Type;

        public readonly LuaProgram Program;
        public readonly int Address;
        public readonly Frame Arguments;
        public readonly Frame Locals;

        public readonly LuaFunction NativeFunction;
        public readonly LuaInstanceFunction InstanceNativeFunction;

        public Closure( LuaProgram program, int address, Frame arguments, Frame locals )
        {
            Type = ClosureType.Lua;

            Program = program;
            Address = address;
            Arguments = arguments;
            Locals = locals;
        }

        public Closure( LuaFunction function )
        {
            Type = ClosureType.Native;

            NativeFunction = function;
        }

        public Closure( LuaInstanceFunction function )
        {
            Type = ClosureType.InstanceNative;

            InstanceNativeFunction = function;
        }
    }
}
