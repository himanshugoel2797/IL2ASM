using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace TestApplication
{
    class Test2
    {
        public Test2()
        {

        }

        ~Test2()
        {

        }

        public virtual void Tester(ref int b)
        {
            Console.Write("Test");
        }
    }

    class Program : Test2, IDisposable
    {
        struct F
        {
            public int a;
            public float b;
            public ushort c;
            public string d;
        }

        F b;
        Program p;
        int a;

        public int this[int a]
        {
            get
            {
                return a;
            }
            set
            {

            }
        }

        public override void Tester(ref int b)
        {
            this.b.a = b;
            Console.WriteLine("Test2" + b.ToString());
        }

        public override string ToString()
        {
            return "T";
        }

        static void Main(string[] args)
        {
            F a;
            a.a = 50;
            a.d = "T";
            a.d.ToLower();

            int z = 0;
            
            Program prog = new Program();
            prog.Tester(ref z);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
