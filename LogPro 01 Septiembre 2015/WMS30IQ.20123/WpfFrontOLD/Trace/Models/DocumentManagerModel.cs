using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IDocumentManagerModel
    {
        // Company CurCompany { get; }
        Document Record { get; set; }
        DocumentAddress RecordShippingAddress { get; set; }
        DocumentAddress RecordBildAddress { get; set; }
        // IList<Document> EntityList { get; set; }

        //Variables del Header
        IList<DocumentType> DocTypeList { get; set; }
        IList<Location> LocationList { get; set; }
        IList<ShippingMethod> ShippingMethodList { get; set; }
        IList<PickMethod> PickingMethodList { get; set; }
        //IList<DocumentConcept> DocConceptList { get; set; }
        //IList<Status> DocStatusList { get; set; }

        //Variables del Detail
        IList<DocumentLine> DocumentLineList { get; set; }
        DocumentLine DocumentLine { get; set; }

    }

    public class DocumentManagerModel : BusinessEntityBase, IDocumentManagerModel
    {
        private Document record;
        public Document Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private DocumentAddress _RecordShippingAddress;
        public DocumentAddress RecordShippingAddress
        {
            get { return _RecordShippingAddress; }
            set
            {
                _RecordShippingAddress = value;
                OnPropertyChanged("RecordShippingAddress");
            }
        }

        private DocumentAddress _RecordBildAddress;
        public DocumentAddress RecordBildAddress
        {
            get { return _RecordBildAddress; }
            set
            {
                _RecordBildAddress = value;
                OnPropertyChanged("RecordBildAddress");
            }
        }

        //Variables del Header
        private IList<DocumentType> _DocTypeList;
        public IList<DocumentType> DocTypeList
        {
            get { return _DocTypeList; }
            set
            {
                _DocTypeList = value;
                OnPropertyChanged("DocTypeList");
            }
        }

        /*private IList<DocumentConcept> _DocConceptList;
        public IList<DocumentConcept> DocConceptList
        {
            get { return _DocConceptList; }
            set 
            {
                _DocConceptList = value;
                OnPropertyChanged("DocConceptList");
            }
        }

        private IList<Status> _DocStatusList;
        public IList<Status> DocStatusList
        {
            get { return _DocStatusList; }
            set
            {
                _DocStatusList = value;
                OnPropertyChanged("DocStatusList");
            }
        }*/

        private IList<AccountAddress> _ShippingAddressList;
        public IList<AccountAddress> ShippingAddressList
        {
            get { return _ShippingAddressList; }
            set
            {
                _ShippingAddressList = value;
                OnPropertyChanged("ShippingAddressList");
            }
        }

        private IList<AccountAddress> _BildAddressList;
        public IList<AccountAddress> BildAddressList
        {
            get { return _BildAddressList; }
            set
            {
                _BildAddressList = value;
                OnPropertyChanged("BildAddressList");
            }
        }

        private IList<Location> _LocationList;
        public IList<Location> LocationList
        {
            get { return _LocationList; }
            set
            {
                _LocationList = value;
                OnPropertyChanged("LocationList");
            }
        }

        private IList<ShippingMethod> _ShippingMethodList;
        public IList<ShippingMethod> ShippingMethodList
        {
            get { return _ShippingMethodList; }
            set
            {
                _ShippingMethodList = value;
                OnPropertyChanged("ShippingMethodList");
            }
        }

        private IList<PickMethod> _PickingMethodList;
        public IList<PickMethod> PickingMethodList
        {
            get { return _PickingMethodList; }
            set
            {
                _PickingMethodList = value;
                OnPropertyChanged("PickingMethodList");
            }
        }

        //Variables del Detail
        private IList<DocumentLine> _DocumentLineList;
        public IList<DocumentLine> DocumentLineList
        {
            get { return _DocumentLineList; }
            set
            {
                _DocumentLineList = value;
                OnPropertyChanged("DocumentLineList");
            }
        }

        private DocumentLine _DocumentLine;
        public DocumentLine DocumentLine
        {
            get { return _DocumentLine; }
            set
            {
                _DocumentLine = value;
                OnPropertyChanged("DocumentLine");
            }
        }

        
    }
}
