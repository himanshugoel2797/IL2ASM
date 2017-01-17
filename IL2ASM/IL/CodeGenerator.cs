using IL2ASM.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.IL
{
    public partial class Compiler
    {
        public int GetLocalSize(MethodBody body)
        {
            //Local variables
            int local_sz = 0;

            for (int i = 0; i < body.LocalVariables.Count; i++)
                if (body.LocalVariables[i].LocalType.IsPointer | body.LocalVariables[i].LocalType.IsArray | body.LocalVariables[i].LocalType.IsByRef | body.LocalVariables[i].LocalType.IsClass)
                    local_sz += PointerSize;
                else
                {
                    if (!FieldSets.FieldTables.ContainsKey(body.LocalVariables[i].LocalType))
                        FieldSets.AddType(body.LocalVariables[i].LocalType);

                    local_sz += FieldSets.FieldTables[body.LocalVariables[i].LocalType].Size;
                }

            return local_sz;
        }

        public int GetArgsSize(ParameterInfo[] p)
        {
            //Local variables
            int local_sz = 0;

            for (int i = 0; i < p.Length; i++)
                if (p[i].ParameterType.IsPointer | p[i].ParameterType.IsArray | p[i].ParameterType.IsByRef | p[i].ParameterType.IsClass)
                    local_sz += PointerSize;
                else
                {
                    if (!FieldSets.FieldTables.ContainsKey(p[i].ParameterType))
                        FieldSets.AddType(p[i].ParameterType);

                    local_sz += FieldSets.FieldTables[p[i].ParameterType].Size;
                }

            return local_sz;
        }

        private static readonly string[] SpecialFunctions = new string[]
        {
            "GetType",
            "GetHashCode",
        };

        private static readonly MethodInfo[] SpecialFunctionSubs = new MethodInfo[]
        {
            typeof(NativeObject).GetMethod("NativeGetType"),
            typeof(NativeObject).GetMethod("NativeGetHashCode"),
        };

        public string HandleSpecialFunction(MethodInfo origFunc, MethodInfo subsFunc)
        {

            return "";
        }

        public string CompileMethod(MethodInfo info)
        {
            if (SpecialFunctions.Contains(info.Name) && info.GetMethodBody() == null)
            {
                //Generate desired symbol, but fake the contents with another function
                int i = 0;

                for (i = 0; i < SpecialFunctions.Length; i++)
                    if (SpecialFunctions[i] == info.Name)
                        break;

                return HandleSpecialFunction(info, SpecialFunctionSubs[i]);
            }

            if (info.IsAbstract | info.IsSpecialName)
                return "";

            var body = info.GetMethodBody();
            if (body == null)
            {
                throw new NotImplementedException();
            }

            ILParser p = new ILParser(body.GetILAsByteArray());
            StringBuilder b = new StringBuilder();

            //Generate the symbol
            b.AppendLine("//" + info.ReflectedType.Name + "_" + info.Name);
            b.AppendLine(Target.GenerateSymbol(info.IsPublic, info.IsStatic, true, false, true, true, Helpers.GetMethodName(info)));
            b.AppendLine(Target.FunctionEntry(Helpers.GetMethodName(info)));

            do
            {
                OpCode op = p.GetCurrentOpCode();
                uint p_cnt = p.GetParameterCount();

                if (op == OpCodes.Nop)
                {
                    continue;
                }
                else if (op == OpCodes.Call)
                {
                    //Get the target method info
                    int tkn = (int)p.GetParameter(0);

                    var mthd_base = info.Module.ResolveMethod(tkn);

                    if(!ProcessedTokens.Contains(tkn))
                    {
                        VTableSets.AddType(mthd_base.ReflectedType);
                    }

                    ParameterInfo[] m_param = mthd_base.GetParameters();
                    bool m_isStatic = mthd_base.IsStatic;
                    string fn_name = Helpers.GetMethodName(mthd_base.MetadataToken);

                    //Set the arg stack pointer to right before the arguments and emit a call
                    if (m_param != null)
                    {
                        if(m_isStatic)
                            b.AppendLine(Target.Call(GetArgsSize(m_param), fn_name));
                        else
                        {
                            //Get the offset of this method in the vtable for the type

                        }
                            
                    }
                    else
                    {
                        //TODO error out

                    }
                }
                else if (op == OpCodes.Ret)
                {
                    ulong returnSz = 0;

                    //Delay pushes until the pushed data needs to be used
                    //Allowing for optimization 
                    //target.FreeStackSpace()

                    //if (stack.Count > 0)
                    //    returnSz = stack.Pop().Size;


                    //if (returnSz == 0)
                    //    builder.AppendLine(target.Ret(args_sz));
                    //else
                    //    builder.AppendLine(target.Ret(args_sz, (int)returnSz));
                }

                //For static calls, on a call or newobj instruction, resolve the token and see if it has been processed already, if not, process it.
                //For instance calls, get the function from the instance's vtable


            } while (p.NextInstruction());

            b.AppendLine(Target.FunctionExit(Helpers.GetMethodName(info)));

            return b.ToString();
        }
    }
}
