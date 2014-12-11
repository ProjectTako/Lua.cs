namespace Lua
{
    public class LuaCompilerOptions
    {
        /// <summary>
        /// Generate debug information for stack traces.
        /// </summary>
        public bool GenerateDebugInfo { get; set; }

        /// <summary>
        /// Force all declarations in the script root to be stored in the global object.
        /// </summary>
        public bool MakeRootDeclarationsGlobal { get; set; }

        /// <summary>
        /// Make undefined variables resolve to globals.
        /// </summary>
        public bool UseImplicitGlobals { get; set; }

        public LuaCompilerOptions()
        {
            GenerateDebugInfo = true;
            MakeRootDeclarationsGlobal = false;
            UseImplicitGlobals = false;
        }
    }
}
