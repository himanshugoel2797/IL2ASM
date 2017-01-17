using IL2ASM.IL;
using IL2ASM.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IL2ASM
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assem = Assembly.LoadFile(System.IO.Path.GetFullPath("TestApplication.exe"));

            var x86_64_target = new Target.x86_64.Target();
            Compiler compiler = new Compiler(x86_64_target);

            compiler.CompileAssembly(assem);

            Console.WriteLine(compiler.Code);

            /*
            for(int i = 0; i < assem.DefinedTypes.Count(); i++)
            {
                string r = TargetManager.ParseClass("x86_64_gas", assem.DefinedTypes.ElementAt(i));
                Console.WriteLine(r);
            }*/

            while (true) ;
        }
    }
}
