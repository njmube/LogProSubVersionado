using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErpConnect.DynamicsGP;
using Entities.General;
using Entities;
using Entities.Master;

namespace ErpConnect
{
    // Abstract class Connect Factory
    public abstract class ConnectFactory
    {

        // List of Erp types supported by the factory


        //Methods to implement.
        public abstract IDocumentService Documents();
        public abstract IReferenceService References();


        public static Company FactoryCompany;

        /// <summary>
        /// Factory que retorna la conexion al ERP Especifico
        /// </summary>
        /// <param name="whichFactory"></param>
        /// <returns></returns>
        public static ConnectFactory getConnectFactory(Company company)
        {
            FactoryCompany = company;

            switch (FactoryCompany.ErpConnection.ConnectionType.RowID)
            {
                    
                case CnnType.GPeConnect :
                    return new DynamicsGP.DynamicsGP_ec();

                case CnnType.Everest:
                    return new Everest.Everest();

                case CnnType.UnoEE :
                    return new UNOEE.Unoee();

                //case CnnType.GPWebServices:
                //    return new ConnectUnoEE();
                //case 3:
                //    return new ConnectDynamicsGP_ws();

                default:
                    return null;
            }
        }
    }
}