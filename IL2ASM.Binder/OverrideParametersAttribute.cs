using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Binder
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class OverrideParametersAttribute : Attribute
    {
        readonly Type override_target;
        readonly Type parent_type;

        // This is a positional argument
        public OverrideParametersAttribute(Type override_target, Type parent)
        {
            this.override_target = override_target;
            this.parent_type = parent;       
        }

        public Type OverrideTarget
        {
            get { return override_target; }
        }

        public Type ParentType
        {
            get { return parent_type; }
        }
    }
}
