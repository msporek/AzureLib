namespace AzureLib;

public class AzureEntity
{
    public string SID { get; set; }

    public AzureEntity()
    {
    }

    public AzureEntity(string sid)
    {
        this.SID = sid;
    }
}
