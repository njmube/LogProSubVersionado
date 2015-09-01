using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;

namespace Entities
{



    public class ListValues
    {
        public enum ErrorCategory
        {
            Persistence, ErpConnection, Presentation, WcfService, Business, Messages, ErpPosting, Printer, WebService, Device
        } 
 
        public enum EventType { Info = 1, Warn = 2, Error = 3, Fatal = 4}

        public static NumberFormatInfo DoubleFormat()
        {
          return  new NumberFormatInfo { NumberDecimalSeparator = "." };
        }
    }


    public static class ProcessActivityType
    {
        public const short Automatic = 1;
        public const short Manual = 2;
    }


    public static class SFileType
    {
        public const string PDF = "PDF";
    }


    public static class LabelType
    {
        
        public const int BinLocation = 1001;
        
        public const int ProductLabel = 1002;
        
        public const int CustomerLabel = 1003;

        public const int UniqueTrackLabel = 1005;

        public const int Message = 2001;

    }



    public static class BasicRol
    {
        public const int Manager = 2;
        public const int Picker = 3;
        public const int Admin = 1;
    }



    public static class ConceptType
    {

        public const int Damage_Scrap = 302;

    }

    public static class BinType
    {
        public const short In_Out = 0;
        public const short In_Only = 1;
        public const short Out_Only = 2;
        public const short Exclude_Out = 3;
        public const short Exclude_In = 4;
    }

    public static class AddressType
    {
        
        public const int Billing = 1;
        
        public const int Shipping = 2;
        
        public const int Contact = 3;
        
        public const int Other = 4;
    }

    public static class AccntType
    {
        public const int Customer = 1;
        public const int Vendor = 2;
        public const int Transportation = 3;
    }

    public static class STrackOptions
    {
        public const short SerialNumber = 1;
        public const short LotCode = 2;
        public const short ExpirationDate = 3;
    }

    public static class SDataTypes
    {
        public const short String = 1;
        public const short Number = 2;
        public const short Bool = 3;
        public const short DateTime = 4;
        public const short ReceivingAlert = 5;
        public const short ShippingAlert = 6;
        public const short ProductQuality = 7;

    }


    public static class DefaultBin
    {
        public const string MAIN = "MAIN";
        public const string PICKING = "PICKING";
        public const string PUTAWAY = "MAIN";
        public const string PROCESS = "PROCESS";
        public const string RETURN = "RETURN";
        public const string NOCOUNT = "NOCOUNT";
        public const string DAMAGE = "DAMAGE";
        public const string INSPECTION = "INSPECTION";

    }


    public static class CnnType
    {
        public const int GPeConnect = 1;
        public const int GPWebServices = 2;
        public const int Everest = 3;
        public const int UnoEE = 4;
        public const int Printer = 10;
    }


    public static class CnnEntityType
    {
        public const int References = 1;
        public const int Documents = 2;
    }




    public static class EntityStatus
    {
        
        public const int Active = 1001;        
        public const int Inactive = 1002;
        public const int Locked = 1003;
    }

    public static class NodeType
    {
        public const int PreLabeled = 1;
        public const int Received = 2;
        //public const int Labeled = 3;
        public const int Stored = 4;
        public const int Picked = 5;
        public const int Packed = 6;
        public const int Released = 7;
        public const int Voided = 8;
        public const int Process = 9;

        //Entidades de Workflow
        public const int Package = 100;
   }

    public static class DocStatus
    {
        
        public const int New = 101;
        
        public const int InProcess = 102;
        
        public const int Completed = 103;
        
        public const int Cancelled = 104;

        public const int Posted = 105;

        public const int NotCompleted = 106; //Indica que se hizo en WMS pero no en el ERP

        public const int PENDING = 999;
    }

    public static class SDocClass
    {
        
        public const short Receiving = 1;
        
        public const short Shipping = 2;
        
        public const short Inventory = 3;
        
        public const short Task = 4;
        
        public const short Posting = 5;
        
        public const short Label = 10;
    }

    public static class SDocType
    {
        
        public const int PurchaseOrder = 101;

        public const int Return = 102;

        public const int WarehouseTransferReceipt = 103;

        public const int InTransitShipment = 104;


        public const int ReceivingTask = 401;

        public const int PickTicket = 402;

        public const int CrossDock = 403;


        
        public const int PurchaseReceipt = 501;
        
        public const int InventoryAdjustment = 502;

        public const int SalesShipment = 503;

        public const int ReceiptConfirmation = 504;

