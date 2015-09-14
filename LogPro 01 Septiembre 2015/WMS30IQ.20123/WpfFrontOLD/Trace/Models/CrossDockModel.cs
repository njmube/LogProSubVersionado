using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{


    public interface ICrossDockModel 
    {
        IList<ShowData> DocumentData { get; set; }
        IList<DocumentBalance> DocumentBalance { get; set; }
        Node Node { get; set; }
        IList<DocumentLine> DocumentLines { get; set; }
        Document Document { get; set; }
        IList<Document> HistoryList { get; set; }
        Document HistDoc { get; set; }
        IList<DocumentLine> HistLines { get; set; }
        IList<ShowData> HistData { get; set; }
        IList<TaskDocumentRelation> CrossDocs { get; set; }

        IList<DocumentBalance> CrossDockBalance { get; set; }

        IList<Document> AvailableDocs { get; set; }
        IList<Document> AssignedDocs { get; set; }

        Boolean AnyReceived { get; set; }
    }



    public class CrossDockModel : BusinessEntityBase, ICrossDockModel
    {

        #region ICrossDockModel Members


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



        private IList<DocumentBalance> _CrossDockBalance;
        public IList<DocumentBalance> CrossDockBalance
        {
            get
            {
                return _CrossDockBalance;
            }
            set
            {
                _CrossDockBalance = value;
                OnPropertyChanged("CrossDockBalance");
            }
        }



        private IList<Document> _AssignedDocs;
        public IList<Document> AssignedDocs
        {
            get
            {
                return _AssignedDocs;
            }
            set
            {
                _AssignedDocs = value;
                OnPropertyChanged("AssignedDocs");
            }
        }


        private IList<Document> _AvailableDocs;
        public IList<Document> AvailableDocs
        {
            get
            {
                return _AvailableDocs;
            }
            set
            {
                _AvailableDocs = value;
                OnPropertyChanged("AvailableDocs");
            }
        }

        private IList<DocumentLine> _documentlines;
        public IList<DocumentLine> HistLines
        {
            get
            {
                return _documentlines;
            }
            set
            {
                _documentlines = value;
                OnPropertyChanged("HistLines");
            }
        }

        private IList<ShowData> _documentdata;
        public IList<ShowData> HistData
        {
            get
            { return _documentdata; }
            set
            {
                _documentdata = value;
                OnPropertyChanged("HistData");
            }
        }

        private Document _document;
        public Document HistDoc
        {
            get
            {
                return _document;
            }
            set
            {
                _document = value;
                OnPropertyChanged("HistDoc");
            }
        }


        private IList<Document> documentlist;
        public IList<Document> HistoryList
        {
            get
            { return documentlist; }
            set
            {
                documentlist = value;
                OnPropertyChanged("HistoryList");
            }
        }


        private IList<TaskDocumentRelation> cross;
        public IList<TaskDocumentRelation> CrossDocs
        {
            get
            { return cross; }
            set
            {
                cross = value;
                OnPropertyChanged("CrossDocs");
            }
        }


        private Boolean _AnyReceived;
        public Boolean AnyReceived
        {
            get
            {
                return _AnyReceived;
            }
            set
            {
                _AnyReceived = value;
                OnPropertyChanged("AnyReceived");
            }
        }


        #endregion
    }
}