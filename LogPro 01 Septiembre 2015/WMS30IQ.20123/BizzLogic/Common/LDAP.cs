using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;
using Integrator.Dao;
using Entities.Profile;
using System.Linq;
using Entities.General;
using Integrator;
using UtilTool;
using Entities.Master;

namespace BizzLogic.Logic
{

    public sealed class LDAPAuth : IDisposable
    {
        #region Variables
        private string stPath;
        private string stFilterAttribute;
        DirectoryEntry entry;
        public DaoFactory Factory { get; set; }
        #endregion

        #region Constructor / destructor
        public LDAPAuth()
        {
            Factory = new DaoFactory();
        }

        public void Dispose()
        {
            if (this.entry != null)
                this.entry.Dispose();
        }

        #endregion

        #region Funciones
       
        //[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public string Groups
        {
            get
            {
                StringBuilder groupNames = new StringBuilder();
                try
                {
                    SearchResult result;

                    using (DirectorySearcher search = new DirectorySearcher(this.entry))
                    {
                        search.Filter = "(cn=" + this.stFilterAttribute + ")";

                        search.PropertiesToLoad.Add("memberOf");

                        result = search.FindOne();
                    }

                    int propertyCount = result.Properties["memberOf"].Count;

                    string dn;

                    int equalsIndex, commaIndex;

                    for (int propertyCounter = 0; propertyCounter < propertyCount; propertyCounter++)
                    {
                        dn = result.Properties["memberOf"][propertyCounter].ToString();

                        equalsIndex = dn.IndexOf("=", 1);

                        commaIndex = dn.IndexOf(",", 1);

                        if (-1 == equalsIndex)
                        {
                            return null;
                        }

                        groupNames.Append(dn.Substring((equalsIndex + 1),
                                          (commaIndex - equalsIndex) - 1));
                        groupNames.Append("|");
                    }

                    return groupNames.ToString();

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

        }
        

        //[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public string GetDataEntry(string property)
        {
            string stRetValue = null;
            if (this.entry.Properties.Contains(property) && this.entry.Properties[property].Value != null)
                stRetValue = this.entry.Properties[property].Value as string;

            return stRetValue;
        }
        
        #endregion



        #region Metodos


        private String GetEncrypt(string data, string criptKey)
        {
            Crypto cpt = new Crypto(Crypto.SymmProvEnum.DES);
            return cpt.Encrypting(data, criptKey);
        }                    


        //[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public SysUser IsValidUser(SysUser user)
        {
            //Variables Auxiliares
            SysUser UsuarioValidar;            

            if (String.IsNullOrEmpty(user.UserName) || String.IsNullOrEmpty(user.Password))
              throw new Exception("Username or password not contains data.");         

            SearchResult result;        

            try
            {
                //Valida contra la aplicacion - debe haber una variable que revise si se permite
                if (!string.IsNullOrEmpty(user.Domain))
                {
                    user.Password = GetEncrypt(user.Password, user.UserName);
                    IList<SysUser> userList = Factory.DaoSysUser().Select(user);

                    if (userList != null && userList.Count > 0)
                    {
                        return userList.First();
                    }
                    else
                    {
                        throw new Exception("Invalid username or password.");
                    }
                } 


               stPath = user.Domain;

                /*entry = new DirectoryEntry(stPath, user.UserName, user.Password);
                entry.RefreshCache();

                using (DirectorySearcher search = new DirectorySearcher(entry))
                {
                    search.Filter = "(SAMAccountName=" + user.UserName + ")";
                    search.PropertiesToLoad.Add("cn");
                    result = search.FindOne();

                    if (result == null)
                        throw new Exception("User not found in the selected domain.");

                    this.stPath = result.Path;
                    this.stFilterAttribute = result.Properties["cn"][0].ToString();
                    entry = result.GetDirectoryEntry();
                    entry.RefreshCache();

                    return Factory.DaoSysUser().Select(new SysUser { UserName = user.UserName }).First();

                }*/

                //Obtengo el usuario para validar
                UsuarioValidar = Factory.DaoSysUser().Select(new SysUser { UserName = user.UserName }).First();

                //Evaluo que el usuario tenga un rol

                return UsuarioValidar;

            }
            catch (Exception e)
            {
                throw new Exception("Error validating user "+ user.UserName +".\n" + WriteLog.GetTechMessage(e));
            }
        }


        public static IList<ShowData> GetDomainList()
        {
            IList<ShowData> domainList = new List<ShowData>();

            ShowData curDomain;

            //Use LDAP Authentication - read Option
            DaoFactory factory = new DaoFactory();
            try
            {
                string ldapConfig = factory.DaoConfigOption().Select(new ConfigOption { Code = "USELDAP" }).FirstOrDefault().DefValue;
                
                //Muestra dominios si la opcion es True
                if (ldapConfig == "T")
                {

                    DirectoryEntry en = new DirectoryEntry("LDAP://");
                    // Search for objectCategory type "Domain"
                    DirectorySearcher srch = new DirectorySearcher("objectCategory=Domain");
                    SearchResultCollection coll = srch.FindAll();
                    // Enumerate over each returned domain.
                    foreach (SearchResult rs in coll)
                    {
                        curDomain = new ShowData();
                        ResultPropertyCollection resultPropColl = rs.Properties;
                        foreach (object domainKey in resultPropColl["name"])
                        {
                            curDomain.DataKey = domainKey.ToString().ToUpper();

                            foreach (object domainPath in resultPropColl["adspath"])
                                curDomain.DataValue = domainPath.ToString();
                        }

                        domainList.Add(curDomain);
                    }
                }
            }
            catch { }

            //Adding Default Domain
            domainList.Add(new ShowData { DataKey = "WMSEXPRESS", DataValue = "" });

            return domainList;

        }

        public static IList<ShowData> GetCustomersList()
        {
            //Variables Auxiliares
            IList<ShowData> CustomerList = new List<ShowData>();
            ShowData Customer;
            IList<Location> ListadoClientes;
            DaoFactory factory = new DaoFactory();

            //Obtenemos el listado de clientes habilitados, Status = 1001
            ListadoClientes = factory.DaoLocation().Select(new Location { Status = new Status { StatusID = 1001 } });

            //Recorro el listado de clientes y creo el listado
            foreach (Location Cliente in ListadoClientes)
            {
                //Inicializo la variable del cliente
                Customer = new ShowData();

                //Asigno los datos
                Customer.DataKey = Cliente.Name;
                Customer.DataValue = Cliente.LocationID.ToString();

                //Adicionamos el cliente al listado
                CustomerList.Add(Customer);
            }

            return CustomerList;
        }


        public SysUser IsValidUserDevice(SysUser user)
        {

            if ((String.IsNullOrEmpty(user.UserName) && user.UserID == 0) || String.IsNullOrEmpty(user.Password))
                throw new Exception("Username or password not contains data.");


            try
            {
                    SysUser tmpUser = null;
                    if (user.UserID > 0)
                    {
                        tmpUser = Factory.DaoSysUser().SelectById(new SysUser { UserID = user.UserID });

                        if (tmpUser == null)
                            throw new Exception("Error validating user, please retry.");
                        else
                            user.UserName = tmpUser.UserName;
                    }

                    user.Password = GetEncrypt(user.Password, user.UserName);
                    IList<SysUser> userList = Factory.DaoSysUser().Select(user);

                    if (userList != null && userList.Count > 0)
                        return userList.First();
                    else
                        throw new Exception("Invalid username or password.");

            }
            catch (Exception e)
            {
                throw new Exception("Error validating user " + user.UserName + ".\n" + WriteLog.GetTechMessage(e));
            }
        }
        
        #endregion
    }
}
