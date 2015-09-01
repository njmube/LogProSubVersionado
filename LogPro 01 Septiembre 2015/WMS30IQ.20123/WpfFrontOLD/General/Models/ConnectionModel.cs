using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.WMSBusinessService;


namespace WpfFront.Models
{
    public interface IConnectionModel
    {
        
        Connection Record { get; set; }
        ConnectionType CnType { get; set; }
        IList<Connection> ListConnection { get; set; }
        IList<ConnectionType> ListCnnType { get; set; }

    }

    public class ConnectionModel: BusinessEntityBase, IConnectionModel
    {

        private IList<ConnectionType> _ListCnnType;
        public IList<ConnectionType> ListCnnType
        {
            get { return _ListCnnType; }
            set
            {
                _ListCnnType = value;
                OnPropertyChanged("ListCnnType");
            }
        }


        private IList<Connection> _ListConnection;
        public IList<Connection> ListConnection
        {
            get { return _ListConnection; }
            set
            {
                _ListConnection = value;
                OnPropertyChanged("ListConnection");
            }
        }


        private Connection record;
        public Connection Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }


        private ConnectionType cntype;
        public ConnectionType CnType
        {
            get { return cntype; }
            set
            {
                cntype = value;
                OnPropertyChanged("CnType");
            }
        }

    }
}
