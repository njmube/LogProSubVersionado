using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Integrator;
using Entities;
using Entities.Trace;
using Entities.General;
using Entities.Master;
using Entities.Profile;
using System.Xml;
using System.Data.SqlClient;


namespace ErpConnect.DynamicsGP
{

    public partial class ReferenceService_ec : SQLBase, IReferenceService
    {

        private StringWriter swriter;
        private DataSet ds;
        private WmsTypes WType;
        private Company CurCompany;


        public ReferenceService_ec(Company factoryCompany)
        {
            swriter = new StringWriter();
            WType = new WmsTypes();
            ds = new DataSet();
            CurCompany = factoryCompany;            
        
        }



        public void TestConnection(Company data)
        {
            try
            {
                SqlConnection gpConnect = new SqlConnection(data.ErpConnection.CnnString);
                gpConnect.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        //UNITS
        /// <summary>
        /// Get Unit from Dynamics GP
        /// </summary>
        /// <returns></returns>
        public IList<Unit> GetAllUnits() {

            Unit tmpData;
            IList<Unit> list = new List<Unit>();

            try
            {

                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("WSUofMSchedule", false, 2, 0, "", true)); //WSUofMSchedule

                if (ds.Tables.Count == 0)
                    return null;


                //En el dataset, Tables: 1 - UOFM
                foreach (DataRow dr in ds.Tables[2].Rows)
                {
                    //Solo para unidades equivalentes en unidad base
                    if (Double.Parse(dr["QTYBSUOM"].ToString(), ListValues.DoubleFormat()) > 0)
                    {
                        //Map Properties
                        tmpData = new Unit();

                        tmpData.Company = CurCompany; // WType.GetDefaultCompany();
                        tmpData.Name = dr["UOFM"].ToString();
                        tmpData.ErpCode = dr["UOFM"].ToString();
                        tmpData.ErpCodeGroup = dr["UOMSCHDL"].ToString();
                        tmpData.BaseAmount = double.Parse(dr["QTYBSUOM"].ToString(), ListValues.DoubleFormat());
                        tmpData.IsFromErp = true;
                        list.Add(tmpData);
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetUnits", ListValues.EventType.Error, ex, null, 
                    ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }
        
        }


        //LOCATIONS
        /// <summary>
        /// Get Locations from Dynamics GP
        /// </summary>
        /// <returns></returns>
        public IList<Location> GetAllLocations()
        {
            return GetLocations("");
        }

        public IList<Location> GetLocationsSince(DateTime sinceDate)
        {
            return GetLocations(" MODIFDT >= " + sinceDate.ToString("yyyy-MM-dd"));
        }

        public IList<Location> GetLocations(string sWhere)
        {

            Location tmpData;
            IList<Location> list = new List<Location>();

            try
            {
                //Lamar los documents que necesita del Erp usando econnect
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("WSWarehouse", false, 2, 0, sWhere, true));
                
                if (ds.Tables.Count == 0)
                    return null;
                
                
                Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                //En el dataset, Tables: 1 - Locations
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    if (string.IsNullOrEmpty(dr["LOCNCODE"].ToString()))
                        continue;

                    //Map Properties
                    tmpData = new Location();

                    tmpData.Company = CurCompany;
                    tmpData.ErpCode = dr["LOCNCODE"].ToString();
                    tmpData.Name = dr["LOCNDSCR"].ToString();
                    tmpData.AddressLine1 = dr["ADDRESS1"].ToString();
                    tmpData.AddressLine2 = dr["ADDRESS2"].ToString();
                    tmpData.AddressLine3 = dr["ADDRESS3"].ToString();
                    tmpData.City = dr["CITY"].ToString();
                    tmpData.State = dr["STATE"].ToString();
                    tmpData.ZipCode = dr["ZIPCODE"].ToString();
                    tmpData.Phone1 = dr["PHONE1"].ToString();
                    tmpData.Phone2 = dr["PHONE2"].ToString();
                    tmpData.Phone3 = dr["FAXNUMBR"].ToString();
                    //tmpData.ContactPerson = dr[""].ToString();
                    tmpData.IsDefault = false;
                    tmpData.Status = status;
                    tmpData.IsFromErp = true;

                    list.Add(tmpData);

                }

                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetLocations", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }
        }

        //SHIPPING METHODS
        /// <summary>
        /// Get Shipping Methods from Dynamics GP
        /// </summary>
        /// <returns></returns>
        public IList<ShippingMethod> GetAllShippingMethods()
        {

            ShippingMethod tmpData;
            IList<ShippingMethod> list = new List<ShippingMethod>();

            try
            {
                //Lamar los documents que necesita del Erp usando econnect
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("WSShippingMethod", false, 2, 0, "", true));
                
                if (ds.Tables.Count == 0)
                    return null;

                //En el dataset, Tables: 1 - Methods
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    //Map Properties
                    tmpData = new ShippingMethod();

                    tmpData.Company = CurCompany;
                    tmpData.Name = dr["SHIPMTHD"].ToString() + ", " + dr["SHMTHDSC"].ToString();
                    tmpData.ErpCode = dr["SHIPMTHD"].ToString();
                    tmpData.IsFromErp = true;

                    list.Add(tmpData);

                }

                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetAllShippingMethods", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }
        }


        //PRODUCTS


        public IList<ProductCategory> GetAllProductCategories()
        {

            ProductCategory tmpData;
            IList<ProductCategory> list = new List<ProductCategory>();

            try
            {

                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("WSItemClass", false, 2, 0, "", true)); //WSUofMSchedule

                if (ds.Tables.Count == 0)
                    return null;


                //En el dataset, Tables: 1 - UOFM
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    //Solo para unidades equivalentes en unidad base
                    if (!string.IsNullOrEmpty(dr["ITMCLSCD"].ToString()))
                    {
                        //Map Properties
                        tmpData = new ProductCategory();

                        tmpData.Company = CurCompany; // WType.GetDefaultCompany();
                        tmpData.Name = dr["ITMCLSDC"].ToString().ToUpper();
                        tmpData.ErpCode = dr["ITMCLSCD"].ToString();
                        tmpData.IsFromErp = true;
                        list.Add(tmpData);
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetAllProductCategory", ListValues.EventType.Error, ex, null,
                    ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }

        }

        public IList<Product> GetAllProducts() { return GetProducts(""); }

        public IList<Product> GetProductsLastXDays(int days) {
            return GetProducts("DATEDIFF(day,MODIFDT,GETDATE()) <= " + days.ToString()); 
        }

        public IList<Product> GetProductById(string code) { return GetProducts("ITEMNMBR = '" + code + "'"); }

        public IList<Product> GetProductsSince(DateTime sinceDate)
        {
            return GetProducts(" (CREATDDT >= '" + sinceDate.ToString("yyyy-MM-dd") + "' OR MODIFDT >= '" + sinceDate.ToString("yyyy-MM-dd") + "')");
        }

        public IList<Product> GetProductsByQuery(string sWhere) { return GetProducts(sWhere); }


        private IList<Product> GetProducts(string sWhere) {


            Product tmpData = null;
            IList<Product> list = new List<Product>();
            IList<Unit> unitList = GetAllUnits();
            IList<Unit> curList = unitList;
            Unit curUnit;
            ProductCategory curCategory;
            string flag = "";


            try
            {
                //Lamar los documents que necesita del Erp usando econnect
                //sWhere = string.IsNullOrEmpty(sWhere) ? "ITEMTYPE=1" : "ITEMTYPE=1 AND " + sWhere;

                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("Item", false, 2, 0, sWhere, true));

                Console.WriteLine(ds.Tables.Count.ToString());

                if (ds.Tables.Count == 0)
                    return list;

                //Company company = WType.GetDefaultCompany();
                //Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                //En el dataset, Tables: 1 - Item
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    try
                    {
                        //Map Properties
                        tmpData = new Product();

                        flag = "BASE";

                        tmpData.Company = CurCompany;
                        tmpData.Name = dr["ITEMDESC"].ToString();
                        tmpData.ProductCode = dr["ITEMNMBR"].ToString();
                        tmpData.Description = dr["ITEMDESC"].ToString();
                        tmpData.IsKit = (dr["ITEMTYPE"].ToString() == "4") ? true : false;
                        tmpData.ErpTrackOpt = short.Parse(dr["ITMTRKOP"].ToString());

                        tmpData.Manufacturer = dr["ITMSHNAM"].ToString();
                        tmpData.Reference = dr["ITMGEDSC"].ToString();


                        try { tmpData.CountRank = short.Parse(dr["ABCCODE"].ToString()); }
                        catch { }

                        try { tmpData.ProductCost = double.Parse(dr["CURRCOST"].ToString()); }
                        catch { }

                        tmpData.Status = GetProductStatus(int.Parse(dr["ITEMTYPE"].ToString()));
                        try { tmpData.Weight = double.Parse(dr["ITEMSHWT"].ToString()) / 100; }
                        catch { }

                        //Basic Unit
                        flag = "BASEUNIT";
                        curUnit = new Unit();
                        curUnit.Company = CurCompany;
                        curUnit.ErpCodeGroup = dr["UOMSCHDL"].ToString();
                        curUnit.BaseAmount = 1;
                        tmpData.BaseUnit = WType.GetUnit(curUnit);
                        tmpData.IsFromErp = true;
                        tmpData.PrintLabel = true;


                        //Product Category
                        if (!string.IsNullOrEmpty(dr["ITMCLSCD"].ToString()))
                        {
                            flag = "CATEGORY";
                            try
                            {
                                curCategory = new ProductCategory();
                                curCategory.Company = CurCompany;
                                curCategory.ErpCode = dr["ITMCLSCD"].ToString();
                                tmpData.Category = WType.GetProductCategory(curCategory);
                            }
                            catch { }
                        }

                        //Purchase Units
                        if (!string.IsNullOrEmpty(dr["PRCHSUOM"].ToString()))
                        {
                            try
                            {
                                curUnit = new Unit();
                                curUnit.Company = CurCompany;
                                curUnit.ErpCodeGroup = dr["UOMSCHDL"].ToString();
                                curUnit.ErpCode = dr["PRCHSUOM"].ToString();
                                tmpData.PurchaseUnit = WType.GetUnit(curUnit);
                            }
                            catch { }
                        }

                        //Sale Unit
                        if (!string.IsNullOrEmpty(dr["SELNGUOM"].ToString()))
                        {
                            try
                            {

                                curUnit = new Unit();
                                curUnit.Company = CurCompany;
                                curUnit.ErpCodeGroup = dr["UOMSCHDL"].ToString();
                                curUnit.ErpCode = dr["SELNGUOM"].ToString();
                                tmpData.SaleUnit = WType.GetUnit(curUnit);
                            }
                            catch { }
                        }

                        //Obteniendo las unidades que ese producto puede tener
                        flag = "UNITLIST";
                        curList = unitList.Where(unit => unit.ErpCodeGroup == dr["UOMSCHDL"].ToString()).ToList();

                        flag = "PRODUCT_TRACK";
                        tmpData.ProductTrack = GetProductTrack(int.Parse(dr["ITMTRKOP"].ToString()), tmpData);

                        flag = "PRODUCT_UNITS";
                        tmpData.ProductUnits = GetProductUnits(tmpData, curList);

                        flag = "VENDORS";
                        tmpData.ProductAccounts = GetProductVendors(tmpData);

                        //Productos Alternos.
                        flag = "ALTERN";
                        try
                        {
                            tmpData.AlternProducts = GetAlternateProducts(tmpData, dr["ALTITEM1"].ToString().Trim(),
                                dr["ALTITEM2"].ToString().Trim());
                        }
                        catch (Exception ex) { Console.WriteLine(flag + " " + ex.Message); }

                        list.Add(tmpData);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(flag + " " + ex.Message);
                    }
                    
                }

                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetProducts" + tmpData.ProductCode + "," + flag, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }
        }

