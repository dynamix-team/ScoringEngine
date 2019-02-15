using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace Engine.Core
{
    /// <summary>
    /// Extend to create an engine framework
    /// </summary>
    public abstract class EngineFrame
    {

        private readonly Dictionary<ushort, CheckState> STATE = new Dictionary<ushort, CheckState>();

        private readonly List<ushort> QUEUE = new List<ushort>();

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
            if(!STATE.ContainsKey((ushort)id))
                STATE[(ushort)id] = (id, state);
            else
                STATE[(ushort)id] += state;
            if (!QUEUE.Contains((ushort)id))
                QUEUE.Add((ushort)id);
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
                    Batch[i] = CheckState.StatePair(QUEUE[0], STATE[QUEUE[0]]);
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
        protected void Expect(ushort id, Scoring.ScoringItem ExpectedState)
        {
            EXPECTED[id] = ExpectedState;
        }

        /// <summary>
        /// Check the state of an expected check
        /// </summary>
        /// <param name="id">The ID of the check to evaluate</param>
        /// <returns></returns>
        internal void Evaluate(ushort id)
        {
            if (!EXPECTED.ContainsKey(id) || !STATE.ContainsKey(id))
                return; //Cant evaluate a non-existant ID
            if (EXPECTED[id].State == Scoring.ScoringState.Failed)
                return; //Cant evaluate a failed scoring state
            if (STATE[id].Failed)
                EXPECTED[id].State = Scoring.ScoringState.Failed;
            else
                EXPECTED[id].State = (STATE[id].State == EXPECTED[id].ExpectedState) ? Scoring.ScoringState.Completed : Scoring.ScoringState.InProgress;
        }
        #endif
        #endregion

        /// <summary>
        /// Fail the check permanently
        /// </summary>
        /// <param name="id">The check to fail</param>
        public void Fail(ushort id)
        {
            STATE[id].EngineFlags |= (byte)CheckRuntimeFlags.Failure;
        }

        /// <summary>
        /// Flags that are set and managed by the engine at runtime
        /// </summary>
        [Flags]
        public enum CheckRuntimeFlags : byte
        {
            /// <summary>
            /// The check failed and will no longer be scored
            /// </summary>
            Failure = 1,

            /// <summary>
            /// The check state is corrupted, just discard
            /// </summary>
            Invalid = 128
        }

        /// <summary>
        /// The state of a check
        /// </summary>
        private sealed class CheckState
        {
            internal byte DefFlags;
            internal byte EngineFlags;
            internal uint State;

            internal bool Failed
            {
                get { return (EngineFlags & (byte)CheckRuntimeFlags.Failure) > 0; }
            }

            public static implicit operator CheckState((uint,uint) pair)
            {
                CheckState state = new CheckState();
                //first two bytes are ID
                //3rd byte is static flags
                //4th byte is runtime flags
                state.DefFlags = (byte)(pair.Item1 >> 16);
                state.EngineFlags = (byte)(pair.Item1 >> 24);
                state.State = pair.Item2;
                return state;
            }

            public static CheckState operator +(CheckState checkstate, uint newstate)
            {
                checkstate.State = newstate;
                return checkstate;
            }

            /// <summary>
            /// Create a state pair (id + flags) => state from an ID and a state
            /// </summary>
            /// <param name="id">The id of the check</param>
            /// <param name="state">The check state object</param>
            /// <returns></returns>
            public static (uint, uint) StatePair(ushort id, CheckState state)
            {
                return (id | ((uint)state.DefFlags << 16) | ((uint)state.EngineFlags << 24), state.State);
            }
        }
    }
}
