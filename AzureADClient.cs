using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureLib;

/// <summary>
/// Class comes with the functionalities for retrieving information from Azure Active Directory about Users, Groups, User membership in 
/// Groups, Domains. It comes with logic meant for finding Azure AD objects by a variety of criteria. 
/// </summary>
public class AzureADClient : IAzureADClient
{
    private GraphServiceClient _graphServiceClient;

    public AzureADClient(GraphServiceClient graphServiceClient)
    {
        ArgumentNullException.ThrowIfNull(graphServiceClient, nameof(graphServiceClient));

        this._graphServiceClient = graphServiceClient;
    }

    public async Task<List<Domain>> GetAllDomainsAsync()
    {
        List<Domain> allDomains = new List<Domain>();

        IGraphServiceDomainsCollectionRequest domainsCollectionRequest = this._graphServiceClient.Domains.Request();
        do
        {
            IGraphServiceDomainsCollectionPage domainsPage = await domainsCollectionRequest.GetAsync();
            List<Domain> newDomains = domainsPage.ToList();
            if (!newDomains.Any())
            {
                break;
            }

            allDomains.AddRange(newDomains);

            domainsCollectionRequest = domainsPage.NextPageRequest;
        } while (domainsCollectionRequest != null);

        return allDomains;
    }

    public async Task<List<Domain>> GetAllVerifiedDomainsAsync()
    {
        List<Domain> allDomains = await this.GetAllDomainsAsync();
        return allDomains.Where(d => (d.IsVerified.HasValue) && (d.IsVerified.Value)).ToList();
    }

    public async Task<User> FindUserAsync(Func<User, bool> userPredicate)
    {
        ArgumentNullException.ThrowIfNull(userPredicate, nameof(userPredicate));

        IGraphServiceUsersCollectionRequest usersRequest = this._graphServiceClient.Users.Request();
        do
        {
            IGraphServiceUsersCollectionPage usersPage = await usersRequest.GetAsync();
            List<User> newUsers = usersPage.ToList();
            if (!newUsers.Any())
            {
                break;
            }

            User userFound = newUsers.FirstOrDefault(g => userPredicate(g));
            if (userFound != null)
            {
                return userFound;
            }

            usersRequest = usersPage.NextPageRequest;
        } while (usersRequest != null);

        return null;
    }

    public async Task<List<User>> GetAllUserMembersOfGroupAsync(Group group)
    {
        ArgumentNullException.ThrowIfNull(group, nameof(group));

        List<User> allUserMembers = new List<User>();
        List<DirectoryObject> allMembers = await this.GetGroupMembersAsync(group);
        foreach (DirectoryObject member in allMembers)
        {
            if (member is User)
            {
                allUserMembers.Add((User)member);
            }
            else if (member is Group)
            {
                allUserMembers.AddRange(await this.GetAllUserMembersOfGroupAsync((Group)member));
            }
        }

        return allUserMembers;
    }

    public async Task<List<DirectoryObject>> GetGroupMembersAsync(Group group)
    {
        ArgumentNullException.ThrowIfNull(group, nameof(group));

        List<DirectoryObject> allMembers = new List<DirectoryObject>();

        IGroupMembersCollectionWithReferencesRequest groupMembersRequest = this._graphServiceClient.Groups[group.Id].Members.Request();
        do
        {
            IGroupMembersCollectionWithReferencesPage groupMembersPage = await groupMembersRequest.GetAsync();
            List<DirectoryObject> groupMembersResult = groupMembersPage.ToList();
            if (groupMembersResult.Any())
            {
                groupMembersResult.ForEach(m => allMembers.Add(m));
            }
            else
            {
                break;
            }

            groupMembersRequest = groupMembersPage.NextPageRequest;
        } while (groupMembersRequest != null);

        return allMembers;
    }

    public async Task<List<DirectoryObject>> GetGroupDirectMembershipInGroupsAsync(Group group)
    {
        ArgumentNullException.ThrowIfNull(group, nameof(group));

        List<DirectoryObject> allDirectoryEntries = new List<DirectoryObject>();

        IGroupMemberOfCollectionWithReferencesRequest groupMemberOfRequest = this._graphServiceClient.Groups[group.Id].MemberOf.Request();
        do
        {
            IGroupMemberOfCollectionWithReferencesPage groupMemberOfResultPage = await groupMemberOfRequest.GetAsync();
            List<DirectoryObject> groupNewMemberOfResults = groupMemberOfResultPage.ToList();
            if (groupNewMemberOfResults.Any())
            {
                allDirectoryEntries.AddRange(groupNewMemberOfResults);
            }
            else
            {
                break;
            }

            groupMemberOfRequest = groupMemberOfResultPage.NextPageRequest;
        } while (groupMemberOfRequest != null);

        return allDirectoryEntries;
    }

    public async Task<List<Group>> GetUserAllMembershipInGroupsAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        Queue<Group> groupsToCheckUpQueue = new Queue<Group>();
        List<DirectoryObject> allDirectMembershipInGroups = await this.GetUserDirectMembershipInGroupsAsync(user);
        allDirectMembershipInGroups.OfType<Group>().ToList().ForEach(dir => groupsToCheckUpQueue.Enqueue(dir));

        List<Group> allMemberships = new List<Group>();

        Group nextGroupToCheck = null;
        while ((groupsToCheckUpQueue.TryDequeue(out nextGroupToCheck)) && (nextGroupToCheck != null))
        {
            allMemberships.Add(nextGroupToCheck);

            List<DirectoryObject> nextGroupMemberships = await this.GetGroupDirectMembershipInGroupsAsync(nextGroupToCheck);
            if (nextGroupMemberships.Any())
            {
                nextGroupMemberships.OfType<Group>().ToList().ForEach(m => groupsToCheckUpQueue.Enqueue(m));
            }
        }

