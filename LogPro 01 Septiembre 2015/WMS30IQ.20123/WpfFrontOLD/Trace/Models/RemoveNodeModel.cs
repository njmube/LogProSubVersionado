using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using WpfFront.Presenters;

namespace WpfFront.Models
{
    public interface IRemoveNodeModel
    {
        DocumentBalance Record { get; set; }
        //IList<ProductStock> LstManual { get; set; }
        IList<NodeTrace> LstPrinted { get; set; }
        int QtyManualOld { get; set; }
        double QtyPrintedOld { get; set; }
        Object ParentWindow { get; set; }
    }


    public class RemoveNodeModel : BusinessEntityBase, IRemoveNodeModel
    {

        private DocumentBalance record;
        public DocumentBalance Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private Object parentWindow;
        public Object ParentWindow
        {
            get { return parentWindow; }
            set
            {
                parentWindow = value;
                OnPropertyChanged("ParentWindow");
            }
        }

        private IList<NodeTrace> lstPrinted;
        public IList<NodeTrace> LstPrinted
        {
            get { return lstPrinted; }
            set
            {
                lstPrinted = value;
                OnPropertyChanged("LstPrinted");
            }
        }

        private int qtyManualOld;
        public int QtyManualOld
        {
            get { return qtyManualOld; }
            set
            {
                qtyManualOld = value;
                OnPropertyChanged("QtyManualOld");
            }
        }

        private double qtyPrintedOld;
        public double QtyPrintedOld
        {
            get { return qtyPrintedOld; }
            set
            {
                qtyPrintedOld = value;
                OnPropertyChanged("QtyPrintedOld");
            }
        }
    }
}
