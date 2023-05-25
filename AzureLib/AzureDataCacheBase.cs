using System;
using System.Collections.Generic;

namespace AzureLib;

public abstract class AzureDataCacheBase : IAzureDataCache
{
    protected Dictionary<string, List<AzureEntity>> _securityGroupWithMembers = new Dictionary<string, List<AzureEntity>>(StringComparer.OrdinalIgnoreCase);

    protected object _locker = new object();

    protected AzureADClient _azureADClient;

    #region IAzureDataCache members

    public virtual bool IsSIDMemberOfGroupSID(string checkMemberSID, string checkMemberUPN, string groupSID)
    {
        lock (this._locker)
        {
            Queue<string> queueOfMembers = new Queue<string>();

            queueOfMembers.Enqueue(groupSID);

            string nextCandidateSID = null;
            while ((queueOfMembers.TryDequeue(out nextCandidateSID)) && (!string.IsNullOrWhiteSpace(nextCandidateSID)))
            {
                if (nextCandidateSID.Equals(checkMemberSID, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                List<AzureEntity> nextCandidateMembers = null;
                if (this._securityGroupWithMembers.TryGetValue(nextCandidateSID, out nextCandidateMembers))
                {
                    foreach (AzureEntity member in nextCandidateMembers)
                    {
                        if (member is AzureEntityGroup)
                        {
                            queueOfMembers.Enqueue(member.SID);
                        }
                        else if (member is AzureEntityUser)
                        {
                            AzureEntityUser memberUser = member as AzureEntityUser;
                            if ((string.Equals(memberUser.SID, checkMemberSID, StringComparison.OrdinalIgnoreCase)) ||
                                (string.Equals(memberUser.UserPrincipalName, checkMemberUPN, StringComparison.OrdinalIgnoreCase)))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }

    public Dictionary<string, List<AzureEntity>> GetAllSecurityGroupsWithMembers()
    {
        lock (this._locker)
        {
            return new Dictionary<string, List<AzureEntity>>(this._securityGroupWithMembers);
        }
    }

    public abstract void RecacheAllGroups();

    #endregion

    public AzureDataCacheBase()
    {
    }
}
