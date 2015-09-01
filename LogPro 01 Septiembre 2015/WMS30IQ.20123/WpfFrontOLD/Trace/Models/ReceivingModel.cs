using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
//using WpfFront.BusinessObject;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Windows;
using System.Collections;

namespace WpfFront.Models
{

    public interface IReceivingModel 
    {
        
        IList<Document> DocumentList { get; set; }
        IList<Document> PendingDocumentList { get; set; }
        IList<DocumentLine> DocumentLines { get; set; }
        IList<Product> DocProducts { get; set; }
        IList<Unit> ProductUnits { get; set; }
        IList<Unit> PackingUnits { get; set; }
        //IList<Account> VendorList { get; set; }

        IList<DocumentBalance> DocumentBalance { get; set; }
        IList<DocumentBalance> PendingToPostList { get; set; }

        IList<Label> LabelsAvailable { get; set; }
        IList<Status> DocStatus { get; set; }


        Node Node {get; set;}
        Double ReceivingQuantity { get; set; }
        Document Document { get; set; }
        Product Product { get; set; }
        IList<ShowData> DocumentData { get; set; }
        SysUser User { get; }
        Label CurScanedLabel { get; set; }
        Int32 CurQtyPending { get; set; } 


        IList<ProductTrackRelation> LabelTrackData { get; set; }
        //IList<Label> ManualTrackList { get; set; }
        //Int32 RemainingQty { get; set; }
        //Unit TrackUnit { get; set; }
        //Int32 MaxTrackQty { get; set; } //Maxim Quantity to Enter when tracking options activate, if serial number must be 1


        Bin Bin { get; set; }



 
        //Posting
        Document PostedReceipt { get; set; }
        IList<ShowData> ReceiptData { get; set; }
        IList<DocumentLine> ReceiptLines { get; set; }
        IList<Document> Receipts { get; set; }

        //Assigned Bins
        IList<ProductStock> ProductBinStock { get; set; }
        IList<int> Priority { get; set; }

        Unit PackUnit { get; set; }
        //IList<Bin> BinList { get; set; }
        string VendorItem { get; set; }
        Document CrossDock { get; set; }

        //Allow Receive 20 marzo 09
        Boolean AllowReceive { get; set; }
        Boolean AnyReceived { get; set; }
        Boolean PutAwayDirect { get; set; }

        Hashtable BinDirectionList { get; }

        //Returns 13 oct 2009
        Double RetOnHnd { get; set; }
        Double RetDamage { get; set; }
        Double RetTotal { get; }

    }



    public class ReceivingModel : BusinessEntityBase, IReceivingModel
    {

        #region IReceivingModel Members

        public Hashtable BinDirectionList 
        { get { return App.BinDirectionList; } }

        private Bin bin;
        public Bin Bin
        {
            get
            { return bin; }
            set
            {
                bin = value;
                OnPropertyChanged("Bin");
            }
        }


        //private Unit trackunit;
        //public Unit TrackUnit        {
        //    get
        //    { return trackunit; }
        //    set
        //    {
        //        trackunit = value;
        //        OnPropertyChanged("TrackUnit");
        //    }
        //}


        //private Int32 maxtrackqty;
        //public Int32 MaxTrackQty
        //{
        //    get
        //    { return maxtrackqty; }
        //    set
        //    {
        //        maxtrackqty = value;
        //        OnPropertyChanged("MaxTrackQty");
        //    }
        //}


        //private Int32 remqty;
        //public Int32 RemainingQty
        //{
        //    get
        //    { return remqty; }
        //    set
        //    {
        //        remqty = value;
        //        OnPropertyChanged("RemainingQty");
        //    }
        //}


