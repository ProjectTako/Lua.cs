namespace Lua.Compiler.Expressions
{
    interface IConstantExpression
    {
        /// <summary>
        /// Used by SwitchExpression
        /// </summary>
        LuaValue GetValue();
    }
}
