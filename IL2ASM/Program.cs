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

        struct F
        {
            public int a;
            public double b;
        }

        static void TestMethod()
        {
            F a;
            a.a = 0;
            a.a += 50;
        }

        static void Main(string[] args)
        {
            Assembly assem = Assembly.LoadFile(System.IO.Path.GetFullPath("TestApplication.exe"));
            Console.WriteLine(TargetManager.ParseAssembly("x86_64_gas", assem));

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
