using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {

        }
    }
    internal static class StringExtensions
    {
        public static void Dump(this string value)
        {
            Console.WriteLine(value);
        }
    }
}
