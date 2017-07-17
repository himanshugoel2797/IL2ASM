using IL2ASM.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.IL
{
    public partial class Compiler
    {
        public static readonly Type[] SpecialCaseTypes = new Type[] { typeof(Array), typeof(Enum), typeof(object), typeof(void), typeof(string), typeof(System.ValueType), typeof(System.Runtime.CompilerServices.RuntimeHelpers), typeof(IntPtr), typeof(UIntPtr), typeof(TypedReference), typeof(byte), typeof(sbyte), typeof(ushort), typeof(short), typeof(char), typeof(uint), typeof(int), typeof(ulong), typeof(long), typeof(float), typeof(double), typeof(bool) };

        public FieldCollection FieldSets;
        public VTableCollection VTableSets;

        private ITarget Target;
        private StringBuilder b;
        private HashSet<int> ProcessedTokens;

        public string Code
        {
            get
            {
                return b?.ToString();
            }
        }

        public int PointerSize
        {
            get
            {
                return Target.PointerSize;
            }
        }

        public Compiler(ITarget TargetCodeGenerator)
        {
            Target = TargetCodeGenerator;
            FieldSets = new FieldCollection(PointerSize);
            VTableSets = new VTableCollection();
            ProcessedTokens = new HashSet<int>();

            b = new StringBuilder();
        }

        public void CompileAssembly(Assembly a)
        {

            //Start by adding all its types to the VTableCollection and FieldCollection
            var types = a.GetTypes();
            foreach (Type t in types)
            {
                FieldSets.AddType(t);
                VTableSets.AddType(t);
            }

            //Now emit all the field sets and vtables
            foreach (KeyValuePair<Type, VTableCollection.VTable> tv in VTableSets.VTables)
            {
                b.AppendLine(Target.GenerateSymbol(tv.Key.IsPublic, false, true, true, false, true, Helpers.GetVTableName(tv.Key)));
                b.AppendLine(Target.GenerateVTable(tv.Value));
            }

            //Now start to emit code for all the methods
            for (int i = 0; i < VTableSets.VTables.Keys.Count; i++)
            {
                Type t = VTableSets.VTables.Keys.ElementAt(i);

                //This token is being processed now, add it first to resolve self references.
                ProcessedTokens.Add(t.MetadataToken);

                //Walk the VTable for this and generate each called method
                for (int j = 0; j < VTableSets.VTables[t].Entries.Count; j++)
                {
                    VTableCollection.VTableEntry v = VTableSets.VTables[t].Entries.ElementAt(j);

                    //This token is being processed now, add it first to resolve self references.
                    ProcessedTokens.Add(v.Info.MetadataToken);
                    
                    //Process this method, generating code
                    b.AppendLine(CompileMethod(v.Info));
                }

            }

        }
    }
}
