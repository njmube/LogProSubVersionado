using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IDocumentTypeSequenceModel
    {
        SysUser User { get; }
        DocumentTypeSequence Record { get; set; }
        IList<DocumentTypeSequence> EntityList { get; set; }

    }

    public class DocumentTypeSequenceModel: BusinessEntityBase, IDocumentTypeSequenceModel
    {

        public SysUser User { get { return App.curUser; } }

        private IList<DocumentTypeSequence> entitylist;
        public IList<DocumentTypeSequence> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private DocumentTypeSequence record;
        public DocumentTypeSequence Record
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
