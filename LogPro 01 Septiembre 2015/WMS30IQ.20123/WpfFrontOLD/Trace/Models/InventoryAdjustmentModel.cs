using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Collections.Specialized;
using System.Linq;

namespace WpfFront.Models
{

    public interface IInventoryAdjustmentModel 
    {
        
        IList<Unit> ProductUnits { get; set; }
        IList<Product> Products { get; set; }
        Double Quantity { get; set; }
        IList<DocumentLine> LinesToProcess { get; set; }
        Label SourceLocation { get; set; }
        //IList<ShowData> SourceData { get; set; }
        //SysUser User { get; }
        Document Document { get; set; }
        // adjustment section
        Document Adjustment { get; set; }
        IList<Document> Adjustments { get; set; }
        IList<ShowData> AdjustmentData { get; set; }
        IList<DocumentLine> AdjustmentLines { get; set; }
        IList<DocumentConcept> DocumentConcepts { get; }

        // control the serialQty to be read
        int QtySerials { get; set; }        // seriales a leer
        int QtySerialsRead { get; set; }    // seriales leidos
    }



    public class InventoryAdjustmentModel : BusinessEntityBase, IInventoryAdjustmentModel
    {

        #region IModel Members


        public IList<DocumentConcept> DocumentConcepts { 
            get { return App.DocumentConceptList.Where(f=>f.DocClass.DocClassID == SDocClass.Inventory).ToList(); } 
        }

        private Document document;
        public Document Document
        {
            get
            { return document; }
            set
            {
                document = value;
                OnPropertyChanged("Document");
            }
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



        private Double quantity;
        public Double Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                quantity = value;
                OnPropertyChanged("Quantity");
            }
        }


        private IList<Product> products;
        public IList<Product> Products
        {
            get
            {
                return products;
            }
            set
            {
                products = value;
                OnPropertyChanged("Products");
            }
        }


        private IList<DocumentLine> linestoprocess;
        public IList<DocumentLine> LinesToProcess
        {
            get
            {
                return linestoprocess;
            }
            set
            {
                linestoprocess = value;
                OnPropertyChanged("LinesToProcess");
            }
        }





        private Label sourcelocation;
        public Label SourceLocation
        {
            get
            {
                return sourcelocation;
            }
            set
            {
                sourcelocation = value;
                OnPropertyChanged("SourceLocation");
            }
        }


        private IList<Document> adjustments;
        public IList<Document> Adjustments
        {
            get
            { return adjustments; }
            set
            {
                adjustments = value;
                OnPropertyChanged("Adjustments");
            }
        }

        private IList<ShowData> adjustmentData;
        public IList<ShowData> AdjustmentData
        {
            get
            { return adjustmentData; }
            set
            {
                adjustmentData = value;
                OnPropertyChanged("AdjustmentData");
            }
        }

        private Document adjustment;
        public Document Adjustment
        {
            get
            { return adjustment; }
            set
            {
                adjustment = value;
                OnPropertyChanged("Adjustment");
            }
        }

        private IList<DocumentLine> adjustmentLines;
        public IList<DocumentLine> AdjustmentLines
        {
            get
            {
                return adjustmentLines;
            }
            set
            {
                adjustmentLines = value;
                OnPropertyChanged("AdjustmentLines");
            }
        }


        private int qtySerials;
        public int QtySerials
        {
            get
            { return qtySerials; }
            set
            {
                qtySerials = value;
                OnPropertyChanged("QtySerials");
            }
        }

        private int qtySerialsRead;
        public int QtySerialsRead
        {
            get
            { return qtySerialsRead; }
            set
            {
                qtySerialsRead = value;
                OnPropertyChanged("QtySerialsRead");
            }
        }
        #endregion
    }
}