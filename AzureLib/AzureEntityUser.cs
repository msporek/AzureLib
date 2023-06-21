namespace AzureLib;

/// <summary>
/// Model class that represents Azure Users. 
/// </summary>
public class AzureEntityUser : AzureEntity
{
    public string UserPrincipalName { get; set; }

    public AzureEntityUser()
        : base()
    {
    }

    public AzureEntityUser(string userPrincipalName, string userSID)
        : base(userSID)
    {
        this.UserPrincipalName = userPrincipalName;
    }
}
