namespace AzureLib;

/// <summary>
/// Base class to represent Azure Entities that contain a Security Identifier 
/// represented by the <see cref="AzureEntity.SID"/> property. 
/// </summary>
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
