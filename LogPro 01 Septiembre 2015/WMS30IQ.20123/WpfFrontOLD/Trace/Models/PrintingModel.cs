using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{


    public interface IPrintingModel 
    {
        
        IList<DocumentType> LabelType { get; set; }
        IList<Unit> ProductUnits { get; set; }
        //IList<Product> Products { get; set; }
        Double PrintingQuantity { get; set; }
        IList<DocumentBalance> LinesToPrint { get; set; }
        IList<Label> LabelsToPrint { get; set; }
        IList<LabelTemplate> TemplateList { get; set; }
        IList<Printer> PrinterList { get; set; }

        //ByDocument
        IList<Document> DocumentList { get; set; }
        SysUser User { get; }
        Node Node { get; set; }

        //Packing Units
        IList<Unit> PackingUnits { get; set; }
        Unit PackUnit { get; set; }
        Document Document { get; set; }
        bool? ShowOnlyPack { get; set; }
               
    }



    public class PrintingModel : BusinessEntityBase, IPrintingModel
    {

        #region IPrintingModel Members

        private IList<Printer> printerlist;
        public IList<Printer> PrinterList
        {
            get
            { return printerlist; }
            set
            {
                printerlist = value;
                OnPropertyChanged("PrinterList");
            }
        }


        private Node node;
        public Node Node
        {
            get
            {
                return node;
            }
            set
            {
                node = value;
                OnPropertyChanged("Node");
            }
        }

        private IList<DocumentType> labeltype;
        public IList<DocumentType> LabelType
        {
            get
            { return labeltype; }
            set
            {
                labeltype = value;
                OnPropertyChanged("LabelType");
            }
        }     


        public SysUser User
        {
            get { return App.curUser; }
        }     



        private IList<Unit> productunits;
        public IList<Unit> ProductUnits
        {
            get
            {
                return productunits;
            }
            set
            {
                productunits = value;
                OnPropertyChanged("ProductUnits");
            }
        }



        private Double printingquantity;
        public Double PrintingQuantity
        {
            get
            {
                return printingquantity;
            }
            set
            {
                printingquantity = value;
                OnPropertyChanged("PrintingQuantity");
            }
        }


        //private IList<Product> products;
        //public IList<Product> Products
        //{
        //    get
        //    {
        //        return products;
        //    }
        //    set
        //    {
        //        products = value;
        //        OnPropertyChanged("Products");
        //    }
        //}


        private IList<Label> labelstoprint;
        public IList<Label> LabelsToPrint
        {
            get
            {
                return labelstoprint;
            }
            set
            {
                labelstoprint = value;
                OnPropertyChanged("LabelsToPrint");
            }
        }

        private IList<DocumentBalance> linestoprint;
        public IList<DocumentBalance> LinesToPrint
        {
            get
            {
                return linestoprint;
            }
            set
            {
                linestoprint = value;
                OnPropertyChanged("LinesToPrint");
            }
        }


        private IList<LabelTemplate> templatelist;
        public IList<LabelTemplate> TemplateList
        {
            get
            { return templatelist; }
            set
            {
                templatelist = value;
                OnPropertyChanged("TemplateList");
            }
        }


        private IList<Document> documentlist;
        public IList<Document> DocumentList
        {
            get
            {
                return documentlist;
            }
            set
            {
                documentlist = value;
                OnPropertyChanged("DocumentList");
            }
        }

        private IList<Unit> packingunits;
        public IList<Unit> PackingUnits
        {
            get
            {
                return packingunits;
            }
            set
            {
                packingunits = value;
                OnPropertyChanged("PackingUnits");
            }
        }


        private Unit packunit;
        public Unit PackUnit
        {
            get
            { return packunit; }
            set
            {
                packunit = value;
                OnPropertyChanged("PackUnit");
            }
        }

        private Document _Document;
        public Document Document
        {
            get
            {
                return _Document;
            }
            set
            {
                _Document = value;
                OnPropertyChanged("Document");
            }
        }

        private bool? _ShowOnlyPack;
        public bool? ShowOnlyPack
        {
            get
            {
                return _ShowOnlyPack;
            }
            set
            {
                _ShowOnlyPack = value;
                OnPropertyChanged("ShowOnlyPack");
            }
        }

        #endregion
    }
}