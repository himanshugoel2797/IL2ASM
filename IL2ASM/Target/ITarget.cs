using System;
using System.Reflection;

namespace IL2ASM.Target
{
    internal interface ITarget
    {
        void InitializeTarget(GenericParser p);

        string FunctionEntry(string name);
        string FunctionExit(string name);

        int RegisterCount { get; }

        string AllocateRegister();
        string HandleSpillover();
        void FreeRegister();

        string Add();
        string PushParameter(int index);
        string PushVariable(int index);

        string Pop();

        string GenerateSymbol(bool isPublic, bool isStatic, bool read, bool write, bool exec, bool isInited, string name);
        string AllocateSpace(int size);
    }
}