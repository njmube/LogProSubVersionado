using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities.Master;
using Entities.Trace;
using Entities.General;
using ErpConnect;
using System.Configuration;
using Integrator;
using Entities;
using Entities.Profile;

namespace BizzLogic.Logic
{

    public partial class ErpDataMngr : BasicMngr
    {
        private ConnectFactory ErpFactory { get; set; }
        private WmsTypes WType;
        private DocumentMngr DocMngr;
        private int historicDays;
        private Rules Rules { get; set; }
        //private TransactionMngr TranMngr { get; set; }

        public ErpDataMngr()
        {
            //Factory = new DaoFactory();
            WType = new WmsTypes(Factory);
            DocMngr = new DocumentMngr();
            //TranMngr = new TransactionMngr();
            Rules = new Rules(Factory);
        }


        public void SetConnectMngr(Company company)
        {
            if (ErpFactory == null)
            {
                ErpFactory = ConnectFactory.getConnectFactory(company);

                try
                {
                    historicDays = int.Parse(Factory.DaoConfigOptionByCompany()
                        .Select( new ConfigOptionByCompany
                                {
                                    ConfigOption = new ConfigOption { Code = "QUERYDAYS" },
                                    Company = company,
                                }
                        ).FirstOrDefault().Value);
                }
                catch { historicDays = WmsSetupValues.HistoricDays; }

            }
        }


        public void TestConnection(Company company)
        {
            if (ErpFactory == null)
                SetConnectMngr(company);

            try
            {
                ErpFactory.References().TestConnection(company);
            }
            catch (Exception ex)
            {
                throw new Exception(WriteLog.GetTechMessage(ex));
            }
        }




        #region ErpReferences


        // UNITS
        public Boolean GetErpAllUnits(Company company)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllUnits Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessUnits(ErpFactory.References().GetAllUnits());
            return true;
        }


        private void ProcessUnits(IList<Unit> list)
        {
            if (list == null)
                return;

            Unit curUnit = null;


            foreach (Unit e in list)
            {
                try
                {

                    curUnit = e;
                    //Evalua si el elemento ya existe 
                    IList<Unit> exList = Factory.DaoUnit().Select(e);

                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;

                    if (exList.Count == 0)
                    {
                        e.CreatedBy = WmsSetupValues.SystemUser;
                        e.CreationDate = DateTime.Now;
                        Factory.DaoUnit().Save(e);
                    }
                    else
                    {
                        e.UnitID = exList.First().UnitID;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.CreationDate = exList.First().CreationDate;
                        Factory.DaoUnit().Update(e);
                    }

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("ProcessUnits:" + curUnit.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    //throw;
                }
            }

        }



        //LOCATIONS
        public Boolean GetErpAllLocations(Company company)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllLocations Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessLocations(ErpFactory.References().GetAllLocations()); return true;
        }


