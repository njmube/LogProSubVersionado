using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using System.Linq;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Windows;

namespace WpfFront.Models
{

    public interface IShippingModel 
    {
        
        IList<Document> DocumentList { get; set; }
        IList<Document> PendingDocumentList { get; set; }
        IList<DocumentLine> DocumentLines { get; set; }
        IList<Product> DocProducts { get; set; }
        IList<Unit> ProductUnits { get; set; }
        //IList<Unit> PackingUnits { get; set; }
        //IList<Account> CustomerList { get; set; }

        IList<DocumentBalance> DocumentBalance { get; set; }
        IList<DocumentBalance> PendingToPostList { get; set; }

        IList<Label> LabelsAvailable { get; set; }
        IList<Status> DocStatus { get; set; }


        Node Node {get; set;}
        Double ShippingQuantity { get; set; }
        Document Document { get; set; }
        Product Product { get; set; }
        IList<ShowData> DocumentData { get; set; }
        SysUser User { get; }
        Label CurScanedLabel { get; set; }
        IList<ProductTrackRelation> TrackData { get; set; }
        IList<Label> ManualTrackList { get; set; }

        Int32 RemainingQty { get; set; }
        Label BinLabel { get; set; }
        IList<ProductStock> ProductBinStock { get; set; }

        Int32 CurQtyPending { get; set; } //Picking Manual
        Int32 MaxTrackQty { get; set; } //Maxim Quantity to Enter when tracking options activate, if serial number must be 1
        Unit TrackUnit { get; set; }
        IList<PickMethod> PickMethods { get; }


        // //Posting
        IList<Document> Shipments { get; set; }
        Document PostedShipment { get; set; }
        IList<ShowData> ShipmentData { get; set; }
        IList<DocumentLine> ShipmentLines  { get; set; }

        //Packages
        IList<DocumentPackage> DocPackages { get; set; }
        IList<DocumentPackage> OpenPackages { get; set; }
        Label CurPackage { get; set; }
        Visibility ShowMultiPack {get;set;}

    }



    public class ShippingModel : BusinessEntityBase, IShippingModel
    {

        #region IShippingModel Members

        private IList<DocumentPackage> _OpenPackages;
        public IList<DocumentPackage> OpenPackages
        {
            get
            { return _OpenPackages; }
            set
            {
                _OpenPackages = value;
                OnPropertyChanged("OpenPackages");
            }
        }


        private Label _CurPackage;
        public Label CurPackage
        {
            get
            { return _CurPackage; }
            set
            {
                _CurPackage = value;
                OnPropertyChanged("CurPackage");
            }
        }


        private IList<DocumentPackage> _DocPackages;
        public IList<DocumentPackage> DocPackages
        {
            get
            { return _DocPackages; }
            set
            {
                _DocPackages = value;
                OnPropertyChanged("DocPackages");
            }
        }



        private IList<ShowData> _ShipmentData;
        public IList<ShowData> ShipmentData
        {
            get
            { return _ShipmentData; }
            set
            {
                _ShipmentData = value;
                OnPropertyChanged("ShipmentData");
            }
        }

        private Document _PostedShipment;
        public Document PostedShipment
        {
            get
            {
                return _PostedShipment;
            }
            set
            {
                _PostedShipment = value;
                OnPropertyChanged("PostedShipment");
            }
        }


        private IList<DocumentLine> _ShipmentLines;
        public IList<DocumentLine> ShipmentLines
        {
            get
            {
                return _ShipmentLines;
            }
            set
            {
                _ShipmentLines = value;
                OnPropertyChanged("ShipmentLines");
            }
        }


        private Label bin;
        public Label BinLabel
        {
            get
            { return bin; }
            set
            {
                bin = value;
                OnPropertyChanged("BinLabel");
            }
        }


        private Unit trackunit;
        public Unit TrackUnit
        {
            get
            { return trackunit; }
            set
            {
                trackunit = value;
                OnPropertyChanged("TrackUnit");
            }
        }

        private Int32 maxtrackqty;
        public Int32 MaxTrackQty
        {
            get
            { return maxtrackqty; }
            set
            {
                maxtrackqty = value;
                OnPropertyChanged("MaxTrackQty");
            }
        }

        private Int32 remqty;
        public Int32 RemainingQty
        {
            get
            { return remqty; }
            set
            {
                remqty = value;
                OnPropertyChanged("RemainingQty");
            }
        }


        private Boolean _AllowReceive;
        public Boolean AllowReceive
        {
            get
            { return _AllowReceive; }
            set
            {
                _AllowReceive = value;
                OnPropertyChanged("AllowReceive");
            }
        }


        private Boolean _AnyReceived;
        public Boolean AnyReceived
        {
            get
            { return _AnyReceived; }
            set
            {
                _AnyReceived = value;
                OnPropertyChanged("AnyReceived");
            }
        }


