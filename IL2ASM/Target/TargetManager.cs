using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM.Target
{
    static class TargetManager
    {
        private static Dictionary<string, ITarget> parsers;

        static TargetManager()
        {
            parsers = new Dictionary<string, ITarget>();

            parsers["x86_64_gas"] = new x86_64.Target();
        }

        private static string ParseAssembly(string arch, GenericParser p, Assembly a)
        {
            if (!parsers.ContainsKey(arch))
                throw new Exception();

            StringBuilder code = new StringBuilder();
            parsers[arch].InitializeTarget(p);

            /*
            var refs = a.GetReferencedAssemblies();
            for (int i = 0; i < refs.Length; i++)
            {
                Assembly b = Assembly.Load(refs[i].FullName);
                Console.WriteLine(b.FullName);
                code.AppendLine(ParseAssembly(arch, b));
            }*/

            var types = a.GetTypes();

            //First parse all the classes, build their structures, allocate static globals
            for (int i = 0; i < types.Length; i++)
            {
                code.AppendLine(p.ParseClass(types[i], parsers[arch]));
            }

            //Now actually compile all their data
            for(int i = 0; i < types.Length; i++)
            {
                code.AppendLine(p.CompileClass(types[i], parsers[arch]));
            }

            return code.ToString();
        }

        public static string ParseAssembly(string arch, Assembly a)
        {
            return ParseAssembly(arch, new GenericParser(), a);
        }
    }
}
