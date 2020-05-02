using System;
using Newtonsoft.Json;

namespace DevTest
{
    class Base
    {
        public int Number { get; set; }
    }

    class Derived : Base
    {
        public string Str { get; set; }
    }
    
    
    
    class Program
    {
        static void Main(string[] args)
        {
            var message = new Derived();
            var ser = JsonConvert.SerializeObject(message);
            Console.WriteLine(ser);
        }
    }
}