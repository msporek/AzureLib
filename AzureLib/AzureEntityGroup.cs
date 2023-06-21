namespace AzureLib;

/// <summary>
/// Model class that represents Azure Groups of Users. 
/// </summary>
public class AzureEntityGroup : AzureEntity
{
    public AzureEntityGroup()
        : base()
    {
    }

    public AzureEntityGroup(string groupSID)
        : base(groupSID)
    {
    }
}
