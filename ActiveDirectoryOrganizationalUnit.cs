using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Dynamic;

namespace ActiveDirectoryUtilities
{
    // This class represents an Organizational Unit (OU) of ActiveDirectory
    public class ActiveDirectoryOrganizationalUnit: DirectoryEntry
    {
        // Username and password of the user that will be used to perform the operations against ActiveDirectory
        public string OperatingUsername { get; set; }
        public string OperatingUserPassword { get; set; }

        // Any child OUs
        public List<ActiveDirectoryOrganizationalUnit> OrganizationalUnits { get; set; }
        // If there are any users of the unit, they would be listed here
        public List<DirectoryEntry> Users {get; set; }

        protected ActiveDirectoryOrganizationalUnit()
        {
            
        }

        // Inherit base constructors
        public ActiveDirectoryOrganizationalUnit(string operatingUsername, string operatingUserPassword, string path, bool retrieveDescendants = false, int? depth = null)
            : base(path, operatingUsername, operatingUserPassword)
        {
            OperatingUsername = operatingUsername;
            OperatingUserPassword = operatingUserPassword;
            if (string.IsNullOrWhiteSpace(OperatingUsername) || string.IsNullOrWhiteSpace(OperatingUserPassword))
            {
                throw new ArgumentException("OperatingUsername and OperatingPassword are both required");
            }
            OrganizationalUnits = new List<ActiveDirectoryOrganizationalUnit>();
            Users = new List<DirectoryEntry>();
            Populate(retrieveDescendants, depth);
        }

        public ActiveDirectoryOrganizationalUnit(ActiveDirectory activeDirectory, string path, bool retrieveDescendants = false, int? depth = null)
            : this(activeDirectory.OperatingUsername, activeDirectory.OperatingUserPassword, path, retrieveDescendants, depth)
        {
           
        }

        // Populates the organizational unit and optionally its descendants.
        // `path` is the ActiveDirectory path of the organization unit. If none is provided, the whole ActiveDirectory OU will be returned
        // `retrieveDescendants` specifies whether or not the function should retrieve the descendants of the unit
        // `depth` specifies how many levels of descendants the function should retrieve
        public ActiveDirectoryOrganizationalUnit Populate(bool retrieveDescendants = false, int? depth = null)
        {
            // Clear current users and OUs
            Users = new List<DirectoryEntry>();
            OrganizationalUnits = new List<ActiveDirectoryOrganizationalUnit>();

            foreach (DirectoryEntry result in Children)
            {
                switch (result.SchemaClassName.ToLower())
                {
                    case "user":
                        if (result.NativeGuid == null) continue;
                        var flags = (int)result.Properties["userAccountControl"].Value;
                        if (Convert.ToBoolean(flags & 0x0002)) continue;
                        Users.Add(result);
                        break;
                    case "organizationalunit":
                        if (retrieveDescendants && (depth == null || (int)depth >= 0))
                        {
                            OrganizationalUnits.Add(new ActiveDirectoryOrganizationalUnit(OperatingUsername, OperatingUserPassword, result.Path, true, depth - 1));
                        }
                        break;
                }
            }
            return this;
        }
    }
}