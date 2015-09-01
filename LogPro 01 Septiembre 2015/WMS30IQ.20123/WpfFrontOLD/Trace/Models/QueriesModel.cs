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
    public interface IQueriesModel
    {

        void CheckRules();

        Boolean IsValidModel();

        IList<IqReport> ListReportSystems { get; set; }

        IqReport ReportSystem { get; set; }

        //IqReport ReportUser { get; set; }

        IqReportTable ReportTableSystem { get; set; }

        DataTable Details { get; set; }

        //IqReportSetting ReportSetting { get; set; }

        //IList<IqReportSetting> ReportSettings { get; set; }

        IList<IqReportColumn> ReportColumns { get; }
        IList<IqReportColumn> AllColumns { get; set; }
        IList<IqReportColumn> PendingColumns { get;  }
        IList<IqReportColumn> AllowFilterColumns { get; }

        IList<IqReportColumn> ColumnsFiltered { get; } 

        StringDictionary StrOperator { get; }
        StringDictionary NumOperator { get; }
        StringDictionary Aggregation { get; }

        // 
        string Query { get; set; }
        DataSet QueryParams { get; set; }

    }


    public class QueriesModel : BusinessEntityBase, IQueriesModel
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



        private IList<IqReportColumn> _AllColumns;
        public IList<IqReportColumn> AllColumns
        {
            get
            {
                return _AllColumns;
            }
            set
            {
                _AllColumns = value;
                OnPropertyChanged("ReportColumns");
                OnPropertyChanged("PendingColumns");
                OnPropertyChanged("ColumnsFiltered");
                OnPropertyChanged("AllowFilterColumns");
            }
        }


        public IList<IqReportColumn> PendingColumns
        {
            get
            {
                return _AllColumns.Where(f => f.IsSelected != true).OrderBy(f => f.NumOrder).ToList();
            }
        }

        public IList<IqReportColumn> ReportColumns
        {
            get
            {
                return _AllColumns.Where(f => f.IsSelected == true).OrderBy(f => f.NumOrder).ToList();
            }
        }


        public IList<IqReportColumn> ColumnsFiltered
        {
            get
            {
                return _AllColumns.Where(f => !string.IsNullOrEmpty(f.OptionsDesc)).ToList();
            }
        }

        public IList<IqReportColumn> AllowFilterColumns
        {
            get
            {
                return _AllColumns.Where(f => f.IsSelected == true && f.IsFiltered == true).OrderBy(f => f.Alias).ToList();
            }
        }

        //
        private DataSet queryParams;

        public DataSet QueryParams
        {
            get
            {
                return queryParams;
            }
            set
            {
                this.SetPropertyValue("QueryParams", ref queryParams, value);
            }
        }

        private string query;

        public string Query
        {
            get
            {
                return query;
            }
            set
            {
                this.SetPropertyValue("Query", ref query, value);
            }
        }
    }


}