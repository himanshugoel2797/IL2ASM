using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace IL2ASM.Target
{
    internal interface ITarget
    {
        void InitializeTarget(GenericParser p);

        string FunctionEntry(string name);
        string FunctionExit(string name);

        int RegisterCount { get; }
        int PointerSize { get; }

        string EmitNop();

        string Ret(int arg_size);
        string Ret(int arg_size, int target_sz);

        string Call(int arg_size, string target);

        string AllocateStackSpace(int space);
        string FreeStackSpace(int space);

        string AllocateRegister();
        string HandleSpillover();
        void FreeRegister();

        string Add();
        string PushParameter(int index);
        string PushVariable(int index);

        string Pop();

        string GenerateSymbol(bool isPublic, bool isStatic, bool read, bool write, bool exec, bool isInited, string name);
        string AllocateSpace(int size);

        string GenerateVTable(List<string> table, Dictionary<string, MethodDefinition> mthds, Dictionary<string, ConstructorDefinition> ctors);
    }
}