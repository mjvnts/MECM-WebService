// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScsSemaphore.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.Util
{
    using System;
    using System.Threading;

    /// <summary>
    /// the <see cref="ScsSemaphore"/> class is used to
    /// synchronize threads. Control the threads
    /// by aquiring and releasing one or more slots. If one
    /// ore more slots are still available then the thread is allowed
    /// to run, if no more slots are available the thread has to wait
    /// until the number of required slots are available.
    /// </summary>
    public class ScsSemaphore
    {
        /// <summary>
        /// <value name="MaxDefaultSlots">the number of
        /// default slots used by <see cref="ScsSemaphore"/></value>
        /// </summary>
        private const int MaxDefaultSlots = 2;
        /// <summary>
        /// <value name="currentSlotsUsed">the number
        /// of slots that are currently accquired</value>
        /// </summary>
        private Int32 _currentSlotsUsed;
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        /// <summary>
        /// <value name="maxSlots">the number of maximum
        /// slots that can be accquired</value>
        /// </summary>
        private Int32 _maxSlots;
        // ReSharper restore FieldCanBeMadeReadOnly.Local
        /// <summary>
        /// <value name="syncLock">read only object
        /// to lock the semaphore</value>
        /// </summary>
        private readonly object _syncLock;

        /// <summary>
        /// initializes a new <see cref="ScsSemaphore"/> with
        /// the number of available slots submitted by
        /// <paramref name="maxSlots"/> 
        /// </summary>
        /// <param name="maxSlots">the number of slots</param>
        /// <exception cref="ArgumentException">throws an exception if the value for <param name="maxSlots"> is negative</param></exception>
        public ScsSemaphore(Int32 maxSlots)
        {
            if (maxSlots <= 0) { throw new ArgumentException("the value for maxSlots must be greater than or equal 0"); }
            this._maxSlots = maxSlots;
            this._syncLock = new object();
            this._currentSlotsUsed = 0;
        }

        /// <summary>
        /// initializes a new <see cref="ScsSemaphore"/> with
        /// the default number of slots (2)
        /// </summary>
        public ScsSemaphore() : this(ScsSemaphore.MaxDefaultSlots) { }

        /// <summary>
        /// gets the number of maximum slots
        /// </summary>
        public int MaxSlots
        {
            get
            {
                return this._maxSlots;
            }
        }

        /// <summary>
        /// gets how many slots currently are accquired
        /// </summary>
        /// <returns></returns>
        public Int32 GetCurrentSlotsAccquired()
        {
            return this._currentSlotsUsed;
        }

        /// <summary>
        /// accquires the number of slot submitted by <paramref name="numSlots"/>
        /// </summary>
        /// <param name="numSlots">number of slots to accquire</param>
        public void AcquireSlots(Int32 numSlots)
        {
            lock (this._syncLock)
            {
                while (this._currentSlotsUsed + numSlots > this._maxSlots)
                {
                    try
                    {
                        Monitor.Wait(this._syncLock);
                    }
                    catch (ThreadInterruptedException)
                    {
                        //only catch interrupted exception
                    }
                }
                this._currentSlotsUsed += numSlots;
                Monitor.PulseAll(this._syncLock);
            }
        }

        /// <summary>
        /// accquires the maximum number of slots defined
        /// by <see cref="_maxSlots"/>
        /// </summary>
        public void AcquireAllSlots()
        {
            this.AcquireSlots(this._maxSlots);
        }

        /// <summary>
        /// releases the number of slots submitted by <paramref name="numSlots"/>
        /// </summary>
        /// <param name="numSlots">the number of slots to release</param>
        public void ReleaseSlots(Int32 numSlots)
        {
            lock (this._syncLock)
            {
                while (this._currentSlotsUsed - numSlots < 0)
                {
                    try
                    {
                        Monitor.Wait(this._syncLock);
                    }
                    catch (ThreadInterruptedException)
                    {
                        //only catch interrupted exception
                    }
                }
                this._currentSlotsUsed -= numSlots;
                Monitor.PulseAll(this._syncLock);
            }
        }

        /// <summary>
        /// releases the maximum number of slots defined
        /// by <see cref="_maxSlots"/>
        /// </summary>
        public void ReleaseAllSlots()
        {
            this.ReleaseSlots(this._maxSlots);
        }
    }
}
