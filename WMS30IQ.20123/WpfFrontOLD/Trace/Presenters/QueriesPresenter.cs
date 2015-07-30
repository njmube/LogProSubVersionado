using System;
using System.Collections.Generic;
using System.Data;
using Assergs.Windows;
using Microsoft.Practices.Unity; 
using WMComposite.Events;
using WpfFront.Services;
using WpfFront.Views;
using WpfFront.Models;
using WpfFront.WMSBusinessService;
using System.Linq;
using Xceed.Wpf.DataGrid;
using System.Text;
using Xceed.Wpf.DataGrid.Views;
using WpfFront.Common;
using Xceed.Wpf.DataGrid.Settings;
using System.Windows;
using WpfFront.Common.UserControls;
using System.Collections;


namespace WpfFront.Presenters
{
    public class QueriesPresenter : IQueriesPresenter
    {
        private readonly IUnityContainer container;

        private readonly WMSServiceClient service;

        public IQueriesView View { get; set; }

        private ToolWindow window;
        public ToolWindow Window
        {
            get
            {
                return window;
            }
            set
            {
                //if (value != null)
                //    value.IsMaximized = true;
                window = value;
            }
        }


        public QueriesPresenter(IUnityContainer container, IQueriesView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<QueriesModel>();
            View.SaveData += new EventHandler<DataEventArgs<IqReport>>(this.OnSave);
            View.RunData += new EventHandler<DataEventArgs<String>>(this.OnRun);
            View.SelectionData += new EventHandler<DataEventArgs<IqReport>>(OnSelectionChanged);
            View.UpdateData += new EventHandler<DataEventArgs<IqReport>>(View_UpdateData);
            View.RemoveFromSelected += new EventHandler<DataEventArgs<IqReportColumn>>(View_RemoveFromSelected);
            View.AddToSelected += new EventHandler<DataEventArgs<IqReportColumn>>(View_AddToSelected);
            View.UpdateFilter += new EventHandler<DataEventArgs<IqReportColumn>>(View_UpdateFilter);
            View.DeleteReport += new EventHandler<DataEventArgs<IqReport>>(View_DeleteReport);
            View.LoadProcess += new EventHandler<EventArgs>(View_LoadProcess);

            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("LOADING_TOOL"));

            //View.Model.CheckRules();
            Initialize();

            pw.Close();
        }



        void View_LoadProcess(object sender, EventArgs e)
        {
            LoadProcessToDo();
        }



