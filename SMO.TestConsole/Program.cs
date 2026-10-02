using SMO.SAPINT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAP.Middleware.Connector;
using SharpSapRfc;
using SharpSapRfc.Plain;
using SMO.SAPINT.Function;
using SMO.Repository.Common;
using NHibernate.Linq;
using SMO.Core.Entities;
using SMO;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Net;
using System.DirectoryServices;
namespace SMO.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var ldapServer = "LDAP://118.70.198.214/DC=fecon/DC=vn";
            ////var result = IsAuthenticated(ldapServer, "fcldap", "Fcn@2019");

            //String uid = "fcldap";
            //// assign password
            //String password = "Fcn@2018";
            //// define LDAP connection
            //DirectoryEntry root = new DirectoryEntry("LDAP://118.70.198.214/ou=FECON USE,DC=fecon,DC=vn", uid, password, AuthenticationTypes.None);

            //try
            //{
            //    // attempt to use LDAP connection
            //    object connected = root.NativeObject;
            //    // no exception, login successful
            //}
            //catch (Exception ex)
            //{
            //    var test = ex.ToString();
            //}

            SMOUtilities.BroadcastMail();
        }

        public static bool IsAuthenticated(string ldap, string usr, string pwd)
        {
            bool authenticated = false;

            try
            {
                DirectoryEntry entry = new DirectoryEntry(ldap, usr, pwd);
                object nativeObject = entry.NativeObject;
                authenticated = true;
            }
            catch (DirectoryServicesCOMException cex)
            {
                Console.WriteLine(cex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return authenticated;
        }
    }
}
