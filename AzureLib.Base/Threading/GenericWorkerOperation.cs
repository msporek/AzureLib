using System;

namespace AzureLib.Base.Threading;

public class GenericWorkerOperation : WorkerOperation
{
    protected Action actionToRun;

    public GenericWorkerOperation(Action actionToRun)
        : base()
    {
        this.OperationKey = Guid.NewGuid().ToString("D");
        this.actionToRun = actionToRun;
    }

    public override void Run()
    {
        if (this.actionToRun != null)
        {
            this.actionToRun();
        }
    }

    public override string OperationKey { get; }
}
