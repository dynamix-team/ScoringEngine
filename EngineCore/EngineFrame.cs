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
        private readonly Dictionary<uint, uint> STATE = new Dictionary<uint, uint>();
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
            }
            return Batch;
        }

        /// <summary>
        /// Is the engine an online engine
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsOnline()
        {
            return true;
        }

        /// <summary>
        /// Collect the states of all of the checks on the system
        /// </summary>
        internal protected virtual async System.Threading.Tasks.Task Tick()
        {
            await System.Threading.Tasks.Task.Delay(1);
        }
    }
}
