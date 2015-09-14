using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using System.Linq;
using WpfFront.WMSBusinessService;
using WpfFront.Common;

namespace WpfFront.Models
{

    public interface IShippingConsoleModel 
    {
        IList<Document> TodayList { get; set; }
        IList<Document> MonthList { get; set; }
        IList<Document> WeekList { get; set; }
        IList<Document> OrderByPicker { get; set; }
        IList<UserByRol> PickerList { get; set; }
        SysUser CurPicker { get; set; }

    }



    public class ShippingConsoleModel : BusinessEntityBase, IShippingConsoleModel
    {

        private SysUser _CurPicker;
        public SysUser CurPicker
        {
            get
            { return _CurPicker; }
            set
            {
                _CurPicker = value;
                OnPropertyChanged("CurPicker");
            }
        }



        private IList<UserByRol> _PickerList;
        public IList<UserByRol> PickerList
        {
            get
            { return _PickerList; }
            set
            {
                _PickerList = value;
                OnPropertyChanged("PickerList");
            }
        }

        private IList<Document> todaylist;
        public IList<Document> TodayList
        {
            get
            { return todaylist; }
            set
            {
                todaylist = value;
                OnPropertyChanged("TodayList");
            }
        }


        private IList<Document> orderbypicker;
        public IList<Document> OrderByPicker
        {
            get
            { return orderbypicker; }
            set
            {
                orderbypicker = value;
                OnPropertyChanged("OrderByPicker");
            }
        }

        private IList<Document> monthlist;
        public IList<Document> MonthList
        {
            get
            { return monthlist; }
            set
            {
                monthlist = value;
                OnPropertyChanged("MonthList");
            }
        }



        private IList<Document> weeklist;
        public IList<Document> WeekList
        {
            get
            { return weeklist; }
            set
            {
                weeklist = value;
                OnPropertyChanged("WeekList");
            }
        }
    }
}