        public Boolean GetErpLocationsSince(Company company, DateTime sinceDate)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpLocationsSince Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessLocations(ErpFactory.References().GetLocationsSince(sinceDate));
            return true;
        }


        private void ProcessLocations(IList<Location> list)
        {

            if (list == null)
                return;

            Location qLoc = null;



            foreach (Location e in list)
            {

                try
                {


                    qLoc = new Location();
                    qLoc.Company = e.Company;
                    qLoc.ErpCode = e.ErpCode;


                    //Evalua si el elemento ya existe 
                    IList<Location> exList = Factory.DaoLocation().Select(qLoc);
                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;

                    if (exList.Count == 0)
                    {
                        e.CreationDate = DateTime.Now;
                        e.CreatedBy = WmsSetupValues.SystemUser;
                        Factory.DaoLocation().Save(e);
                    }
                    else
                    {
                        e.LocationID = exList.First().LocationID;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.CreationDate = exList.First().CreationDate;
                        e.BatchNo = exList.First().BatchNo;
                        Factory.DaoLocation().Update(e);
                    }

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("ProcessLocations:" + qLoc.Company.Name + "," + qLoc.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    //throw;
                }
            }

        }



        //SHIPPING METHGODS
        public Boolean GetErpAllShippingMethods(Company company)
        {

            if (company == null) {
                ExceptionMngr.WriteEvent("GetErpAllShippingMethods Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessShippingMethods(ErpFactory.References().GetAllShippingMethods()); return true;

        }


        private void ProcessShippingMethods(IList<ShippingMethod> list)
        {
            if (list == null)
                return;

            ShippingMethod curShp;



            foreach (ShippingMethod e in list)
            {
                try
                {

                    curShp = new ShippingMethod();
                    curShp.Company = e.Company;
                    curShp.ErpCode = e.ErpCode;


                    //Evalua si el elemento ya existe 
                    IList<ShippingMethod> exList = Factory.DaoShippingMethod().Select(curShp);
                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;

                    if (exList.Count == 0)
                    {
                        e.CreationDate = DateTime.Now;
                        e.CreatedBy = WmsSetupValues.SystemUser;
                        Factory.DaoShippingMethod().Save(e);
                    }
                    else
                    {
                        e.ShpMethodID = exList.First().ShpMethodID;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.CreationDate = exList.First().CreationDate;
                        Factory.DaoShippingMethod().Update(e);
                    }

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("ProcessShippingMethods:", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    //throw;
                }
            }

        }





        // PRODUCTS  
        public Boolean GetErpAllProducts(Company company)
        {

            if (company == null) {
                ExceptionMngr.WriteEvent("GetErpAllProducts Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            //Llama primero a las categorias
            GetErpAllCategories(company);

            /*
            IList<ProductCategory> catList = Factory.DaoProductCategory().Select(new ProductCategory());

            
            foreach (ProductCategory cat in catList)
            {
                Console.WriteLine(cat.ErpCode.Replace("'", "''"));
                ProcessProducts(ErpFactory.References().GetProductsByQuery("LTRIM(RTRIM(ITMCLSCD))='"+ cat.ErpCode.Replace("'","''") +"'")); 
            }
             

            //Los que no tienen categoria
            ProcessProducts(ErpFactory.References().GetProductsByQuery("LTRIM(RTRIM(ITMCLSCD))=''")); 
            */

            // CAA [2010/07/19]
            // ciclo de n registros solo para GP
            bool cycle = false;
            switch (company.ErpConnection.ConnectionType.RowID)
            {
                case CnnType.GPeConnect:
                    cycle = true;
                    break;
                case CnnType.Everest:
                    cycle = false;
                    break;
                case CnnType.UnoEE:
                    cycle = false;
                    break; 
                default:
                    cycle = false;
                    break;
            }

            IList<Product> list;
            bool canContinue = true;
            int record = 0;
            int step = 50;
            if (cycle)
            {
                while (canContinue)
                {
                    Console.WriteLine(record.ToString() + " to " + (record + step).ToString());
                    list = ErpFactory.References().GetProductsByQuery(" (DEX_ROW_ID > " + record.ToString() + " AND DEX_ROW_ID <= " + (record + step).ToString() + ")");

                    if (list == null || list.Count == 0)
                        canContinue = false;
                    else
                    {
                        ProcessProducts(list);
                        record += step;
                    }
                }
            }
            else
            {
                list = ErpFactory.References().GetProductsByQuery("");
                ProcessProducts(list);
            }

            return true;
        }

        // PRODUCT CATEGORY
        public Boolean GetErpAllCategories(Company company)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllCategories Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessCategories(ErpFactory.References().GetAllProductCategories());
            return true;
        }


        private void ProcessCategories(IList<ProductCategory> list)
        {

            if (list == null)
                return;

            ProductCategory curRecord = null;



            foreach (ProductCategory e in list)
            {

                try
                {
                    //Evalua si el elemento ya existe 

                    curRecord = new ProductCategory { 
                        Company = new Company { CompanyID = e.Company.CompanyID },
                        ErpCode = e.ErpCode
                    };

                    IList<ProductCategory> exList = Factory.DaoProductCategory().Select(curRecord);

                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;

                    if (exList.Count == 0)
                    {
                        e.CreatedBy = WmsSetupValues.SystemUser;
                        e.CreationDate = DateTime.Now;
                        Factory.DaoProductCategory().Save(e);
                    }
                    else
                    {
                        e.CategoryID = exList.First().CategoryID;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.CreationDate = exList.First().CreationDate;
                        e.ModifiedBy = WmsSetupValues.SystemUser;
                        e.ModDate = DateTime.Now;
                        e.ExplodeKit = exList.First().ExplodeKit;
                        Factory.DaoProductCategory().Update(e);
                    }

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("ProcessCategories:" + curRecord.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    ////throw;
                }
            }

        }


        public Boolean GetErpProductById(Company company, string code)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpProductById Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessProducts(ErpFactory.References().GetProductById(code)); return true;
        }


        public Boolean GetErpProductsLastXDays(Company company, int days)
        {

            if (company == null) {
                ExceptionMngr.WriteEvent("GetErpProductsLastXDays Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessProducts(ErpFactory.References().GetProductsLastXDays(days));
            return true;
        }


        public Boolean GetErpProductsSince(Company company, DateTime sinceDate)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpProductsSince Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessProducts(ErpFactory.References().GetProductsSince(sinceDate));
            return true;
        }


        public Boolean GetErpProductsByQuery(Company company, string  SWhere)
        {

            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpProductsByQuery Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessProducts(ErpFactory.References().GetProductsByQuery(SWhere));
            return true;
        }


        private void ProcessProducts(IList<Product> list)
        {
            if (list == null)
                return;

            Product qProd = null;
            UnitProductRelation curUnit;
            ProductTrackRelation curRecord;
            ProductAccountRelation curPvendor;


            foreach (Product e in list)
            {
                try
                {
                    qProd = new Product();
                    qProd.Company = e.Company;
                    qProd.ProductCode = e.ProductCode;

                    //Evalua si el elemento ya existe 
                    IList<Product> exList = Factory.DaoProduct().Select(qProd,0);
                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;

                    if (exList.Count == 0)
                    {
                        e.CreationDate = DateTime.Now;
                        e.CreatedBy = WmsSetupValues.SystemUser;
                        Factory.DaoProduct().Save(e);
                    }
                    else
                    {
                        e.ProductID = exList.First().ProductID;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.CreationDate = exList.First().CreationDate;

                        //Otros datos del producto que deben preservarse
                        e.UpcCode = exList.First().UpcCode;
                        e.Reference = exList.First().Reference;
                        e.Manufacturer = exList.First().Manufacturer;
                        e.IsBinRestricted = exList.First().IsBinRestricted;
                        e.Comment = exList.First().Comment;
                        //e.AlternateCode1 = exList.First().AlternateCode1;
                        //e.AlternateCode2 = exList.First().AlternateCode2;
                        e.PrintLabel = exList.First().PrintLabel;
                        e.UnitsPerPack = exList.First().UnitsPerPack;
                        e.PickMethod = exList.First().PickMethod;
                        e.DefaultTemplate = exList.First().DefaultTemplate;
                        e.DefVendorNumber = exList.First().DefVendorNumber;

                        //Evaluar los UnitRelations
                        int i = 0;
                        foreach (UnitProductRelation line in e.ProductUnits)
                        {
                            curUnit = new UnitProductRelation();
                            curUnit.Product = line.Product;
                            curUnit.UnitErpCode = line.UnitErpCode;
                            curUnit.Unit = line.Unit;
                            IList<UnitProductRelation> listLines = Factory.DaoUnitProductRelation().Select(curUnit);

                            if (listLines.Count > 0)
                            {
                                e.ProductUnits[i].RowID = listLines.First().RowID;
                                e.ProductUnits[i].CreationDate = listLines.First().CreationDate;
                                e.ProductUnits[i].CreatedBy = listLines.First().CreatedBy;
                                e.ProductUnits[i].ModDate = DateTime.Now;
                                e.ProductUnits[i].ModifiedBy = WmsSetupValues.SystemUser;
                            }
                            else
                            {
                                e.ProductUnits[i].CreationDate = DateTime.Now;
                                e.ProductUnits[i].CreatedBy = WmsSetupValues.SystemUser;
                            }

                            i++;
                        }


                        //Evaluar los ProductTrackOptions
                        if (e.ProductTrack != null)
                        {
                            int y = 0;
                            foreach (ProductTrackRelation line in e.ProductTrack)
                            {
                                curRecord = new ProductTrackRelation();
                                curRecord.Product = line.Product;
                                curRecord.TrackOption = line.TrackOption;
                                IList<ProductTrackRelation> trackLines = Factory.DaoProductTrackRelation().Select(curRecord);

                                if (trackLines.Count > 0)
                                {
                                    e.ProductTrack[y].RowID = trackLines.First().RowID;
                                    e.ProductTrack[y].CreationDate = trackLines.First().CreationDate;
                                    e.ProductTrack[y].CreatedBy = trackLines.First().CreatedBy;
                                    e.ProductTrack[y].ModDate = DateTime.Now;
                                    e.ProductTrack[y].ModifiedBy = WmsSetupValues.SystemUser;
                                }
                                else
                                {
                                    e.ProductTrack[y].CreationDate = DateTime.Now;
                                    e.ProductTrack[y].CreatedBy = WmsSetupValues.SystemUser;
                                }

                                y++;
                            }
                        }


                        //Evaluar los ProductVendor

                        try
                        {
                            IList<ProductAccountRelation> pAlt = Factory.DaoProductAccountRelation()
                                .Select(new ProductAccountRelation { IsFromErp = true, Product = new Product { ProductID = e.ProductID } }); ;

                            if (pAlt != null && pAlt.Count > 0)
                                foreach (ProductAccountRelation pax in pAlt)
                                    Factory.DaoProductAccountRelation().Delete(pax);
                        }
                        catch { }


                        if (e.ProductAccounts != null)
                        {
                            int y = 0;
                            foreach (ProductAccountRelation line in e.ProductAccounts)
                            {
                                curPvendor = new ProductAccountRelation();
                                curPvendor.Product = line.Product;
                                curPvendor.Account = line.Account;
                                curPvendor.ItemNumber = line.ItemNumber;
                                IList<ProductAccountRelation> pVendLines = Factory.DaoProductAccountRelation().Select(curPvendor);

                                if (pVendLines.Count > 0)
                                {
                                    e.ProductAccounts[y].RowID = pVendLines.First().RowID;
                                    e.ProductAccounts[y].CreationDate = pVendLines.First().CreationDate;
                                    e.ProductAccounts[y].CreatedBy = pVendLines.First().CreatedBy;
                                    e.ProductAccounts[y].ModDate = DateTime.Now;
                                    e.ProductAccounts[y].ModifiedBy = WmsSetupValues.SystemUser;
                                }
                                else
                                {
                                    e.ProductAccounts[y].CreationDate = DateTime.Now;
                                    e.ProductAccounts[y].CreatedBy = WmsSetupValues.SystemUser;
                                }

                                y++;
                            }
                        }


                        //Evaluar los ProductAlternate

                        //ProductAlternate curAlt;
                        /*
                        try
                        {
                            IList<ProductAlternate> pAlt = Factory.DaoProductAlternate()
                                .Select(new ProductAlternate { IsFromErp = true, Product = new Product { ProductID = e.ProductID } }); 

                            if (pAlt != null && pAlt.Count > 0)
                                foreach (ProductAlternate pax in pAlt)
                                    try { Factory.DaoProductAlternate().Delete(pax); }
                                    catch (Exception ex) { Console.WriteLine("PA:" + ex.Message); }
                        }
                        catch { }
                        */


                        if (e.AlternProducts != null)
                        {
                            int y = 0;
                            foreach (ProductAlternate line in e.AlternProducts)
                            {
                                e.AlternProducts[y].CreationDate = DateTime.Now;
                                e.AlternProducts[y].CreatedBy = WmsSetupValues.SystemUser;
                                y++;

                                try
                                {
                                    ProductAlternate exist = Factory.DaoProductAlternate()
                                    .Select(new ProductAlternate { 
                                        AlternProduct = new Product { ProductID = line.AlternProduct.ProductID },
                                        Product = new Product { ProductID = e.ProductID } }).First();

                                    Factory.DaoProductAlternate().Delete(exist);
                                }
                                catch { }

                                //curAlt = new ProductAlternate();
                                //curAlt.Product = line.Product;
                                //curAlt.AlternProduct = line.AlternProduct;
                            }
                        }

                        Factory.DaoProduct().Update(e);
                    }


                }
                catch (Exception ex)
                {
                    //Factory.Rollback();
                    ExceptionMngr.WriteEvent("ProcessProducts:" + qProd.ProductCode, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    ////throw;
                }
            }

        }



        // ACCOUNTS  
        public Boolean GetErpAllAccounts(Company company)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllAccounts Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            //Customers
            ProcessErpAccounts(ErpFactory.References().GetAllCustomers());
            //Vendors
            ProcessErpAccounts(ErpFactory.References().GetAllVendors());
            return true;
        }


        public Boolean GetErpAccountsLastXDays(Company company, int days)
        {
            if (company == null) {
                ExceptionMngr.WriteEvent("GetErpAccountsLastXDays Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            //Customers
            ProcessErpAccounts(ErpFactory.References().GetCustomersLastXDays(days));
            //Vendors
            ProcessErpAccounts(ErpFactory.References().GetVendorsLastXDays(days));

            return true;
        }


        public Boolean GetErpAccountById(Company company, string code)
        {

            if (company == null) {
                ExceptionMngr.WriteEvent("GetErpAccountById Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            //Customers
            ProcessErpAccounts(ErpFactory.References().GetCustomerById(code));
            //Vendors
            ProcessErpAccounts(ErpFactory.References().GetVendorById(code));

            return true;
        }


        public Boolean GetErpAccountsSince(Company company, DateTime sinceDate)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAccountsSince Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);

            //Customers
            ProcessErpAccounts(ErpFactory.References().GetCustomersSince(sinceDate));
            //Vendors
            ProcessErpAccounts(ErpFactory.References().GetVendorsSince(sinceDate));

            return true;
        }


        private void ProcessErpAccounts(IList<Account> list)
        {

            if (list == null)
                return;


            DaoFactory Factory = new DaoFactory();

            Account qAcc = null;
            AccountAddress curLine;
            AccountTypeRelation curAccType;


            foreach (Account e in list)
            {
                try
                {


                    qAcc = new Account();
                    qAcc.Company = e.Company;
                    qAcc.AccountCode = e.AccountCode;
                    qAcc.BaseType = e.BaseType;

                    //Evalua si la entidad ya existe 
                    IList<Account> exList = Factory.DaoAccount().Select(qAcc);
                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;

                    //Si existe
                    if (exList.Count == 0)
                    {
                        e.CreationDate = DateTime.Now;
                        e.CreatedBy = WmsSetupValues.SystemUser;
                        Factory.DaoAccount().Save(e);
                    }
                    else
                    {
                        e.AccountID = exList.First().AccountID;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.CreationDate = exList.First().CreationDate;

                        //Preservar
                        e.UserDefine1 = exList.First().UserDefine1; //Shipping Mails


                        //Evaluar los Account Types
                        int i = 0;
                        foreach (AccountTypeRelation line in e.AccountTypes)
                        {
                            curAccType = new AccountTypeRelation();
                            curAccType.Account = e;
                            curAccType.ErpCode = line.ErpCode;
                            curAccType.AccountType = line.AccountType;
                            IList<AccountTypeRelation> listLines = Factory.DaoAccountTypeRelation().Select(curAccType);

                            if (listLines.Count > 0)
                            {
                                e.AccountTypes[i].ModDate = DateTime.Now;
                                e.AccountTypes[i].ModifiedBy = WmsSetupValues.SystemUser;
                                e.AccountTypes[i].CreationDate = listLines.First().CreationDate;
                                e.AccountTypes[i].CreatedBy = listLines.First().CreatedBy;
                                e.AccountTypes[i].RowID = listLines.First().RowID;
                            }
                            else
                            {
                                e.AccountTypes[i].CreationDate = DateTime.Now;
                                e.AccountTypes[i].CreatedBy = WmsSetupValues.SystemUser;
                            }

                            i++;
                        }


                        //Evaluar los document Lines
                        i = 0;
                        foreach (AccountAddress line in e.AccountAddresses)
                        {
                            curLine = new AccountAddress();
                            curLine.Account = e;
                            curLine.ErpCode = line.ErpCode;
                            IList<AccountAddress> listLines = Factory.DaoAccountAddress().Select(curLine);

                            if (listLines.Count > 0)
                            {
                                e.AccountAddresses[i].ModDate = DateTime.Now;
                                e.AccountAddresses[i].ModifiedBy = WmsSetupValues.SystemUser;
                                e.AccountAddresses[i].AddressID = listLines.First().AddressID;
                                e.AccountAddresses[i].CreationDate = listLines.First().CreationDate;
                                e.AccountAddresses[i].CreatedBy = listLines.First().CreatedBy;
                            }
                            else
                            {
                                e.AccountAddresses[i].CreationDate = DateTime.Now;
                                e.AccountAddresses[i].CreatedBy = WmsSetupValues.SystemUser;
                            }

                            i++;
                        }

                        Factory.DaoAccount().Update(e);
                    }

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("ProcessErpAccounts:" + qAcc.Name, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    ////throw;
                }
            }


        }


        #endregion



        #region ErpReferencesInventory


        // KIT ASEMMBLY HEADER
        public Boolean GetErpAllKitAssembly(Company company)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAllKitAssembly Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }

            SetConnectMngr(company);

            ProcessKitAssembly(ErpFactory.References().GetAllKitAssembly(), company);
            return true;
        }



        public Boolean GetErpKitAssemblySince(Company company, DateTime sinceDate)
        {
            if (company == null)
            {
                ExceptionMngr.WriteEvent("GetErpAccountsSince Company " + company.Name, ListValues.EventType.Fatal, null, null, ListValues.ErrorCategory.Business);
                return false;
            }


            SetConnectMngr(company);


            ProcessKitAssembly(ErpFactory.References().GetKitAssemblySince(sinceDate), company);

            return true;
        }


        private void ProcessKitAssembly(IList<KitAssembly> list, Company company)
        {
            if (list == null)
                return;

            KitAssembly curRecord = null;


            foreach (KitAssembly e in list)
            {
                try
                {

                    curRecord  =  new KitAssembly();
                    curRecord.Product = e.Product;
                    //Evalua si el elemento ya existe 
                    IList<KitAssembly> exList = Factory.DaoKitAssembly().Select(curRecord,0);

                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;

                    if (exList.Count == 0)
                    {
                        e.CreatedBy = WmsSetupValues.SystemUser;
                        e.CreationDate = DateTime.Now;
                        Factory.DaoKitAssembly().Save(e);
                    }
                    else
                    {
                        e.RowID = exList.First().RowID;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.CreationDate = exList.First().CreationDate;
                        Factory.DaoKitAssembly().Update(e);
                    }

                    
                    //Elimna las formulas para ese producto y las crea de nuevo
                    foreach (KitAssemblyFormula kaf in Factory.DaoKitAssemblyFormula()
                        .Select(new KitAssemblyFormula { KitAssembly = new KitAssembly { Product = new Product { ProductID = e.Product.ProductID } } }))
                        Factory.DaoKitAssemblyFormula().Delete(kaf);


                    //Procesa el assembly Formula que le corresponde
                    // CAA [2010/07/19]
                    // condición busqueda kit padre
                    string query = "";
                    switch (company.ErpConnection.ConnectionType.RowID)
                    {
                        case CnnType.GPeConnect:
                            query = " ITEMNMBR = '" + e.Product.ProductCode + "' ";
                            break;
                        case CnnType.Everest:
                            query = " KIT_CODE = '" + e.Product.ProductCode + "' ";
                            break;
                        case CnnType.UnoEE:
                            query = " (formula.f134_id_cia = " + e.Product.Company.ErpCode + ") AND  itmPadre.f120_id  = '" + e.Product.ProductCode + "'";
                            break;
                        default:
                            query = "";
                            break;
                    }
                    ProcessKitAssemblyFormula(ErpFactory.References().GetKitAssemblyFormula(query));

                }
                catch (Exception ex)
                {
                    //Factory.Rollback();
                    ExceptionMngr.WriteEvent("ProcessKitAssembly:" + curRecord.Product.ProductCode, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    //throw;
                }
            }

        }


        private void ProcessKitAssemblyFormula(IList<KitAssemblyFormula> list)
        {
            if (list == null)
                return;

            KitAssemblyFormula curRecord = null;


            foreach (KitAssemblyFormula e in list)
            {
                try
                {

                    curRecord = e;
                    //Evalua si el elemento ya existe 
                    IList<KitAssemblyFormula> exList = Factory.DaoKitAssemblyFormula().Select(e);

                    e.ModDate = DateTime.Now;
                    e.ModifiedBy = WmsSetupValues.SystemUser;

                    if (exList.Count == 0)
                    {
                        e.CreatedBy = WmsSetupValues.SystemUser;
                        e.CreationDate = DateTime.Now;
                        Factory.DaoKitAssemblyFormula().Save(e);
                    }
                    else
                    {
                        e.RowID = exList.First().RowID;
                        e.CreatedBy = exList.First().CreatedBy;
                        e.CreationDate = exList.First().CreationDate;
                        Factory.DaoKitAssemblyFormula().Update(e);
                    }

                }
                catch (Exception ex)
                {
                    Factory.Rollback();
                    ExceptionMngr.WriteEvent("ProcessKitAssemblyFormula:" + curRecord.KitAssembly.Product.ProductCode, ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                    //throw;
                }
            }

        }



        #endregion
    }
}