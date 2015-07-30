using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErpConnect;
using Entities.Trace;
using Entities.Master;
using System.IO;
using Integrator;
using System.Data;

namespace ErpConnect.Everest
{
    public class DocumentService : SQLBase, IDocumentService
    {
        private StringWriter swriter;
        private DataSet ds;
        private WmsTypes WType;
        private Company CurCompany;

        public DocumentService(Company factoryCompany)
        {
            CurCompany = factoryCompany;
            swriter = new StringWriter();
            WType = new WmsTypes();
            ds = new DataSet();
        }


        #region IDocumentService Members

        public IList<Document> GetAllReceivingDocuments()
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetReceivingDocumentsLastXDays(int days)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetReceivingDocumentById(string code)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetReceivingDocumentsSince(DateTime sinceDate)
        {
            throw new NotImplementedException();
        }

        public string CreatePurchaseOrder(Document document)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetAllShippingDocuments(int docType, bool userRemain)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetShippingDocumentsLastXDays(int days, int docType, bool userRemain)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetShippingDocumentById(string code, int docType, bool userRemain)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetShippingDocumentsSince(DateTime sinceDate, int docType, bool userRemain)
        {
            throw new NotImplementedException();
        }

        public string CreateSalesOrder(Document document, string docPrefix, string batch)
        {
            throw new NotImplementedException();
        }

        public string CreateCustomer(Entities.Master.Account customer)
        {
            throw new NotImplementedException();
        }

        public string CreateCustomerAddress(Entities.Master.AccountAddress address)
        {
            throw new NotImplementedException();
        }

        public bool CreateInventoryAdjustment(Document inventoryAdj, short adjType)
        {
            throw new NotImplementedException();
        }

        public bool CreatePurchaseReceipt(Document receivingTask, IList<NodeTrace> traceList, bool costZero)
        {
            throw new NotImplementedException();
        }

        public Document GetReceiptPostedStatus(Document postedReceipt)
        {
            throw new NotImplementedException();
        }

        public Document GetAdjustmentPostedStatus(Document document)
        {
            throw new NotImplementedException();
        }

        public bool FulFillSalesDocument(Document ffDocument, string shortAgeOption, bool fistTimeFulfill, string batchNo)
        {
            throw new NotImplementedException();
        }

        public bool CreateSalesInvoice()
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetKitAssemblyDocumentsSince(DateTime sinceDate)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetKitAssemblyDocuments()
        {
            throw new NotImplementedException();
        }

        public Document GetAssemblyOrderPostedStatus(Document order)
        {
            throw new NotImplementedException();
        }

        public bool AssemblyOrderWasDeleted(Document order)
        {
            throw new NotImplementedException();
        }

        public string CreateKitAssemblyOrderBasedOnSalesDocument(Document shipment, Entities.Master.Product product, double quantity, string sequence)
        {
            throw new NotImplementedException();
        }

        public string CancelKitAssemblyOrderBasedOnSalesDocument(Document data)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetAllLocationTransferDocuments()
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetLocationTransferDocumentsSince(DateTime sinceDate)
        {
            throw new NotImplementedException();
        }

        //public void UpdateSalesDocumentBatch(string salesDocument, string batchNumber)
        //{
        //    throw new NotImplementedException();
        //}

        public IList<Entities.General.ProductStock> GetErpStock(Entities.General.ProductStock data, bool detailed)
        {
            throw new NotImplementedException();
        }

        public Document GetSalesOrderPostedStatus(Document curDocument)
        {
            throw new NotImplementedException();
        }

        public bool SalesOrderWasDeleted(Document curDocument)
        {
            throw new NotImplementedException();
        }

        public void ReceiptReturn(Document prDocument, IList<Label> listofReturn)
        {
            throw new NotImplementedException();
        }

        public IList<Document> GetPurchaseReturnsSince(DateTime sinceDate)
        {
            throw new NotImplementedException();
        }

        public Boolean FulFillMergedSalesDocument(Document ssDocument, IList<DocumentLine> iList, bool fistTimeFulfill, string batchNumber)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDocumentService Members


        public void UpdateSalesDocumentBatch(string salesDocument, string batchNumber)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDocumentService Members


        public int SaveUpdateErpDocumentLine(DocumentLine docLine, bool removeLine)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDocumentService Members


        public void CreateTransferReceipt(Document prDocument, IList<NodeTrace> traceList)
        {
            return;
        }

        #endregion

        #region IDocumentService Members


        public bool CreateLocationTransfer(Document docTranfer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}