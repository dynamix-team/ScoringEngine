using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
//TODO: Failure states through event binding: translator binds the failure event to a built in engine function, FAIL(ID), which for online reports to the networking layer, and offline, fails the eval.
//Online will report a detected failure, and allow server to qualify it through server side logic

//TODO: optional flag for checks to prevent state regression (optimization)

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

        private const string ScoresName = "scores.json";
        private const string ScoringDataTemplate =
        @"
        {
            ""scoring_info"": 
            {
                ""name"": ""{{name}}"",
                ""start_time"": ""{{start_time}}"",
                ""running_time"": ""{{running_time}}"",
                ""checks_total"": {{checks_total}},
                ""checks_complete"": {{checks_complete}}
            },
            ""checks"": 
            {
                {{checks}}
            }
        }
        ";

        private const string CheckDataTemplate =
        @"
            ""{{check_id}}"": 
            {
                ""points"": {{points}},
                ""description"": ""{{description}}""
            }
        ";

        private static string PublicReadPath
        {
            get
            {
                if (Engine == null)
                    return Path.Combine(Environment.CurrentDirectory, "public-read");
                return Path.Combine(Environment.CurrentDirectory, Engine.__PUBLIC);
            }
        }

        private static string ScoresPath
        {
            get
            {
                return Path.Combine(PublicReadPath, ScoresName);
            }
        }

        /// <summary>
        /// Start the engine. Can only be called once
        /// </summary>
        /// <param name="engine"></param>
        public static void StartEngine(EngineFrame engine)
        {
            if (Engine != null)
                return; //throw new InvalidOperationException("An engine is already running. Cannot start a new engine in this instance.");
            Engine = engine;
            try
            {
                Directory.CreateDirectory(PublicReadPath); //if this gets deleted during a comp there is an issue with the anti-cheat driver. This is mainly for debugging
            }
            catch
            {
            }
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

                (uint,uint)[] Batch = Engine.PollBatch(Networking.BatchSize);
#if OFFLINE
                await OfflineTick(Batch);
#else
                await Networking.SendStates(Batch);
#endif
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
            public short NumPoints = 0;
            /// <summary>
            /// This vuln's id
            /// </summary>
            public ushort ID = 0;

            public delegate string ScoringStatus();

            public ScoringStatus SuccessStatus = () => { return "Check passed."; }; //Quick explanation: We want status safestring'd, but safestring is not cross assembly for security reasons, so
                                                                                    //we make this a delegate so that the engine derivative with safestring can return the assigned safestring...
            public ScoringStatus FailureStatus = () => { return "Check failed."; };

            public override string ToString()
            {
                return State switch
                {
                    ScoringState.Completed => FormatStateObj(ID, NumPoints, SuccessStatus()),
                    ScoringState.Failed => FormatStateObj(ID, NumPoints, FailureStatus()),
                    _ => ""
                };
            }
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
                foreach(var checkresult in Batch)
                {
                    Engine.Evaluate((ushort)checkresult.Item1);
                }
                WriteScoringReport();
            });
        }

        private static void WriteScoringReport()
        {
            try
            {
                string result = ScoringDataTemplate;
                string[] scoringitems = Engine?.ScoringItems ?? new string[0];
                int TotalItems = Engine?.Count ?? 0;

                //TODO Image name
                result = result.Replace("{{name}}", "Dynamix Debugging Image");
                //TODO Start Time
                result = result.Replace("{{start_time}}", "??");
                //TODO Running Time
                result = result.Replace("{{running_time}}", "??");

                result = result.Replace("{{checks_total}}", TotalItems.ToString());
                result = result.Replace("{{checks_complete}}", scoringitems.Length.ToString());

                string checktext = "";

                for(int i = 0; i < scoringitems.Length; i++)
                {
                    checktext += scoringitems[i] + "\r\n" + (i == scoringitems.Length - 1 ? "" : ",");
                }

                result = result.Replace("{{checks}}", checktext);

                File.WriteAllText(ScoresPath, result);
            }
            catch { } //File is probably busy
        }

        private static string FormatStateObj(ushort id, short pointValue, string description)
        {
            return CheckDataTemplate.Replace("{{check_id}}", id.ToString()).Replace("{{points}}", pointValue.ToString()).Replace("{{description}}", description.ToString());
        }
#endif
#endregion
    }
}