        private Int32 curqtypending;
        public Int32 CurQtyPending
        {
            get
            { return curqtypending; }
            set
            {
                curqtypending = value;
                OnPropertyChanged("CurQtyPending");
            }
        }


        private IList<ProductTrackRelation> trackdata;
        public IList<ProductTrackRelation> TrackData
        {
            get
            { return trackdata; }
            set
            {
                trackdata = value;
                OnPropertyChanged("TrackData");
            }
        }


        private Label curscanedlabel;
        public Label CurScanedLabel
        {
            get
            { return curscanedlabel; }
            set
            {
                curscanedlabel = value;
                OnPropertyChanged("CurScanedLabel");
            }
        }



        private IList<Status> docstatus;
        public IList<Status> DocStatus
        {
            get
            { return docstatus; }
            set
            {
                docstatus = value;
                OnPropertyChanged("DocStatus");
            }
        }


        //private IList<Account> customerlist;
        //public IList<Account> CustomerList
        //{
        //    get
        //    { return customerlist; }
        //    set
        //    {
        //        customerlist = value;
        //        OnPropertyChanged("CustomerList");
        //    }
        //}


        private IList<ShowData> documentdata;
        public IList<ShowData> DocumentData
        {
            get
            { return documentdata; }
            set
            {
                documentdata = value;
                OnPropertyChanged("DocumentData");
            }
        }


        private IList<ProductStock> productbinstock;
        public IList<ProductStock> ProductBinStock
        {
            get
            { return productbinstock; }
            set
            {
                productbinstock = value;
                OnPropertyChanged("ProductBinStock");
            }
        }



        public SysUser User
        {
            get { return App.curUser; }
        }


        private IList<Label> labelsavailable;
        public IList<Label> LabelsAvailable
        {
            get
            { return labelsavailable; }
            set
            {
                labelsavailable = value;
                OnPropertyChanged("LabelsAvailable");
            }
        }


        private IList<Label> manualtracklist;
        public IList<Label> ManualTrackList
        {
            get
            { return manualtracklist; }
            set
            {
                manualtracklist = value;
                OnPropertyChanged("ManualTrackList");
            }
        }


        private IList<Document> documentlist;
        public IList<Document> DocumentList
        {
            get
            { return documentlist; }
            set
            {
                documentlist = value;
                OnPropertyChanged("DocumentList");
            }
        }


        private IList<Document> _Shipments;
        public IList<Document> Shipments
        {
            get
            { return _Shipments; }
            set
            {
                _Shipments = value;
                OnPropertyChanged("Shipments");
            }
        }


        private IList<Document> pendingdocumentlist;
        public IList<Document> PendingDocumentList
        {
            get
            { return pendingdocumentlist; }
            set
            {
                pendingdocumentlist = value;
                OnPropertyChanged("PendingDocumentList");
            }
        }


        private IList<DocumentLine> documentlines;
        public IList<DocumentLine> DocumentLines
        {
            get
            {
                return documentlines;
            }
            set
            {
                documentlines = value;
                OnPropertyChanged("DocumentLines");
            }
        }


        private IList<Product> docproducts;
        public IList<Product> DocProducts
        {
            get
            {
                return docproducts;
            }
            set
            {
                docproducts = value;
                OnPropertyChanged("DocProducts");
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

        //private IList<Unit> packingunits;
        //public IList<Unit> PackingUnits
        //{
        //    get
        //    {
        //        return packingunits;
        //    }
        //    set
        //    {
        //        packingunits = value;
        //        OnPropertyChanged("PackingUnits");
        //    }
        //}


        private IList<DocumentBalance> documentbalance;
        public IList<DocumentBalance> DocumentBalance
        {
            get
            {
                return documentbalance;
            }
            set
            {
                documentbalance = value;
                OnPropertyChanged("DocumentBalance");
            }
        }


        private IList<DocumentBalance> pendingtopostlist;
        public IList<DocumentBalance> PendingToPostList
        {
            get
            {
                return pendingtopostlist;
            }
            set
            {
                pendingtopostlist = value;
                OnPropertyChanged("PendingToPostList");
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



        private Double shippingquantity;
        public Double ShippingQuantity
        {
            get
            {
                return shippingquantity;
            }
            set
            {
                shippingquantity = value;
                OnPropertyChanged("ShippingQuantity");
            }
        }

        private Document document;
        public Document Document
        {
            get
            {
                return document;
            }
            set
            {
                document = value;
                OnPropertyChanged("Document");
            }
        }

        private Product product;
        public Product Product
        {
            get
            {
                return product;
            }
            set
            {
                product = value;
                OnPropertyChanged("Product");
            }
        }

        private Visibility _ShowMultiPack;
        public Visibility ShowMultiPack
        {
            get { return _ShowMultiPack;  }
            set { _ShowMultiPack = value; OnPropertyChanged("ShowMultiPack"); }
        }


        public IList<PickMethod> PickMethods
        {
            get
            { return App.PickMethodList; }
        }

        #endregion
    }
}