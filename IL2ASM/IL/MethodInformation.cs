using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.IL
{
    public class MethodInformation
    {
        private MethodInfo methodInfo;

        public MethodInformation(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
        }
    }
}
