using System;
using System.Threading.Tasks;

namespace LinuxDebuggingConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Dynamix GNU debugging console v1.0");
            Engine.Core.Engine engine = new Engine.Core.Engine();
            Engine.Core.Scoring.StartEngine(engine);

            while (true)
            {
                await Task.Delay(10000);
                Console.WriteLine(DateTime.Now.ToShortTimeString() + ": i am still alive...");
            }
        }
    }
}
