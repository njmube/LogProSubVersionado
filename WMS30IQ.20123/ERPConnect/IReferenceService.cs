using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Master;
using Entities.General;

namespace ErpConnect
{
    /// <summary>
    /// Interfase de Referencias a implemetar por la factory
    /// </summary>
    public interface IReferenceService
    {
        void TestConnection(Company company);

        IList<Unit> GetAllUnits();

        IList<Location> GetAllLocations();
        IList<Location> GetLocationsSince(DateTime sinceDate);

        IList<ShippingMethod> GetAllShippingMethods();

        IList<ProductCategory> GetAllProductCategories();
        IList<Product> GetAllProducts();
        IList<Product> GetProductsLastXDays(int days);
        IList<Product> GetProductById(string code);
        IList<Product> GetProductsSince(DateTime sinceDate);
        IList<Product> GetProductsByQuery(string sWhere);


        IList<Account> GetAllVendors();
        IList<Account> GetVendorsLastXDays(int days);
        IList<Account> GetVendorById(string code);
        IList<Account> GetVendorsSince(DateTime sinceDate);


        IList<Account> GetAllCustomers();
        IList<Account> GetCustomersLastXDays(int days);
        IList<Account> GetCustomerById(string code);
        IList<Account> GetCustomersSince(DateTime sinceDate);


        //Kit Assembly
        IList<KitAssembly> GetAllKitAssembly();
        IList<KitAssembly> GetKitAssemblySince(DateTime sinceDate);
        IList<KitAssemblyFormula> GetKitAssemblyFormula(string sWhere);

    }
}
