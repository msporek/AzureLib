namespace AzureLib;

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
