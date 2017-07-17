using IL2ASM.Builtins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.IL
{
    public class VTableCollection
    {
        public struct VTableEntry
        {
            public MethodInfo Info;
        }

        public struct VTable
        {
            public List<VTableEntry> Entries { get; set; }
        }

        public Dictionary<Type, VTable> VTables;

        public VTableCollection()
        {
            //Generate a vtable for t
            VTables = new Dictionary<Type, VTable>();
        }

        public void AddType(Type t)
        {
            if (t == null)
                return;

            if (VTables.ContainsKey(t))
                return;

            if (Compiler.SpecialCaseTypes.Contains(t))
            {
                //Manually handle these cases, fake their presence, special case them in the code generator
                if (t == typeof(object))
                {
                    var vTable = new VTable()
                    {
                        Entries = new List<VTableEntry>()
                    };

                    vTable.Entries.Add(new VTableEntry()
                    {
                        Info = typeof(NativeObject).GetMethods((BindingFlags)Int32.MaxValue)[0]
                    });

                    VTables[typeof(object)] = vTable;
                }
                else if (t == typeof(System.ValueType))
                {
                    var vTable = new VTable()
                    {
                        Entries = new List<VTableEntry>()
                    };

                    vTable.Entries.Add(new VTableEntry()
                    {
                        Info = typeof(NativeValueType).GetMethods((BindingFlags)Int32.MaxValue)[0]
                    });

                    VTables[typeof(System.ValueType)] = vTable;
                }

                return;
            }

            if (t.BaseType != null)
            {
                Console.WriteLine($"AddType: {t.Name}");
                AddType(t.BaseType);
            }

            var vtable = new List<VTableEntry>();

            if (t.BaseType != null)
            {
                var vtable_p = VTables[t.BaseType].Entries;
                for (int i = 0; i < vtable_p.Count; i++)
                    vtable.Add(vtable_p[i]);
            }


            //Add all the mangled names for the methods
            var mthds = t.GetMethods();
            for (int i = 0; i < mthds.Length; i++)
            {
                string mangled_mthd_name = Helpers.GetMethodSignature(mthds[i]);
                int token = mthds[i].MetadataToken;

                //If this method doesn't already exist in the final vtable, add it
                bool found = false;
                for (int j = 0; j < vtable.Count; j++)
                {
                    if (Helpers.GetMethodSignature(vtable[j].Info) == mangled_mthd_name)
                    {
                        vtable[j] = new VTableEntry()
                        {
                            Info = mthds[i],
                        };
                        found = true;
                    }
                }
                if (!found && !mthds[i].IsStatic)
                {
                    vtable.Add(new VTableEntry()
                    {
                        Info = mthds[i],
                    });
                }
            }

            VTables[t] = new VTable()
            {
                Entries = vtable
            };
        }

    }
}