        public const int InTransitReception = 505;
        

        
        public const int SalesOrder = 201;

        public const int BackOrder = 202;

        public const int SalesInvoice = 203;

        public const int WarehouseTransferShipment = 204;

        public const int PurchaseReturn = 205;

        public const int MergedSalesOrder = 206;



        public const int PrintingLot = 302;

        public const int KitAssemblyTask = 404;

        public const int ReplenishPackTask = 405;

        public const int ChangeLocation = 406;

        public const int CountTask = 407;

        public const int ProcessTask = 408;



    }

    public static class GP_DocType
    {

        public const int PO_Standard = 1;    
        public const int PO_DropShipment = 2;   
        public const int PO_Blanket = 3;   
        public const int PO_DropShipBlanket = 4;   

        public const int IV_Adjustment = 1;        
        public const int IV_Variance = 2;
        
        public const int PR_Shipment = 1;
        public const int PR_InTransit_Inventory = 2;  
        public const int PR_Shipment_Invoice = 3;

        public const int SO_Order = 2;
        public const int SO_Invoice = 3;
        public const int SO_Return = 4;
        public const int SO_BackOrder = 5;


    }

    public static class GPBatchNumber
    {
        
        public const string Receipt = "WMS_RECEIPT";
        
        public const string Inventory = "WMS_INVENTORY";
        
        public const string Shipping = "WMS_SHIPPING";
    }


    public static class PickingMethod
    {
        
        public const short ZONE = 1;

        public const short FIFO = 2;

        public const short LIFO = 3;

        public const short FEFO = 4;
    }


    public static class OptionTypes
    {
        public const short Application = 1;
        public const short Report = 2;
        public const short Device = 3;
    }


    public static class WmsSetupValues
    {
        public const int NumRegs = 100; //Maximo de registros en una consulta de busqueda
        public const string  SystemUser = "system";
        public const int HistoricDays = 180;
        public const int HistoricDaysToShow = 90;
        public const int DefaultBinsToShow = 5;
        public const string ImageDir = "imgs";
        public const int NumRegsDevice = 30; 
        public const int MaxBinLength = 10;
        public const string CustomUnit = "Custom";
        public const string RdlTemplateDir = "RDL";
        public const string PrintReportDir = "PrintReport";
        public const string WebServer = "WebServer";
        public const string DefaultLabelTemplate = "LBL_ReceivingLabel_4x6.rdl";
        public const string AssemblyLabelTemplate = "LBL_ProductUniqueLabel_4x2.rdl";
        public const string ProductLabelTemplate = "LBL_ProductGeneric_4x2.rdl";
        public const int ReplenishmentExpHours = 48;
        public const string DefaultPackLabelTemplate = "DOC_CartonContent.rdl";
        public const string DefaultPalletTemplate = "DOC_PalletContent.rdl";
        public const string DEFAULT = "DEFAULT";
        public const int MaxBinRank = 999999;
        public const string DefTpl_DocumentLabel = "LBL_DocumentLabel.rdl";
        public const string AutoSerialNumber = "AUTOSERIAL";
        public const int LabelLength = 14;
        public const string DefaultPalletLabelTemplate = "DOC_PalletContent.rdl";
        public const string AdminUser = "admin";
        public const string HAZMAT_REPORT = "DOC_HazmatDeclaration.rdl";
        public const string Counting_Bach = "WMS_INVCOUNT";
    }

    public static class EntityID
    {
        public const short Company = 1;
        public const short Account = 2;
        public const short AccountAddress = 3;
        public const short Product = 4;
        public const short Location = 5;
        public const short Document = 6;
        public const short Process = 7;
        public const short Bin = 8;
        public const short LogError = 9;
        public const short DocumentType = 10;
        public const short BusinessAlert = 11;
        public const short Label = 20;
    }

    public static class ShortAge
    {
        public const string BackOrder = "BACKORDER";
        public const string Cancel = "CANCEL";
    }


    public static class BasicProcess
    {
        public const string Picking = "Picking";
        public const string ReceiptAcknowledge = "ReceiptAcknowledge";
        public const string Shipping = "Shipping";
        public const string Audit = "AuditRequest";
    }

    public static class KitType
    {
        public const int Custom = 100;  // kits personalizados
        public const int ERP = 2;
    }

    public static class ExplodeKit
    {
        public const int Always = 1;
        public const int IfNotStock = 2;
        public const int Caterpillar = 3;
        public const int CaterpillarKit = 5;
    }



}
