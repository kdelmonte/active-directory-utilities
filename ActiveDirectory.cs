using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace ActiveDirectoryUtilities
{
    public class ActiveDirectory
    {
        // Authenticates a username/password combination against the specified domain
        public static bool AuthenticateAgainstDomain(string domainName, string domainAccountUserName, string domainAccountPassword, string userName, string password)
        {
            using (var domain = new PrincipalContext(ContextType.Domain, domainName, domainAccountUserName, domainAccountPassword))
            {
                return domain.ValidateCredentials(userName, password);
            }
        }

        // Finds a user by their username. If no user is found, it returns null
        public static UserPrincipal GetDomainUserByUserName(string domainName, string domainAccountUserName, string domainAccountPassword, string userName)
        {
            try
            {
                var pc = new PrincipalContext(ContextType.Domain, domainName, domainAccountUserName, domainAccountPassword);
                var u = UserPrincipal.FindByIdentity(pc, userName);
                return u;
            }
            catch
            {
                return null;
            }
        }

        // Finds a domain group by name. If no group is found, it returns null. The `groupName` may contain wildcards.
        public static GroupPrincipal GetDomainGroupByName(string domainName, string domainAccountUserName, string domainAccountPassword, string groupName)
        {
            var groups = GetDomainGroupsByName(domainName, domainAccountUserName, domainAccountPassword, groupName);
            if (!groups.Any()) return null;
            var group = groups.First();
            return group;
        }

        // Finds all domain groups by name. The `filter` may contain wildcards.
        private static List<GroupPrincipal> GetDomainGroupsByName(string domainName, string domainAccountUserName, string domainAccountPassword, string filter = "*")
        {
            var pc = new PrincipalContext(ContextType.Domain, domainName, domainAccountUserName, domainAccountPassword);
            var group = new GroupPrincipal(pc) { Name = filter };
            var searcher = new PrincipalSearcher { QueryFilter = @group };
            var results = searcher.FindAll();
            var rtn = results.Cast<GroupPrincipal>().ToList();
            rtn = rtn.OrderBy(gp => gp.SamAccountName).ToList();
            return rtn;
        }

        // Finds all domain groups by name by name. The `filter` may contain wildcards.
        // `recursive` specifies whether or not the function should 
        // `depth` specifies how many levels of descendants the method should retrieve
        // `path` is the ActiveDirectory path of the organization unit. If none is provided, the whole ActiveDirectory will be returned
        public static ActiveDirectoryOrganizationalUnit GetDomainOrganizationalUnit(string domainName, string userName, string password, 
            string path = null, bool recursive = false, int? depth = null)
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
                        if (recursive && (depth == null || (int)depth >= 0))
                        {
                            rtn.OrganizationalUnits.Add(GetDomainOrganizationalUnit(domainName, userName, password, result.Path, true, depth - 1));
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
        public static List<string> GetDomainGroupNames(string domainName, string domainAccountUserName, string domainAccountPassword, string filter = "*")
        {
            var results = GetDomainGroupsByName(domainName, domainAccountUserName, domainAccountPassword, filter);
            return results.Select(principal => principal.SamAccountName).ToList();
        }

        // Name of the ActiveDirectory domain
        public string DomainName { get; set; }

        // Username of the user that will be used to perform the operations against ActiveDirectory
        public string UserName { get; set; }
        public string Password { get; set; }

        // In order to instantiate this class, the user must pass these required values
        public ActiveDirectory(string domainName, string username, string password)
        {
            DomainName = domainName;
            Password = username;
            Password = password;
        }

        // Instance level methods that just call the static methods above
        public bool AuthenticateAgainstDomain(string userName, string password)
        {
            return AuthenticateAgainstDomain(DomainName, UserName,
                Password, userName, password);
        }

        public UserPrincipal GetDomainUserByUserName(string userName)
        {
            return GetDomainUserByUserName(DomainName, UserName,
                Password, userName);
        }

        public GroupPrincipal GetDomainGroupByName(string groupName)
        {
            return GetDomainGroupByName(DomainName, UserName,
                Password, groupName);
        }

        public ActiveDirectoryOrganizationalUnit GetDomainOrganizationalUnit(string path = null, bool recursive = false,
            int? depth = null)
        {
            return GetDomainOrganizationalUnit(DomainName, UserName,
                Password, path, recursive, depth);
        }

        public List<GroupPrincipal> GetDomainGroups(string filter = "*")
        {
            return GetDomainGroupsByName(DomainName, UserName,
                Password, filter);
        }

        public List<string> GetDomainGroupNames(string filter = "*")
        {
            return GetDomainGroupNames(DomainName, UserName,
                Password, filter);
        }

        
    }
}