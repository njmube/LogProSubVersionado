using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities.General;
using Entities.Master;

namespace ErpConnect
{
    /// <summary>
    /// Interfase de documentos a implemetar por la factory
    /// </summary>
    public interface IDocumentService
    {

        IList<Document> GetAllReceivingDocuments();

        IList<Document> GetReceivingDocumentsLastXDays(int days);

        IList<Document> GetReceivingDocumentById(string code);

        IList<Document> GetReceivingDocumentsSince(DateTime sinceDate);

        String CreatePurchaseOrder(Document document);






        IList<Document> GetAllShippingDocuments(int docType, bool userRemain);

        IList<Document> GetShippingDocumentsLastXDays(int days, int docType, bool userRemain);

        IList<Document> GetShippingDocumentById(string code, int docType, bool userRemain);

        IList<Document> GetShippingDocumentsSince(DateTime sinceDate, int docType, bool userRemain);

        String CreateSalesOrder(Document document, string docPrefix, string batch);

        String CreateCustomer(Account customer);

        String CreateCustomerAddress(AccountAddress address);



        Boolean CreateInventoryAdjustment(Document inventoryAdj, short adjType);

        Boolean CreatePurchaseReceipt(Document receivingTask, IList<NodeTrace> traceList, bool costZero);

        Document GetReceiptPostedStatus(Document postedReceipt);

        Document GetAdjustmentPostedStatus(Document document);

        Boolean FulFillSalesDocument(Document ffDocument, string shortAgeOption, bool fistTimeFulfill, string batchNo);

        Boolean CreateSalesInvoice();



        //Inventory - Kit/Assembly
        IList<Document> GetKitAssemblyDocumentsSince(DateTime sinceDate);

        IList<Document> GetKitAssemblyDocuments();

        Document GetAssemblyOrderPostedStatus(Document order);

        Boolean AssemblyOrderWasDeleted(Document order);

        String CreateKitAssemblyOrderBasedOnSalesDocument(Document shipment, Product product, 
            double quantity, string sequence);

        string CancelKitAssemblyOrderBasedOnSalesDocument(Document data);


        //Location Transfer

        IList<Document> GetAllLocationTransferDocuments();
        
        IList<Document> GetLocationTransferDocumentsSince(DateTime sinceDate);

        void UpdateSalesDocumentBatch(String salesDocument, String batchNumber);

        IList<ProductStock> GetErpStock(ProductStock data, bool detailed);

        Document GetSalesOrderPostedStatus(Document curDocument);

        bool SalesOrderWasDeleted(Document curDocument);

        void ReceiptReturn(Document prDocument, IList<Label> listofReturn);

        IList<Document> GetPurchaseReturnsSince(DateTime sinceDate);

        Boolean FulFillMergedSalesDocument(Document ssDocument, IList<DocumentLine> iList, bool fistTimeFulfill, string batchNumber);

        int SaveUpdateErpDocumentLine(DocumentLine docLine, bool removeLine);

        void CreateTransferReceipt(Document prDocument, IList<NodeTrace> traceList);

        bool CreateLocationTransfer(Document docTranfer);
    }
}
