using System.Collections.Generic;

namespace AzureLib;

public interface IAzureDataCache
{
    bool IsSIDMemberOfGroupSID(string checkMemberSID, string checkMemberUPN, string groupSID);

    Dictionary<string, List<AzureEntity>> GetAllSecurityGroupsWithMembers();

    void RecacheAllGroups();
}
