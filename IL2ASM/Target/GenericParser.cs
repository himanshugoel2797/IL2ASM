using IL2ASM.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Target
{
    public struct ClassEntry
    {
        public string Namespace;
        public string Class;
        public string Name;
        public string FinalName;

        public int MetadataToken;

        public bool IsPublic;
        public bool IsStatic;
    }

    public enum StackItemType
    {
        Constant,
        Address,
        Value
    }

    public struct StackItem
    {
        public StackItemType Type;
        public ulong Value;
        public ulong Size;
    }

    public struct FieldDefinition
    {
        public ClassEntry Entry;
        public Type Type;
        public bool IsProperty;

        public int Size;
        public int Offset;
    }

    public struct MethodDefinition
    {
        public ClassEntry Entry;
        public ParameterInfo[] Parameters;
        public Type ReturnType;
        public MethodInfo Info;
    }

    public struct ConstructorDefinition
    {
        public ClassEntry Entry;
        public ParameterInfo[] Parameters;
        public MethodBody Body;
        public ConstructorInfo Info;
    }

    public struct ClassDefinition
    {
        public int Size;
        public int MetadataToken;
        public string ParentClass;
        public Type Type;

        public Dictionary<string, FieldDefinition> FieldTable;  //Track via mangled names
        public Dictionary<string, MethodDefinition> MethodVTable;   //Track via mangled names
        public Dictionary<string, ConstructorDefinition> ConstructorTable;  //Track via mangled names

        public List<string> FinalVTable;    //Track mangled names, combine with VTables above to get final names
    }

    class GenericParser
    {
        public const int MaxArguments = 512;
        public const int MaxLocalVariables = 512;

        public Dictionary<string, ClassDefinition> ClassDefinitions;

        public Dictionary<int, MethodDefinition> MethodMetadataTokens;
        public Dictionary<int, ConstructorDefinition> CTorMetadataTokens;
        public Dictionary<int, FieldDefinition> FieldMetadataTokens;
        public Dictionary<int, ClassDefinition> ClassMetadataTokens;
        public Dictionary<int, string> StringTable;
        public List<string> FinalStringTable;

        private HashSet<int> ProcessedTokens;

        public GenericParser()
        {
            ProcessedTokens = new HashSet<int>();
            ClassDefinitions = new Dictionary<string, ClassDefinition>();

            MethodMetadataTokens = new Dictionary<int, MethodDefinition>();
            CTorMetadataTokens = new Dictionary<int, ConstructorDefinition>();
            FieldMetadataTokens = new Dictionary<int, FieldDefinition>();
            ClassMetadataTokens = new Dictionary<int, ClassDefinition>();

            StringTable = new Dictionary<int, string>();
            FinalStringTable = new List<string>();
        }

        #region Type Substitution API
        public void ReplaceDefinition(Type src, ClassDefinition dst)
        {
            int metadata_token = src.MetadataToken;

            if (dst.FinalVTable == null)
                dst.FinalVTable = new List<string>();

            ClassMetadataTokens[metadata_token] = dst;
            ClassDefinitions[MangleClassName(src)] = dst;
        }

        #endregion

        #region Name Conversion/Mangling
        private string CleanupName(string name)
        {
            return name.Replace(".", "_").Replace("[]", "Array");
        }

        private string MangleFieldName(FieldInfo info)
        {
            string res = $"_{info.MetadataToken.ToString("X8")}";

            return CleanupName(res);
        }

        private string MangleConstructorName(ConstructorInfo info)
        {
            string res = $"ctor";

            var parameters = info.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                res += "_" + MangleClassName(parameters[i].ParameterType);
            }

            return CleanupName(res);
        }

        private string MangleMethodName(MethodInfo info)
        {
            string res = $"{MangleClassName(info.ReturnType)}_{info.Name}";

            var parameters = info.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                res += "_" + MangleClassName(parameters[i].ParameterType);
            }

            return CleanupName(res);
        }

        private string MangleClassName(Type t)
        {
            if (t.IsNested)
                return CleanupName($"{t.Namespace}_{t.DeclaringType.Name}_{t.Name}");

            return CleanupName($"{t.Namespace}_{t.Name}");
        }

        private string FinalFieldName(FieldInfo t)
        {
            return "_" + t.MetadataToken.ToString("X8") + "_";
        }

        private string FinalConstructorName(ConstructorInfo t)
        {
            return "_" + t.MetadataToken.ToString("X8") + "_";
        }

        private string FinalMethodName(MethodInfo t)
        {
            return "_" + t.MetadataToken.ToString("X8") + "_";
        }

        private string FinalClassName(Type t)
        {
            return "_" + t.MetadataToken.ToString("X8") + "_";
        }
        #endregion

        private string ParseIL(Module m, MethodBody body, ParameterInfo[] param, ITarget target)
        {
            StringBuilder builder = new StringBuilder();

            byte[] il = body.GetILAsByteArray();
            ILParser p = new ILParser(il);

            Dictionary<int, List<int>> arg_slots = new Dictionary<int, List<int>>();

            Stack<StackItem> stack = new Stack<StackItem>();
            ulong[] args = new ulong[MaxArguments];
            ulong[] locals = new ulong[MaxLocalVariables];

            //First obtain the maximum number of slots of each kind needed, preallocate the space for them on the stack
            int arg_cnt = param.Length;
            int args_sz = 0;

            int local_cnt = body.LocalVariables.Count;
            int local_sz = 0;

            for(int i = 0; i < body.LocalVariables.Count; i++)
                if (body.LocalVariables[i].LocalType.IsPointer | body.LocalVariables[i].LocalType.IsArray | body.LocalVariables[i].LocalType.IsByRef)
                    local_sz += target.PointerSize;
                else
                    local_sz += ClassDefinitions[MangleClassName(body.LocalVariables[i].LocalType)].Size;

            for (int i = 0; i < param.Length; i++)
                if (param[i].ParameterType.IsPointer | param[i].ParameterType.IsArray | param[i].ParameterType.IsByRef)
                    args_sz += target.PointerSize;
                else
                    args_sz += ClassDefinitions[MangleClassName(param[i].ParameterType)].Size;

            //First allocate enough space for the arguments and locals
            if(args_sz + local_sz != 0)
                builder.AppendLine(target.AllocateStackSpace(args_sz + local_sz));
            
            do
            {
                //For each register allocation, we want to pick disjoint sets of slots where a connection represents overlap
                //Graph coloring where a connection represents overlap in usage
                //Start by iterating to find the first and last time a slot is used




                /*builder.Append(p.GetCurrentOpCode().Name);
                
                for (uint i = 0; i < p.GetParameterCount(); i++)
                {
                    var parameter = p.GetParameter(i);
                    var param_type = p.GetParameterType(i);

                    if (param_type == OperandType.InlineMethod && (MethodMetadataTokens.ContainsKey((int)parameter) || CTorMetadataTokens.ContainsKey((int)parameter)))
                    {
                        if (MethodMetadataTokens.ContainsKey((int)parameter))
                            builder.Append(" " + MethodMetadataTokens[(int)parameter].Entry.Name);
                        else
                            builder.Append(" " + CTorMetadataTokens[(int)parameter].Entry.Class);
                    }
                    else if (param_type == OperandType.InlineType && ClassMetadataTokens.ContainsKey((int)parameter))
                    {
                        builder.Append(" " + ClassMetadataTokens[(int)parameter].Type.Name);
                    }
                    else if (param_type == OperandType.InlineString)
                    {
                        builder.Append(" " + m.ResolveString((int)parameter));
                    }
                    else
                        builder.Append(" " + p.GetParameter(i).ToString("X16"));
                }*/

                builder.AppendLine(target.EmitOpCodes(p));

                builder.AppendLine();
            }
            while (p.NextInstruction());

            return builder.ToString();
        }

        #region Method Handling
        private string GenerateMethodSignature(MethodInfo info, ITarget target)
        {
            string method = "";
            string method_name = MangleMethodName(info);

            method += target.GenerateSymbol(info.IsPublic, info.IsStatic, true, false, true, true, FinalMethodName(info));

            return method;
        }

        private string GenerateMethodEntryCode(MethodInfo info, ITarget target, out string mthd_name)
        {
            //Setup stack frame and allocate stack space
            string code = "";

            mthd_name = MangleMethodName(info);

            //Handle custom declarations here
            if (ClassDefinitions.ContainsKey(MangleClassName(info.DeclaringType)))
            {
                //Change the mapping
                info = ClassDefinitions[MangleClassName(info.DeclaringType)].Type.GetMethod(info.Name);

                if (info == null)
                    return "";
            }

            code += target.FunctionEntry(mthd_name);

            if (!info.IsVirtual)
            {
                var parameters = info.GetMethodBody().LocalVariables;

                for (int i = 0; i < parameters.Count; i++)
                {

                }
            }

            //If not static, first parameter is the instance structure

            //A class is defined by a hard coded struct of function pointers
            //First we will go through all classes, building up an array of translated function names
            //Interfaces method offsets need to be the same among all implementing classes, alternatively, each interface contains a lookup table for the offset of the target method.
            //Inheritance is done by having the parent's structure at top level


            return code;
        }

        private string GenerateMethodExitCode(MethodInfo info, string mthd_name, ITarget target)
        {
            //Free stack space, setup return value, return
            string code = "";

            code += target.FunctionExit(mthd_name);
            return code;
        }

        public string ParseMethod(MethodInfo info, ITarget target)
        {
            if (ProcessedTokens.Contains(info.MetadataToken))
                return "";

            MethodBody bdy = info.GetMethodBody();
            StringBuilder builder = new StringBuilder();

            string mthd_name = "";

            builder.AppendLine(GenerateMethodSignature(info, target));
            builder.AppendLine(GenerateMethodEntryCode(info, target, out mthd_name));
            builder.AppendLine(ParseIL(info.Module, info.GetMethodBody(), info.GetParameters(), target));
            builder.AppendLine(GenerateMethodExitCode(info, mthd_name, target));

            ProcessedTokens.Add(info.MetadataToken);

            return builder.ToString();
        }
        #endregion

        #region Constructor handling
        private string GenerateConstructorSignature(ConstructorInfo info, ITarget target)
        {
            string method = "";
            string method_name = MangleConstructorName(info);

            method += target.GenerateSymbol(info.IsPublic, info.IsStatic, true, false, true, true, FinalConstructorName(info));

            return method;
        }

        private string GenerateConstructorEntryCode(ConstructorInfo info, ITarget target, out string mthd_name)
        {
            //Setup stack frame and allocate stack space
            string code = "";
            mthd_name = MangleConstructorName(info);

            //Handle custom declarations here
            if (ClassDefinitions.ContainsKey(MangleClassName(info.DeclaringType)))
            {
                //Change the mapping
                var ctor_cur_params = info.GetParameters();
                Type[] ctor_params = new Type[ctor_cur_params.Length];
                for (int i = 0; i < ctor_params.Length; i++)
                    ctor_params[i] = ctor_cur_params[i].ParameterType;

                info = ClassDefinitions[MangleClassName(info.DeclaringType)].Type.GetConstructor(ctor_params);

                if (info == null)
                    return "";
            }

            code += target.FunctionEntry(mthd_name);

            if (!info.IsVirtual)
            {
                var parameters = info.GetMethodBody().LocalVariables;

                for (int i = 0; i < parameters.Count; i++)
                {

                }
            }

            //If not static, first parameter is the instance structure

            //A class is defined by a hard coded struct of function pointers
            //First we will go through all classes, building up an array of translated function names
            //Interfaces method offsets need to be the same among all implementing classes, alternatively, each interface contains a lookup table for the offset of the target method.
            //Inheritance is done by having the parent's structure at top level

            return code;
        }

        private string GenerateConstructorExitCode(ConstructorInfo info, string mthd_name, ITarget target)
        {
            //Free stack space, setup return value, return
            string code = "";


            code += target.FunctionExit(mthd_name);
            return code;
        }

        public string ParseConstructor(ConstructorInfo info, ITarget target)
        {
            if (ProcessedTokens.Contains(info.MetadataToken))
                return "";

            MethodBody bdy = info.GetMethodBody();
            StringBuilder builder = new StringBuilder();

            string mthd_name = "";

            builder.AppendLine(GenerateConstructorSignature(info, target));
            builder.AppendLine(GenerateConstructorEntryCode(info, target, out mthd_name));
            builder.AppendLine(ParseIL(info.Module, info.GetMethodBody(), info.GetParameters(), target));
            builder.AppendLine(GenerateConstructorExitCode(info, mthd_name, target));

            ProcessedTokens.Add(info.MetadataToken);

            return builder.ToString();
        }
        #endregion

        public void ParseClass(Type t, ITarget target)
        {
            string mangled_parent_class_name = "";
            if (t.BaseType != null)
                mangled_parent_class_name = MangleClassName(t.BaseType);

            string mangled_class_name = MangleClassName(t);
            int sz = 0;

            Dictionary<string, FieldDefinition> field_defs = new Dictionary<string, FieldDefinition>();
            Dictionary<string, MethodDefinition> method_defs = new Dictionary<string, MethodDefinition>();
            Dictionary<string, ConstructorDefinition> ctor_defs = new Dictionary<string, ConstructorDefinition>();
            List<string> vtable = new List<string>();

            var mthds = t.GetMethods((BindingFlags)int.MaxValue);
            var ctors = t.GetConstructors();


            //First add size for struct from parent class
            if (t.BaseType != null && !ClassDefinitions.ContainsKey(mangled_parent_class_name))
                ParseClass(t.BaseType, target);

            //We want to add the parent's vtable on top first
            if (t.BaseType != null)
            {
                var vtable_p = ClassDefinitions[mangled_parent_class_name].FinalVTable;
                for (int i = 0; i < vtable_p.Count; i++)
                {
                    vtable.Add(vtable_p[i]);

                    if (ClassDefinitions[mangled_parent_class_name].MethodVTable.ContainsKey(vtable_p[i]))
                        method_defs[vtable_p[i]] = ClassDefinitions[mangled_parent_class_name].MethodVTable[vtable_p[i]];
                    else
                        ctor_defs[vtable_p[i]] = ClassDefinitions[mangled_parent_class_name].ConstructorTable[vtable_p[i]];
                }
            }



            //Add struct declaration for parent class, if present
            if (t.BaseType != null)
                sz += ClassDefinitions[mangled_parent_class_name].Size;

            //Now add variables from this class
            //Add size and alignment for each field
            var fields = t.GetFields((BindingFlags)int.MaxValue);
            for (int i = 0; i < fields.Length; i++)
            {
                var field_type = fields[i].FieldType;
                var field_type_name = MangleClassName(field_type);
                var mangled_field_name = MangleFieldName(fields[i]);

                if (!ClassDefinitions.ContainsKey(field_type_name))
                    ParseClass(field_type, target);

                int field_size = ClassDefinitions[field_type_name].Size;

                //Round up to the field size
                if (!fields[i].IsStatic && sz % field_size != 0)
                    sz += (field_size - (sz % field_size));

                field_defs[fields[i].Name] = new FieldDefinition()
                {
                    Entry = new ClassEntry()
                    {
                        Name = fields[i].Name,
                        Class = t.Name,
                        Namespace = t.Namespace,
                        IsPublic = fields[i].IsPublic,
                        IsStatic = fields[i].IsStatic,
                        MetadataToken = fields[i].MetadataToken,
                        FinalName = FinalFieldName(fields[i]),
                    },
                    IsProperty = false,
                    Type = fields[i].FieldType,
                    Offset = sz,
                    Size = field_size
                };

                FieldMetadataTokens[fields[i].MetadataToken] = field_defs[fields[i].Name];

                if (!fields[i].IsStatic)
                    sz += field_size;
            }

            //Add all the mangled names for the methods
            for (int i = 0; i < mthds.Length; i++)
            {
                method_defs[MangleMethodName(mthds[i])] = new MethodDefinition()
                {
                    Entry = new ClassEntry()
                    {
                        Name = mthds[i].Name,
                        Class = mthds[i].DeclaringType.Name,
                        Namespace = mthds[i].DeclaringType.Namespace,
                        IsPublic = mthds[i].IsPublic,
                        IsStatic = mthds[i].IsStatic,
                        MetadataToken = mthds[i].MetadataToken,
                        FinalName = FinalMethodName(mthds[i]),
                    },
                    Parameters = mthds[i].GetParameters(),
                    ReturnType = mthds[i].ReturnType,
                    Info = mthds[i],
                };

                MethodMetadataTokens[mthds[i].MetadataToken] = method_defs[MangleMethodName(mthds[i])];

                //If this method doesn't already exist in the final vtable, add it
                if (!vtable.Contains(MangleMethodName(mthds[i])))
                    vtable.Add(MangleMethodName(mthds[i]));

            }

            for (int i = 0; i < ctors.Length; i++)
            {
                ctor_defs[MangleConstructorName(ctors[i])] = new ConstructorDefinition()
                {
                    Entry = new ClassEntry()
                    {
                        Name = ctors[i].Name,
                        Class = ctors[i].DeclaringType.Name,
                        Namespace = ctors[i].DeclaringType.Namespace,
                        IsPublic = ctors[i].IsPublic,
                        IsStatic = ctors[i].IsStatic,
                        MetadataToken = ctors[i].MetadataToken,
                        FinalName = FinalConstructorName(ctors[i]),
                    },
                    Parameters = ctors[i].GetParameters(),
                    Info = ctors[i],
                };

                CTorMetadataTokens[ctors[i].MetadataToken] = ctor_defs[MangleConstructorName(ctors[i])];

                //If this ctor doesn't already exist in the final vtable, add it
                if (!vtable.Contains(MangleConstructorName(ctors[i])))
                    vtable.Add(MangleConstructorName(ctors[i]));
            }


            ClassDefinition def = new ClassDefinition()
            {
                Size = sz,
                ParentClass = mangled_parent_class_name,
                ConstructorTable = ctor_defs,
                FieldTable = field_defs,
                MethodVTable = method_defs,
                MetadataToken = t.MetadataToken,
                FinalVTable = vtable,
                Type = t,
            };
            ClassMetadataTokens[t.MetadataToken] = def;
            ClassDefinitions[mangled_class_name] = def;
        }

        public string CompileClass(Type t, ITarget target)
        {
            StringBuilder builder = new StringBuilder();

            var mthds = t.GetMethods((BindingFlags)int.MaxValue);
            var ctors = t.GetConstructors();

            var def = ClassDefinitions[MangleClassName(t)];


            //Allocate space for the static field
            //if field is static, don't track it here, same with static methods

            var key_col = def.FieldTable.Values;
            for (int i = 0; i < key_col.Count; i++)
            {
                var element = key_col.ElementAt(i);
                if (element.Entry.IsStatic)
                {
                    //Append the mangled name to the instruction stream
                    builder.AppendLine(target.GenerateSymbol(element.Entry.IsPublic, element.Entry.IsStatic, true, true, false, true, element.Entry.FinalName));

                    //Allocate space for the variable
                    builder.AppendLine(target.AllocateSpace(element.Size));
                }
            }

            if (def.FinalVTable.Count > 0)
            {
                builder.AppendLine(target.GenerateSymbol(true, true, true, false, false, true, MangleClassName(t)));
                builder.AppendLine(target.GenerateVTable(def.FinalVTable, def.MethodVTable, def.ConstructorTable));

                for (int i = 0; i < def.FinalVTable.Count; i++)
                {
                    if (def.MethodVTable.ContainsKey(def.FinalVTable[i]))
                        builder.AppendLine(ParseMethod(def.MethodVTable[def.FinalVTable[i]].Info, target));
                    else
                        builder.AppendLine(ParseConstructor(def.ConstructorTable[def.FinalVTable[i]].Info, target));
                }
            }

            return builder.ToString();
        }
    }
}
