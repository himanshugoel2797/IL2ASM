using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CLRToCOSR
{
    public class VTableCollection : ICollection
    {
        public struct VTable
        {
            public MethodData[] Methods { get; set; }
        }

        private List<VTable> vtableEntries;

        public int Count
        {
            get
            {
                return ((ICollection)vtableEntries).Count;
            }
        }

        public object SyncRoot
        {
            get
            {
                return ((ICollection)vtableEntries).SyncRoot;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return ((ICollection)vtableEntries).IsSynchronized;
            }
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)vtableEntries).CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return ((ICollection)vtableEntries).GetEnumerator();
        }

        public void Add(Type t)
        {
            VTable v = new VTable();
            var mthds = t.GetMethods((BindingFlags)int.MaxValue);

            v.Methods = new MethodData[mthds.Length];
            for(int i = 0; i < mthds.Length; i++)
            {
                v.Methods[i] = new MethodData();

                var p = mthds[i].GetParameters();
                v.Methods[i].GenericArgs = mthds[i].GetGenericArguments();
                v.Methods[i].ReturnType = mthds[i].ReturnType;

                for(int j = 0; j < p.Length; j++)
                {
                    ParamData pData = new ParamData();
                    pData.Type = p[j].ParameterType;
                    
                }

                //While we're at it, add the method body as part of the vtable for the final pass if available
            }
        }
    }
}
