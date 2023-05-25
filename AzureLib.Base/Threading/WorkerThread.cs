using System;
using System.Collections.Generic;
using System.Threading;

namespace AzureLib.Base.Threading;

public class WorkerThread
{
    public const int DefaultMaxQueueSize = 10000;

    #region Public properties and methods

    public string ThreadName
    {
        get { return this.thread.Name; }
    }

    public int MaxQueueSize
    {
        get { return this.queueMaxSize; }
    }

    public int CurrentQueueSize
    {
        get
        {
            lock (this.queueOfOperations)
            {
                return this.queueOfOperations.Count;
            }
        }
    }

    public bool IsActive
    {
        get { return this.isActive; }
    }

    public bool IsBusy
    {
        get { return this.isBusy; }
    }

    public virtual bool TryEnqueue(WorkerOperation request)
    {
        lock (this.queueOfOperations)
        {
            if (this.queueOfOperations.Count < this.queueMaxSize)
            {
                this.queueOfOperations.AddLast(request);
                this.actionEnqueued.Set();

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public void Start()
    {
        this.thread.Start();
    }

    public void Stop()
    {
        this.threadStopped.Set();
    }

    #endregion

    #region Constructors

    public WorkerThread(string threadName, int queueMaxSize = WorkerThread.DefaultMaxQueueSize)
    {
        if (queueMaxSize <= 0)
        {
            throw new ArgumentException("\"queueMaxSize\" is supposed to be a positive value.");
        }

        this.queueMaxSize = queueMaxSize;

        this.thread = new Thread(Run);
        this.thread.Name = threadName;
    }

    public event EventHandler<GenericEventArgs<WorkerOperation>> OperationCompleted;

    protected virtual void OnOperationCompleted(WorkerOperation workerOperation)
    {
        EventHandler<GenericEventArgs<WorkerOperation>> handler = this.OperationCompleted;
        if (handler != null)
        {
            handler(this, new GenericEventArgs<WorkerOperation>(workerOperation));
        }
    }

    #endregion

    #region Protected and private methods

    private WorkerOperation TakeNextRequestFromQueue()
    {
        WorkerOperation request = null;
        lock (this.queueOfOperations)
        {
            // If the queue is not empty - try to get its first item and remove it from the queue
            if (this.queueOfOperations.Count > 0)
            {
                request = this.queueOfOperations.First.Value;
                this.queueOfOperations.RemoveFirst();
            }

            // If the queue is empty after removing first item - we need to reset methodEnqueued_
            if (this.queueOfOperations.Count == 0)
            {
                this.actionEnqueued.Reset();
            }
        }

        return request;
    }

    /// <summary>
    /// Main method of the thread. 
    /// </summary>
    private void Run()
    {
        try
        {
            this.isActive = true;

            WaitHandle[] eventHandles = new WaitHandle[] { this.threadStopped, this.actionEnqueued };
            while (isActive)
            {
                int index = WaitHandle.WaitAny(eventHandles);
                switch (index)
                {
                    case 0:
                        {
                            // Thread was stopped, change IsActive to false and terminate it
                            this.isActive = false;
                            break;
                        }
                    case 1:
                        {
                            // There is a new method to be executed - taking it from the queue
                            WorkerOperation nextRequest = this.TakeNextRequestFromQueue();

                            // If method was successfully got from the queue - invoking it
                            if (nextRequest != null)
                            {
                                this.isBusy = true;

                                this.RunOperation(nextRequest);
                                this.OnOperationCompleted(nextRequest);

                                this.isBusy = false;
                            }

                            break;
                        }
                }
            }
        }
        catch (Exception ex)
        {
        }
        finally
        {
            this.isBusy = false;
        }
    }

    protected virtual void RunOperation(WorkerOperation operation)
    {
        try
        {
            operation.Run();
        }
        catch (Exception ex)
        {
            // TODO: Pass this information up by an event. 
            // ACLogger.Instance.Error($"Error occured on running WorkerOperation of key: {operation.OperationKey}.", ex);
        }
    }

    private TimeSpan minTimeSpanToWait = TimeSpan.FromSeconds(5);

    #endregion

    #region Private fields

    // Max queue size. 
    protected int queueMaxSize = 1;

    // Thread object. 
    protected Thread thread = null;

    // Is this thread active and busy. 
    protected bool isActive = false;
    protected bool isBusy = false;

    // Some thread synchronization objects. 
    protected ManualResetEvent actionEnqueued = new ManualResetEvent(false);
    protected ManualResetEvent threadStopped = new ManualResetEvent(false);

    // Queue of methods to be invoked. 
    protected LinkedList<WorkerOperation> queueOfOperations = new LinkedList<WorkerOperation>();

    #endregion
}