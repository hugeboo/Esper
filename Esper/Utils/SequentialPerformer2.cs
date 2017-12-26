using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    /// <summary>
    /// Asynchronously executes tasks exactly in the same order as they come.
    /// In contrast with SequentialPerformer do it in background threads of the thread pool.
    /// </summary>
    public sealed class SequentialPerformer2
    {
        /// <summary>
        /// Maximum task queue size. 
        /// &lt; -1 means &quot;not limited&quot;
        /// </summary>
        public int MaxQueueSize { get; set; } = -1;

        /// <summary>
        /// If true failed task (one throwing an exception) is returned to the beginning of the queue to be repeated when task execution is resumed.
        /// </summary>
        public bool RedoFailedTasks { get; set; }

        /// <summary>
        /// When blocking call to any method of this class is performed (e.g. Suspend() or Clear()) - 
        /// interval between successive re-checkings of the fact whether task queue is already stopped.
        /// </summary>
        public int BlockedCallPollingIntervalMs { get; set; } = 10;

        /// <summary>
        /// Set to true to suspend processing of task queue after any task error occurs.
        /// </summary>
        public bool SuspendOnError { get; set; }

        /// <summary>
        /// Returns a count of items in the queue
        /// </summary>
        public int Count
        {
            get { return this._TaskQueue.Count; }
        }

        public bool TaskRunning
        {
            get { return _TaskRunning; }
        }

        /// <param name="iMaxQueueSize">
        /// maximal size of queue - when reached new tasks are not added
        /// &lt; 1 means &quot;not limited&quot;
        /// </param>
        /// <param name="sThreadName">name for the worker threads</param>
        public SequentialPerformer2(
                Action<SequentialPerformer2, Exception> taskExceptionHandler = null,
                int maxQueueSize = -1)
        {
            TaskExceptionHandler = taskExceptionHandler;
            MaxQueueSize = maxQueueSize;
            BlockedCallPollingIntervalMs = 10;
        }

        public SequentialPerformer2(
                bool initiallySuspended,
                Action<SequentialPerformer2, Exception> taskExceptionHandler = null,
                int maxQueueSize = -1) 
            : this(taskExceptionHandler, maxQueueSize)
        {
            _Suspended = initiallySuspended;
        }

        /// <summary>
        /// If non-null used for handling exceptions occurred when performing tasks
        /// </summary>
        public Action<SequentialPerformer2, Exception> TaskExceptionHandler { get; set; }

        /// <summary>
        /// Adds another task to a queue;
        /// Returns false if failed to add (maximal queue size is reached)
        /// </summary>
        public bool AddTask(Action task)
        {
            int maxQueueSize = MaxQueueSize;
            bool taskAdded = false;
            bool startProcessingThread = false;
            _Lock.DoLocked(() =>
            {
                if (maxQueueSize > 0 && _TaskQueue.Count > maxQueueSize)
                    return;

                _TaskQueue.AddLast(task);
                taskAdded = true;

                if (!_ProcessingQueue && !_Suspended)
                {
                    _ProcessingQueue = true;
                    startProcessingThread = true;
                }
            });
            if (startProcessingThread)
            {
                Action a = () =>
                {
                    //QRResources.Localization.SetupCurrentThreadLanguage();
                    ProcessQueue();
                };
                Task.Factory.StartNew(a);
            }
            return taskAdded;
        }

        /// <summary>
        /// Suspends performing of tasks
        /// </summary>
        /// <param name="bBlockTillSuspended">
        /// set to true to black calling thread until task performing is actually suspended
        /// </param>
        public void Suspend(bool blockTillSuspended = false)
        {
            _Suspended = true;
            if (blockTillSuspended)
            {
                BlockTillTaskQueueProcessingStopped();
            }
        }

        /// <summary>
        /// Resumes performing of tasks (if their execution was suspended)
        /// </summary>
        public void Resume()
        {
            if (!_Suspended)
            {
                return;
            }
            _Suspended = false;
            bool startProcessingTask = false;
            _Lock.DoLocked(() =>
           {
               if (!_ProcessingQueue && _TaskQueue.Count > 0)
               {
                   _ProcessingQueue = true;
                   startProcessingTask = true;
               }
           });
            if (startProcessingTask)
            {
                Action a = () =>
                {
                    //QRResources.Localization.SetupCurrentThreadLanguage();
                    ProcessQueue();
                };
                Task.Factory.StartNew(ProcessQueue);
            }
        }

        /// <summary>
        /// Clears task queue.
        /// </summary>
        /// <param name="blockTillCleared">
        /// set to true to black calling thread until task performing queue is actually cleared
        /// </param>
        public void Clear(bool blockTillCleared = false)
        {
            if (blockTillCleared)
            {
                bool oldSuspended = _Suspended;
                _Suspended = true;
                BlockTillTaskQueueProcessingStopped(() => _TaskQueue.Clear());
                _Suspended = oldSuspended;
            }
            else
            {
                _Lock.DoLocked(() => _TaskQueue.Clear());
            }
        }

        /// <summary>
        /// Sequentially processes task queue in the separate thread (from the thread pool).
        /// </summary>
        void ProcessQueue()
        {
            while (!_Suspended)
            {
                Action task = null;
                _Lock.DoLocked(() =>
                {
                    task = _TaskQueue.FirstOrDefault();
                    if (task != null)
                    {
                        _TaskQueue.RemoveFirst();
                        _TaskRunning = true;
                    }
                    else
                    {
                        _TaskRunning = false;
                        _ProcessingQueue = false;
                    }
                });
                if (task == null)
                    break;

                bool succeeded;
                try
                {
                    task();
                    succeeded = true;
                }
                catch (Exception x)
                {
                    succeeded = false;
                    var eh = TaskExceptionHandler;
                    if (eh != null)
                    {
                        try
                        {
                            eh(this, x);
                        }
                        catch { }
                    }
                }
                if (!succeeded)
                {
                    if (RedoFailedTasks)
                    {
                        _Lock.DoLocked(() => _TaskQueue.AddFirst(task));
                    }
                    if (SuspendOnError)
                    {
                        _Suspended = true;
                    }
                }

                _Lock.DoLocked(() => _TaskRunning = false);
            }
            _Lock.DoLocked(() => _ProcessingQueue = false);
        }

        #region Methods for internal usage

        void BlockTillTaskQueueProcessingStopped(Action actionOnStopUnderQueueLock = null)
        {
            bool stopped = false;
            while (!stopped)
            {
                _Lock.DoLocked(() =>
                {
                    if (!_ProcessingQueue)
                    {
                        stopped = true;
                        if (actionOnStopUnderQueueLock != null)
                        {
                            actionOnStopUnderQueueLock();
                        }
                    }
                });

                Thread.Sleep(BlockedCallPollingIntervalMs);
            }
        }

        #endregion

        /// <summary>
        /// Temporary stores tasks to be performed
        /// </summary>
        readonly LinkedList<Action> _TaskQueue = new LinkedList<Action>();

        /// <summary>
        /// Protects task adding / retrieving
        /// </summary>
        readonly QRSpinLock _Lock = new QRSpinLock();

        /// <summary>
        /// If true there is a worker thread currently processing the task queue.
        /// </summary>
        volatile bool _ProcessingQueue;

        /// <summary>
        /// Is set to true to temporarily suspend task execution.
        /// </summary>
        volatile bool _Suspended;

        volatile bool _TaskRunning;
    }
}
