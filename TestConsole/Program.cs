using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiveDirectoryUtilities;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var domainName = "";

            // Set credentials of the user account that we will use to perform the domain 
            // the active directory operations. This user must have access to the AD
            var operatingUserName = "";
            var operatingUserPassword = "";

            // Instantiate the class using the variables above
            var activeDirectory = new ActiveDirectory(domainName,operatingUserName,operatingUserPassword);

            // Authenticate a user
            if (activeDirectory.AuthenticateUser("", ""))
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
