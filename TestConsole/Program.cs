using System;
using ActiveDirectoryUtilities;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the domain name");
            var domainName = Console.ReadLine();

            // Set credentials of the user account that we will use to perform the domain 
            // the active directory operations. This user must have access to the AD
            Console.WriteLine("Enter the operating user name");
            var operatingUsername = Console.ReadLine();
            Console.WriteLine("Enter the operating user password");
            var operatingUserPassword = Console.ReadLine();

            // Instantiate the class using the variables above
            Console.WriteLine("Test reading entire Active Directory");
            var activeDirectory = new ActiveDirectory(domainName,operatingUsername,operatingUserPassword);
            activeDirectory.Populate(true);

            // Authenticate a user
            Console.WriteLine("Testing user authentication");
            Console.WriteLine("Enter the user name");
            var username = Console.ReadLine();
            Console.WriteLine("Enter the password");
            var password = Console.ReadLine();
            if (activeDirectory.AuthenticateUser(username, password))
            {
                Console.WriteLine("You have been authenticated");
            }
            else
            {
                Console.WriteLine("User/Password combination is incorrect");
            }

            Console.ReadKey();
        }
    }
}
