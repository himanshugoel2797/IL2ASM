using IL2ASM.Binder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Builtins
{
    class NativeObject
    {
        private static int LastHashCode = 0;
        private int HashCode;

        public NativeObject()
        {
            HashCode = LastHashCode++;
        }
        
        public virtual bool NativeEquals(NativeObject b)
        {
            return false;
        }

        public static bool Equals(NativeObject a, NativeObject b)
        {
            return a.Equals(b);
        }
        
        public static bool NativeReferenceEquals(NativeObject a, NativeObject b)
        {
            return (a.HashCode == b.HashCode);
        }

        public Type NativeGetType()
        {
            return null;
        }

        public virtual int NativeGetHashCode()
        {
            return HashCode;
        }

        public string NativeToString()
        {
            return "Unknown";
        }
    }
}
