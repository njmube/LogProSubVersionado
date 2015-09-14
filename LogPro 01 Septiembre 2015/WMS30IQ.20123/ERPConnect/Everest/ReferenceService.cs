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
using System.Data.SqlClient;
using System.Data;
using Integrator;

namespace ErpConnect.Everest
{
    public class ReferenceService: SQLBase, IReferenceService
    {
        private Company CurCompany;
        private DataSet ds;
        private WmsTypes WType;
        private String Query;

        public ReferenceService(Company factoryCompany)
        {
            CurCompany = factoryCompany;
            WType = new WmsTypes();
            ds = new DataSet();
        }


        #region IReferenceService Members

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

        public IList<Entities.Master.Unit> GetAllUnits()
        {

            Unit tmpData;
            IList<Unit> list = new List<Unit>();

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = "SELECT Code, descript, 1 factor " +
                  " FROM MEASURE WHERE code = 'EA' " ;

                ds = ReturnDataSet(Query, null, "MEASURE", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                //En el dataset, Tables: 1 - UOFM
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //Solo para unidades equivalentes en unidad base
                    //if (Double.Parse(dr["factor"].ToString(), ListValues.DoubleFormat()) > 0)
                    //{
                        //Map Properties
                        tmpData = new Unit();

                        tmpData.Company = CurCompany; // WType.GetDefaultCompany();
                        tmpData.Name = dr["descript"].ToString();
                        tmpData.ErpCode = dr["Code"].ToString();
                        tmpData.ErpCodeGroup = WmsSetupValues.DEFAULT;
                        tmpData.BaseAmount = double.Parse(dr["factor"].ToString(), ListValues.DoubleFormat());
                        tmpData.IsFromErp = true;
                        list.Add(tmpData);
                    //}
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
            return GetLocations(" DW_UPDATE_DATE >= " + sinceDate.ToString("yyyy-MM-dd"));
        }

        public IList<Location> GetLocations(string sWhere)
        {

            Location tmpData;
            IList<Location> list = new List<Location>();

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);
                // CAA ?????
                Query = "select DEP_CODE,DESCRIPT,STREET_ADDRESS,CITY,STATE,ZIP,TEL1,TEL2,FAX " +
                     " from DEPART where ACTIVE ='T'";

                ds = ReturnDataSet(Query, sWhere, "DEPART", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;


                Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                //En el dataset, Tables: 1 - Locations
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //Map Properties
                    tmpData = new Location();

                    tmpData.Company = CurCompany;
                    tmpData.ErpCode = dr["DEP_CODE"].ToString();
                    tmpData.Name = dr["DESCRIPT"].ToString();
                    tmpData.AddressLine1 = dr["STREET_ADDRESS"].ToString();
                    //tmpData.AddressLine2 = dr["ADDRESS2"].ToString();
                    //tmpData.AddressLine3 = dr["ADDRESS3"].ToString();
                    tmpData.City = dr["CITY"].ToString();
                    tmpData.State = dr["STATE"].ToString();
                    tmpData.ZipCode = dr["ZIP"].ToString();
                    tmpData.Phone1 = dr["TEL1"].ToString();
                    tmpData.Phone2 = dr["TEL2"].ToString();
                    tmpData.Phone3 = dr["FAX"].ToString();
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


        public IList<ShippingMethod> GetAllShippingMethods()
        {
            ShippingMethod tmpData;
            IList<ShippingMethod> list = new List<ShippingMethod>();

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = " select DELIV_CODE, DELIV_DESC from DEL_MTHD where ACTIVE ='T' ";

                ds = ReturnDataSet(Query, "", "DEL_MTHD", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //Map Properties
                    tmpData = new ShippingMethod();

                    tmpData.Company = CurCompany;
                    tmpData.Name = dr["DELIV_DESC"].ToString();
                    tmpData.ErpCode = dr["DELIV_CODE"].ToString();
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

        public IList<Entities.Master.ProductCategory> GetAllProductCategories()
        {
            ProductCategory tmpData;
            IList<ProductCategory> list = new List<ProductCategory>();

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = "select DISTINCT c.category, c.descript from CATEGORY c JOIN items i ON c.category = i.category " +
                 " where c.ACTIVE ='T' AND i.ACTIVE ='T' " ;

                ds = ReturnDataSet(Query, "", "CATEGORY", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //if (!string.IsNullOrEmpty(dr["f149_id"].ToString()))
                    //{
                        //Map Properties
                        tmpData = new ProductCategory();

                        tmpData.Company = CurCompany;
                        tmpData.Name = dr["descript"].ToString().ToUpper();
                        tmpData.ErpCode = dr["category"].ToString();
                        tmpData.IsFromErp = true;
                        list.Add(tmpData);
                    //}
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

        public IList<Product> GetProductsLastXDays(int days)
        {
            return GetProducts("DATEDIFF(day,i.DW_UPDATE_DATE,GETDATE()) <= " + days.ToString());
        }

        public IList<Product> GetProductById(string code) { return GetProducts("i.ITEMNO = '" + code + "'"); }

        public IList<Product> GetProductsSince(DateTime sinceDate)
        {
            // CAA ???
            return GetProducts(" (i.f120_fecha_creacion >= '" + sinceDate.ToString("yyyy-MM-dd") + "' OR i.DW_UPDATE_DATE >= '" + sinceDate.ToString("yyyy-MM-dd") + "')");
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

                Query = "select i.ITEMNO,i.DESCRIPT,i.CATEGORY, i.MATRIX_ITEM_TYPE,i.SERIALIZE,i.FORCE_LOT,i.LINK_SERIAL, i.COST, 'EACH' as SALEUNIT,'EACH' as PURCHUNIT, isNull(m.DESCRIPT,'') AS Manuf " +
                   " from items i LEFT OUTER JOIN MANUFACT m ON i.MANUCODE = m.code where i.ACTIVE ='T' "; // AND m.ACTIVE ='T'

                ds = ReturnDataSet(Query, sWhere, "items", Command.Connection);

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
                        tmpData.Name = dr["DESCRIPT"].ToString();
                        tmpData.ProductCode = dr["ITEMNO"].ToString();
                        tmpData.Description = dr["DESCRIPT"].ToString();
                        tmpData.IsKit = (dr["MATRIX_ITEM_TYPE"].ToString() == "3") ? true : false;

                        // CAA ????
                        tmpData.ErpTrackOpt = IsTrackOpt(dr["FORCE_LOT"].ToString(), dr["SERIALIZE"].ToString());
                        tmpData.Manufacturer = dr["Manuf"].ToString(); 
                        // tmpData.Reference = dr["f120_rowid"].ToString();

                        /*
                        try { tmpData.CountRank = ; 
                        catch { }
                        */

                        // ???   AVG_COST ???
                        try { tmpData.ProductCost = double.Parse(dr["COST"].ToString()); }
                        catch { }

                        tmpData.Status = active;
                        //tmpData.Status = GetProductStatus(int.Parse(dr["ITEMTYPE"].ToString()));

                        /*
                        try { tmpData.Weight = double.Parse(dr["ITEMSHWT"].ToString()) / 100; }
                        catch { }
                        // ???
                        */

                        //Basic Unit             
                        flag = "BASEUNIT";
                        curUnit = new Unit();
                        curUnit.Company = CurCompany;
                        curUnit.ErpCodeGroup = WmsSetupValues.DEFAULT; 
                        curUnit.BaseAmount = 1;
                        try
                        {
                            tmpData.BaseUnit = WType.GetUnit(curUnit);
                        }
                        catch { }
                        tmpData.IsFromErp = true;
                        tmpData.PrintLabel = true;


                        //Product Category
                        if (!string.IsNullOrEmpty(dr["CATEGORY"].ToString()))
                        {
                            flag = "CATEGORY";
                            try
                            {
                                curCategory = new ProductCategory();
                                curCategory.Company = CurCompany;
                                curCategory.ErpCode = dr["CATEGORY"].ToString();
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
                            curUnit.ErpCode = dr["PURCHUNIT"].ToString();
                            tmpData.PurchaseUnit = WType.GetUnit(curUnit);
                        }
                        catch { }


                        //Sale Unit
                        try
                        {

                            curUnit = new Unit();
                            curUnit.Company = CurCompany;
                            curUnit.ErpCodeGroup = WmsSetupValues.DEFAULT;
                            curUnit.ErpCode = dr["SALEUNIT"].ToString();
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

                       // CAA ??
                       flag = "VENDORS";
                       tmpData.ProductAccounts = GetProductVendors(tmpData);

                        // CAA ???
                        //Productos Alternos.   
                        /*
                        flag = "ALTERN";
                        try
                        {
                            tmpData.AlternProducts = GetAlternateProducts(tmpData);
                        }
                        catch (Exception ex) { Console.WriteLine(flag + " " + ex.Message); }
                        */

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

        private IList<ProductAccountRelation> GetProductVendors(Product product)
        {
            IList<ProductAccountRelation> productVendor = new List<ProductAccountRelation>();
            ProductAccountRelation curId = null;
            AccountType accType = WType.GetAccountType(new AccountType { AccountTypeID = AccntType.Vendor });

            string curItemVendor = "";

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = "SELECT VEND_CODE,VENDOR_PART_NO " +
                  " FROM ITEM_REPLENISH_VENDOR WHERE ITEM_CODE = '" + product.ProductCode+"' ";

                ds = ReturnDataSet(Query, null, "ITEM_REPLENISH_VENDOR", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    curId = new ProductAccountRelation();

                    curId.ItemNumber = dr["VENDOR_PART_NO"].ToString();

                    //Si esta vacio se sale al proximo
                    if (string.IsNullOrEmpty(curId.ItemNumber))
                        continue;

                    curItemVendor = dr["VEND_CODE"].ToString() + ":" + curId.ItemNumber;

                    curId.IsFromErp = true;
                    curId.AccountType = accType;
                    curId.Account = WType.GetAccount(new Account { AccountCode = dr["VEND_CODE"].ToString(), BaseType = new AccountType { AccountTypeID = AccntType.Vendor } });
                    curId.Product = product;
                    curId.CreatedBy = WmsSetupValues.SystemUser;
                    curId.CreationDate = DateTime.Now;

                    productVendor.Add(curId);
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

        private IList<ProductAlternate> GetAlternateProducts(Product tmpData)
        {
            IList<ProductAlternate> list = new List<ProductAlternate>();
            ProductAlternate curAlternate;

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                // items equivalentes asociados al item en 
                Query = "SELECT f128_rowid_item_equivalente " +
                  " FROM t128_mc_items_equivalentes WHERE f128_rowid_item = " + tmpData.Reference +
                  " AND f128_id_cia =" + CurCompany.ErpCode;

                ds = ReturnDataSet(Query, null, "t128_mc_items_equivalentes", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    curAlternate = new ProductAlternate();
                    curAlternate.Product = tmpData;
                    curAlternate.CreatedBy = WmsSetupValues.SystemUser;
                    curAlternate.CreationDate = DateTime.Now;
                    curAlternate.IsFromErp = true;
                    curAlternate.AlternProduct = WType.GetProduct(new Product { Reference = row["f128_rowid_item_equivalente"].ToString(), Company = CurCompany });
                    list.Add(curAlternate);
                }

            }
            catch { }
            return list;
        }


        private short IsTrackOpt(string lote, string serial)
        {
            if (!string.IsNullOrEmpty(serial) && serial.Equals("T"))
                return 2;
            else
                if (!string.IsNullOrEmpty(lote) && lote.Equals("T"))
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


        //VENDORS
        public IList<Account> GetAllVendors() { return GetVendors(""); }

        public IList<Account> GetVendorsLastXDays(int days) { return GetVendors(""); }

        public IList<Account> GetVendorById(string code) { return GetVendors("v.VEND_CODE = '" + code + "'"); }

        public IList<Account> GetVendorsSince(DateTime sinceDate)
        {
            return GetVendors(" v.CREAT_DATE >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        private IList<Account> GetVendors(string sWhere)
        {

            IList<Account> list = new List<Account>();
            Account tmpData;
            AccountTypeRelation tmpAccTypeRel;

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = " select v.VEND_CODE, v.NAME, v.CONTCODE, IsNull(a.first_name+' '+a.last_name,a.name) as CONTNAME " +
                        " from VENDORS v LEFT OUTER JOIN ADDRESS a ON v.CONTCODE = a.ADDR_CODE WHERE v.ACTIVE ='T' ";

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

                    tmpData.AccountCode = dr["VEND_CODE"].ToString();
                    if (string.IsNullOrEmpty(tmpData.AccountCode))
                        continue;

                    tmpData.Company = CurCompany;
                    tmpData.ContactPerson = dr["CONTNAME"].ToString();


                    tmpData.Name = dr["NAME"].ToString();
                    // tmpData.Phone = dr["PHNUMBR1"].ToString();
                    // tmpData.UserDefine1 = dr["f200_nit"].ToString();
                    // tmpData.UserDefine2 = dr["USERDEF2"].ToString();

                    //Account Type
                    tmpAccTypeRel = new AccountTypeRelation();
                    tmpAccTypeRel.Account = tmpData;
                    tmpAccTypeRel.AccountType = accType;
                    tmpAccTypeRel.ErpCode = dr["VEND_CODE"].ToString();
                    tmpAccTypeRel.Status = status;
                    tmpAccTypeRel.CreationDate = DateTime.Now;
                    tmpAccTypeRel.CreatedBy = WmsSetupValues.SystemUser;
                    tmpData.AccountTypes = new AccountTypeRelation[] { tmpAccTypeRel };
                    tmpData.IsFromErp = true;
                    tmpData.BaseType = accType;

                    //Asignacion de Lines.... Datos del contacto del vendor (proveedor)
                    // caa ???  info de ADDRESS ? 
                    // tmpData.AccountAddresses = GetVendorAddress(tmpData, dr["f200_rowid"].ToString());

                    list.Add(tmpData);
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

                Query = "select p.f202_id_sucursal, p.f202_descripcion_sucursal, c.f015_direccion1, c.f015_direccion2, c.f015_direccion3, c.f015_telefono, " +
                        " c.f015_cod_postal, c.f015_email, pa.f011_descripcion pais, d.f012_descripcion dpto, city.f013_descripcion city " +
                        "   from t202_mm_proveedores p JOIN dbo.t015_mm_contactos c ON p.f202_rowid_contacto = c.f015_rowid " +
                        "     LEFT OUTER JOIN dbo.t011_mm_paises pa ON c.f015_id_pais = pa.f011_id " +
                        "     LEFT OUTER JOIN dbo.t012_mm_deptos d ON c.f015_id_depto = d.f012_id AND c.f015_id_pais = d.f012_id_pais " +
                        "     LEFT OUTER JOIN dbo.t013_mm_ciudades city ON c.f015_id_ciudad = city.f013_id AND c.f015_id_pais = city.f013_id_pais AND c.f015_id_depto = city.f013_id_depto " +
                        "   where p.f202_rowid_tercero= " + personId + " AND p.f202_id_cia = " + CurCompany.ErpCode + " AND c.f015_id_cia = " + CurCompany.ErpCode;

                ds = ReturnDataSet(Query, "", "t015_mm_contactos", Command.Connection);

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
            Account tmpData;
            AccountTypeRelation tmpAccTypeRel;

            try
            {
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                // CAA ???  CUST ???
                Query = "select DISTINCT t.f200_rowid, t.f200_id, t.f200_nit, t.f200_razon_social, c.f015_contacto " +
                    " from dbo.t200_mm_terceros t JOIN t201_mm_clientes cli ON t.f200_rowid = cli.f201_rowid_tercero AND cli.f201_id_cia = " + CurCompany.ErpCode +
                    "   LEFT OUTER JOIN t015_mm_contactos c ON t.f200_rowid_contacto = c.f015_rowid " +
                    " WHERE t.f200_ind_cliente = 1 AND t.f200_id_cia=" + CurCompany.ErpCode + " AND c.f015_id_cia = " + CurCompany.ErpCode;

                ds = ReturnDataSet(Query, sWhere, "f200_ind_cliente", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                //Company company = WType.GetDefaultCompany();
                AccountType accType = WType.GetAccountType(new AccountType { AccountTypeID = AccntType.Customer });
                Status status = WType.GetStatus(new Status { StatusID = EntityStatus.Active });

                foreach (DataRow dr in ds.Tables[0].Rows)
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

                Query = "select cli.f201_id_sucursal, cli.f201_descripcion_sucursal, c.f015_direccion1, c.f015_direccion2, c.f015_direccion3, c.f015_telefono, " +
                        " c.f015_cod_postal, c.f015_email, p.f011_descripcion pais, d.f012_descripcion dpto, city.f013_descripcion city " +
                        "   from t201_mm_clientes cli JOIN dbo.t015_mm_contactos c ON cli.f201_rowid_contacto = c.f015_rowid " +
                        "     LEFT OUTER JOIN dbo.t011_mm_paises p ON c.f015_id_pais = p.f011_id " +
                        "     LEFT OUTER JOIN dbo.t012_mm_deptos d ON c.f015_id_depto = d.f012_id AND c.f015_id_pais = d.f012_id_pais " +
                        "     LEFT OUTER JOIN dbo.t013_mm_ciudades city ON c.f015_id_ciudad = city.f013_id AND c.f015_id_pais = city.f013_id_pais AND c.f015_id_depto = city.f013_id_depto " +
                        "   where cli.f201_rowid_tercero= " + personId + " AND cli.f201_id_cia = " + CurCompany.ErpCode + " AND c.f015_id_cia = " + CurCompany.ErpCode;

                ds = ReturnDataSet(Query, "", "t015_mm_contactos", Command.Connection);

                if (ds == null || ds.Tables.Count == 0)
                    return null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    tmpData = new AccountAddress();
                    tmpData.Account = account;
                    tmpData.Name = dr["f201_id_sucursal"].ToString();
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


        #region Inventory References


        public IList<KitAssembly> GetAllKitAssembly()
        {
            return GetKitAssembly("");
        }

        public IList<KitAssembly> GetKitAssemblySince(DateTime sinceDate)
        {
            // caa ???
            return GetKitAssembly("  >= '" + sinceDate.ToString("yyyy-MM-dd") + "'");
        }


        private IList<KitAssembly> GetKitAssembly(string sWhere)
        {
            //retorna la lista de Kits Headers

            try
            {

                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                Query = " select DISTINCT KIT_CODE from X_KIT WHERE DESC_TYPE=1 ";
                DataSet ds = ReturnDataSet(Query, sWhere, "X_KIT", Command.Connection);


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

                        tmpData.Product = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["KIT_CODE"].ToString() }); ;
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

                Query = "select KIT_CODE, ITEM_CODE, ITEM_QTY from X_KIT WHERE DESC_TYPE=1  ";
                DataSet ds = ReturnDataSet(Query, sWhere, "X_KIT", Command.Connection);

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

                        tmpData.KitAssembly = WType.GetKitAssembly(new KitAssembly { Product = new Product { Company = CurCompany, ProductCode = dr["KIT_CODE"].ToString() } });
                        tmpData.Status = statusOK;
                        tmpData.EfectiveDate = DateTime.Now;
                        tmpData.ObsoleteDate = DateTime.Now;
                        tmpData.Component = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["ITEM_CODE"].ToString() });
                        tmpData.Unit = tmpData.Component.BaseUnit;
                        tmpData.FormulaQty = Double.Parse(dr["ITEM_QTY"].ToString()); ;
                        tmpData.Ord = 1;
                        tmpData.ScrapPercent = 0;
                        tmpData.DirectProduct = WType.GetProduct(new Product { Company = CurCompany, ProductCode = dr["KIT_CODE"].ToString() });

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


        #endregion
    }
}
