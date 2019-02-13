using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Engine.Core
{
    /// <summary>
    /// The scoring singleton
    /// </summary>
    public static class Scoring
    {
        /// <summary>
        /// Number of ms to delay in between scoring
        /// </summary>
        private const int EngineTickDelay = 10000;

        internal static EngineFrame Engine;
        private static Thread scoring_thread;

        /// <summary>
        /// Start the engine. Can only be called once
        /// </summary>
        /// <param name="engine"></param>
        public static void StartEngine(EngineFrame engine)
        {
            if (Engine != null)
                return; //throw new InvalidOperationException("An engine is already running. Cannot start a new engine in this instance.");
            Engine = engine;
            scoring_thread = new Thread(new ThreadStart(Score))
            {
                IsBackground = true
            };
            scoring_thread.Start();
        }

        /// <summary>
        /// Main Scoring thread
        /// </summary>
        private static async void Score()
        {
            while(Engine?.EngineRunning() ?? false)
            {
                await Engine.Tick();

                (uint,uint)[] Batch = Engine.GetBatch(Networking.BatchSize);

                if(Engine.IsOnline())
                {
                    await Networking.SendStates(Batch);
                }
                else
                {
#if DEBUG

                    try
                    {
                        string result = "";
                        foreach(var statepair in Batch)
                        {
                            result += statepair.Item1.ToString("X") + " => " + statepair.Item2.ToString("X") + "\r\n";
                        }
                        System.IO.File.WriteAllText("Debugging.txt", result);
                    }
                    catch
                    {

                    }
#endif
                    //else send batch to scoring utilities
                }

                await Task.Delay(EngineTickDelay);
            }
        }
    }
}
