using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public virtual void Tester()
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
        }

        static F b;

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

        public override void Tester()
        {
            this.GetType();
            Console.WriteLine("Test2");
        }

        static void Main(string[] args)
        {
            F a;
            a.a = 50;

            Program prog = new Program();
            prog.Tester();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
