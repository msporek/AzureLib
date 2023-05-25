namespace AzureLib;

public static class GroupExtensions
{
    public static string GetGroupSIDOrObjectID(this Microsoft.Graph.Group group)
    {
        if (group == null)
        {
            return null;
        }

        string groupSID = group.OnPremisesSecurityIdentifier;

        // Testing using a Group that is only in Azure AD, not replicated from AD to Azure AD use the Azure AD Object Id
        if (string.IsNullOrWhiteSpace(groupSID))
        {
            return group.Id;
        }
        else
        {
            return groupSID;
        }
    }
}
