using System.Collections.Generic;

namespace ActiveDirectoryUtilities
{
    // This class represents an Organizational Unit (OU) of ActiveDirectory
    public class ActiveDirectoryOrganizationalUnit
    {
        // Name of the unit
        public string Name;
        // Any child OUs
        public List<ActiveDirectoryOrganizationalUnit> OrganizationalUnits = new List<ActiveDirectoryOrganizationalUnit>();
        // The AD path of the unit
        public string Path;
        // If there are any users of the unit, they would be listed here
        public List<string> Users = new List<string>();
    }
}