        void View_DeleteReport(object sender, DataEventArgs<IqReport> e)
        {
            //Eliminar El reporte y sus Hijos.

            try
            {
                IqReport data = e.Value;
                data.ModifiedBy = App.curUser.UserName;
                data.ModDate = DateTime.Now;
                data.Status = App.EntityStatusList.Where(f => f.StatusID == EntityStatus.Inactive).First();
                service.UpdateIqReport(data);
                Util.ShowMessage(Util.GetResourceLanguage("REPORT_DELETED"));

                View.Model.ReportSystem = null;
                View.Model.ListReportSystems = service.GetIqReport(new IqReport { Status = new Status { StatusID = EntityStatus.Active } });
                View.Model.Details = null;
                View.GridDet.Columns.Clear();

                View.GridDet.Items.Refresh();
                View.CboReport.Items.Refresh();

            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("ERROR_SAVING_THE_REPORTS") + "\n" + ex.Message);
            }
        }



        void View_UpdateFilter(object sender, DataEventArgs<IqReportColumn> e)
        {
            View.Model.AllColumns.Where(f => f.ReportColumnId == e.Value.ReportColumnId).First().Options = e.Value.Options;
            //View.Model.AllColumns = View.Model.AllColumns;
            //View.LvFilters.Items.Refresh();
        }



        void View_AddToSelected(object sender, DataEventArgs<IqReportColumn> e)
        {
            if (e.Value == null)
                return;

            try
            {
                e.Value.IsSelected = true;
                //service.UpdateIqReportColumn(e.Value);
            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("ERROR_PROCESSING_RECORD") + "\n" + ex.Message);
            }
        }




        void View_RemoveFromSelected(object sender, DataEventArgs<IqReportColumn> e)
        {
            if (e.Value == null)
                return;

            try
            {
                e.Value.IsSelected = false;
                //service.UpdateIqReportColumn(e.Value);
            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("ERROR_PROCESSING_RECORD") + "\n" + ex.Message);
            }
        }


        void View_UpdateData(object sender, DataEventArgs<IqReport> e)
        {
            if (e.Value == null)
                return;

            try
            {
                IqReport data = e.Value;
                data.ModifiedBy = App.curUser.UserName;
                data.ModDate = DateTime.Now;
                service.UpdateIqReport(data);
                Util.ShowMessage(Util.GetResourceLanguage("REPORT_UPDATED"));

            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("ERROR_SAVING_THE_REPORTS") + "\n" + ex.Message);
            }
        }


        public void Initialize()
        {
            View.Model.ListReportSystems = service.GetIqReport(new IqReport { Status = new Status { StatusID = EntityStatus.Active } });
       }


        private void OnSave(object sender, DataEventArgs<IqReport> e)
        {
            if (e.Value == null)
                return;


            try
            {
                IqReport data = e.Value;
                data.CreatedBy = App.curUser.UserName;
                data.CreationDate = DateTime.Now;
                data.IsForSystem = false;
                View.Model.ReportSystem = service.SaveIqReport(data);
                View.Model.ListReportSystems = service.GetIqReport(new IqReport { Status = new Status { StatusID = EntityStatus.Active } });
                Util.ShowMessage(Util.GetResourceLanguage("REPORT_SAVED"));

            }
            catch (Exception ex)
            {
                Util.ShowError(Util.GetResourceLanguage("ERROR_SAVING_THE_REPORTS") + "\n" + ex.Message);
            }

        }



        private void OnRun(object sender, DataEventArgs<String> e)
        {

            UpdateFilters();
            RunReport(e.Value, true);

        }



        private void UpdateFilters()
        {
            //Actualiza los filtros y condiciones del query

            if (View.StkFilters.Children == null || View.StkFilters.Children.Count == 0)
                return;


            DictionaryEntry operatorItem;
            QueryFilter filter;
            string strOption = "", opDesc = "";


            foreach (Object ucObj in View.StkFilters.Children)
            {
                filter = ucObj as QueryFilter;

                if (filter.cboStrComp.SelectedIndex == -1 || string.IsNullOrEmpty(filter.txtFilter.Text))
                {
                    View.Model.AllColumns.Where(f => f.ReportColumnId == filter.RepColumn.ReportColumnId).First().Options = "";
                    View.Model.AllColumns.Where(f => f.ReportColumnId == filter.RepColumn.ReportColumnId).First().OptionsDesc = "";
                    continue;
                }

                operatorItem = (DictionaryEntry)filter.cboStrComp.SelectedItem;

                //Save Report Colum Options            

                if (filter.RepColumn.IsFiltered == true && operatorItem.Value.ToString() != "" && !string.IsNullOrEmpty(filter.txtFilter.Text))
                {
                    if (operatorItem.Key.Equals("between (range)"))
                    {
                        strOption = "B:" + filter.txtFilter.Text + ":" + filter.txtFilter1.Text;
                        opDesc = filter.RepColumn.Alias + " Between [" + filter.txtFilter.Text + " and " + filter.txtFilter1.Text + "]";
                    }
                    else
                    {
                        //KEY:VALUE (Cuando es un Filter)
                        strOption = "F:" + operatorItem.Value + ":" + filter.txtFilter.Text;
                        opDesc = filter.RepColumn.Alias + " " + operatorItem.Key + " [" + filter.txtFilter.Text + "]";

                    }

                }

                filter.RepColumn.Options = strOption;
                filter.RepColumn.OptionsDesc = opDesc;

                View.Model.AllColumns.Where(f => f.ReportColumnId == filter.RepColumn.ReportColumnId).First().Options = filter.RepColumn.Options;
                View.Model.AllColumns.Where(f => f.ReportColumnId == filter.RepColumn.ReportColumnId).First().OptionsDesc = filter.RepColumn.OptionsDesc;
            }
            
        }


        private void RunReport(string numRegs, bool showError)
        {
            ProcessWindow pw = new ProcessWindow(Util.GetResourceLanguage("LOADING_REPORT"));

            try
            {

                //Clean the Grid;
                View.Model.Details = null;
                View.GridDet.Columns.Clear();
                View.GridDet.Items.Refresh();



                bool hasAggregation = false;

                String dml = "Select "; //Query
                if (!string.IsNullOrEmpty(numRegs))
                {
                    int top;
                    try { top = int.Parse(numRegs); }
                    catch { top = 1000; }

                    dml = "Select TOP " + top.ToString() + " ";
                }


                //Reporte Actual Seleccionado
                IList<IqReportTable> reportTables = View.Model.ReportSystem.ReportTables.OrderBy(f => f.Secuence).ToList();

                //Selecciona solo las que fueron marcadas
                IList<IqReportColumn> reportColumns = View.Model.ReportColumns.Where(f => f.IsSelected).ToList();

                //Si se agrega una de las posibles agregaciones debe agregar las demas por defecto con Count.
                hasAggregation = View.Model.ReportColumns.Any(f => !string.IsNullOrEmpty(f.Options) && f.Options.StartsWith("A:"));


                //Lista con los parametros y where conditions
                DataTable qParams = new DataTable();
                DataSet ds = null;
                qParams.Columns.Add("Col1"); //Parameter
                qParams.Columns.Add("Col2");
                qParams.Columns.Add("Col3"); // caa. Save conditions to the Scheduled Count

                StringBuilder sqlWhere = new StringBuilder("");
                string columPath = "", aggregateColumn = "", aggFunc, groupBy = ""; //Temporar con el path de la columna
                int p = 0; //Contador de paramentros
                System.Data.DataRow dr; //Temporal para los valores del parametro actual
                Object[] curRow;


                //Recorre las Columnas
                foreach (IqReportColumn rc in reportColumns.OrderBy(f => f.NumOrder))
                {
                    if (rc.IsCalculated == true)
                    {
                        dml += rc.ColumnFormula + " as " + rc.Alias + ", ";

                        if (hasAggregation && rc.IsAggregate != true)
                            groupBy += rc.BaseWhere + ", ";  //Base Where is Used for Calculated Fields

                    }

                    else
                    {

                        columPath = rc.ReportTable.Alias + "." + rc.Column.Name;

                        if (hasAggregation && rc.IsAggregate == true) //Si tiene agregacion
                        {
                            if (rc.Options != null && rc.Options.Contains("A:"))
                            {
                                aggregateColumn = GetAggregate(rc.Options, columPath);
                                aggFunc = aggregateColumn.Split('(')[0]; //Sacando el nombre de la funcion de agregacion
                                aggregateColumn += " as " + aggFunc + rc.Alias + ", ";
                            }
                            else
                                aggregateColumn = "Count(" + columPath + ") as Count" + rc.Alias + ", "; //Count por defecto.

                            dml += aggregateColumn;
                        }
                        else
                        {
                            //No tiene Aggregacion                    
                            dml += columPath + " as " + rc.Alias + ", ";
                            if (hasAggregation)
                                groupBy += columPath + ", ";
                        }



                        //Si es un filter acumula el string y los parametros para los filter.
                        if (rc.Options != null && rc.Options.StartsWith("F:"))
                        {

                            dr = qParams.NewRow();
                            curRow = GetParamValues(rc.Options, p, dr);
                            sqlWhere.Append(" AND " + columPath + " " + curRow[0]);

                            dr[0] = "id" + p.ToString();
                            dr[1] = curRow[1];
                            dr[2] = rc.OptionsDesc; // columPath + " = " + curRow[1];

                            //Adicionando las Row al datatable de parametros
                            qParams.Rows.Add(dr);

                            p++;
                        }

                        //Si es un rango debe enviar dos parametros de una vez,
                        else if (rc.Options != null && rc.Options.StartsWith("B:"))
                        {

                            dr = qParams.NewRow();
                            string[] pData = rc.Options.Split(':');
                            sqlWhere.Append(" AND " + columPath + " >= " + ":id" + p.ToString());
                            dr[0] = "id" + p.ToString();
                            dr[1] = pData[1];
                            dr[2] = rc.OptionsDesc; // columPath + " >= " + pData[1];

                            //Adicionando las Row al datatable de parametros
                            qParams.Rows.Add(dr);
                            p++;

                            dr = qParams.NewRow();
                            sqlWhere.Append(" AND " + columPath + " <= " + ":id" + p.ToString());
                            dr[0] = "id" + p.ToString();
                            dr[1] = pData[2];
                            //dr[2] = columPath + " <= " + pData[2];

                            //Adicionando las Row al datatable de parametros
                            qParams.Rows.Add(dr);
                            p++;


                        }



                        //Basic Where - Aplica para todas las columnas
                        if (!string.IsNullOrEmpty(rc.BaseWhere))
                            sqlWhere.Append(" AND " + columPath + " " + rc.BaseWhere);

                    }

                }


                dml = dml.Substring(0, dml.Length - 2); //Para que elimine la coma y el espacio del ultimo valor
                dml += " FROM ";


                //Obtiene las Tablas que usan las columnas
                IList<IqReportTable> tablesUsed = reportColumns.Select(f => f.ReportTable).Distinct().ToList();

                for (int i = 0; i < reportTables.Count; i++)
                {
                    //revisa si la tabla es de nivel 1 y sigue, si es de nivel dos 
                    //y no ha sido usada no necesita meterla en el Join
                    if (reportTables[i].NumLevel > 1 &&
                        tablesUsed.Where(f => f.ReportTableId == reportTables[i].ReportTableId).Count() == 0)
                        continue;

                    if (i == 0)
                        dml += reportTables[i].Table.Name + " as " + reportTables[i].Alias + " ";
                    else
                        dml += reportTables[i].JoinQuery + " " + reportTables[i].Table.Name + " as " + reportTables[i].Alias + " ON " + reportTables[i].WhereCondition + " ";
                }


                //Adiciona Los Filtros - Tratar de preparar el query por optimizacion.
                if (!string.IsNullOrEmpty(sqlWhere.ToString()))
                    dml += " Where 1=1 " + sqlWhere.ToString();

                //Adiciona La Aggregacion
                if (!string.IsNullOrEmpty(groupBy))
                    dml += " Group By " + groupBy.Substring(0, groupBy.Length - 2);


                //Si hay filtros envia el data set, si no va en NULL
                if (qParams.Rows.Count > 0)
                {
                    ds = new DataSet();
                    ds.Tables.Add(qParams);
                }


                DataSet dataSet = service.GetIqReportDataSet(dml, ds);
                // CAA
                View.Model.Query = dml;
                View.Model.QueryParams = ds;
                

                if (dataSet == null)
                {
                    pw.Close();
                    
                    if (showError)
                        Util.ShowError(Util.GetResourceLanguage("NO_DATA_TO_SHOW"));

                    View.NumRecords.Text = Util.GetResourceLanguage("NO_RECORDS_FOUND");
                    return;
                }




                //DataGrid Add Report Columns
                // CAA Según el reporte se habilita o no, opc de seleccionar registros de la grilla
                // Ene 07  2010
                //El roporte acepta procesos como Schedule y otros
                if (!string.IsNullOrEmpty(View.Model.ReportSystem.Process))
                {
                    //Column Check
                    Column curColumn;
                    curColumn = new Column
                    {
                        FieldName = "Mark",
                        Title = "Mark",
                        ReadOnly = false,
                        Width = 35
                    };
                    View.GridDet.Columns.Add(curColumn); 

                    //Nueva Columna Mark en caso de que acepte procesos.
                    dataSet.Tables[0].Columns.Add(new DataColumn { ColumnName = "Mark", ReadOnly = false, DataType = typeof(bool), DefaultValue = true });

                    View.Model.Details = dataSet.Tables[0];
                    AddViewColumns(dataSet.Tables[0].Columns); 
                }
                else
                {
                    AddViewColumns(dataSet.Tables[0].Columns); 
                    View.Model.Details = dataSet.Tables[0];

                    //Si el reporte tiene settings las carga.
                    if (!string.IsNullOrEmpty(View.Model.ReportSystem.Settings))
                    {
                        SettingsRepository settingRep = Util.XmlDeSerializer(View.Model.ReportSystem.Settings);
                        View.GridDet.LoadUserSettings(settingRep, UserSettings.All);
                    }
                }


                View.NumRecords.Text = View.Model.Details.Rows.Count.ToString() + Util.GetResourceLanguage("RECORDS_FOUND");
                

                //Strech Mode
                View.TbView.ColumnStretchMode = ColumnStretchMode.None;
                View.GridDet.Items.Refresh();


                pw.Close();

                View.StkReport.Visibility = Visibility.Collapsed;
                if (View.Model.Details != null && View.Model.Details.Rows.Count > 0)
                    View.StkReport.Visibility = Visibility.Visible;            
                else
                    if (showError)
                        Util.ShowError(Util.GetResourceLanguage("NO_DATA_TO_SHOW"));



            }
            catch (Exception ex)
            {
                pw.Close();
                if (showError)
                    Util.ShowError(Util.GetResourceLanguage("NO_DATA_TO_SHOW1") + "\n" + ex.Message);
            }
        }



        private void LoadProcessToDo()
        {
            View.UcCountSch.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(View.Model.ReportSystem.Process))
                return;

            View.UcCountSch.Query = View.Model.Query;
            View.UcCountSch.QueryParam = View.Model.QueryParams;

            System.Data.DataRow dr;
            DataTable dt = View.Model.Details.Copy();
            dt.Rows.Clear();

            foreach (Object obj in View.GridDet.Items)
            {
                dr = obj as System.Data.DataRow;

                if (bool.Parse(dr["Mark"].ToString()) != true)
                    continue;

                dt.ImportRow(dr);
            }

            View.UcCountSch.Products = dt; //View.Model.Details;
            View.UcCountSch.Visibility = Visibility.Visible;
        }



        private void AddViewColumns(DataColumnCollection columnList)
        {
            Column curColumn;
            foreach (DataColumn column in columnList)
            {
                if (column.ColumnName == "Mark")
                    continue;

                curColumn = new Column
                {
                    FieldName = column.ColumnName,
                    Title = column.ColumnName,
                    TextWrapping = TextWrapping.Wrap,
                    ReadOnly = true                    
                };

                View.GridDet.Columns.Add(curColumn);
            }
        }



        private string GetAggregate(string aggrOp, string column)
        {
            return aggrOp.Split(':')[1].Replace("_val", column);
        }


        private object[] GetParamValues(string columOptions, int numParam, System.Data.DataRow dr)
        {
            //Usada solo para parametros tipo Filtro (F)
            string[] param = columOptions.Split(':');
            //0. Type F or A
            //1. Opeartor Formula reemplaza el template _val por el parametro
            string curOperator = param[1].Trim().Split(' ')[0];
            curOperator = curOperator.Replace("_"," ") + " :id" + numParam.ToString(); //Operador + el parametro  
            //2. DataValue
            string curValue = param[1].Trim().Split(' ')[1].Replace("_val", param[2]);
            return new object[] { curOperator, curValue };  
        }



        private void OnSelectionChanged(object sender, DataEventArgs<IqReport> e)
        {
            ReportSelectedChange(e.Value);
        }

        private void ReportSelectedChange(IqReport rep)
        {

            if (rep == null)
                return;

            IList<IqReportColumn> columnList = new List<IqReportColumn>();
            foreach (IqReportTable rt in rep.ReportTables)
                foreach (IqReportColumn rc in rt.ReportColumns)
                    columnList.Add(rc);

            View.Model.AllColumns = columnList.OrderBy(f => f.NumOrder).ToList();


            //Habilitando O Desabilitando el Boton de Delete
            View.BtnDelRep.IsEnabled = false;
            if (rep.IsForSystem == false && rep.CreatedBy == App.curUser.UserName)
                View.BtnDelRep.IsEnabled = true;


            LoadReportFilters();


            //Habilitando O Desabilitando el Boton de Update
            //View.BtnUpdRep.IsEnabled = false;
            //if (rep.IsForSystem == false && rep.CreatedBy == App.curUser.UserName)
            //    View.BtnUpdRep.IsEnabled = true;
        }



        private void LoadReportFilters()
        {
            View.StkFilters.Children.Clear();

            if (View.Model.AllowFilterColumns == null || View.Model.AllowFilterColumns.Count == 0)
                return;

            //Carga dinamicamente los UC control para poder filtrar la cosulta.
            QueryFilter ucObj = null;
            foreach (IqReportColumn col in View.Model.AllowFilterColumns)
            {
                ucObj = new QueryFilter();
                ucObj.cboStrComp.SelectedValue = " = _val";
                ucObj.RepColumn = col;
                View.StkFilters.Children.Add(ucObj);
            }
        }



        //INQUIRY SHORTCUT
        public bool LoadShortCut(string look, int iqRepCol) {

            IqReportColumn rc = service.GetIqReportColumn(new IqReportColumn { ReportColumnId = iqRepCol, IsSelected = true }).First();
            
            View.Model.ReportSystem = service.GetIqReport(new IqReport { ReportId = rc.ReportTable.Report.ReportId }).First();
            ReportSelectedChange(View.Model.ReportSystem);

            //Load The Filter - El filtro por la opcion escogida.
            View.Model.AllColumns.Where(f => f.ReportColumnId == iqRepCol).First().Options = "F: = _val:" + look;
            //Adicionalmente adicionando el filtro de la bodega. 47 (valor fixed)
            View.Model.AllColumns.Where(f => f.ReportColumnId == 47).First().Options = "F: = _val:" + App.curLocation.ErpCode;



            
            View.Model.AllColumns = View.Model.AllColumns;
            //View.LvFilters.Items.Refresh();

            RunReport("", false);

            return (View.Model.Details != null && View.Model.Details.Rows.Count > 0) ? true : false;
        }



    }

    public interface IQueriesPresenter
    {
        IQueriesView View { get; set; }

        ToolWindow Window { get; set; }

        void Initialize();

        bool LoadShortCut(string look, int iqRepCol);
    }




}