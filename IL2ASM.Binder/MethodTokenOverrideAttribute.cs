using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Binder
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
    public sealed class MethodTokenOverrideAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly Type type;
        readonly string mthd;

        // This is a positional argument
        public MethodTokenOverrideAttribute(Type type, string mthd_name)
        {
            this.type = type;
            this.mthd = mthd_name;
        }

        // This is a named argument
        public Type Type { get { return type; } }
        public string Method { get { return mthd; } }

        public int MetadataToken { get { return type.GetMethod(mthd).MetadataToken; } }
    }
}
