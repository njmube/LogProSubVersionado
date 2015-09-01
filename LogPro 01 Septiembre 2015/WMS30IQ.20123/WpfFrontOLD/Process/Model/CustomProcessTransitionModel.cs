using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface ICustomProcessTransitionModel
    {
        CustomProcessTransition Record { get; set; }
        IList<CustomProcessTransition> EntityList { get; set; }
        IList<Status> StatusList { get; }
        IList<CustomProcess> CustomProcessList { get; set; }
        IList<CustomProcessActivity> CustomProcessActivityList { get; set; }
        IList<CustomProcessActivity> CustomProcessNextActivityList { get; set; }
        IList<CustomProcessContext> ProcessContextList { get; set; }
    }

    public class CustomProcessTransitionModel: BusinessEntityBase, ICustomProcessTransitionModel
    {
        public IList<Status> StatusList { get { return App.EntityStatusList; } }

        private IList<CustomProcess> _CustomProcessList;
        public IList<CustomProcess> CustomProcessList {
            get { return _CustomProcessList; }
            set
            {
                _CustomProcessList = value;
                OnPropertyChanged("CustomProcessList");
            }
        }


        private IList<CustomProcessContext> _ProcessContextList;
        public IList<CustomProcessContext> ProcessContextList
        {
            get { return _ProcessContextList; }
            set
            {
                _ProcessContextList = value;
                OnPropertyChanged("ProcessContextList");
            }
        }

        private IList<CustomProcessTransition> entitylist;
        public IList<CustomProcessTransition> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private CustomProcessTransition record;
        public CustomProcessTransition Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


        private IList<CustomProcessActivity> _CustomProcessActivityList;
        public IList<CustomProcessActivity> CustomProcessActivityList
        {
            get { return _CustomProcessActivityList; }
            set
            {
                _CustomProcessActivityList = value;
                OnPropertyChanged("CustomProcessActivityList");
            }
        }


        private IList<CustomProcessActivity> _CustomProcessNextActivityList;
        public IList<CustomProcessActivity> CustomProcessNextActivityList
        {
            get { return _CustomProcessNextActivityList; }
            set
            {
                _CustomProcessNextActivityList = value;
                OnPropertyChanged("CustomProcessNextActivityList");
            }
        }

    }
}
