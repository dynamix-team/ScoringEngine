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

#if OFFLINE
                    await OfflineTick(Batch);
#endif
                }
                
                await Task.Delay(EngineTickDelay);
            }
        }

        #region OFFLINE
        #if OFFLINE
        /// <summary>
        /// The state of a scoring item
        /// </summary>
        public enum ScoringState
        {
            InProgress,
            Completed,
            Failed
        }

        /// <summary>
        /// An offline scoring item
        /// </summary>
        public sealed class ScoringItem
        {
            /// <summary>
            /// The state of this scoring item
            /// </summary>
            public Scoring.ScoringState State = ScoringState.InProgress;
            /// <summary>
            /// The expected value of this scoring item
            /// </summary>
            public uint ExpectedState = 0;
            /// <summary>
            /// The number of points this scoring item is worth
            /// </summary>
            public ushort NumPoints = 0;

            public delegate string ScoringStatus();

            public ScoringStatus SuccessStatus = () => { return "Check passed."; }; //Quick explanation: We want status safestring'd, but safestring is not cross assembly for security reasons, so
                                                                                    //we make this a delegate so that the engine derivative with safestring can return the assigned safestring...
            public ScoringStatus FailureStatus = () => { return "Check failed."; };

        }

        /// <summary>
        /// Score an offline engine
        /// </summary>
        /// <param name="Batch">The batch to score</param>
        /// <returns></returns>
        private static async Task OfflineTick((uint, uint)[] Batch)
        {
            await Task.Run(() =>
            {

            });
        }
        #endif
        #endregion
    }
}
