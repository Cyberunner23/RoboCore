using System;

namespace DevTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var message = new TestMessage();
            message.Thing = 5;
            Console.WriteLine(message.GetMessageTypeHash());
        }
    }
}