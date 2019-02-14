using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core
{
    /// <summary>
    /// Extend to create an engine framework
    /// </summary>
    public abstract class EngineFrame
    {
        internal readonly Dictionary<uint, uint> STATE = new Dictionary<uint, uint>();
        private readonly List<uint> QUEUE = new List<uint>();

        private bool Exit;

        /// <summary>
        /// Used for CRITICAL exits. In the future, this will invoke a bluescreen
        /// </summary>
        protected void AbortEngine()
        {
            Exit = true;
        }

        /// <summary>
        /// Is the engine running?
        /// </summary>
        /// <returns></returns>
        public bool EngineRunning()
        {
            return !Exit;
        }

        /// <summary>
        /// Register a check's state to pipe into the appropriate scoring channel
        /// </summary>
        /// <param name="id">The id of the state</param>
        /// <param name="state">The state of the check</param>
        protected void RegisterCheck(uint id, uint state)
        {
            STATE[id] = state;
            if (!QUEUE.Contains(id))
                QUEUE.Add(id);
        }

        /// <summary>
        /// Gets a batch to push to the server, while mutating the queue.
        /// </summary>
        /// <param name="BatchSize">The maximum amount of items to retrieve</param>
        /// <returns></returns>
        internal (uint,uint)[] GetBatch(byte BatchSize)
        {
            BatchSize = Math.Min((byte)Math.Min(128, QUEUE.Count), BatchSize);
            (uint, uint)[] Batch = new (uint, uint)[BatchSize];
            for(byte i = 0; i < BatchSize; i++)
            {
                try
                {
                    Batch[i] = (QUEUE[0], STATE[QUEUE[0]]);
                }
                catch
                {
                    Batch[i] = (0xFFFFFFFF, 0); //to catch key not found exception which should be impossible unless someone manually modifies the memory
                }
                QUEUE.RemoveAt(0);
            }
            return Batch;
        }

        /// <summary>
        /// Is the engine an online engine
        /// </summary>
        /// <returns></returns>
        public bool IsOnline()
        {
#if OFFLINE
            return false;
#else
            return true;
#endif
        }

        /// <summary>
        /// Collect the states of all of the checks on the system
        /// </summary>
        internal protected abstract System.Threading.Tasks.Task Tick();

        #region OFFLINE
        #if OFFLINE
        /// <summary>
        /// A table of expected values for states
        /// </summary>
        private readonly Dictionary<ushort, Scoring.ScoringItem> EXPECTED = new Dictionary<ushort, Scoring.ScoringItem>();

        /// <summary>
        /// Register an offline check to be scored
        /// </summary>
        internal protected void Expect(ushort id, Scoring.ScoringItem ExpectedState)
        {
            EXPECTED[id] = ExpectedState;
        }
        #endif
        #endregion

    }
}
