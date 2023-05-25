using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureLib.Base.Threading;

public class WorkerThreadPool
{
    private string poolName;

    private int count = 0;

    private List<WorkerThread> threads;

    private int lastPushedToWorkerIndex = -1;

    private HashSet<string> enqueuedOperationsKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private object locker = new object();

    public WorkerThreadPool(string poolName, int count)
    {
        this.poolName = poolName;
        this.count = count;

        this.threads = new List<WorkerThread>();
        for (int i = 0; i < this.count; i++)
        {
            WorkerThread workerThread = new WorkerThread($"WorkerThreadPool{this.poolName} {i + 1}", 10000000);
            workerThread.OperationCompleted += WorkerThread_OperationCompleted;
            workerThread.Start();

            this.threads.Add(workerThread);
        }
    }

    private void WorkerThread_OperationCompleted(object sender, GenericEventArgs<WorkerOperation> operation)
    {
        if (operation == null)
        {
            return;
        }

        lock (this.locker)
        {
            this.enqueuedOperationsKeys.Remove(operation.Data.OperationKey);
        }
    }

    public bool CheckIsAllProcessingCompleted()
    {
        lock (this.locker)
        {
            WorkerThread thisThread = this.threads.FirstOrDefault();
            return ((thisThread.CurrentQueueSize == 0) && (!thisThread.IsBusy));
        }
    }

    public void Schedule(WorkerOperation operation)
    {
        lock (this.locker)
        {
            if (this.enqueuedOperationsKeys.Contains(operation.OperationKey))
            {
                return;
            }

            int currentIndex = this.lastPushedToWorkerIndex + 1;
            while (true)
            {
                if (currentIndex >= this.count)
                {
                    currentIndex = 0;
                }

                if (this.threads[currentIndex].TryEnqueue(operation))
                {
                    this.enqueuedOperationsKeys.Add(operation.OperationKey);
                    break;
                }

                currentIndex++;
            }

            this.lastPushedToWorkerIndex = currentIndex;
        }
    }
}