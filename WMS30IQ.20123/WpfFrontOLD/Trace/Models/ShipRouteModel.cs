using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;

namespace WpfFront.Models
{
    public interface IShipRouteModel
    {
        IList<Document> DocumentList { get; set; }
        IList<DocumentLine> TotalList { get; set; }
        IList<ShowData> DateList { get; set; }
        IList<ShippingMethod> RouteList { get; set; }
        IList<Document> OpenDocList { get; set; }
        Document CurDoc { get; set; }
        IList<Location> Locations { get; }

    }

    public class ShipRouteModel : BusinessEntityBase, IShipRouteModel
    {

        public IList<Location> Locations { get { return App.LocationList; } }

        private Document _CurDoc;
        public Document CurDoc
        {
            get { return this._CurDoc; }
            set
            {
                _CurDoc = value;
                OnPropertyChanged("CurDoc");
            }
        }

        private IList<Document> _OpenDocList;
        public IList<Document> OpenDocList
        {
            get { return this._OpenDocList; }
            set
            {
                _OpenDocList = value;
                OnPropertyChanged("OpenDocList");
            }
        }


        private IList<ShowData> _DateList;
        public IList<ShowData> DateList
        {
            get { return _DateList; }
            set
            {
                _DateList = value;
                OnPropertyChanged("DateList");
            }
        }


        private IList<ShippingMethod> _RouteList;
        public IList<ShippingMethod> RouteList
        {
            get { return _RouteList; }
            set
            {
                _RouteList = value;
                OnPropertyChanged("RouteList");
            }
        }


        private IList<Document> entityList;
        public IList<Document> DocumentList
        {
            get { return this.entityList; }
            set
            {
                entityList = value;
                OnPropertyChanged("DocumentList");
            }
        }

        private IList<DocumentLine> entityList2;
        public IList<DocumentLine> TotalList
        {
            get { return this.entityList2; }
            set
            {
                entityList2 = value;
                OnPropertyChanged("TotalList");
            }
        }
    }
}
