using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Xceed.Wpf.DataGrid;
using System.Collections;
using System.Xml;
using System.Data;
using WpfFront.WMSBusinessService;
using System.Collections.Specialized;
using WpfFront.Common.Query;
using System.Linq;


namespace WpfFront.Models
{
    public interface IIqToolModel
    {

        void CheckRules();

        Boolean IsValidModel();

        IList<IqReport> ListReportSystems { get; set; }

        IqReport ReportSystem { get; set; }
        IqReportTable CurTable { get; set; }

        IqReportTable ReportTableSystem { get; set; }

        DataTable Details { get; set; }

        //IList<IqReportColumn> ReportColumns { get; }
        IList<IqColumn> AllColumns { get; set; }

        //IList<IqReportColumn> ColumnsFiltered { get; } 

        StringDictionary StrOperator { get; }
        StringDictionary NumOperator { get; }
        StringDictionary Aggregation { get; }

    }


    public class IqToolModel : BusinessEntityBase, IIqToolModel
    {
        public StringDictionary StrOperator { get { return Operators.GetStrOperator(); } }
        public StringDictionary NumOperator { get { return Operators.GetNumOperator(); } }
        public StringDictionary Aggregation { get { return Operators.GetAggregation(); } }


        public void CheckRules()
        {
            this.CheckAllRules();
        }

        public Boolean IsValidModel()
        {
            return this.IsValid();
        }

        private IList<IqReport> listReportSystems;

        public IList<IqReport> ListReportSystems
        {
            get
            {
                return listReportSystems;
            }
            set
            {
                listReportSystems = value;
                OnPropertyChanged("ListReportSystems");
                //this.SetPropertyValue("ListReportSystems", ref listReportSystems, value);

            }
        }

        private IqReport reportSystem;

        public IqReport ReportSystem
        {
            get
            {
                return reportSystem;
            }
            set
            {
                this.SetPropertyValue("ReportSystem", ref reportSystem, value);

            }
        }


        private IqReportTable curTable;

        public IqReportTable CurTable
        {
            get
            {
                return curTable;
            }
            set
            {
                curTable = value;
                OnPropertyChanged("CurTable");

            }
        }



        private IqReport reportUser;

        public IqReport ReportUser
        {
            get
            {
                return reportUser;
            }
            set
            {
                this.SetPropertyValue("ReportUser", ref reportUser, value);
            }
        }
        private IqReportTable reportTableSystem;

        public IqReportTable ReportTableSystem
        {
            get
            {
                return reportTableSystem;
            }
            set
            {
                this.SetPropertyValue("ReportTableSystem", ref reportTableSystem, value);
            }
        }

        private DataTable details;

        public DataTable Details
        {
            get
            {
                return details;
            }
            set
            {
                this.SetPropertyValue("Details", ref details, value);
            }
        }

        //private IqReportSetting reportSetting;

        //public IqReportSetting ReportSetting
        //{
        //    get
        //    {
        //        return reportSetting;
        //    }
        //    set
        //    {
        //        this.SetPropertyValue("ReportSetting", ref reportSetting, value);
        //    }
        //}

        //private IList<IqReportSetting> reportSettings;

        //public IList<IqReportSetting> ReportSettings
        //{
        //    get
        //    {
        //        return reportSettings;
        //    }
        //    set
        //    {
        //        this.SetPropertyValue("ReportSettings", ref reportSettings, value);
        //    }
        //}



        private IList<IqColumn> _AllColumns;
        public IList<IqColumn> AllColumns
        {
            get
            {
                return _AllColumns;
            }
            set
            {
                _AllColumns = value;
                OnPropertyChanged("AllColumns");
                //OnPropertyChanged("PendingColumns");
                //OnPropertyChanged("ColumnsFiltered");
                //OnPropertyChanged("AllowFilterColumns");
            }
        }


        //public IList<IqReportColumn> PendingColumns
        //{
        //    get
        //    {
        //        return _AllColumns.Where(f => f.IsSelected != true).OrderBy(f => f.NumOrder).ToList();
        //    }
        //}

        //public IList<IqReportColumn> ReportColumns
        //{
        //    get
        //    {
        //        return _AllColumns.Where(f => f.IsSelected == true).OrderBy(f => f.NumOrder).ToList();
        //    }
        //}


        //public IList<IqReportColumn> ColumnsFiltered
        //{
        //    get
        //    {
        //        return _AllColumns.Where(f => !string.IsNullOrEmpty(f.OptionsDesc)).ToList();
        //    }
        //}

        //public IList<IqReportColumn> AllowFilterColumns
        //{
        //    get
        //    {
        //        return _AllColumns.Where(f => f.IsSelected == true && f.IsFiltered == true).OrderBy(f => f.Alias).ToList();
        //    }
        //}

    }


}