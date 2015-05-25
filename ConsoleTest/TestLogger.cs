using System;
using NeuronDocumentSync.Interfaces;

namespace ConsoleTest
{
    class TestLogger: INeuronLogger
    {
        public void Log(string message)
        {
            Console.WriteLine("From Logger");
            Console.WriteLine(message);
        }
    }
}
