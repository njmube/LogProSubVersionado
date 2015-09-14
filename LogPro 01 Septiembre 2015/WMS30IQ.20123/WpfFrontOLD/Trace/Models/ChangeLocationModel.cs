using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using System.Collections.Specialized;

namespace WpfFront.Models
{

    public interface IChangeLocationModel 
    {

        IList<Bin> BinList { get; set; }
        IList<Document> DocumentList { get; set; }
        Label SourceLocation { get; set; }
        Label DestLocation { get; set; }
        IList<ProductStock> LinesToProcess { get; set; }
        IList<Label> LabelsToProcess { get; set; }
        Boolean WithContent { get; set; }
        Boolean WithDest { get; set; }

        IList<ProductStock> LinesMoved { get; set; }
        IList<Label> LabelsMoved { get; set; }

    }



    public class ChangeLocationModel : BusinessEntityBase, IChangeLocationModel
    {

        #region IModel Members

        private IList<Bin> _BinList;
        public IList<Bin> BinList
        {
            get
            { return _BinList; }
            set
            {
                _BinList = value;
                OnPropertyChanged("BinList");
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


        private Label _SourceLocation;
        public Label SourceLocation
        {
            get
            {
                return _SourceLocation;
            }
            set
            {
                _SourceLocation = value;
                OnPropertyChanged("SourceLocation");
            }
        }


        private Label _DestLocation;
        public Label DestLocation
        {
            get
            {
                return _DestLocation;
            }
            set
            {
                _DestLocation = value;
                OnPropertyChanged("DestLocation");
            }
        }


        private IList<ProductStock> linestoprocess;
        public IList<ProductStock> LinesToProcess
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


        private IList<Label> labelstoprocess;
        public IList<Label> LabelsToProcess
        {
            get
            {
                return labelstoprocess;
            }
            set
            {
                labelstoprocess = value;
                OnPropertyChanged("LabelsToProcess");
            }
        }


        private Boolean _WithContent;
        public Boolean WithContent
        {
            get
            {
                return _WithContent;
            }
            set
            {
                _WithContent = value;
                OnPropertyChanged("WithContent");
            }
        }

        private Boolean _WithDest;
        public Boolean WithDest
        {
            get
            {
                return _WithDest;
            }
            set
            {
                _WithDest = value;
                OnPropertyChanged("WithDest");
            }
        }



        private IList<ProductStock> _LinesMoved;
        public IList<ProductStock> LinesMoved
        {
            get
            {
                return _LinesMoved;
            }
            set
            {
                _LinesMoved = value;
                OnPropertyChanged("LinesMoved");
            }
        }


        private IList<Label> _LabelsMoved;
        public IList<Label> LabelsMoved
        {
            get
            {
                return _LabelsMoved;
            }
            set
            {
                _LabelsMoved = value;
                OnPropertyChanged("LabelsMoved");
            }
        }



        #endregion
    }
}