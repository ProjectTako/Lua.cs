namespace Lua.VirtualMachine
{
    struct ReturnAddress
    {
        public readonly LuaProgram Program;
        public readonly int Address;
        public readonly Frame Arguments;

        public ReturnAddress( LuaProgram program, int address, Frame arguments )
        {
            Program = program;
            Address = address;
            Arguments = arguments;
        }
    }
}
