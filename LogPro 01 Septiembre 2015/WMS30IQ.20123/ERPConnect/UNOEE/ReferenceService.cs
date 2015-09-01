using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErpConnect;
using Entities;
using Entities.Trace;
using Entities.General;
using Entities.Master;
using Entities.Profile;
using System.Xml;
using System.Data.SqlClient;
using System.Data;
using Integrator;
using System.Globalization;

namespace ErpConnect.UNOEE
{
    class ReferenceService : SQLBase, IReferenceService
    {
        private Company CurCompany;
        private DataSet ds;
        private WmsTypes WType;
        private String Query;
        private IList<ConnectionErpSetup> ErpSetup;
        private String Separator;

        public ReferenceService(Company factoryCompany)
        {
            CurCompany = factoryCompany;
            WType = new WmsTypes();
            ds = new DataSet();

            if (ErpSetup == null)
                ErpSetup = WType.GetConnectionErpSetup(new ConnectionErpSetup
                {
                    EntityType = CnnEntityType.References,
                    ConnectionTypeID =  CnnType.UnoEE 
                });

            Separator = GetErpQuery("SEPARATOR");

        }





        public void TestConnection(Entities.Master.Company company)
        {
            try
            {
                SqlConnection gpConnect = new SqlConnection(company.ErpConnection.CnnString);
                gpConnect.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private string GetErpQuery(string entityCode)
        {
            try
            {
                return  ErpSetup.Where(f => f.EntityCode == entityCode)
                    .First().QueryString.Replace("__CIA", CurCompany.CompanyID.ToString());
            }
            catch { throw new Exception("Erp Setup Query not defined for " + entityCode); }
        }


        public IList<Unit> GetAllUnits()
        {

            Unit tmpData;
            IList<Unit> list = new List<Unit>();

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("UNITS");

                ds = ReturnDataSet(Query, null, "UNITS", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;


                //En el dataset, Tables: 1 - UOFM
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //Solo para unidades equivalentes en unidad base
                    if (Double.Parse(dr["factor"].ToString(), new NumberFormatInfo { NumberDecimalSeparator = Separator }) > 0)
                    {
                        //Map Properties
                        tmpData = new Unit();

                        tmpData.Company = CurCompany; // WType.GetDefaultCompany();
                        tmpData.Name = dr["f101_descripcion"].ToString();
                        tmpData.ErpCode = dr["f101_id"].ToString();
                        tmpData.ErpCodeGroup = WmsSetupValues.DEFAULT;
                        tmpData.BaseAmount = double.Parse(dr["factor"].ToString(), new NumberFormatInfo { NumberDecimalSeparator = Separator });
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


        public IList<Location> GetAllLocations()
        {
            return GetLocations("");
        }

        public IList<Location> GetLocationsSince(DateTime sinceDate)
        {
            return GetLocations(" f150_ts >= " + sinceDate.ToString("yyyy-MM-dd"));
        }


        public IList<Location> GetLocations(string sWhere)
        {

            Location tmpData;
            IList<Location> list = new List<Location>();

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("LOCATIONS");

                ds = ReturnDataSet(Query, sWhere, "LOCATIONS", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;


                Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                //En el dataset, Tables: 1 - Locations
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //Map Properties
                    tmpData = new Location();

                    tmpData.Company = CurCompany;
                    tmpData.ErpCode = dr["f150_id"].ToString();
                    tmpData.Name = dr["f150_descripcion"].ToString();
                    tmpData.AddressLine1 = dr["f157_descripcion"].ToString();
                    tmpData.BatchNo = dr["f150_rowid"].ToString();
                    /*
                    tmpData.AddressLine2 = dr["ADDRESS2"].ToString();
                    tmpData.AddressLine3 = dr["ADDRESS3"].ToString();
                    tmpData.City = dr["CITY"].ToString();
                    tmpData.State = dr["STATE"].ToString();
                    tmpData.ZipCode = dr["ZIPCODE"].ToString();
                    tmpData.Phone1 = dr["PHONE1"].ToString();
                    tmpData.Phone2 = dr["PHONE2"].ToString();
                    tmpData.Phone3 = dr["FAXNUMBR"].ToString();
                    //tmpData.ContactPerson = dr[""].ToString();
                     */
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


        public IList<ShippingMethod> GetAllShippingMethods()
        {
            //ShippingMethod tmpData;
            IList<ShippingMethod> list = new List<ShippingMethod>();

            try
            {
                /*
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = "";

                ds = ReturnDataSet(Query, "", "", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                //En el dataset, Tables: 1 - Methods
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //Map Properties
                    tmpData = new ShippingMethod();

                    tmpData.Company = CurCompany;
                    tmpData.Name = dr["SHIPMTHD"].ToString() + ", " + dr["SHMTHDSC"].ToString();
                    tmpData.ErpCode = dr["SHIPMTHD"].ToString();
                    tmpData.IsFromErp = true;

                    list.Add(tmpData);

                }
                */
                return list;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("GetAllShippingMethods", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                //throw;
                return null;
            }
        }


        public IList<Entities.Master.ProductCategory> GetAllProductCategories()
        {
            ProductCategory tmpData;
            IList<ProductCategory> list = new List<ProductCategory>();

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("CATEGORIES");

                ds = ReturnDataSet(Query, "", "CATEGORIES", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (!string.IsNullOrEmpty(dr["f149_id"].ToString()))
                    {
                        //Map Properties
                        tmpData = new ProductCategory();

                        tmpData.Company = CurCompany; 
                        tmpData.Name = dr["f149_descripcion"].ToString().ToUpper();
                        tmpData.ErpCode = dr["f149_id"].ToString();
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
            return GetProducts("DATEDIFF(day,i.f120_fecha_actualizacion,GETDATE()) <= " + days.ToString()); 
        }

        public IList<Product> GetProductById(string code) { return GetProducts("i.f120_id = " + code); }

        
        public IList<Product> GetProductsSince(DateTime sinceDate)
        {
            return GetProducts(" (i.f120_fecha_creacion >= '" + sinceDate.ToString("yyyy-MM-dd") + "' OR i.f120_fecha_actualizacion >= '" + sinceDate.ToString("yyyy-MM-dd") + "')");
        }

        public IList<Product> GetProductsByQuery(string sWhere) { return GetProducts(sWhere); }


        private IList<Product> GetProducts(string sWhere)
        {
            Product tmpData = null;
            IList<Product> list = new List<Product>();
            IList<Unit> unitList = GetAllUnits();
            IList<Unit> curList = unitList;
            Unit curUnit;
            ProductCategory curCategory;
            string flag = "";
            Status active = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("ITEMS");

                ds = ReturnDataSet(Query, sWhere, "ITEMS", Command.Connection);

                Console.WriteLine(ds.Tables.Count.ToString());

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {
                        //Map Properties
                        tmpData = new Product();

                        flag = "BASE";

                        tmpData.Company = CurCompany;

                        //ID de la tabla t120_item
                        tmpData.ProductCode = dr["f120_id"].ToString();
                        tmpData.Name = dr["f120_referencia"].ToString().Trim() + " - " + dr["f120_descripcion_corta"].ToString();
                        tmpData.Description = dr["f120_descripcion"].ToString();
                        tmpData.IsKit = (dr["f120_ind_tipo_item"].ToString() == "3") ? true : false;

                        tmpData.ErpTrackOpt = IsTrackOpt(dr["f120_ind_lote"].ToString(), dr["f120_ind_serial"].ToString());
                        tmpData.Manufacturer = "";

                        //ID de la tabla t121_item_ext
                        tmpData.UpcCode = dr["extid"].ToString();
                        tmpData.Reference = dr["f120_referencia"].ToString();


                        tmpData.Status = active;

                        //Basic Unit             
                        flag = "BASEUNIT";
                        curUnit = new Unit();
                        curUnit.Company = CurCompany;
                        curUnit.ErpCodeGroup = WmsSetupValues.DEFAULT; //dr["f120_id_unidad_inventario"].ToString();
                        curUnit.BaseAmount = 1;
                        try
                        {
                            tmpData.BaseUnit = WType.GetUnit(curUnit);
                        }
                        catch { }
                        tmpData.IsFromErp = true;
                        tmpData.PrintLabel = true;


                        //Product Category
                        if (!string.IsNullOrEmpty(dr["f120_id_tipo_inv_serv"].ToString()))
                        {
                            flag = "CATEGORY";
                            try
                            {
                                curCategory = new ProductCategory();
                                curCategory.Company = CurCompany;
                                curCategory.ErpCode = dr["f120_id_tipo_inv_serv"].ToString();
                                tmpData.Category = WType.GetProductCategory(curCategory);
                            }
                            catch { }
                        }
                        
                        //Purchase Units
                            try
                            {

                                curUnit = new Unit();
                                curUnit.Company = CurCompany;
                                curUnit.ErpCodeGroup = WmsSetupValues.DEFAULT;
                                curUnit.ErpCode = dr["f120_id_unidad_inventario"].ToString();
                                tmpData.PurchaseUnit  = WType.GetUnit(curUnit);
                            }
                            catch { }
                        

                        //Sale Unit
                            try
                            {

                                curUnit = new Unit();
                                curUnit.Company = CurCompany;
                                curUnit.ErpCodeGroup = WmsSetupValues.DEFAULT;
                                curUnit.ErpCode = dr["f120_id_unidad_orden"].ToString();
                                tmpData.SaleUnit = WType.GetUnit(curUnit);
                            }
                            catch { }
                       
                        //Obteniendo las unidades que ese producto puede tener
                        flag = "UNITLIST";   
                        curList = unitList;  // .Where(unit => unit.ErpCodeGroup == dr["f120_id_unidad_inventario"].ToString()).ToList();

                        flag = "PRODUCT_TRACK";
                        tmpData.ProductTrack = GetProductTrack(tmpData.ErpTrackOpt, tmpData);

                        flag = "PRODUCT_UNITS";
                        tmpData.ProductUnits = GetProductUnits(tmpData, curList);

                         /*
                        flag = "VENDORS";
                        tmpData.ProductAccounts = GetProductVendors(tmpData);
                        */

                        //Productos Alternos.   
                        flag = "ALTERN";
                        try
                        {
                            tmpData.AlternProducts = GetAlternateProducts(tmpData);
                        }
                        catch (Exception ex) { Console.WriteLine(flag + " " + ex.Message); }

                        list.Add(tmpData);
                    }
                    catch (Exception ex)
                    {
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


        private IList<ProductAlternate> GetAlternateProducts(Product tmpData)
        {
            IList<ProductAlternate> list = new List<ProductAlternate>();
            ProductAlternate curAlternate;


                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("ITEM_EQUIVALENCE").Replace("__ITEM",tmpData.ProductCode);

                ds = ReturnDataSet(Query, null, "ITEM_EQUIVALENCE", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    try
                    {
                        curAlternate = new ProductAlternate();
                        curAlternate.Product = tmpData;
                        curAlternate.CreatedBy = WmsSetupValues.SystemUser;
                        curAlternate.CreationDate = DateTime.Now;
                        curAlternate.IsFromErp = true;
                        curAlternate.AlternProduct = WType.GetProduct(new Product { ProductCode = row["f128_rowid_item_equivalente"].ToString(), Company = CurCompany });
                        list.Add(curAlternate);
                    }
                    catch { }
                }


            return list;
        }


        private short IsTrackOpt(string lote, string serial)
        {
            if (!string.IsNullOrEmpty(serial) && serial.Equals("1"))
                return 2;
            else
                if (!string.IsNullOrEmpty(lote) && lote.Equals("1"))
                    return 3;
                else
                    return 1;
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

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("ITEM_UNITS").Replace("__ITEM", product.ProductCode);

                ds = ReturnDataSet(Query, null, "ITEM_UNITS", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                Unit e;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    e = unitList.Where(f => f.ErpCode == row["f101_id"].ToString()).First();

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


        //VENDORS
        public IList<Account> GetAllVendors() { return GetVendors(""); }


        public IList<Account> GetVendorsLastXDays(int days) { return GetVendors(""); }


        public IList<Account> GetVendorById(string code) { return GetVendors("t.f200_rowid = '" + code + "'"); }


        public IList<Account> GetVendorsSince(DateTime sinceDate)
        {
            return GetVendors(" p.f202_fecha_ingreso >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        private IList<Account> GetVendors(string sWhere)
        {

            IList<Account> list = new List<Account>();
            Account tmpData;
            AccountTypeRelation tmpAccTypeRel;

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("VENDORS");

                ds = ReturnDataSet(Query, sWhere, "VENDORS", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;


                //Company company = WType.GetDefaultCompany();
                AccountType accType = WType.GetAccountType(new AccountType { AccountTypeID = AccntType.Vendor });
                Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //Map Properties
                    tmpData = new Account();


                    try
                    {
                        tmpData.AccountCode = dr["f200_id"].ToString();
                        if (string.IsNullOrEmpty(tmpData.AccountCode))
                            continue;

                        tmpData.Company = CurCompany;
                        tmpData.ContactPerson = dr["f015_contacto"].ToString();


                        tmpData.Name = dr["f200_razon_social"].ToString();
                        //tmpData.Phone = dr["PHNUMBR1"].ToString();
                        tmpData.UserDefine1 = dr["f200_nit"].ToString();
                        tmpData.UserDefine2 = dr["f202_id_sucursal"].ToString();

                        //Account Type
                        tmpAccTypeRel = new AccountTypeRelation();
                        tmpAccTypeRel.Account = tmpData;
                        tmpAccTypeRel.AccountType = accType;
                        tmpAccTypeRel.ErpCode = dr["f200_rowid"].ToString();
                        tmpAccTypeRel.Status = status;
                        tmpAccTypeRel.CreationDate = DateTime.Now;
                        tmpAccTypeRel.CreatedBy = WmsSetupValues.SystemUser;
                        tmpData.AccountTypes = new AccountTypeRelation[] { tmpAccTypeRel };
                        tmpData.IsFromErp = true;
                        tmpData.BaseType = accType;

                        //Asignacion de Lines.... Datos del contacto del vendor (proveedor)
                        tmpData.AccountAddresses = GetVendorAddress(tmpData, dr["f200_rowid"].ToString());

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


        private IList<AccountAddress> GetVendorAddress(Account account, string personId)
        {

            AccountAddress tmpData;
            IList<AccountAddress> list = new List<AccountAddress>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("VENDOR_ADDRESS").Replace("__ACCOUNT",personId);

                ds = ReturnDataSet(Query, "", "VENDOR_ADDRESS", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tmpData = new AccountAddress();
                    tmpData.Account = account;
                    tmpData.Name = dr["f202_descripcion_sucursal"].ToString();
                    tmpData.ErpCode = dr["f202_id_sucursal"].ToString();
                    tmpData.AddressLine1 = dr["f015_direccion1"].ToString();
                    tmpData.AddressLine2 = dr["f015_direccion2"].ToString();
                    tmpData.AddressLine3 = dr["f015_direccion3"].ToString();
                    tmpData.City = dr["city"].ToString();
                    tmpData.State = dr["dpto"].ToString();
                    tmpData.ZipCode = dr["f015_cod_postal"].ToString();
                    tmpData.Country = dr["pais"].ToString();
                    tmpData.Phone1 = dr["f015_telefono"].ToString();
                    //tmpData.Phone2 = dr["PHNUMBR2"].ToString();
                    //tmpData.Phone3 = dr["FAXNUMBR"].ToString();
                    tmpData.Email = dr["f015_email"].ToString();
                    tmpData.Status = lineStatus;
                    tmpData.IsMain = false;
                    tmpData.ContactPerson = account.ContactPerson;
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


        public IList<Account> GetCustomerById(string code) { return GetCustomers(" t.f200_rowid = '" + code + "'"); }


        public IList<Account> GetCustomersSince(DateTime sinceDate)
        {
            return GetCustomers(" cli.f201_fecha_ingreso >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }

        private IList<Account> GetCustomers(string sWhere)
        {

            IList<Account> list = new List<Account>();
            Account tmpData = null;
            AccountTypeRelation tmpAccTypeRel;

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("CUSTOMERS");

                ds = ReturnDataSet(Query, sWhere, "f200_ind_cliente", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                //Company company = WType.GetDefaultCompany();
                AccountType accType = WType.GetAccountType(new AccountType { AccountTypeID = AccntType.Customer });
                Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    try
                    {

                        //Map Properties
                        tmpData = new Account();
                        tmpData.AccountCode = dr["f200_id"].ToString();

                        if (string.IsNullOrEmpty(tmpData.AccountCode))
                            continue;

                        tmpData.Company = CurCompany;
                        tmpData.ContactPerson = dr["f015_contacto"].ToString();

                        tmpData.Name = dr["f200_razon_social"].ToString();
                        //tmpData.Phone = dr["PHONE1"].ToString();
                        tmpData.UserDefine1 = dr["f200_nit"].ToString();
                        //tmpData.UserDefine2 = dr["USERDEF2"].ToString();

                        //Account Type
                        tmpAccTypeRel = new AccountTypeRelation();
                        tmpAccTypeRel.Account = tmpData;
                        tmpAccTypeRel.AccountType = accType;
                        tmpAccTypeRel.ErpCode = dr["f200_rowid"].ToString();
                        tmpAccTypeRel.Status = status;
                        tmpAccTypeRel.CreationDate = DateTime.Now;
                        tmpAccTypeRel.CreatedBy = WmsSetupValues.SystemUser;
                        tmpData.AccountTypes = new AccountTypeRelation[] { tmpAccTypeRel };
                        tmpData.IsFromErp = true;
                        tmpData.BaseType = accType;

                        //Asignacion de Lines     
                        tmpData.AccountAddresses = GetCustomerAddress(tmpData, dr["f200_rowid"].ToString());

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


        private IList<AccountAddress> GetCustomerAddress(Account account, string personId)
        {

            AccountAddress tmpData;
            IList<AccountAddress> list = new List<AccountAddress>();
            Status lineStatus = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("CUSTOMER_ADDRESS").Replace("__ACCOUNT", personId);

                ds = ReturnDataSet(Query, "", "t015_mm_contactos", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tmpData = new AccountAddress();
                    tmpData.Account = account;
                    tmpData.Name = dr["f201_descripcion_sucursal"].ToString();
                    tmpData.ErpCode = dr["f201_id_sucursal"].ToString();
                    tmpData.AddressLine1 = dr["f015_direccion1"].ToString();
                    tmpData.AddressLine2 = dr["f015_direccion2"].ToString();
                    tmpData.AddressLine3 = dr["f015_direccion3"].ToString();
                    tmpData.City = dr["city"].ToString();
                    tmpData.State = dr["dpto"].ToString();
                    tmpData.ZipCode = dr["f015_cod_postal"].ToString();
                    tmpData.Country = dr["pais"].ToString();
                    tmpData.Phone1 = dr["f015_telefono"].ToString();
                    //tmpData.Phone2 = dr["PHNUMBR2"].ToString();
                    //tmpData.Phone3 = dr["FAXNUMBR"].ToString();
                    tmpData.Email = dr["f015_email"].ToString();
                    tmpData.Status = lineStatus;
                    tmpData.IsMain = false;
                    tmpData.ContactPerson = account.ContactPerson;
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

        
 

        public IList<KitAssembly> GetAllKitAssembly()
        {
            return GetKitAssembly("");
        }


        public IList<KitAssembly> GetKitAssemblySince(DateTime sinceDate)
        {
            return GetKitAssembly(" f134_ts >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        private IList<KitAssembly> GetKitAssembly(string sWhere)
        {
            //retorna la lista de Kits Headers

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = GetErpQuery("KITS");

                DataSet ds = ReturnDataSet(Query, sWhere, "KITS", Command.Connection);


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

                        tmpData.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["f120_id"].ToString() }); ;
                        tmpData.Unit = tmpData.Product.BaseUnit; 
                        tmpData.AsmType = 2;
                        tmpData.Status = status;
                        tmpData.EfectiveDate = DateTime.Now; 
                        tmpData.ObsoleteDate = DateTime.Now;
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

                Query = GetErpQuery("KIT_FORMULA");

                DataSet ds = ReturnDataSet(Query, sWhere, "KIT_FORMULA", Command.Connection);


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

                        tmpData.KitAssembly = WType.GetKitAssembly(new KitAssembly { Product = new Product { Company = CurCompany, ProductCode = dr["id_padre"].ToString() } });
                        tmpData.Status = statusOK; 
                        tmpData.EfectiveDate = DateTime.Now; 
                        tmpData.ObsoleteDate = DateTime.Now; 
                        tmpData.Component = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["id_hijo"].ToString() });
                        tmpData.Unit = tmpData.Component.BaseUnit; 
                        tmpData.FormulaQty = Double.Parse(dr["cantidad"].ToString()); ;
                        tmpData.Ord = 1; 
                        tmpData.ScrapPercent = 0;
                        tmpData.DirectProduct = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["id_padre"].ToString() });

                        list.Add(tmpData);

                    }
                    catch (Exception ex)
                    {
                        ExceptionMngr.WriteEvent("GetKitAssemblyFormula: Kit=>" + tmpData.KitAssembly.Product.ProductCode + ", Component=>" + tmpData.Component.ProductCode, ListValues.EventType.Error, ex, null,
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




    }
}
