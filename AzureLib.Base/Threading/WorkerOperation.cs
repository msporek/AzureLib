namespace AzureLib.Base.Threading;

public abstract class WorkerOperation
{
    public WorkerOperation()
    {
    }

    public abstract void Run();

    public abstract string OperationKey { get; }
}
