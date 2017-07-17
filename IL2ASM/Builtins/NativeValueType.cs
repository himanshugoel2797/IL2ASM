using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Builtins
{
    class NativeValueType
    {
        private static int LastHashCode = 0;
        private int HashCode;

        public NativeValueType()
        {
            HashCode = LastHashCode++;
        }

        public virtual bool NativeEquals(NativeValueType b)
        {
            return false;
        }

        public static bool Equals(NativeValueType a, NativeValueType b)
        {
            return a.Equals(b);
        }

        public static bool NativeReferenceEquals(NativeValueType a, NativeValueType b)
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
