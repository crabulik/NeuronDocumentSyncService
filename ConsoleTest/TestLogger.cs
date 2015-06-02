using System;
using NeuronDocumentSync.Interfaces;

namespace ConsoleTest
{
    class TestLogger
    {
        public void Log(string message)
        {
            Console.WriteLine("From Logger");
            Console.WriteLine(message);
        }
    }
}
