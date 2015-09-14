using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IReportModel
    {

        IList<MenuOptionType> EntityList { get; set; }
        MenuOption Report { get; set; }

    }

    public class ReportModel: BusinessEntityBase, IReportModel
    {


        private IList<MenuOptionType> entitylist;
        public IList<MenuOptionType> EntityList
        {
            get { return entitylist; }
            set
            {
                entitylist = value;
                OnPropertyChanged("EntityList");
            }
        }


        private MenuOption report;
        public MenuOption Report
        {
            get { return report; }
            set
            {
                report = value;
                OnPropertyChanged("Report");
            }
        }

    }
}
