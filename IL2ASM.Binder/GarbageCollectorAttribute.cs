using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Binder
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GarbageCollectorAttribute : Attribute
    {
        public GarbageCollectorAttribute()
        {

        }
    }
}
