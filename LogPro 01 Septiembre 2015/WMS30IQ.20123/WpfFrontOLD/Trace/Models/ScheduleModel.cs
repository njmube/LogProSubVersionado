using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IScheduleModel
    {
        Company CurCompany { get; }
        CountSchedule Record { get; set; }
        string Conditions { get; set; }
        IList<CountSchedule> EntityList { get; set; }
        //IList<Unit> UnitGroupList { get; set; }

    }
    public class ScheduleModel : BusinessEntityBase, IScheduleModel
    {
        public Company CurCompany { get { return App.curCompany; } }

        private IList<CountSchedule> entitylist;
        public IList<CountSchedule> EntityList
        {
            get { return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }

        private CountSchedule record;
        public CountSchedule Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private string conditions;
        public string Conditions
        {
            get { return conditions; }
            set
            {
                conditions = value;
                OnPropertyChanged("Conditions");
            }
        }

    }
}
