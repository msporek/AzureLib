using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureLib;

public interface IAzureADClient
{
    Task<List<Domain>> GetAllDomainsAsync();

    Task<List<Domain>> GetAllVerifiedDomainsAsync();

    Task<User> FindUserAsync(Func<User, bool> userPredicate);

    Task<List<DirectoryObject>> GetGroupMembersAsync(Group group);

    Task<List<DirectoryObject>> GetGroupDirectMembershipInGroupsAsync(Group group);

    Task<List<Group>> GetUserAllMembershipInGroupsAsync(User user);

    Task<List<DirectoryObject>> GetUserDirectMembershipInGroupsAsync(User user);

    Task<List<Group>> GetAllGroupsAsync();

    Task<List<User>> GetAllUsersAsync();

    Task<Group> FindGroupAsync(Func<Group, bool> groupPredicate);

    Task<Group> GetGroupByDisplayNameAsync(string groupDisplayName);

    Task<User> GetUserAsync(string userUPN, string userSID);

    Task<User> GetUserBySIDAsync(string userSID);

    Task<User> GetUserAsync(string userUPN, string userSID, bool suppressUserNotFound);

    Task<List<DirectoryObject>> GetUserDirectMembershipGroupsAsync(string userUPN, string userSID);
}
