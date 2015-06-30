using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace ActiveDirectoryUtilities
{
    public class ActiveDirectory: ActiveDirectoryOrganizationalUnit
    {
        // Authenticates a username/password combination against the specified domain
        public static bool AuthenticateUser(string domainName, string operatingUsername, string operatingUserPassword, string userName, string password)
        {
            using (var domain = new PrincipalContext(ContextType.Domain, domainName, operatingUsername, operatingUserPassword))
            {
                return domain.ValidateCredentials(userName, password);
            }
        }

        // Finds a user by their username. If no user is found, it returns null
        public static UserPrincipal GetUserByUserName(string domainName, string operatingUsername, string operatingUserPassword, string userName)
        {
            try
            {
                var pc = new PrincipalContext(ContextType.Domain, domainName, operatingUsername, operatingUserPassword);
                var u = UserPrincipal.FindByIdentity(pc, userName);
                return u;
            }
            catch
            {
                return null;
            }
        }

        // Finds a domain group by name. If no group is found, it returns null. The `groupName` may contain wildcards.
        public static GroupPrincipal GetGroupByName(string domainName, string operatingUsername, string operatingUserPassword, string groupName)
        {
            var groups = GetGroupsByName(domainName, operatingUsername, operatingUserPassword, groupName);
            if (!groups.Any()) return null;
            var group = groups.First();
            return group;
        }

        // Finds all domain groups by name. The `filter` may contain wildcards.
        private static List<GroupPrincipal> GetGroupsByName(string domainName, string operatingUsername, string operatingUserPassword, string filter = "*")
        {
            var pc = new PrincipalContext(ContextType.Domain, domainName, operatingUsername, operatingUserPassword);
            var group = new GroupPrincipal(pc) { Name = filter };
            var searcher = new PrincipalSearcher { QueryFilter = @group };
            var results = searcher.FindAll();
            var rtn = results.Cast<GroupPrincipal>().ToList();
            rtn = rtn.OrderBy(gp => gp.SamAccountName).ToList();
            return rtn;
        }

        // Finds all organizational units and optionally their descendants.
        // `path` is the ActiveDirectory path of the organization unit. If none is provided, the whole ActiveDirectory OU will be returned
        // `retrieveDescendants` specifies whether or not the function should retrieve the descendants of the unit
        // `depth` specifies how many levels of descendants the function should retrieve
        public static ActiveDirectoryOrganizationalUnit GetOrganizationalUnit(string domainName, string userName, string password, 
            string path = null, bool retrieveDescendants = false, int? depth = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = null;
            }
            var rtn = new ActiveDirectoryOrganizationalUnit();
            var defaultPath = "LDAP://" + domainName;
            var ldap = new DirectoryEntry(path ?? defaultPath, userName, password);
            foreach (DirectoryEntry result in ldap.Children)
            {
                switch (result.SchemaClassName.ToLower())
                {
                    case "user":
                        if (result.NativeGuid == null) continue;
                        var flags = (int)result.Properties["userAccountControl"].Value;
                        if (Convert.ToBoolean(flags & 0x0002)) continue;
                        rtn.Users.Add(result.Name.Split('=')[1].Trim());
                        break;
                    case "organizationalunit":
                        if (retrieveDescendants && (depth == null || (int)depth >= 0))
                        {
                            rtn.OrganizationalUnits.Add(GetOrganizationalUnit(domainName, userName, password, result.Path, true, depth - 1));
                        }
                        break;
                }
            }
            rtn.Name = ldap.Name.Split('=')[1].Trim();
            rtn.Path = ldap.Path;
            ldap.Dispose();
            return rtn;
        }

        // Only gets the names of the domain groups that match the filter passed
        public static List<string> GetGroupNames(string domainName, string operatingUsername, string operatingUserPassword, string filter = "*")
        {
            var results = GetGroupsByName(domainName, operatingUsername, operatingUserPassword, filter);
            return results.Select(principal => principal.SamAccountName).ToList();
        }

        // Name of the ActiveDirectory domain
        public string DomainName { get; set; }

        // Username of the user that will be used to perform the operations against ActiveDirectory
        public string OperatingUsername { get; set; }
        public string OperatingUserPassword { get; set; }

        // In order to instantiate this class, the user must pass these required values
        public ActiveDirectory(string domainName, string operatingUsername, string operatingUserPassword)
        {
            DomainName = domainName;
            OperatingUsername = operatingUsername;
            OperatingUserPassword = operatingUserPassword;
        }

        // Retrieves the Active Directory base organizational unit and optionally retrieves its descendants
        public void Populate(bool retrieveDescendants = false, int? depth = null)
        {
            var organizationalUnit = GetOrganizationalUnit(null, retrieveDescendants, depth);
            Name = organizationalUnit.Name;
            OrganizationalUnits = organizationalUnit.OrganizationalUnits;
            Path = organizationalUnit.Path;
            Users = organizationalUnit.Users;
        }

        // Instance level methods that just call the static methods above
        public bool AuthenticateUser(string userName, string password)
        {
            return AuthenticateUser(DomainName, OperatingUsername,
                OperatingUserPassword, userName, password);
        }

        public UserPrincipal GetUserByUserName(string userName)
        {
            return GetUserByUserName(DomainName, OperatingUsername,
                OperatingUserPassword, userName);
        }

        public GroupPrincipal GetGroupByName(string groupName)
        {
            return GetGroupByName(DomainName, OperatingUsername,
                OperatingUserPassword, groupName);
        }

        public ActiveDirectoryOrganizationalUnit GetOrganizationalUnit(string path = null, bool retrieveDescendants = false,
            int? depth = null)
        {
            return GetOrganizationalUnit(DomainName, OperatingUsername,
                OperatingUserPassword, path, retrieveDescendants, depth);
        }

        public List<GroupPrincipal> GetGroupsByName(string filter = "*")
        {
            return GetGroupsByName(DomainName, OperatingUsername,
                OperatingUserPassword, filter);
        }

        public List<string> GetGroupNames(string filter = "*")
        {
            return GetGroupNames(DomainName, OperatingUsername,
                OperatingUserPassword, filter);
        }
    }
}