        private IList<ProductAlternate> GetAlternateProducts(Product tmpData, string item1, string item2)
        {
            IList<ProductAlternate> list = new List<ProductAlternate>();
            ProductAlternate curAlternate; 

            //Item 1
            if (!string.IsNullOrEmpty(item1))
            {
                curAlternate = new ProductAlternate();
                curAlternate.Product = tmpData;
                curAlternate.CreatedBy = WmsSetupValues.SystemUser;
                curAlternate.CreationDate = DateTime.Now;
                curAlternate.IsFromErp = true;

                try
                {
                    curAlternate.AlternProduct = WType.GetProduct(new Product { ProductCode = item1, Company = CurCompany });
                    list.Add(curAlternate);
                    //Console.WriteLine(curAlternate.AlternProduct.ProductCode);
                }
                catch { }
            }

            //Item 2
            if (!string.IsNullOrEmpty(item2))
            {
                curAlternate = new ProductAlternate();
                curAlternate.Product = tmpData;
                curAlternate.CreatedBy = WmsSetupValues.SystemUser;
                curAlternate.CreationDate = DateTime.Now;
                curAlternate.IsFromErp = true;

                try
                {
                    curAlternate.AlternProduct = WType.GetProduct(new Product { ProductCode = item2, Company = CurCompany });
                    list.Add(curAlternate);
                    //Console.WriteLine(curAlternate.AlternProduct.ProductCode);
                }
                catch { }
            }

            return list;
        }




