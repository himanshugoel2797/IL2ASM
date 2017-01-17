using IL2ASM.Builtins;
using IL2ASM.Builtins.Types;
using IL2ASM.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.IL
{
    public class FieldCollection
    {
        public struct FieldData
        {
            public FieldInfo Info;
            public int Offset;
            public int Size;
        }

        public struct FieldTable
        {
            public List<FieldData> Entries;
            public int Size;
        }

        public Dictionary<Type, FieldTable> FieldTables;

        private int ptrSize;

        public FieldCollection(int pointerSize)
        {
            FieldTables = new Dictionary<Type, FieldTable>();
            ptrSize = pointerSize;
        }

        public void AddType(Type t)
        {
            if (t == null)
                return;

            if (FieldTables.ContainsKey(t))
                return;

            if (Compiler.SpecialCaseTypes.Contains(t))
            {
                //Manually handle these cases
                //These are only meant to fake the sizes, operations are all handled by the code generator.
                if (t == typeof(object))
                {
                    var fTable = new FieldTable()
                    {
                        Entries = new List<FieldData>(),
                        Size = sizeof(int),
                    };

                    fTable.Entries.Add(new FieldData()
                    {
                        Info = typeof(NativeObject).GetFields((BindingFlags)Int32.MaxValue)[0],
                        Offset = 0,
                        Size = fTable.Size
                    });

                    FieldTables[typeof(object)] = fTable;
                }
                else if (t == typeof(void))
                {
                    var fTable = new FieldTable()
                    {
                        Entries = new List<FieldData>(),
                        Size = 0,
                    };

                    fTable.Entries.Add(new FieldData()
                    {
                        Info = typeof(NativeInt32).GetFields((BindingFlags)Int32.MaxValue)[0],
                        Offset = 0,
                        Size = fTable.Size
                    });

                    FieldTables[typeof(void)] = fTable;
                }
                else if (t == typeof(Enum))
                {
                    var fTable = new FieldTable()
                    {
                        Entries = new List<FieldData>(),
                        Size = 0,
                    };

                    fTable.Entries.Add(new FieldData()
                    {
                        Info = typeof(NativeInt32).GetFields((BindingFlags)Int32.MaxValue)[0],
                        Offset = 0,
                        Size = fTable.Size
                    });

                    FieldTables[typeof(Enum)] = fTable;
                }
                else
                {
                    var fTable = new FieldTable()
                    {
                        Entries = new List<FieldData>(),
                        Size = Marshal.SizeOf(t),
                    };

                    fTable.Entries.Add(new FieldData()
                    {
                        Info = t.GetFields((BindingFlags)Int32.MaxValue)[0],
                        Offset = 0,
                        Size = fTable.Size
                    });

                    FieldTables[t] = fTable;
                }

                return;
            }

            if (t.BaseType != null && t.BaseType != t && !t.IsEnum)
            {
                Console.WriteLine(t.BaseType.Name);
                AddType(t.BaseType);
            }

            var ftable = new List<FieldData>();
            int offset = 0;
            int netSize = 0;

            if (t.BaseType != null && !t.IsEnum)
            {
                var ftable_p = FieldTables[t.BaseType].Entries;
                for (int i = 0; i < ftable_p.Count; i++)
                {
                    var f = new FieldData()
                    {
                        Info = ftable_p[i].Info,
                        Offset = offset,
                        Size = ftable_p[i].Size,
                    };

                    ftable.Add(f);
                    netSize += ftable_p[i].Size;
                    offset += ftable_p[i].Size;
                }
            }


            //Add all the mangled names for the methods
            var fields = t.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                string mangled_field_name = Helpers.GetFieldSignature(fields[i]);
                int token = fields[i].MetadataToken;

                //If this method doesn't already exist in the final vtable, add it
                bool found = false;
                for (int j = 0; j < ftable.Count; j++)
                {
                    if (Helpers.GetFieldSignature(ftable[j].Info) == mangled_field_name)
                    {
                        ftable[j] = new FieldData()
                        {
                            Info = fields[i],
                            Offset = ftable[j].Offset,
                            Size = ftable[j].Size,
                        };

                        found = true;
                    }
                }
                if (!found)
                {
                    int sz = 0;
                    if (fields[i].FieldType.IsClass)
                        sz = ptrSize;
                    else if (fields[i].FieldType.IsEnum)
                    {
                        sz = sizeof(int);
                    }
                    else
                    {
                        if (!FieldTables.ContainsKey(fields[i].FieldType))
                            AddType(fields[i].FieldType);

                        sz = FieldTables[fields[i].FieldType].Size;
                    }

                    ftable.Add(new FieldData()
                    {
                        Info = fields[i],
                        Offset = offset,
                        Size = sz,
                    });

                    netSize += sz;
                    offset += sz;
                }
            }

            FieldTables[t] = new FieldTable()
            {
                Entries = ftable,
                Size = netSize,
            };
        }
    }
}
