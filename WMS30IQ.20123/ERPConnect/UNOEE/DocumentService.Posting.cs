using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Trace;
using Entities.Master;
using ErpConnect;
using Integrator;
using Entities;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Specialized;

namespace ErpConnect.UNOEE
{
    public partial class DocumentService : SQLBase, IDocumentService
    {


        public bool CreateInventoryAdjustment(Document inventoryAdj, short adjType)
        {
            throw new NotImplementedException();
        }


        public bool CreatePurchaseReceipt(Document receipt, IList<NodeTrace> traceList, bool costZero)
        {
            try
            {

                if (receipt.DocumentLines == null || receipt.DocumentLines.Count == 0)
                {
                    ExceptionMngr.WriteEvent("CreatePurchaseReceipt: No lines to process.", ListValues.EventType.Error, null, null,
                        ListValues.ErrorCategory.ErpConnection);

                    throw new Exception("CreatePurchaseReceipt: No lines to process.");
                }

                if (receipt.Location == null)
                {
                    ExceptionMngr.WriteEvent("CreatePurchaseReceipt: Document Location is missing.", ListValues.EventType.Error, null, null,
                        ListValues.ErrorCategory.ErpConnection);

                    throw new Exception("CreatePurchaseReceipt: Document Location is missing.");
                }


                
                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                //Obtiene la info del ERP Setup
                StringDictionary docSetup = Unoee.GetUnoEEDocSetup(receipt.DocType.ErpSetup);

                //OBTENER LOS DATATABLES DE LOS CAMPOS DE DOCUMENTOS DE UNOEE
                Query = GetErpQuery("UNOEE_DOC").Replace("__VERSION",docSetup["f_version_reg"])
                    .Replace("__TIPOREG", "451" ).Replace("__SUBTIPO","0");

                DataSet dsSetup = ReturnDataSet(Query, null, "UNOEE_DOC", Command.Connection);

                if (dsSetup == null || dsSetup.Tables.Count == 0)
                    throw new Exception("CreatePurchaseReceipt: Document Header setup not defined.");

                //OBTIENE EL DATATABLE CON LOS DETALLES DEL DOCUMENTO UNOEE
                Query = GetErpQuery("UNOEE_DOC").Replace("__VERSION", docSetup["f_version_reg_detalle"])
                    .Replace("__TIPOREG", "470").Replace("__SUBTIPO", "1");

                DataTable dtDet = ReturnDataTable(Query, null, "UNOEE_DOC_LINE", Command.Connection);

                if (dtDet == null || dtDet.Rows.Count == 0)
                    throw new Exception("CreatePurchaseReceipt: Document Details setup not defined.");


                //Setup completo con la info del Header y Detalle
                dsSetup.Tables.Add(dtDet);


                //HEADER
                DataTable dtHeader = Unoee.GetUnoEEDataTable(dsSetup.Tables[0]);
                DataRow hRow = dtHeader.NewRow();
                String[] oriDoc = receipt.CustPONumber.Split('-');

                //Mapear los datos e=del encabezado del documento de recibo
                hRow["f_numero_reg"] = 2;
                hRow["f_tipo_reg"] = 451;
                hRow["f_subtipo_reg"] = 0;
                hRow["f_version_reg"] = docSetup["f_version_reg"];
                hRow["f_cia"] = CurCompany.ErpCode;
                hRow["f_liquida_impuesto"] = 0;
                hRow["f_consec_auto_reg"] = 1;
                hRow["f350_id_co"] = oriDoc[0]; //CO
                hRow["f350_id_tipo_docto"] = docSetup["f350_id_tipo_docto"];
                //hRow["f350_consec_docto"] = receipt.DocNumber;
                hRow["f350_fecha"] = DateTime.Today.ToString("yyyyMMdd");
                hRow["f350_id_tercero"] = receipt.Vendor.AccountCode;
                hRow["f350_id_clase_docto"] = 409;
                hRow["f350_ind_estado"] = 1;
                hRow["f350_ind_impresión"] = 0;
                hRow["f350_notas"] = "WMS DOC#: " + receipt.DocNumber + ", " +receipt.Comment;
                hRow["f451_id_concepto"] = 401;
                hRow["f451_id_grupo_clase_docto"] = 403;
                hRow["f451_id_sucursal_prov"] = receipt.QuoteNumber;
                hRow["f451_id_tercero_comprador"] = receipt.SalesPersonName;
                hRow["f451_num_docto_referencia"] = receipt.Reference;
                //MOENDAS
                hRow["f451_id_moneda_docto"] = receipt.UserDef1;                
                hRow["f451_id_moneda_local"] = receipt.UserDef1;
                hRow["f451_id_moneda_conv"] = receipt.UserDef1;

                hRow["f451_tasa_conv"] = 0;
                hRow["f451_tasa_local"] = 0;
                              
                
                hRow["f420_id_tipo_docto"] = oriDoc[1]; //OC CRUCE
                hRow["f420_consec_docto"] = oriDoc[2]; //OC CRUCE
                dtHeader.Rows.Add(hRow);


                //DETAILS
                DataTable dtDetail = Unoee.GetUnoEEDataTable(dtDet);
                DataRow dRow;
                int numreg = 3;

                // Next consecutive for a Purchase Receipt
                foreach (DocumentLine dr in receipt.DocumentLines)
                {
                    //Debe ser active, para garantizar que no es Misc, o service Item
                    if (dr.Product.Status.StatusID == EntityStatus.Active)
                    {
                        //DETAILS
                        dRow = dtDetail.NewRow();
                        dRow["f_numero_reg"] = numreg;
                        dRow["f_tipo_reg"] = 470;
                        dRow["f_subtipo_reg"] = 1;
                        dRow["f_version_reg"] = docSetup["f_version_reg_detalle"];
                        dRow["f_cia"] = CurCompany.ErpCode;
                        dRow["f470_id_co"] = oriDoc[0]; //CO;
                        dRow["f470_id_tipo_docto"] = docSetup["f350_id_tipo_docto"];
                        //dRow["f470_consec_docto"] = receipt.DocNumber;
                        dRow["f470_nro_registro"] = numreg-2;
                        dRow["f470_id_item"] = dr.Product.ProductCode;
                        dRow["f470_id_bodega"] = receipt.Location.ErpCode;
                        dRow["f470_id_unidad_medida"] = dr.Unit.ErpCode;
                        try { dRow["f421_fecha_entrega"] = ((DateTime)dr.Date2).ToString("yyyyMMdd"); }
                        catch { }

                        dRow["f470_cant_base"] = dr.Quantity;

                        if ( dr.QtyOnHand > 0 )
                            dRow["f470_cant_2"] = (int)(dr.Quantity/dr.QtyOnHand);
                        
                        dtDetail.Rows.Add(dRow);
                        numreg++;
                    }
                }

                DataSet dsData = new DataSet();
                dsData.Tables.Add(dtHeader);
                dsData.Tables.Add(dtDetail);


                

                //ARMAR EL ARCHIVO PLANO Y ENVIARLO A GRABAR
                Unoee.SendData(CurCompany, dsSetup, dsData, numreg);


                return true;
            }

            catch (Exception ex)
            {


                throw new Exception(WriteLog.GetTechMessage(ex));
            }

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

        public Document GetSalesOrderPostedStatus(Document curDocument)
        {
            throw new NotImplementedException();
        }

        public void ReceiptReturn(Document receipt, IList<Label> listofReturn)
        {
            try
            {

                if (receipt.DocumentLines == null || receipt.DocumentLines.Count == 0)
                {
                    ExceptionMngr.WriteEvent("ReceiptReturn: No lines to process.", ListValues.EventType.Error, null, null,
                        ListValues.ErrorCategory.ErpConnection);

                    throw new Exception("ReceiptReturn: No lines to process.");
                }

                if (receipt.Location == null)
                {
                    ExceptionMngr.WriteEvent("ReceiptReturn: Document Location is missing.", ListValues.EventType.Error, null, null,
                        ListValues.ErrorCategory.ErpConnection);

                    throw new Exception("ReceiptReturn: Document Location is missing.");
                }



                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                //Obtiene la info del ERP Setup
                StringDictionary docSetup = Unoee.GetUnoEEDocSetup(receipt.DocType.ErpSetup);

                //OBTENER LOS DATATABLES DE LOS CAMPOS DE DOCUMENTOS DE UNOEE
                Query = GetErpQuery("UNOEE_DOC").Replace("__VERSION", docSetup["f_version_reg"])
                    .Replace("__TIPOREG", "460").Replace("__SUBTIPO", "0");

                DataSet dsSetup = ReturnDataSet(Query, null, "UNOEE_DOC", Command.Connection);

                if (dsSetup == null || dsSetup.Tables.Count == 0)
                    throw new Exception("ReceiptReturn: Document Header setup not defined.");

                //OBTIENE EL DATATABLE CON LOS DETALLES DEL DOCUMENTO UNOEE
                Query = GetErpQuery("UNOEE_DOC").Replace("__VERSION", docSetup["f_version_reg_detalle"])
                    .Replace("__TIPOREG", "470").Replace("__SUBTIPO", "1");

                DataTable dtDet = ReturnDataTable(Query, null, "UNOEE_DOC_LINE", Command.Connection);

                if (dtDet == null || dtDet.Rows.Count == 0)
                    throw new Exception("ReceiptReturn: Document Details setup not defined.");


                //Setup completo con la info del Header y Detalle
                dsSetup.Tables.Add(dtDet);


                //HEADER
                DataTable dtHeader = Unoee.GetUnoEEDataTable(dsSetup.Tables[0]);
                DataRow hRow = dtHeader.NewRow();
                String[] oriDoc = receipt.CustPONumber.Split('-');

                //Mapear los datos e=del encabezado del documento de recibo
                hRow["f_numero_reg"] = 2;
                hRow["f_tipo_reg"] = 460;
                hRow["f_subtipo_reg"] = 0;
                hRow["f_version_reg"] = docSetup["f_version_reg"];
                hRow["f_cia"] = CurCompany.ErpCode;

                hRow["f_ind_contacto"] = 1;
                hRow["f_campo_reservado"] = 0;
                hRow["f_liquida_impuesto"] = 0;
                hRow["f_consec_auto_reg"] = 1;
                hRow["f350_fecha"] = DateTime.Today.ToString("yyyyMMdd");
                hRow["f350_id_clase_docto"] = 516;
                hRow["f350_id_co"] = oriDoc[0]; //CO
                hRow["f350_id_tipo_docto"] = docSetup["f350_id_tipo_docto"];
                hRow["f350_id_tercero"] = receipt.Customer.AccountCode;
                hRow["f350_ind_estado"] = 1;
                hRow["f350_ind_impresion"] = 0;

                hRow["f350_notas"] = "WMS DOC#: " + receipt.DocNumber;
                hRow["f460_num_docto_referencia"] = receipt.Reference;

                hRow["f460_id_co_fact"] = receipt.Reference;
                hRow["f460_id_cond_pago"] = receipt.Reference;
                hRow["f460_id_moneda_conv"] = receipt.Reference;
                hRow["f460_id_moneda_docto"] = receipt.Reference;
                hRow["f460_id_moneda_local"] = receipt.Reference;
                hRow["f460_id_punto_envio"] = "000";
                hRow["f460_id_sucursal_fact"] = receipt.Reference;
                hRow["f460_id_sucursal_rem"] = receipt.Reference;
                hRow["f460_id_tercero_rem"] = receipt.Reference;
                hRow["f460_id_tercero_vendedor"] = receipt.Reference;
                hRow["f460_id_tipo_cli_fact"] = receipt.Reference;
                hRow["f460_ind_genera_kit"] = 0;
                hRow["f460_notas"] = receipt.Reference;
                hRow["f460_tasa_conv"] = receipt.Reference;
                hRow["f460_tasa_local"] = receipt.Reference;

                dtHeader.Rows.Add(hRow);


                //DETAILS
                DataTable dtDetail = Unoee.GetUnoEEDataTable(dtDet);
                DataRow dRow;
                int numreg = 3;

                // Next consecutive for a Purchase Receipt
                foreach (DocumentLine dr in receipt.DocumentLines)
                {
                    //Debe ser active, para garantizar que no es Misc, o service Item
                    if (dr.Product.Status.StatusID == EntityStatus.Active)
                    {
                        //DETAILS
                        dRow = dtDetail.NewRow();
                        dRow["f_numero_reg"] = numreg;
                        dRow["f_tipo_reg"] = 470;
                        dRow["f_subtipo_reg"] = 1;
                        dRow["f_version_reg"] = docSetup["f_version_reg_detalle"];
                        dRow["f_cia"] = CurCompany.ErpCode;
                        dRow["f470_id_co"] = oriDoc[0]; //CO;
                        dRow["f470_id_tipo_docto"] = docSetup["f350_id_tipo_docto"];
                        dRow["f470_consec_docto"] = receipt.DocNumber;
                        dRow["f470_nro_registro"] = numreg - 2;
                        dRow["f470_id_item"] = dr.Product.ProductCode;
                        dRow["f470_id_bodega"] = receipt.Location.ErpCode;
                        dRow["f470_id_unidad_medida"] = dr.Unit.ErpCode;
                        try { dRow["f421_fecha_entrega"] = ((DateTime)dr.Date2).ToString("yyyyMMdd"); }
                        catch { }

                        dRow["f470_cant_base"] = dr.Quantity;
                        //dRow["f470_notas"] = "";

                        dtDetail.Rows.Add(dRow);
                        numreg++;
                    }
                }

                DataSet dsData = new DataSet();
                dsData.Tables.Add(dtHeader);
                dsData.Tables.Add(dtDetail);




                //ARMAR EL ARCHIVO PLANO Y ENVIARLO A GRABAR
                Unoee.SendData(CurCompany, dsSetup, dsData, numreg);

            }

            catch (Exception ex)
            {
                throw new Exception(WriteLog.GetTechMessage(ex));
            }
        }


        public void CreateTransferReceipt(Document receipt, IList<NodeTrace> traceList)
        {
            try
            {

                if (receipt.DocumentLines == null || receipt.DocumentLines.Count == 0)
                {
                    ExceptionMngr.WriteEvent("CreateTransferReceipt: No lines to process.", ListValues.EventType.Error, null, null,
                        ListValues.ErrorCategory.ErpConnection);

                    throw new Exception("CreateTransferReceipt: No lines to process.");
                }

                if (receipt.Location == null)
                {
                    ExceptionMngr.WriteEvent("CreateTransferReceipt: Document Location is missing.", ListValues.EventType.Error, null, null,
                        ListValues.ErrorCategory.ErpConnection);

                    throw new Exception("CreateTransferReceipt: Document Location is missing.");
                }



                Command.Connection = new SqlConnection(CurCompany.ErpConnection.CnnString);

                //Obtiene la info del ERP Setup
                StringDictionary docSetup = Unoee.GetUnoEEDocSetup(receipt.DocType.ErpSetup);


                //OBTENER LOS DATATABLES DE LOS CAMPOS DE DOCUMENTOS DE UNOEE
                Query = GetErpQuery("UNOEE_DOC").Replace("__VERSION", docSetup["f_version_reg"])
                    .Replace("__TIPOREG", "450").Replace("__SUBTIPO", "0");

                DataSet dsSetup = ReturnDataSet(Query, null, "UNOEE_DOC", Command.Connection);
                

                if (dsSetup == null || dsSetup.Tables.Count == 0)
                    throw new Exception("CreateTransferReceipt: Document Header setup not defined.");

                //OBTIENE EL DATATABLE CON LOS DETALLES DEL DOCUMENTO UNOEE
                Query = GetErpQuery("UNOEE_DOC").Replace("__VERSION", docSetup["f_version_reg_detalle"])
                    .Replace("__TIPOREG", "470").Replace("__SUBTIPO", "0");

                DataTable dtDet = ReturnDataTable(Query, null, "UNOEE_DOC_LINE", Command.Connection);

                if (dtDet == null || dtDet.Rows.Count == 0)
                    throw new Exception("CreateTransferReceipt: Document Details setup not defined.");


                //Setup completo con la info del Header y Detalle
                dsSetup.Tables.Add(dtDet);


                //HEADER
                DataTable dtHeader = Unoee.GetUnoEEDataTable(dsSetup.Tables[0]);
                DataRow hRow = dtHeader.NewRow();
                String[] oriDoc = receipt.CustPONumber.Split('-');



                //Mapear los datos e=del encabezado del documento de recibo
                hRow["f_numero_reg"] = 2;
                hRow["f_tipo_reg"] = 450;
                hRow["f_subtipo_reg"] = 0;
                hRow["f_version_reg"] = docSetup["f_version_reg"];
                hRow["f_cia"] = CurCompany.ErpCode;
                hRow["f_consec_auto_reg"] = 1;
                hRow["f350_id_co"] = receipt.UserDef1; //CO de la bodega destino.
                hRow["f350_id_tipo_docto"] = docSetup["f350_id_tipo_docto"];
                
                //hRow["f350_consec_docto"] = receipt.DocNumber;

                hRow["f350_fecha"] = DateTime.Today.ToString("yyyyMMdd");
                hRow["f350_id_clase_docto"] = 66;
                hRow["f350_ind_estado"] = 1;
                hRow["f350_ind_impresión"] = 0;
                hRow["f350_notas"] = "WMS DOC#: " + receipt.DocNumber;
                hRow["f450_id_concepto"] = 605;

                hRow["f450_id_bodega_salida"] = receipt.UserDef2;
                hRow["f450_id_bodega_entrada"] = receipt.Location.ErpCode;
                hRow["f350_id_co_base"] = oriDoc[0];
                hRow["f350_id_tipo_docto_base"] = oriDoc[1]; //CRUCE
                hRow["f350_consec_docto_base"] = oriDoc[2]; //CRUCE

                //LINK DE DOCUMENTOS
                hRow["f450_docto_alterno"] = receipt.Company.ErpCode + "-" + receipt.DocNumber; 

                dtHeader.Rows.Add(hRow);


                //DETAILS
                DataTable dtDetail = Unoee.GetUnoEEDataTable(dtDet);
                DataRow dRow;
                int numreg = 3;

                // Next consecutive for a Purchase Receipt
                foreach (DocumentLine dr in receipt.DocumentLines)
                {
                    //Debe ser active, para garantizar que no es Misc, o service Item
                    if (dr.Product.Status.StatusID == EntityStatus.Active)
                    {
                        //DETAILS
                        dRow = dtDetail.NewRow();
                        dRow["f_numero_reg"] = numreg;
                        dRow["f_tipo_reg"] = 470;
                        dRow["f_subtipo_reg"] = 0;
                        dRow["f_version_reg"] = docSetup["f_version_reg_detalle"];
                        dRow["f_cia"] = CurCompany.ErpCode;
                        dRow["f470_id_co"] = receipt.UserDef1; //CO;
                        dRow["f470_id_tipo_docto"] = docSetup["f350_id_tipo_docto"];
                        //dRow["f470_consec_docto"] = receipt.DocNumber;
                        //dRow["f470_nro_registro"] = numreg - 2;
                        dRow["f470_id_item"] = dr.Product.ProductCode;
                        dRow["f470_id_bodega"] = receipt.Location.ErpCode;
                        dRow["f470_id_unidad_medida"] = dr.Unit.ErpCode;

                        dRow["f470_cant_base"] = dr.Quantity;
                        dRow["f470_id_concepto"] = 605;
                        dRow["f470_id_motivo"] = "01"; 
                        dRow["f470_id_co_movto"] = receipt.UserDef1;
                        dRow["f470_id_ccosto_movto"] = "Generico.Gral"; //PREGUNTAR
                        dRow["f470_id_un_movto"] = "11";  //dr.PostingUserName; //PREGUNTAR
                        dRow["f470_id_ubicación_aux_ent"] = dr.BinAffected;
                        dRow["f470_id_ubicacion_aux"] = dr.BinAffected;                        
                        dRow["f470_notas"] = dr.Note;

                        dtDetail.Rows.Add(dRow);
                        numreg++;
                    }
                }

                DataSet dsData = new DataSet();
                dsData.Tables.Add(dtHeader);
                dsData.Tables.Add(dtDetail);


                //ARMAR EL ARCHIVO PLANO Y ENVIARLO A GRABAR
                Unoee.SendData(CurCompany, dsSetup, dsData, numreg);

                //Request ERPDocNumber
                //String RetQuery = GetErpReturnQuery("TRANSFER").Replace("__LINK", receipt.Company.ErpCode + "-" + receipt.DocNumber);

                //receipt.PostingDocument = ReturnScalar(RetQuery, "", Command.Connection);

                //Actuliza el Doucmento de Recibo Con el Documento CReado
                //Crear un SP para Reforzar.                
                //WType.UpdateDocument(receipt);

            }

            catch (Exception ex)
            {
                throw new Exception(WriteLog.GetTechMessage(ex));
            }

        }

    }
}
 
