using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CLRToCOSR
{
    public class CLRToCOSR
    {
        public static void Compile(string src, string dst)
        {
            //Load the CLR binary
            Assembly a = Assembly.LoadFile(src);

            //Extract all information and put it into an easier to parse format

            //First iterate over all the types, building up vtables and data tables
            var types = a.GetTypes();
            foreach(Type t in types)
            {

            }

            //Save the formatted data.
        }
    }
}
