# active-directory-utilities

Perform common Active Directory tasks easily...

## Motivation

In .NET projects, the need to perform operations against an Active Directory is very common (i.e, authenticating a user). This library aims to abstract common operations against active directory.

## Install

`Install-Package ActiveDirectoryUtilities`

## Usage example

    var domainName = "MyDomain";

    // Set credentials of the user account that we will use to perform the domain
    // the active directory operations. This user must have access to the AD
    var operatingUserName = "Admin1";
    var operatingUserPassword = "Password";

    // Instantiate the class using the variables above
    var activeDirectory = new ActiveDirectory(domainName,operatingUserName,operatingUserPassword);

    // Authenticate a user
    if (activeDirectory.AuthenticateUser("user1", "somePassword"))
    {
        Console.WriteLine("You have been authenticated");
    }
    else
    {
        Console.WriteLine("User/Password combination is incorrect");
    }

    // You may also do it using a static method
    if (ActiveDirectory.AuthenticateUser(domainName,operatingUserName,operatingUserPassword,"user1", "somePassword"))
    {
        Console.WriteLine("You have been authenticated");
    }
    else
    {
        Console.WriteLine("User/Password combination is incorrect");
    }

## API

**Note**: All methods shown in this section also have a static counterpart.

- `bool AuthenticateUser(string userName, string password)` - Authenticates a username/password combination against the specified domain

- `Populate(bool retrieveDescendants = false, int? depth = null)` - Retrieves the Active Directory base organizational unit and optionally retrieves its descendants

- `UserPrincipal GetUserByUserName(string userName)` - Finds a user by their username. If no user is found, it returns null

- `GroupPrincipal GetGroupByName(string groupName)` - Finds a domain group by name. If no group is found, it returns null. The `groupName` may contain wildcards.

- `ActiveDirectoryOrganizationalUnit GetOrganizationalUnit(string path = null, bool retrieveDescendants = false,int? depth = null)` - Finds all organizational units and optionally their descendants.

  * `path` is the ActiveDirectory path of the organization unit. If none is provided, the whole ActiveDirectory OU will be returned
  * `retrieveDescendants` specifies whether or not the function should retrieve the descendants of the unit
  * `depth` specifies how many levels of descendants the function should retrieve

- `List<GroupPrincipal> GetGroupsByName(string filter = "\*")` - Finds all domain groups by name. The `filter` may contain wildcards.

- `List<string> GetGroupNames(string filter = "\*")` - Only gets the names of the domain groups that match the filter passed

## Contributing

If you would like to contribute, you may do so to the **development** branch.