        private Int32 curqtypending;
        public Int32 CurQtyPending
        {
            get
            { return curqtypending; }
            set
            {
                curqtypending = value;
                _AllowReceive = curqtypending > 0 ? true : false;
                OnPropertyChanged("CurQtyPending");
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


        private Boolean _PutAwayDirect;
        public Boolean PutAwayDirect
        {
            get
            { return _PutAwayDirect; }
            set
            {
                _PutAwayDirect = value;
                OnPropertyChanged("PutAwayDirect");
            }
        }


        private IList<ProductTrackRelation> trackdata;
        public IList<ProductTrackRelation> LabelTrackData
        {
            get
            { return trackdata; }
            set
            {
                trackdata = value;
                OnPropertyChanged("LabelTrackData");
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


        //private IList<Account> vendorlist;
        //public IList<Account> VendorList
        //{
        //    get
        //    { return vendorlist; }
        //    set
        //    {
        //        vendorlist = value;
        //        OnPropertyChanged("VendorList");
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


        private IList<ShowData> receiptdata;
        public IList<ShowData> ReceiptData
        {
            get
            { return receiptdata; }
            set
            {
                receiptdata = value;
                OnPropertyChanged("ReceiptData");
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


        //private IList<Label> manualtracklist;
        //public IList<Label> ManualTrackList
        //{
        //    get
        //    { return manualtracklist; }
        //    set
        //    {
        //        manualtracklist = value;
        //        OnPropertyChanged("ManualTrackList");
        //    }
        //}


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


        private IList<Document> receipts;
        public IList<Document> Receipts
        {
            get
            { return receipts; }
            set
            {
                receipts = value;
                OnPropertyChanged("Receipts");
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



        private IList<DocumentLine> receiptlines;
        public IList<DocumentLine> ReceiptLines
        {
            get
            {
                return receiptlines;
            }
            set
            {
                receiptlines = value;
                OnPropertyChanged("ReceiptLines");
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



        private Double receivingquantity;
        public Double ReceivingQuantity
        {
            get
            {
                return receivingquantity;
            }
            set
            {
                receivingquantity = value;
                OnPropertyChanged("ReceivingQuantity");
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



        private Document postedreceipt;
        public Document PostedReceipt
        {
            get
            {
                return postedreceipt;
            }
            set
            {
                postedreceipt = value;
                OnPropertyChanged("PostedReceipt");
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


        private IList<int> _Priority;       

        public IList<int> Priority
        {
            get
            {
                if (_Priority == null)
                {
                    _Priority = new List<int>();
                    _Priority.Add(1);
                    _Priority.Add(2);
                    _Priority.Add(3);
                    _Priority.Add(4);
                    _Priority.Add(5);
                }
                
                return _Priority; }
            set
            {
                _Priority = value;
                OnPropertyChanged("Priority");
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

        //private IList<Bin> _BinList;
        //public IList<Bin> BinList
        //{
        //    get
        //    { return _BinList; }
        //    set
        //    {
        //        _BinList = value;
        //        OnPropertyChanged("BinList");
        //    }
        //}


        private string _VendorItem;
        public string VendorItem
        {
            get
            { return _VendorItem; }
            set
            {
                _VendorItem = value;
                OnPropertyChanged("VendorItem");
            }
        }


        private Document _cross;
        public Document CrossDock
        {
            get
            {
                return _cross;
            }
            set
            {
                _cross = value;
                OnPropertyChanged("CrossDock");
            }
        }




        private Double _RetOnHnd;
        public Double RetOnHnd
        {
            get
            {
                return _RetOnHnd;
            }
            set
            {
                _RetOnHnd = value;
                _RetTotal = _RetDamage + RetOnHnd;
                OnPropertyChanged("RetOnHnd");
            }
        }


        private Double _RetDamage;
        public Double RetDamage
        {
            get
            {
                return _RetDamage;
            }
            set
            {
                _RetDamage = value;
                _RetTotal = _RetDamage + RetOnHnd;
                OnPropertyChanged("RetDamage");
            }
        }


        private Double _RetTotal;
        public Double RetTotal
        {
            get
            {
                return _RetTotal;
            }
            set
            {
                _RetTotal = value;
                OnPropertyChanged("RetTotal");
            }
        }

        #endregion
    }
}