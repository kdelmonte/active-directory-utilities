using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace ActiveDirectoryUtilities
{
    public class ActiveDirectory: ActiveDirectoryOrganizationalUnit
    {

        // Name of the ActiveDirectory domain
        public string DomainName { get; set; }

        // Returns base path of Active directory
        public string BasePath
        {
            get
            {
                return GetBasePath(DomainName);
            }
        }

        // In order to instantiate this class, the user must pass these required values
        public ActiveDirectory(string domainName, string operatingUsername, string operatingUserPassword)
            : base(operatingUsername, operatingUserPassword, GetBasePath(domainName))
        {
            DomainName = domainName;
        }

        // Instance level methods that just call the static methods
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
        public static ActiveDirectoryOrganizationalUnit GetOrganizationalUnit(string domainName, string operatingUsername, string operatingUserPassword,
            string path = null, bool retrieveDescendants = false, int? depth = null)
        {

            var activeDirectory = new ActiveDirectory(domainName, operatingUsername, operatingUserPassword);
            if (string.IsNullOrWhiteSpace(path))
            {
                path = null;
            }
            else
            {
                path = path.Trim('/');
                if (!path.ToLower().Contains(activeDirectory.BasePath.ToLower()))
                {
                    path = string.Format("{0}/{1}", activeDirectory.BasePath, path);
                }
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                return activeDirectory;
            }
            return new ActiveDirectoryOrganizationalUnit(operatingUsername, operatingUserPassword,
                path, retrieveDescendants, depth);

        }

        // Only gets the names of the domain groups that match the filter passed
        public static List<string> GetGroupNames(string domainName, string operatingUsername, string operatingUserPassword, string filter = "*")
        {
            var results = GetGroupsByName(domainName, operatingUsername, operatingUserPassword, filter);
            return results.Select(principal => principal.SamAccountName).ToList();
        }

        // Build the Active Directory base URL
        private static string GetBasePath(string domainName)
        {
            return string.Format("LDAP://{0}", domainName);
        }
    }
}