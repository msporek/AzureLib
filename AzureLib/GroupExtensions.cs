namespace AzureLib;

/// <summary>
/// Class delivers extension methods for the <see cref="Microsoft.Graph.Group"/> class. 
/// </summary>
public static class GroupExtensions
{
    /// <summary>
    /// Method attempts to retrieve the <see cref="Microsoft.Graph.Group.OnPremisesSecurityIdentifier"/> property of the group, and 
    /// if it is not found, then the <see cref="Microsoft.Graph.Group.Id"/> property is returned for the <paramref name="group"/> argument. 
    /// </summary>
    /// 
    /// <param name="group">Group to get the property from.</param>
    /// 
    /// <returns>On-Premises SID or Id of the provided <paramref name="group"/> argument. If the <paramref name="group"/> is null, then 
    /// null is returned.</returns>
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
