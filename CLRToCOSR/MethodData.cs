using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRToCOSR
{
    public class MethodData
    {
        public string MethodName { get; set; }
        public Type ReturnType { get; set; }
        public ParamData[] Params { get; set; }
        public Type[] GenericArgs { get; set; }
        public MethodFlags Flags { get; set; }
    }
}
