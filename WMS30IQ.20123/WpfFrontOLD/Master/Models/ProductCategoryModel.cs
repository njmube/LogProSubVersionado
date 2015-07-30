using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IProductCategoryModel
    {
        SysUser User { get; }
        IList<Company> CompanyList { get; }
        ProductCategory Record { get; set; }
        IList<ProductCategory> EntityList { get; set; }

    }

    public class ProductCategoryModel: BusinessEntityBase, IProductCategoryModel
    {

        public SysUser User { get { return App.curUser; } }
        public IList<Company> CompanyList { get { return App.CompanyList; } }

        private IList<ProductCategory> entitylist;
        public IList<ProductCategory> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private ProductCategory record;
        public ProductCategory Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


    }
}
