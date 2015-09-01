using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface ILabelTemplateModel
    {
        LabelTemplate Record { get; set; }
        IList<LabelTemplate> EntityList { get; set; }
        IList<DocumentType> LabelTypeList { get; }
        IList<Connection> PrinterList { get; }

    }

    public class LabelTemplateModel: BusinessEntityBase, ILabelTemplateModel
    {

        public IList<Connection> PrinterList { get { return App.PrinterConnectionList; } }

        private IList<LabelTemplate> entitylist;
        public IList<LabelTemplate> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private LabelTemplate record;
        public LabelTemplate Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<DocumentType> labelTypeList;
        public IList<DocumentType> LabelTypeList
        {
            get { return labelTypeList; }
            set {
                    labelTypeList = value;
                    OnPropertyChanged("LabelTypeList");
                }
        }


    }
}