        private Status GetProductStatus(int itemType)
        {
            //Item Types
            //1 - Sales Inventory (Active)
            //2 - Discontinued (Active)
            //3 - Misc (inactivo)
            //4 - Kit (inactivo)
            //5 - Services (inactivo)
            //6 - Flat Fees (inactivo)

            if (itemType == 1 || itemType == 2)
                 return WType.GetStatus(new Status { StatusID = EntityStatus.Active });
            else
                return WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });
        }

        private IList<ProductTrackRelation> GetProductTrack(int trackOption, Product product)
        {
            if (trackOption != 2 && trackOption != 3)
                return null;

            ProductTrackRelation pTrack = null;
            IList<ProductTrackRelation> list = new List<ProductTrackRelation>();

            if (trackOption == 2) //Serial Number
            {
                pTrack = new ProductTrackRelation
                {
                    CreatedBy = WmsSetupValues.SystemUser,
                    CreationDate = DateTime.Now,
                    DisplayName = null,
                    Product = product,
                    IsRequired = true,
                    IsUnique = true,
                    TrackOption = new TrackOption { RowID = STrackOptions.SerialNumber }
                };
                list.Add(pTrack);
            }

            if (trackOption == 3) //LotCode
            {
                pTrack = new ProductTrackRelation
                {
                    CreatedBy = WmsSetupValues.SystemUser,
                    CreationDate = DateTime.Now,
                    DisplayName = null,
                    Product = product,
                    IsRequired = true,
                    IsUnique = false,
                    TrackOption = new TrackOption { RowID = STrackOptions.LotCode }
                };
                list.Add(pTrack);
            }

            return list;

        }

        private IList<UnitProductRelation> GetProductUnits(Product product, IList<Unit> unitList)
        {
            IList<UnitProductRelation> productUnits = new List<UnitProductRelation>();
            UnitProductRelation curId;

            try
            {

                foreach (Unit e in unitList)
                {
                    curId = new UnitProductRelation();
                    curId.BaseAmount = e.BaseAmount;
                    curId.IsBasic = (e.BaseAmount == 1) ? true : false;
                    curId.Product = product;
                    curId.Unit = WType.GetUnit(e);
                    curId.UnitErpCode = e.ErpCode;
                    curId.Status = product.Status;
                    curId.CreatedBy = WmsSetupValues.SystemUser;
                    curId.CreationDate = DateTime.Now;
                    productUnits.Add(curId);
                }

                return productUnits;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetProductUnits", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;              
                return null; 
            }

        }

        private IList<ProductAccountRelation> GetProductVendors(Product product)
        {
            IList<ProductAccountRelation> productVendor = new List<ProductAccountRelation>();
            ProductAccountRelation curId = null;
            AccountType accType = WType.GetAccountType(new AccountType { AccountTypeID = 2 });

            string curItemVendor = "";

            try
            {

                //Ask for the vendor Item record
                string sWhere = " ITEMNMBR = '" + product.ProductCode + "'";
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("VendorItem", false, 2, 0, sWhere, true));

                if (ds.Tables.Count > 0)
                {

                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        curId = new ProductAccountRelation();   

                        curId.ItemNumber = dr["VNDITNUM"].ToString();

                        //Si esta vacio se sale al proximo
                        if (string.IsNullOrEmpty(curId.ItemNumber))
                            continue;

                        curItemVendor = dr["VENDORID"].ToString() + ":" + curId.ItemNumber; 
     
                        curId.IsFromErp = true;
                        curId.AccountType = accType;
                        curId.Account = WType.GetAccount(new Account { AccountCode = dr["VENDORID"].ToString(), BaseType = new AccountType { AccountTypeID = AccntType.Vendor } });
                        curId.Product = product;
                        curId.CreatedBy = WmsSetupValues.SystemUser;
                        curId.CreationDate = DateTime.Now;

                        productVendor.Add(curId);
                    }

                }

                return productVendor;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetProductVendors:" + curItemVendor, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;              
                return null;
            }

        }


        //VENDORS
        public IList<Account> GetAllVendors() { return GetVendors(""); }

        public IList<Account> GetVendorsLastXDays(int days) { return GetVendors(""); }

        public IList<Account> GetVendorById(string code) { return GetVendors("VENDORID = '" + code + "'"); }

        public IList<Account> GetVendorsSince(DateTime sinceDate)
        {
            return GetVendors(" DEX_ROW_TS >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        private IList<Account> GetVendors(string sWhere)
        {

            IList<Account> list = new List<Account>();
            Account tmpData;
            AccountTypeRelation tmpAccTypeRel;

            try
            {

                //Lamar los documents que necesita del Erp usando econnect
                //TODO: Revisar obtener solo lo modificado last X days
                ds = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("Vendor", false, 2, 0, sWhere, true));

                if (ds.Tables.Count == 0)
                    return null;

                //Company company = WType.GetDefaultCompany();
                AccountType accType = WType.GetAccountType(new AccountType {AccountTypeID = AccntType.Vendor });
                Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                //En el dataset, Tables: 1 - CustomerHeader, 2 - CustomerAddress
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    //Map Properties
                    tmpData = new Account();

                    try
                    {
                        tmpData.AccountCode = dr["VENDORID"].ToString();
                        if (string.IsNullOrEmpty(tmpData.AccountCode))
                            continue;

                        tmpData.Company = CurCompany;
                        tmpData.ContactPerson = dr["VNDCNTCT"].ToString();


                        tmpData.Name = dr["VENDNAME"].ToString();
                        tmpData.Phone = dr["PHNUMBR1"].ToString();
                        tmpData.UserDefine1 = dr["USERDEF1"].ToString();
                        tmpData.UserDefine2 = dr["USERDEF2"].ToString();

                        //Account Type
                        tmpAccTypeRel = new AccountTypeRelation();
                        tmpAccTypeRel.Account = tmpData;
                        tmpAccTypeRel.AccountType = accType;
                        tmpAccTypeRel.ErpCode = dr["VENDORID"].ToString();
                        tmpAccTypeRel.Status = status;
                        tmpAccTypeRel.CreationDate = DateTime.Now;
                        tmpAccTypeRel.CreatedBy = WmsSetupValues.SystemUser;
                        tmpData.AccountTypes = new AccountTypeRelation[] { tmpAccTypeRel };
                        tmpData.IsFromErp = true;
                        tmpData.BaseType = accType;

                        //Asignacion de Lines
                        tmpData.AccountAddresses = GetVendorAddress(tmpData,
                            ds.Tables[2].Select("VENDORID='" + dr["VENDORID"].ToString() + "'"));

                        list.Add(tmpData);

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetVendors:" + tmpData.AccountCode + "-" +
                            tmpData.Name, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                    }
                }

                //retornar la lista 
                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetVendors", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }

        }

        private IList<AccountAddress> GetVendorAddress(Account account, DataRow[] dLines)
        {

            AccountAddress tmpData;
            IList<AccountAddress> list = new List<AccountAddress>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            try
            {

                foreach (DataRow dr in dLines)
                {
                    tmpData = new AccountAddress();
                    tmpData.Account = account;
                    tmpData.Name = dr["ADRSCODE"].ToString(); //dr["VNDCNTCT"].ToString();
                    tmpData.ErpCode = dr["ADRSCODE"].ToString();
                    tmpData.AddressLine1 = dr["ADDRESS1"].ToString();
                    tmpData.AddressLine2 = dr["ADDRESS2"].ToString();
                    tmpData.AddressLine3 = dr["ADDRESS3"].ToString();
                    tmpData.City = dr["CITY"].ToString();
                    tmpData.State = dr["STATE"].ToString();
                    tmpData.ZipCode = dr["ZIPCODE"].ToString();
                    tmpData.Country = dr["COUNTRY"].ToString();
                    tmpData.Phone1 = dr["PHNUMBR1"].ToString();
                    tmpData.Phone2 = dr["PHNUMBR2"].ToString();
                    tmpData.Phone3 = dr["FAXNUMBR"].ToString();
                    tmpData.Email = dr["POEmailRecipient"].ToString();   
                    tmpData.Status = lineStatus;
                    tmpData.IsMain = false;
                    tmpData.ContactPerson = dr["VNDCNTCT"].ToString();
                    tmpData.CreationDate = DateTime.Now;
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.IsFromErp = true;

                    list.Add(tmpData);
                }

                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetVendorAddress", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }
        }



        //CUSTOMERS
        public IList<Account> GetAllCustomers() { return GetCustomers(""); }

        public IList<Account> GetCustomersLastXDays(int days) { return GetCustomers(""); }

        public IList<Account> GetCustomerById(string code) { return GetCustomers("CUSTNMBR = '" + code + "'"); }

        public IList<Account> GetCustomersSince(DateTime sinceDate)
        {
            return GetCustomers(" DEX_ROW_TS >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }

        private IList<Account> GetCustomers(string sWhere)
        {

            IList<Account> list = new List<Account>();
            Account tmpData;
            AccountTypeRelation tmpAccTypeRel;

            try
            {

                //Lamar los documents que necesita del Erp usando econnect
                //TODO: Revisar obtener solo lo modificado last X days
               ds  = DynamicsGP_ec.GetDataSet(DynamicsGP_ec.RetreiveData("Customer", false, 2, 0, sWhere, true));
               
                if (ds.Tables.Count == 0)
                   return null;

                //Company company = WType.GetDefaultCompany();
               AccountType accType = WType.GetAccountType(new AccountType { AccountTypeID = AccntType.Customer }); 
               Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                //En el dataset, Tables: 1 - CustomerHeader, 2 - CustomerAddress
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    //Map Properties
                    tmpData = new Account();

                    try
                    {
                        tmpData.AccountCode = dr["CUSTNMBR"].ToString();

                        if (string.IsNullOrEmpty(tmpData.AccountCode))
                            continue;

                        tmpData.Company = CurCompany;
                        tmpData.ContactPerson = dr["CNTCPRSN"].ToString();

                        tmpData.Name = dr["CUSTNAME"].ToString();
                        tmpData.Phone = dr["PHONE1"].ToString();
                        tmpData.UserDefine1 = dr["USERDEF1"].ToString();
                        tmpData.UserDefine2 = dr["USERDEF2"].ToString();

                        //Account Type
                        tmpAccTypeRel = new AccountTypeRelation();
                        tmpAccTypeRel.Account = tmpData;
                        tmpAccTypeRel.AccountType = accType;
                        tmpAccTypeRel.ErpCode = dr["CUSTNMBR"].ToString();
                        tmpAccTypeRel.Status = status;
                        tmpAccTypeRel.CreationDate = DateTime.Now;
                        tmpAccTypeRel.CreatedBy = WmsSetupValues.SystemUser;
                        tmpData.AccountTypes = new AccountTypeRelation[] { tmpAccTypeRel };
                        tmpData.IsFromErp = true;
                        tmpData.BaseType = accType;

                        //Asignacion de Lines
                        tmpData.AccountAddresses = GetCustomerAddress(tmpData,
                            ds.Tables[2].Select("CUSTNMBR='" + dr["CUSTNMBR"].ToString() + "'"));

                        list.Add(tmpData);

                    }
                                        
                    catch (Exception ex){
                        ExceptionMngr.WriteEvent("GetCustomers:" + tmpData.AccountCode + "-"+
                            tmpData.Name, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                    }
                }

                //retornar la lista 
                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetCustomers", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }

        }

        private IList<AccountAddress> GetCustomerAddress(Account account, DataRow[] dLines) {
            
            AccountAddress tmpData;
            IList<AccountAddress> list = new List<AccountAddress>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            try
            {

                foreach (DataRow dr in dLines)
                {
                    tmpData = new AccountAddress();
                    tmpData.Account = account;
                    tmpData.Name = dr["ADRSCODE"].ToString(); //dr["CNTCPRSN"].ToString();
                    tmpData.ErpCode = dr["ADRSCODE"].ToString();
                    tmpData.AddressLine1 = dr["ADDRESS1"].ToString();
                    tmpData.AddressLine2 = dr["ADDRESS2"].ToString();
                    tmpData.AddressLine3 = dr["ADDRESS3"].ToString();
                    tmpData.City = dr["CITY"].ToString();
                    tmpData.State = dr["STATE"].ToString();
                    tmpData.ZipCode = dr["ZIP"].ToString();
                    tmpData.Country = dr["COUNTRY"].ToString();
                    tmpData.Phone1 = dr["PHONE1"].ToString();
                    tmpData.Phone2 = dr["PHONE2"].ToString();
                    tmpData.Phone3 = dr["FAX"].ToString();
                    try { tmpData.Email = dr["Internet_Address"].ToString(); }
                    catch { }
                    tmpData.Status = lineStatus;
                    tmpData.IsMain = false;
                    tmpData.ContactPerson = dr["CNTCPRSN"].ToString(); 
                    tmpData.CreationDate = DateTime.Now;
                    tmpData.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.IsFromErp = true;

                    list.Add(tmpData);
                }

                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetCustomerAddress", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null; 
            }
        }




        #region Inventory References


        public IList<KitAssembly> GetAllKitAssembly()
        {
            return GetKitAssembly("");
        }

        public IList<KitAssembly> GetKitAssemblySince(DateTime sinceDate)
        {
            return GetKitAssembly(" MODIFDT >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        private IList<KitAssembly> GetKitAssembly(string sWhere)
        {
            //retorna la lista de Kits Headers

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                // BM00101 - KitAssemblyHeader
                //DataSet ds = ReturnDataSet("SELECT * FROM BM00101 WHERE Bill_Status = 1 A", sWhere, "BM00101", Command.Connection);
                DataSet ds = ReturnDataSet("select b.* from BM00101 b LEFT OUTER JOIN SY03900 s ON s.NOTEINDX = b.NOTEINDX  WHERE CAST(ISNULL(TXTFIELD,'') AS VARCHAR(50)) <> 'NOTINWMS'", sWhere, "BM00101", Command.Connection);
                

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                List<KitAssembly> list = new List<KitAssembly>();
                Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });


                KitAssembly tmpData = null;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {
                        //Map Properties
                        tmpData = new KitAssembly();

                        tmpData.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["ITEMNMBR"].ToString() }); ;
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString(), ErpCodeGroup = tmpData.Product.BaseUnit.ErpCodeGroup });
                        tmpData.AsmType = 2;
                        tmpData.Status = status;
                        tmpData.EfectiveDate = DateTime.Parse(dr["Effective_Date"].ToString());
                        tmpData.ObsoleteDate = DateTime.Parse(dr["Obsolete_Date"].ToString());
                        tmpData.IsFromErp = true;

                        list.Add(tmpData);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ExceptionMngr.WriteEvent("GetKitAssembly:" + tmpData.Product.ProductCode, ListValues.EventType.Error, ex, null,
                                ListValues.ErrorCategory.ErpConnection);
                        }
                        catch { }

                        //return null;
                    }

                }

                return list;


            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetKitAssembly Connection", ListValues.EventType.Error, ex, null,
                    ListValues.ErrorCategory.ErpConnection);

                return null;
            }
        }


        public IList<KitAssemblyFormula> GetKitAssemblyFormula(string sWhere)
        {
            //retorna la lista de Formulas.

            KitAssemblyFormula tmpData = null;
            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                // BM00101 - KitAssemblyHeader
                DataSet ds = ReturnDataSet("SELECT * FROM BM00111 WHERE Bill_Status = 1 AND Component_Status=1 ", sWhere, "BM00111", Command.Connection);


                if (ds == null || ds.Tables.Count == 0)
                    return null;


                List<KitAssemblyFormula> list = new List<KitAssemblyFormula>();
                Status statusOK = WType.GetStatus(new Status { StatusID = EntityStatus.Active });
                Status statusInactive = WType.GetStatus(new Status { StatusID = EntityStatus.Inactive });

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {
                        //Map Properties
                        tmpData = new KitAssemblyFormula();

                        tmpData.KitAssembly = WType.GetKitAssembly(new KitAssembly { Product = new Product { Company = CurCompany, ProductCode = dr["ITEMNMBR"].ToString() } });
                        tmpData.Status = (dr["Component_Status"].ToString() == "1") ? statusOK : statusInactive;
                        tmpData.EfectiveDate = DateTime.Parse(dr["Effective_Date"].ToString());
                        tmpData.ObsoleteDate = DateTime.Parse(dr["Obsolete_Date"].ToString());
                        tmpData.Component = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["CMPTITNM"].ToString() });
                        tmpData.Unit = WType.GetUnit(new Unit { ErpCode = dr["UOFM"].ToString(), ErpCodeGroup = tmpData.Component.BaseUnit.ErpCodeGroup });
                        tmpData.FormulaQty = Double.Parse(dr["Design_Qty"].ToString()); ;
                        tmpData.Ord = int.Parse(dr["ORD"].ToString()); ;
                        tmpData.ScrapPercent = Double.Parse(dr["Scrap_Percentage"].ToString());
                        tmpData.DirectProduct = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["ITEMNMBR"].ToString() });

                        list.Add(tmpData);

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetKitAssemblyFormula:" + tmpData.KitAssembly.Product.ProductCode + ", " + tmpData.Component.ProductCode, ListValues.EventType.Error, ex, null,
                            ListValues.ErrorCategory.ErpConnection);

                        return null;
                    }
                }


                return list;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetKitAssemblyFormula:", ListValues.EventType.Error, ex, null,
                    ListValues.ErrorCategory.ErpConnection);

                return null;
            }


        }


        #endregion


    }
}