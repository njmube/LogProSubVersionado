using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using WpfFront.Presenters;

namespace WpfFront.Models
{
    public interface IPackageAdminModel
    {
        Document Document { get; set; }
        IList<ProductStock> PackDetails2 { get; set; }
        IList<ProductStock> PackDetails1 { get; set; }
        IList<DocumentPackage> Packages { get; set; }
    }


    public class PackageAdminModel : BusinessEntityBase, IPackageAdminModel
    {

        private Document _Document;
        public Document Document
        {
            get { return _Document; }
            set
            {
                _Document = value;
                OnPropertyChanged("Document");
            }
        }



        private IList<ProductStock> _PackDetails2;
        public IList<ProductStock> PackDetails2
        {
            get { return _PackDetails2; }
            set
            {
                _PackDetails2 = value;
                OnPropertyChanged("PackDetails2");
            }
        }



        private IList<ProductStock> _PackDetails1;
        public IList<ProductStock> PackDetails1
        {
            get { return _PackDetails1; }
            set
            {
                _PackDetails1 = value;
                OnPropertyChanged("PackDetails1");
            }
        }


        private IList<DocumentPackage> _Packages;
        public IList<DocumentPackage> Packages
        {
            get { return _Packages; }
            set
            {
                _Packages = value;
                OnPropertyChanged("Packages");
            }
        }


        private IList<DocumentPackage> _Packages2;
        public IList<DocumentPackage> Packages2
        {
            get { return _Packages2; }
            set
            {
                _Packages2 = value;
                OnPropertyChanged("Packages2");
            }
        }

    }
}
