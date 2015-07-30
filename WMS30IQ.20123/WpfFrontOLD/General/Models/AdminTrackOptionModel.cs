using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IAdminTrackOptionModel
    {
        TrackOption Record { get; set; }
        IList<TrackOption> EntityList { get; set; }
        IList<DataType> DataTypes { get; }

    }

    public class AdminTrackOptionModel: BusinessEntityBase, IAdminTrackOptionModel
    {

        public IList<DataType> DataTypes { get { return App.DataTypeList; } }


        private Boolean enable;
        public Boolean Enable
        {
            get { return enable; }
            set
            {
                enable = value;
                OnPropertyChanged("Enable");
            }
        }


        private IList<TrackOption> entitylist;
        public IList<TrackOption> EntityList
        {
            get {  return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private TrackOption record;
        public TrackOption Record
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
