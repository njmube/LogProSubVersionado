using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IDocumentConceptModel
    {
        SysUser User { get; }
        DocumentConcept Record { get; set; }
        IList<DocumentConcept> EntityList { get; set; }

    }

    public class DocumentConceptModel: BusinessEntityBase, IDocumentConceptModel
    {

        public SysUser User { get { return App.curUser; } }

        private IList<DocumentConcept> entitylist;
        public IList<DocumentConcept> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private DocumentConcept record;
        public DocumentConcept Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<DocumentClass> _documentClassList;
        public IList<DocumentClass> DocumentClassList
        {
            get { return _documentClassList; }
            set
            {
                _documentClassList = value;
                OnPropertyChanged("DocumentClassList");
            }
        }
    }
}
