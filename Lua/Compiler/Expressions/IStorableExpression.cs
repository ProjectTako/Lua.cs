namespace Lua.Compiler.Expressions
{
    interface IStorableExpression
    {
        int CompileStore( FunctionContext context );
    }
}
