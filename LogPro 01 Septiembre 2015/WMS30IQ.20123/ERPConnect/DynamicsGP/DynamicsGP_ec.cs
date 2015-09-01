using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErpConnect;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.GP.eConnect;
using Microsoft.Dynamics.GP.eConnect.Serialization;
using System.Configuration;
using System.Data;
using Entities.Master;

namespace ErpConnect.DynamicsGP
{
    public class DynamicsGP_ec :  ConnectFactory {


        public override IDocumentService Documents()
        { return new DocumentService_ec(FactoryCompany); }


        public override IReferenceService References()
        { return new ReferenceService_ec(FactoryCompany); }


        /// <summary>
        /// Function to Retreive Data from GP Tables using eConnect, Return a string in XML Format
        /// </summary>
        /// <param name="DocumentType">Purchase_Order_Transaction, Sales_Order_Transaction</param>
        /// <param name="fromShadowTable">Direct From GP Tables or From EConnect Out</param>
        /// <param name="OutputType">0=List,1=Master document, 2=Complete document,3=Remove only</param>
        /// <param name="Action">Use this element when you request data from the shadow table. The value controls the type of data that is returned.
        /// 0=All documents
        /// 1=Insertions
        /// 2=Updates
        /// 3=Deletions
        /// 4=Returns all insertions and updates as separate documents for each item.
        /// 5=Combines all insertions and updates into one document for each item </param>
        /// <param name="WhereCondition">Custom Condition</param>
        /// <returns></returns>

        public static string RetreiveData(string DocumentType, bool fromShadowTable,
                int OutputType, int Action, string WhereCondition, bool RemoveShadow)
        {

            // Create a connection string to specify the Microsoft Dynamics GP server and database
            //string cnnString = "data source=(local);initial catalog=TWO;integrated security=SSPI;persist security info=False;packet size=4096";
            //string cnnString = "data source=WEBMASTERNT02;initial catalog=TWO;integrated security=SSPI;persist security info=False;packet size=4096";
            //string cnnString = "Data Source=192.168.1.4;initial catalog=TWO;integrated security=SSPI;persist security info=False;packet size=4096";
            //string cnnString = ConfigurationManager.AppSettings["GPCnnString"];

            try
            {
                // Create an eConnect document type object
                eConnectType myEConnectType = new eConnectType();
                // Create a RQeConnectOutType schema object
                RQeConnectOutType myReqType = new RQeConnectOutType();
                // Create an eConnectOut XML node object
                eConnectOut myeConnectOut = new eConnectOut();

                // Populate the eConnectOut XML node elements
                myeConnectOut.ACTION = Action;
                myeConnectOut.DOCTYPE = DocumentType;
                myeConnectOut.OUTPUTTYPE = OutputType;
                myeConnectOut.FORLIST = fromShadowTable ? 0 : 1;
                myeConnectOut.REMOVE = RemoveShadow ? 1 : 0;
                myeConnectOut.WhereClause = WhereCondition;


                // Add the eConnectOut XML node object to the RQeConnectOutType schema object
                myReqType.eConnectOut = myeConnectOut;

                // Add the RQeConnectOutType schema object to the eConnect document object
                RQeConnectOutType[] myReqOutType = { myReqType };
                myEConnectType.RQeConnectOutType = myReqOutType;

                // Serialize the eConnect document object to a memory stream
                MemoryStream myMemStream = new MemoryStream();
                XmlSerializer mySerializer = new XmlSerializer(myEConnectType.GetType());
                mySerializer.Serialize(myMemStream, myEConnectType);
                myMemStream.Position = 0;

                // Load the serialized eConnect document object into an XML document object
                XmlTextReader xmlreader = new XmlTextReader(myMemStream);
                XmlDocument myXmlDocument = new XmlDocument();
                myXmlDocument.Load(xmlreader);

                // Create an eConnectMethods object
                eConnectMethods requester = new eConnectMethods();

                //string outerXml = myXmlDocument.OuterXml;
                //int rem = 0x02;
                //outerXml = outerXml.Replace((char)rem, ' ');

                // Call the eConnect_Requester method of the eConnectMethods object to retrieve specified XML data
                return requester.eConnect_Requester(FactoryCompany.ErpConnection.CnnString, EnumTypes.ConnectionStringType.SqlClient, myXmlDocument.OuterXml); // outerXml);

            }

            catch (eConnectException ex)
            {
                // Dislay any errors that occur to the console
                throw new Exception(ex.Message);
            }
            catch (Exception e)
            {
                // Dislay any errors that occur to the console
                throw new Exception(e.Message);
            }

        }


        //Return a dataset from a XML string  document
        public static DataSet GetDataSet(string xmlData)
        {
            XmlDocument myXmlOut = new XmlDocument();
            myXmlOut.LoadXml(xmlData);

            // convert to dataset in two lines
            DataSet ds = new DataSet();
            ds.ReadXml(new XmlNodeReader(myXmlOut));

            return ds;
        }


        public static Boolean SendData(string xmlDocument)
        {
            eConnectMethods eConCall = new eConnectMethods();

            try
            {
                //Create a connection string to the Microsoft Dynamics GP database server
                //string cnnString = ConfigurationManager.AppSettings["GPCnnString"];
                //string cnnString = "data source=WEBMASTERNT02;initial catalog=TWO;integrated security=SSPI;persist security info=False;packet size=4096";
                //string cnnString = "Data Source=192.168.1.4;initial catalog=TWO;integrated security=SSPI;persist security info=False;packet size=4096";
                //string cnnString = ConfigurationManager.AppSettings["GPCnnString"];
                


                //Use the eConnect_EntryPoint to create the document in Microsoft Dynamics GP
                eConCall.eConnect_EntryPoint(FactoryCompany.ErpConnection.CnnString, EnumTypes.ConnectionStringType.SqlClient, xmlDocument,
                    EnumTypes.SchemaValidationType.None, "");

                return true;
            }
            catch (eConnectException)
            {
                throw;
                //return false; 
            }
            catch (Exception)
            {
                throw;
                //return false; 
            }
        }


    }
}
