using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.IL
{
    static class Helpers
    {
        private static string CleanupName(string res)
        {
            return res.Replace(".", "_");
        }

        public static string GetClassSignature(Type t)
        {
            return $"_{t.Namespace}_{t.Name}";
        }

        public static string GetFieldSignature(FieldInfo info)
        {
            return GetFieldSignature(info.FieldType, info.Name);
        }

        public static string GetFieldSignature(Type t, string name)
        {
            return CleanupName("_" + GetClassSignature(t) + "_" + name);
        }

        public static string GetMethodSignature(MethodInfo info)
        {
            return GetMethodSignature(info.ReturnType, info.Name, info.GetParameters());
        }

        public static string GetMethodSignature(Type retType, string name, ParameterInfo[] param)
        {
            string res = $"{GetClassSignature(retType)}_{name}";
            
            for (int i = 0; i < param.Length; i++)
            {
                res += "_" + GetClassSignature(param[i].ParameterType);
            }

            return CleanupName(res);
        }

        public static string GetMethodName(MethodInfo info)
        {
            return GetMethodName(info.MetadataToken);
        }

        public static string GetMethodName(int metadata_tkn)
        {
            return "_" + metadata_tkn.ToString("X8") + "_";
        }

        public static string GetClassName(Type t)
        {
            return GetClassSignature(t);
        }

        public static string GetVTableName(Type t)
        {
            return GetClassName(t) + "_VTable";
        }

    }
}
