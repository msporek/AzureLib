using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureLib;

public class AzureDataCache : AzureDataCacheBase
{
    #region IAzureDataCache members

    public override void RecacheAllGroups()
    {
        try
        {
            Dictionary<string, List<AzureEntity>> groupSIDToMemberSIDs = new Dictionary<string, List<AzureEntity>>(StringComparer.OrdinalIgnoreCase);
            Task<List<Microsoft.Graph.Group>> getAllGroupsTask = this._azureADClient.GetAllGroupsAsync();
            getAllGroupsTask.Wait();

            if (getAllGroupsTask.Result != null)
            {
                foreach (Microsoft.Graph.Group group in getAllGroupsTask.Result)
                {
                    if (!string.IsNullOrWhiteSpace(group.OnPremisesSecurityIdentifier))
                    {
                        groupSIDToMemberSIDs[group.OnPremisesSecurityIdentifier] = new List<AzureEntity>();

                        Task<List<Microsoft.Graph.DirectoryObject>> getAllDirectMembershipsTask = this._azureADClient.GetGroupMembersAsync(group);
                        getAllDirectMembershipsTask.Wait();
                        if (getAllDirectMembershipsTask.Result != null)
                        {
                            foreach (Microsoft.Graph.DirectoryObject member in getAllDirectMembershipsTask.Result)
                            {
                                if (member is Microsoft.Graph.User)
                                {
                                    string memberUserUPN = ((Microsoft.Graph.User)member).UserPrincipalName;
                                    string memberUserSID = ((Microsoft.Graph.User)member).OnPremisesSecurityIdentifier;

                                    if ((!string.IsNullOrWhiteSpace(memberUserUPN)) || (!string.IsNullOrWhiteSpace(memberUserSID)))
                                    {
                                        groupSIDToMemberSIDs[group.OnPremisesSecurityIdentifier].Add(new AzureEntityUser(memberUserUPN, memberUserSID));
                                    }
                                }
                                else if (member is Microsoft.Graph.Group)
                                {
                                    string memberGroupSID = ((Microsoft.Graph.Group)member).OnPremisesSecurityIdentifier;
                                    if (!string.IsNullOrWhiteSpace(memberGroupSID))
                                    {
                                        groupSIDToMemberSIDs[group.OnPremisesSecurityIdentifier].Add(new AzureEntityGroup(memberGroupSID));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            lock (this._locker)
            {
                this._securityGroupWithMembers = groupSIDToMemberSIDs;
            }
        }
        catch (Exception ex)
        {
            // Exception handling can be done here. 

            lock (this._locker)
            {
                this._securityGroupWithMembers = new Dictionary<string, List<AzureEntity>>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    #endregion

    public AzureDataCache(AzureADClient azureADClient)
        : base()
    {
        this._azureADClient = azureADClient;
    }
}