        return allMemberships;
    }

    public async Task<List<DirectoryObject>> GetUserDirectMembershipInGroupsAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        List<DirectoryObject> allDirectoryEntries = new List<DirectoryObject>();

        try
        {
            IUserMemberOfCollectionWithReferencesRequest userMemberOfRequest = this._graphServiceClient.Users[user.Id].MemberOf.Request();
            do
            {
                IUserMemberOfCollectionWithReferencesPage userMemberOfResultPage = await userMemberOfRequest.GetAsync();
                List<DirectoryObject> userNewMemberOfResults = userMemberOfResultPage.ToList();
                if (userNewMemberOfResults.Any())
                {
                    allDirectoryEntries.AddRange(userNewMemberOfResults);
                }
                else
                {
                    break;
                }

                userMemberOfRequest = userMemberOfResultPage.NextPageRequest;
            } while (userMemberOfRequest != null);

            return allDirectoryEntries;
        }
        catch (Exception ex)
        {
            return new List<DirectoryObject>();
        }
    }

    public async Task<List<Invitation>> GetAllInvitationsAsync()
    {
        List<Invitation> allInvitations = new List<Invitation>();

        IGraphServiceInvitationsCollectionRequest invitationsRequest = this._graphServiceClient.Invitations.Request();
        do
        {
            GraphResponse<GraphServiceInvitationsCollectionResponse> result =
                await this._graphServiceClient.Invitations.Request().GetResponseAsync();

            IGraphServiceInvitationsCollectionPage invitationsPage = await invitationsRequest.GetAsync();
            List<Invitation> newInvitations = invitationsPage.ToList();
            if (newInvitations.Any())
            {
                allInvitations.AddRange(newInvitations);
            }
            else
            {
                break;
            }

            invitationsRequest = invitationsPage.NextPageRequest;
        } while (invitationsRequest != null);

        return allInvitations;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        List<User> allUsers = new List<User>();

        IGraphServiceUsersCollectionRequest usersRequest =
            this._graphServiceClient.Users
                .Request()
                .Select(
                    u => new
                    {
                        u.Id,
                        u.Mail, 
                        u.DisplayName,
                        u.GivenName,
                        u.UserType,
                        u.UserPrincipalName
                    });
        
        do
        {
            IGraphServiceUsersCollectionPage usersPage = await usersRequest.GetAsync();
            List<User> newUsers = usersPage.ToList();
            if (newUsers.Any())
            {
                allUsers.AddRange(newUsers);
            }
            else
            {
                break;
            }

            usersRequest = usersPage.NextPageRequest;
        } while (usersRequest != null);

        return allUsers;
    }

    public async Task<List<Group>> GetAllGroupsAsync()
    {
        List<Group> allGroups = new List<Group>();

        IGraphServiceGroupsCollectionRequest groupsRequest = this._graphServiceClient.Groups.Request();
        do
        {
            IGraphServiceGroupsCollectionPage groupsPage = await groupsRequest.GetAsync();
            List<Group> newGroups = groupsPage.ToList();
            if (newGroups.Any())
            {
                allGroups.AddRange(newGroups);
            }
            else
            {
                break;
            }

            groupsRequest = groupsPage.NextPageRequest;
        } while (groupsRequest != null);

        return allGroups;
    }

    public async Task<Group> FindGroupAsync(Func<Group, bool> groupPredicate)
    {
        ArgumentNullException.ThrowIfNull(groupPredicate, nameof(groupPredicate));

        IGraphServiceGroupsCollectionRequest groupsRequest = this._graphServiceClient.Groups.Request();
        do
        {
            IGraphServiceGroupsCollectionPage groupsPage = await groupsRequest.GetAsync();
            List<Group> newGroups = groupsPage.ToList();
            if (!newGroups.Any())
            {
                break;
            }

            Group groupFound = newGroups.FirstOrDefault(g => groupPredicate(g));
            if (groupFound != null)
            {
                return groupFound;
            }

            groupsRequest = groupsPage.NextPageRequest;
        } while (groupsRequest != null);

        return null;
    }

    public async Task<Group> GetGroupByDisplayNameAsync(string groupDisplayName)
    {
        return await this.FindGroupAsync(g => groupDisplayName.Equals(g.DisplayName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User> GetUserAsync(string userUPN, string userSID)
    {
        return await this.GetUserAsync(userUPN, userSID, false);
    }

    public async Task<User> GetUserByMailNicknameAsync(string userMailNickname)
    {
        return await this.FindUserAsync(
            u => string.Equals(userMailNickname, u.MailNickname, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User> GetUserBySIDAsync(string userSID)
    {
        return await this.FindUserAsync(
            u => string.Equals(userSID, u.OnPremisesSecurityIdentifier, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<User> GetUserAsync(string userUPN, string userSID, bool suppressUserNotFound)
    {
        return await this.FindUserAsync(
            u => 
                (string.Equals(userUPN, u.UserPrincipalName, StringComparison.OrdinalIgnoreCase)) || 
                ((string.Equals(userSID, u.OnPremisesSecurityIdentifier, StringComparison.OrdinalIgnoreCase))));
    }

    public async Task<List<DirectoryObject>> GetUserDirectMembershipGroupsAsync(string userUPN, string userSID)
    {
        User user = await this.GetUserAsync(userUPN, userSID);
        if (user == null)
        {
            return new List<DirectoryObject>();
        }

        return await this.GetUserDirectMembershipInGroupsAsync(user);
    }
}
