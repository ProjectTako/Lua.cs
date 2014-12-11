Lua.cs
======

Lua.cs is pure C# implementation of Lua 5.2.

Why?
====

Projects like [NLua](https://github.com/NLua/NLua) are great, but they're still just bindings for unmanaged code, which has several pitfalls and other downsides that may not be desirable for everyone. Since Lua.cs is a complete re-implementation written in pure C#, there are no bindings to unmanaged code, which means being able to use existing Lua code and getting the safety and automatic garbage collection of a managed language. Lua.cs aims to be as faithful to the functionality and feature set as the C implementation of Lua 5.2, meaning that all existing code will work just the same as it always has.

Notes
=====

* Developers and users should be aware that they will not see and performance increases from their Lua code when running under Lua.cs. Due to being different implementations and the inherent overhead of .NET, however small, does make it slower than C.
* Some features may not work correctly or may be missing entirely. Please [click here for a list of missing features](https://github.com/Syke94/Lua.cs/labels/missing%20feature). If you would like to report a new missing feature or other bug, please [open a new issue](https://github.com/Syke94/Lua.cs/issues/new) or send in a pull request with the fix.

Credits & Thanks
================

Lua.cs is based primarily on the code that powers the [Mond scripting language](https://github.com/Rohansi/Mond), and is a heavy modification of that project.