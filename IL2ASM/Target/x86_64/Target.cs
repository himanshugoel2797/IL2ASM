using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Target.x86_64
{
    class Target : ITarget
    {
        const int PointerSize = 8;

        public int RegisterCount
        {
            get
            {
                return RegisterAllocator.RegisterCount;
            }
        }

        public string Add()
        {
            throw new NotImplementedException();
        }

        public string AllocateRegister()
        {
            throw new NotImplementedException();
        }

        public string AllocateSpace(int size)
        {
            return $".space {size}";
        }

        public void FreeRegister()
        {
            throw new NotImplementedException();
        }

        public string FunctionEntry(string name)
        {
            return "";
        }

        public string FunctionExit(string name)
        {
            return "";
        }

        public string GenerateSymbol(bool isPublic, bool isStatic, bool read, bool write, bool exec, bool isInited, string name)
        {
            StringBuilder res = new StringBuilder();


            string sectionName = ".bss";

            if (isInited && !write)
                sectionName = ".rodata";

            if (isInited && write)
                sectionName = ".data";

            if (exec && write)
                throw new Exception("Data and Code may not be mixed.");

            if (exec)
                sectionName = ".text";

            res.AppendLine($".section {sectionName}");
            
            if (isPublic)
                res.AppendLine($".global {name}");

            res.Append($"{name}:");

            return res.ToString();
        }

        public string HandleSpillover()
        {
            throw new NotImplementedException();
        }

        public void InitializeTarget(GenericParser parser)
        {
            parser.ReplaceDefinition(typeof(object), new ClassDefinition()
            {
                Size = 4,
                ParentClass = null,
                Type = typeof(Builtins.NativeObject)
            });

            parser.ReplaceDefinition(typeof(ValueType), new ClassDefinition()
            {
                Size = parser.ClassDefinitions["System_Object"].Size,
                ParentClass = "System_Object",
                Type = typeof(Builtins.NativeValueType)
            });

            
            parser.ReplaceDefinition(typeof(int), new ClassDefinition()
            {
                Size = sizeof(int),
                ParentClass = "System_ValueType",

            });

            parser.ReplaceDefinition(typeof(float), new ClassDefinition()
            {
                Size = sizeof(float),
                ParentClass = "System_ValueType",

            });

            parser.ReplaceDefinition(typeof(uint), new ClassDefinition()
            {
                Size = sizeof(uint),
                ParentClass = "System_ValueType",

            });

            parser.ReplaceDefinition(typeof(double), new ClassDefinition()
            {
                Size = sizeof(double),
                ParentClass = "System_ValueType",

            });

        }

        public string Pop()
        {
            throw new NotImplementedException();
        }

        public string PushParameter(int index)
        {
            throw new NotImplementedException();
        }

        public string PushVariable(int index)
        {
            throw new NotImplementedException();
        }
    }
}
