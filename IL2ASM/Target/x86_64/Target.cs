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
        public void InitializeTarget(GenericParser parser)
        {
            parser.ReplaceDefinition(typeof(object), new ClassDefinition()
            {
                Size = 0,
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

            parser.ReplaceDefinition(typeof(void), new ClassDefinition()
            {
                Size = 0,
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

        public string AllocateStackSpace(int space)
        {
            return "sub %rsp, $" + space;
        }

        public string FreeStackSpace(int space)
        {
            return "add %rsp, $" + space;
        }

        public string GetRegisterName(int index)
        {
            return "rax";
        }

        public string ReadRelativeToStack(int offset, int register)
        {
            return "mov %" + GetRegisterName(register) + ", " + offset + "(%rsp)";
        }

        public string EmitNop()
        {
            return "nop";
        }

        public string EmitOpCodes(IL.ILParser p)
        {

            //%rsp = evaluation stack, %rbp = execution stack

            StringBuilder builder = new StringBuilder();
            OpCode op = p.GetCurrentOpCode();

            Stack<StackItem> stack = new Stack<StackItem>();

            if (op == OpCodes.Pop)
            {
                var item = stack.Pop();
                builder.AppendLine("add %rsp, $" + item.Size);
            }
            else if (op == OpCodes.Ldarg)
            {
                //Get the specified argument, push it onto the stack
            }


            return builder.ToString();
        }

        public int PointerSize
        {
            get
            {
                return 8;
            }
        }

        public int RegisterCount
        {
            get
            {
                return RegisterAllocator.RegisterCount;
            }
        }

        public string GenerateVTable(List<string> vtable, Dictionary<string, MethodDefinition> mthds, Dictionary<string, ConstructorDefinition> ctors)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < vtable.Count; i++)
            {
                string name = "";
                if (mthds.ContainsKey(vtable[i]))
                {
                    name = mthds[vtable[i]].Entry.FinalName;
                }
                else
                {
                    name = ctors[vtable[i]].Entry.FinalName;
                }

                builder.AppendLine($".long ${name}");
            }

            return builder.ToString();
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
            //swap the stack pointers, after xchg: rbp = call stack, rsp = eval stack
            return "xchg %rbp, %rsp";
        }

        public string FunctionExit(string name)
        {
            return "";
        }

        public string Ret(int arg_sz)
        {
            //remove the arguments from the stack
            //swap the stack pointers and return, after xchg: rbp = eval stack, rsp = call stack 
            return @"xchg %rbp, %rsp
ret";
        }

        public string Call(int arg_size, string target)
        {
            return
$@"xchg %rbp, %rsp //after xchg: rbp = eval stack, rsp = call stack
call ${target} 
xchg %rbp, %rsp //after xchg: rbp = call stack, rsp = eval stack";
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

        public string Ret(int arg_size, int target_sz)
        {
            throw new NotImplementedException();
        }
    }
}
