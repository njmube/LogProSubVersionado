using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows; using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows;
using WpfFront.Services;
using System.Linq;
using System.Windows.Input;
//using System.Windows.Controls.Primitives;

namespace WpfFront.Presenters
{

    /// <summary>
    /// Jairo Murillo: Oct 08 2009. Se Modifico su uso solo para receiving
    /// Jairo Murillo: Nov 12 2009. Se Depuraron lo smetodos viejso en la region OLD, Se adicion el metodo
    ///                             CreateUniqueTrackLabel (para manejar los seriales con tipolabel 1005)
    /// </summary>


    public interface ITrackOptionPresenter
    {
        ITrackOptionView View { get; set; }
        //void SetupManualTrackOption();
        ToolWindow Window { get; set; }
    }
    

    public class TrackOptionPresenter : ITrackOptionPresenter
    {
        public ITrackOptionView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        ProcessWindow pw = null;
        public ToolWindow Window { get; set; }


        public TrackOptionPresenter(IUnityContainer container, ITrackOptionView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            //this.region = region;
            View.Model = this.container.Resolve<TrackOptionModel>();

            //Event Delegate
            View.AddManualTrackToList += new EventHandler<EventArgs>(this.OnAddManualTrackToList);
            View.RemoveManualTrack += new EventHandler<EventArgs>(this.OnRemoveManualTrack);
            View.LoadSetup += new EventHandler<EventArgs>(View_LoadSetup);
            View.LoadUniqueTracks += new EventHandler<DataEventArgs<Label>>(View_LoadUniqueTracks);
            View.RemoveUniqueTrack += new EventHandler<EventArgs>(View_RemoveUniqueTrack);
            View.UpdateTrackValue += new EventHandler<DataEventArgs<object[]>>(View_UpdateTrackValue);


            View.StkPrdDesc.Visibility = Visibility.Collapsed;
            view.BtnTrackRemove.Visibility = Visibility.Collapsed;
            View.StkAddTrack.Visibility = Visibility.Collapsed;

        }



        void View_UpdateTrackValue(object sender, DataEventArgs<object[]> e)
        {
            //Actualiza el Valor del Track Option para un label especifico.
            try
            {
                LabelTrackOption lblTrak = service.GetLabelTrackOption(new LabelTrackOption
                {
                    Label = new Label { LabelID = long.Parse(e.Value[0].ToString()) }
                }).First();
                
                lblTrak.TrackValue = e.Value[1].ToString();

                service.UpdateLabelTrackOption(lblTrak);
            }
            catch { }
        }






        /// <summary>
        /// Carga el listado de seriales que esa caja contiene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void View_LoadUniqueTracks(object sender, DataEventArgs<Label> e)
        {
            if (e.Value == null || View.Model.MaxTrackQty != 1)
                return;

            View.Model.CurLabel = e.Value;

            View.Model.UniqueTrackList = new List<Label>();


            //View.StkUniqueLabels.Visibility = Visibility.Collapsed;
            //if (View.Model.CurLabel.BaseStartQty > 1)
            //{
            //LLamar el listado de labels hijos
            View.Model.UniqueTrackList = service.GetLabel(
                new Label
                {
                    FatherLabel = new Label { LabelID = View.Model.CurLabel.LabelID },
                    LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel }
                });
            View.StkUniqueLabels.Visibility = Visibility.Visible;
            //}

            if (View.Model.UniqueTrackList == null)
                View.Model.UniqueTrackList = new List<Label>();

            View.BtnUniqueRem.Visibility = Visibility.Collapsed;
            if (View.Model.CurLabel.Node.NodeID == NodeType.Received)
                View.BtnUniqueRem.Visibility = Visibility.Visible;

        }



        void View_LoadSetup(object sender, EventArgs e)
        {
            View.StkAddTrack.Visibility = Visibility.Visible;
            
            SetupManualTrackOptionReceiving();

            View.BtnAddTrack.IsEnabled = true;
            if (View.Model.RemainingQty <= 0)
                View.BtnAddTrack.IsEnabled = false;
        }



        //Opcion creada especificamente para receiving.
        public void SetupManualTrackOptionReceiving()
        {

            if (View.Model.Product == null) //Solo controla el producto en picking
            {
                Util.ShowError("Product not Selected.");
                return;
            }

            ProcessWindow("Loading Tracking Options ...", false);

            //loading track Options
            //Slo muestra las opciones que tenga is required, esas son de datos a entrar las demas son de alertas.
            //View.Model.TrackData = View.Model.Product.ProductTrack.Where(f => f.IsRequired == true).ToList();
            View.Model.TrackData = View.Model.Product.ProductTrack
                .Where(f=>f.TrackOption.DataType.DataTypeID != SDataTypes.ProductQuality).ToList();

            View.Model.MaxTrackQty = 0;
            View.Model.RemainingQty = 0;
            View.StkUniqueLabels.Visibility = Visibility.Collapsed;

            //Detectanto si hay alguna track unique que restrinja el maximo qty a entrar en 1 each.
            for (int i = 0; i < View.Model.TrackData.Count; i++)
                if (View.Model.TrackData[i].IsUnique == true)
                {
                    View.Model.MaxTrackQty = 1;
                    break;
                }

            //Load Label List Columns
            IList<ProductTrackRelation> listTrack = View.Model.TrackData; //.Where(f => f.TrackOption.Name == col.Header.ToString()).ToList();
            

            //1. Se arma el subcontrol que pide dato y el listview que muestra la info.

            System.Windows.Controls.GridViewColumn[] grid =
                new System.Windows.Controls.GridViewColumn[((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Count];
            ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.CopyTo(grid, 0);

            int curColumn = 0;
            int offset = 3; //Numero de columnas fijas
            //Arma le grid que pedira los datos de Tracking
            foreach (System.Windows.Controls.GridViewColumn col in grid)
            {

                if (col != null)
                {

                    //si es el barcode pero estamos adicionando se remueve el barcode de las columnas
                    if (col.Header.ToString().Contains("Pack Barcode"))
                        continue;

                    //si es la columan product, continua porque siempre se meustra
                    if (col.Header.ToString().Contains("Product"))
                        continue;

                    //si es la columan qty, continua porque siempre se meustra
                    if (col.Header.ToString().Contains("Quantity"))
                        continue;

                    //if (col.Header.ToString().Contains("_"))
                        //continue;
                }

                //Solo se muestran las columnas que no sean unicas,
                //Ya que si es unica el valor unico lo toma el LabelCode (ej: SerialNumber)
                if (curColumn < View.Model.TrackData.Count && listTrack[curColumn].TrackOption.IsUnique == false)
                {
                    //Cambia el Header  y el with
                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset].Header = listTrack[curColumn].TrackOption.DisplayName;

                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset].Width = 130;

                }
                else  //Si ya estan todos los elementos del trackData Remueve las demas columnas.
                {
                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset].Header = "";

                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset].Width = 0;

                }
                    //((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Remove(col);
                //((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset];


                curColumn++;
            }


            //Si no hay cantidad pendiente debe cargar el listado de labels con los labels missing
            //Y ejecutar el proceso sobre estos labels.
            UpdateTrackData();

            pw.Close();

            //View.TxtQtyTrack.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
        }

 


        //23 Marzo 09 - Update of tracking data
        private void UpdateTrackData()
        {
            try
            {
                //Carga en la lista de labels, los labels que pertenecen a este documento para incluir su track option
                NodeTrace patterTrace = new NodeTrace
                {
                    Document = View.Model.Document,
                    Node = View.Model.Node,
                    Status = new Status { StatusID = EntityStatus.Active },
                    Label = new Label {  Product = View.Model.Product, Status = new Status { StatusID = EntityStatus.Active } }
                };

                //Lista de labels recibidos
                View.Model.ManualTrackList = service.GetNodeTrace(patterTrace)
                    .Select(f => f.Label).ToList();

                View.ManualTrackList.Items.Refresh();
                if (View.Model.ManualTrackList.Count == 1)
                    View.ManualTrackList.SelectedIndex = 0;

                //Seleccionar los limites de ingreso de datos, cuenta los labels que tengan
                //el dato en blanco indicando que estan pendientes.    
                TrackOption trackName = null; 
                string curTrackVal = ""; //guarda el cur value delegate track data
                int tmpCount = 0;


                //Calculando el remaining depende si es unico el track o no.
                if (View.Model.MaxTrackQty == 1) //Si es Unico
                {
                    try { View.Model.RemainingQty = (int)View.Model.ManualTrackList.Where(f => f.LabelType.DocTypeID == LabelType.ProductLabel).Sum(f => f.CurrQty); }
                    catch { }
                    
                    View.TxtQtyTrack.Text = View.Model.MaxTrackQty.ToString();
                    View.Model.TrackUnit = View.Model.Product.BaseUnit;
                    View.TxtQtyTrack.IsEnabled = false;

                }
                else //Si no.
                {

                    for (int i = 0; i < View.Model.TrackData.Count; i++)
                    {
                        tmpCount = 0;
                        trackName = View.Model.TrackData[i].TrackOption;

                        //Verison Anterior
                        //Obtiene los trackoption missing.
                        //tmpCount = View.Model.ManualTrackList
                        //    .Where(f => f.GetType().GetProperty(trackName).GetValue(f, null) == null
                        //        || f.GetType().GetProperty(trackName).GetValue(f, null).ToString().Trim() == "")
                        //        .Count();

                        //version 14 abril 2009, trackOption del label en otra tabla
                        foreach (Label trLabel in View.Model.ManualTrackList)
                        {

                            try { curTrackVal = trLabel.TrackOptions.Where(f => f.TrackOption.RowID == trackName.RowID).First().TrackValue; }
                            catch { curTrackVal = ""; }

                            if (string.IsNullOrEmpty(curTrackVal))
                                tmpCount++;
                        }

                        //Vuelve la cantidad pendiente como la mayor de las dos, Aplica para no UNIQUE track
                        View.Model.RemainingQty = tmpCount > View.Model.RemainingQty ? tmpCount : View.Model.RemainingQty;
                    }

                    View.TxtQtyTrack.Text = View.Model.RemainingQty.ToString();
                    View.TxtQtyTrack.IsEnabled = true;
                }


                //Cantidad pendiente por poner el tracking
                //if (View.Model.MaxTrackQty == 1)
                //{ //Si es unique debe pedir 1 a 1 hasta todas las unidades basicas
                //    View.TxtQtyTrack.Text = View.Model.MaxTrackQty.ToString();
                //    View.Model.TrackUnit = View.Model.Product.BaseUnit;
                //    View.TxtQtyTrack.IsEnabled = false;
                //}
                //else
                //{
                //    View.TxtQtyTrack.Text = View.Model.RemainingQty.ToString();
                //    View.TxtQtyTrack.IsEnabled = true;
                //}

            }
            catch (Exception ex)
            {
                Util.ShowError("Problem loading received list.\n" + ex.Message);
                return;
            }

        }
       


        private void OnAddManualTrackToList(object sender, EventArgs e)
        {
            //Check If Qty is Valid
            int qty;
            if (!int.TryParse(View.TxtQtyTrack.Text, out qty) || int.Parse(View.TxtQtyTrack.Text) <= 0)
            {
                Util.ShowError("Please enter a valid quantity.");
                return;
            }


            //Check remaining Qty
            if (View.Model.RemainingQty == 0)
            {
                Util.ShowError("Package is full, qty remaining is zero.");
                return;
            }


            //Check remaining Qty
            if (qty > View.Model.RemainingQty)
            {
                Util.ShowError("Qty Remaining is " + View.Model.RemainingQty.ToString() + " please fix.");
                return;
            }


            ProcessWindow("Updating Tracking Options ...", false);            


            TrackOption trackName = null;
            string curTrackVal = "";

            //Revisa que hayan datos de track ingresados y sean validos

            foreach (ProductTrackRelation trackRel in View.Model.TrackData)
            {
                if (string.IsNullOrEmpty(trackRel.TempValue) && trackRel.IsUnique != true)
                {
                    pw.Close();
                    Util.ShowError("Please enter valid track information for " + trackRel.TrackOption.DisplayName);
                    return;
                }
                else if (string.IsNullOrEmpty(trackRel.TempValue))
                {
                    pw.Close();
                    if (!UtilWindow.ConfirmOK("Wish to enter a blank "+ trackRel.TrackOption.DisplayName) == true)
                        return;

                    trackRel.TempValue = WmsSetupValues.AutoSerialNumber;
                }


                //VALIDACION DEL TRACK LABEL CUANDO ES UNICO
                //Si en un unique data like a serial debe validar que no sea un dato existente en la DB
                //ni en los ingresado en la lista actual.
                if (trackRel.IsUnique == true)
                {

                    if (trackRel.TempValue != WmsSetupValues.AutoSerialNumber)
                    {
                        //Chequea hay labels con esa relacion
                        int existsInTrack = service.GetLabelTrackOption(new LabelTrackOption
                        {
                            TrackValue = trackRel.TempValue.Trim(),
                            TrackOption = trackRel.TrackOption,
                            Label = new Label
                            {
                                Product = new Product { ProductID = View.Model.Product.ProductID },
                                Status = App.EntityStatusList.Where(f => f.StatusID == EntityStatus.Active).First()
                            }
                        }).Count;


                        //Chequea hay labels con ese barcode para el mismo producto
                        int existsInLabel = service.GetLabel(new Label
                        {
                            LabelCode = trackRel.TempValue.Trim(),
                            Product = new Product { ProductID = View.Model.Product.ProductID },
                            Status = App.EntityStatusList.Where(f => f.StatusID == EntityStatus.Active).First()
                        }).Count;


                        //Mensaje para receiving - valida que no exista ya el label en el sistema
                        if ((existsInTrack > 0 || existsInLabel > 0) && View.Model.TrackType == 0)
                        {
                            pw.Close();
                            Util.ShowError("Product with the " + trackRel.TrackOption.DisplayName + " " + trackRel.TempValue + " already exists.");
                            return;
                        }

                        trackName = trackRel.TrackOption;

                        //version 14 abril 2009, trackOption del label en otra tabla
                        foreach (Label trLabel in View.Model.ManualTrackList)
                        {

                            try
                            {
                                curTrackVal = trLabel.TrackOptions.Where(f => f.TrackOption.RowID == trackName.RowID).First().TrackValue;
                                if (curTrackVal.Trim().Equals(trackRel.TempValue.Trim()))
                                {
                                    pw.Close();
                                    Util.ShowError("Product with the " + trackRel.TrackOption.DisplayName + " " + trackRel.TempValue + " was already added.");
                                    return;
                                }
                            }
                            catch { }

                        }
                    }

                    //Crea el Label Unico
                    if (!CreateUniqueTrackLabel(trackRel.TempValue))
                    {
                        //Limpiando los Values
                        for (int i = 0; i < View.Model.TrackData.Count; i++)
                            View.Model.TrackData[i].TempValue = "";

                        return;
                    }

                    trackRel.TempValue = "";
                }
              
            }

            //Proceso de Creacion o actualizacion de label segun sea el caso.
            //Para receiving

            bool processOk = true;

            if (View.Model.MaxTrackQty != 1)  //(View.Model.TrackType == 0)               
                UpdateReceivingManualTrackList(qty);
           

            //Proceso final 
            if (processOk)
            {
                View.ManualTrackList.Items.Refresh();
                View.Model.RemainingQty -= qty;

                //Ajustando las cantidades
                if (View.Model.MaxTrackQty == 1)
                    View.TxtQtyTrack.Text = View.Model.MaxTrackQty.ToString();
                else
                    View.TxtQtyTrack.Text = View.Model.RemainingQty.ToString();


                if (View.Model.RemainingQty <= 0)
                    View.BtnAddTrack.IsEnabled = false;

            }


            //Limpiando los Values
            for (int i = 0; i < View.Model.TrackData.Count; i++)
                View.Model.TrackData[i].TempValue = "";

            //Pone el Focus en el elemento del Grid para entrar el siguiente dato            
            pw.Close();
            //View.LvTrackProduct.Focus();

        }


        /// <summary>
        /// Permte Crear un label de tipo 1005 que contiene seriales de determinado producto.
        /// Se crea Solo 1. Y El label father es el que lo contiene que debe estar seleccionado.
        /// </summary>
        private bool CreateUniqueTrackLabel(string labelCode)
        {

            //Si no hay label selected el sistema busca uno por defecto, debe ser uno que tenga 
            //StarQty = 1 para poderlo reemplazar.
            if (View.Model.CurLabel == null) {
                try
                {
                    View.Model.CurLabel = View.Model.ManualTrackList.Where(f=>f.StartQty==1 && 
                       f.LabelType.DocTypeID == LabelType.ProductLabel ).First();
                }
                catch { }
            }

            if (View.Model.CurLabel == null || View.Model.CurLabel.BaseCurrQty == 0)
            {
                pw.Close();
                Util.ShowError("No Pack selected or current Pack is full.");
                return false;
            }

            try
            {
                //Crea el nuevo Label identico pero con el dato ingresado y Father Label el CurLabel.
                Label uniqueLabel = service.CreateUniqueTrackLabel(View.Model.CurLabel, labelCode);


                if (View.Model.CurLabel.StartQty == 1 && View.Model.CurLabel.Unit.BaseAmount == 1)
                {
                    //si curLabel.CurrQty = 1 (El label es reemplazado en la lista principal, si no se va para la lista de la derecha)
                    View.Model.ManualTrackList.Remove(View.Model.CurLabel);
                    View.Model.CurLabel = null;
                    View.Model.ManualTrackList.Add(uniqueLabel);
                    View.ManualTrackList.Items.Refresh();
                }
                else
                {

                    //Aqui se debe cambiar la unidad basica para poder llevar el conteo de las unidades individualmente
                    //3 Dic 2009
                    if (View.Model.CurLabel.Unit.BaseAmount > 1)
                    {
                        
                        View.Model.CurLabel.StartQty = View.Model.CurLabel.StartQty * View.Model.CurLabel.Unit.BaseAmount;
                        View.Model.CurLabel.CurrQty = View.Model.CurLabel.CurrQty * View.Model.CurLabel.Unit.BaseAmount;
                        View.Model.CurLabel.Unit = View.Model.CurLabel.Product.BaseUnit;
                    }

                    
                    //Disminuye el Current Qty del Label padre.
                    View.Model.CurLabel.CurrQty--;
                    if (View.Model.CurLabel.CurrQty == 0)
                        View.Model.CurLabel = null;

                    View.Model.UniqueTrackList.Add(uniqueLabel);

                    View.StkUniqueLabels.Visibility = Visibility.Visible;
                    View.UniqueTrackList.Items.Refresh();
                }


                return true;

            }
            catch (Exception ex)
            {
                pw.Close();
                Util.ShowError("Record could not be added.\n" + ex.Message);
                return false;
            }

        }



        //23 Marzo 09. Actualiza las trackoption de producto ya recibido.
        private void UpdateReceivingManualTrackList(int qty)
        {
            int h = 1;


            //Obtiene los labels a modificar.
            IList<Label> labelToUpdate = new List<Label>(); //View.Model.ManualTrackList;
            TrackOption trackName = null;
            string curTrackVal;

            for (int i = 0; i < View.Model.TrackData.Count; i++)
            {
                trackName = View.Model.TrackData[i].TrackOption;

                foreach (Label trLabel in View.Model.ManualTrackList)
                {

                    try { curTrackVal = trLabel.TrackOptions.Where(f => f.TrackOption.RowID == trackName.RowID).First().TrackValue; }
                    catch { curTrackVal = ""; }

                    if (string.IsNullOrEmpty(curTrackVal) && labelToUpdate.Where(f=>f.LabelID == trLabel.LabelID).Count() == 0) 
                        labelToUpdate.Add(trLabel);
                }

                //Obtiene los trackoption missing.
                //labelToUpdate = labelToUpdate
                //    .Where(f => f.GetType().GetProperty(trackName).GetValue(f, null) == null
                //        || f.GetType().GetProperty(trackName).GetValue(f, null).ToString().Trim() == "")
                //        .ToList();
            }

            //LabelTrackOption curTrack;
            //IList<LabelTrackOption> curLabelTrack;

            //Obtiene la lista de los pendientes por ingresar el track option.
            Label trackedLabel = null;
            foreach (Label curLabel in labelToUpdate)
            {
                foreach (ProductTrackRelation trackRel in View.Model.TrackData)
                    trackedLabel = service.UpdateLabelTracking(curLabel, trackRel.TrackOption, trackRel.TempValue.Trim(), App.curUser.UserName); 
                
                /*
                //Si el label ya tiene track options los trae.
                curLabelTrack = curLabel.TrackOptions == null ? new List<LabelTrackOption>() : curLabel.TrackOptions;

                //Asigna los valores ingresados de las TrackOption al correspodiente campo del label
                foreach (ProductTrackRelation trackRel in View.Model.TrackData)
                {
                    //curLabel.GetType().GetProperty(trackRel.TrackOption.Name).SetValue(curLabel, trackRel.TempValue, null);

                    if (curLabelTrack.Where(f => f.TrackOption.RowID == trackRel.TrackOption.RowID).Count() == 0)
                    {
                        curTrack = new LabelTrackOption
                        {
                            Label = curLabel,
                            CreatedBy = App.curUser.UserName,
                            CreationDate = DateTime.Now,
                            TrackOption = trackRel.TrackOption,
                            TrackValue = trackRel.TempValue.Trim()
                        };

                        curLabelTrack.Add(curTrack);

                    }
                    else //Si ya existe un registro con la informacion para ese trackoption
                    {
                        curLabelTrack.Where(f => f.TrackOption.RowID == trackRel.TrackOption.RowID).First().Label = curLabel;
                        curLabelTrack.Where(f => f.TrackOption.RowID == trackRel.TrackOption.RowID).First().CreatedBy = "";
                        curLabelTrack.Where(f => f.TrackOption.RowID == trackRel.TrackOption.RowID).First().CreationDate = DateTime.Now;
                        curLabelTrack.Where(f => f.TrackOption.RowID == trackRel.TrackOption.RowID).First().TrackOption = trackRel.TrackOption;
                        curLabelTrack.Where(f => f.TrackOption.RowID == trackRel.TrackOption.RowID).First().TrackValue = trackRel.TempValue.Trim();
                    }
                }

                //Adicionando las track options
                curLabel.TrackOptions = curLabelTrack.ToList();

                //Update Label
                curLabel.ModDate = DateTime.Now;
                curLabel.ModifiedBy = App.curUser.UserName;
                curLabel.Printed = View.Model.MaxTrackQty == 1 ? true : false;
                service.UpdateLabel(curLabel);    
                 * */

                //Remueve el viejo de la lista e ingresa el nuevo
                View.Model.ManualTrackList
                    .Remove(View.Model.ManualTrackList.Where(f => f.LabelID == curLabel.LabelID)
                    .First());

                View.Model.ManualTrackList.Add(trackedLabel);

                //h += (int)curLabel.CurrQty; //MODIFICADO SEP 14.
                h++;
                if (h > qty)
                    break;
            }

            View.Model.ManualTrackList = View.Model.ManualTrackList
                .OrderByDescending(f => f.ModDate).ToList();
            View.ManualTrackList.Items.Refresh();

        }


        private void OnRemoveManualTrack(object sender, EventArgs e)
        {

            if (View.ManualTrackList.SelectedItems == null)
                return;


            foreach (object lineObject in View.ManualTrackList.SelectedItems)
            {
                View.Model.ManualTrackList.Remove((Label)lineObject);
                View.Model.RemainingQty++;
            }

            View.ManualTrackList.Items.Refresh();

            if (View.Model.MaxTrackQty > 1)
                View.TxtQtyTrack.Text = View.Model.RemainingQty.ToString();

            View.BtnAddTrack.IsEnabled = true;

        }


        //03 - Marzo 2009 - Ventana de proceso
        private void ProcessWindow(string msg, bool closeBefore)
        {
            if (closeBefore)
                pw.Close();

            pw = new ProcessWindow(msg);
        }


        void View_RemoveUniqueTrack(object sender, EventArgs e)
        {

            if (View.UniqueTrackList.SelectedItems == null)
                return;


            foreach (object lineObject in View.UniqueTrackList.SelectedItems)
            {
                View.Model.UniqueTrackList.Remove((Label)lineObject);


                if (View.Model.CurLabel == null)
                    View.Model.CurLabel = View.ManualTrackList.SelectedItem as Label;

                //No hay label para poder cancelar
                if (View.Model.CurLabel == null)
                    return;

                Label dbLabel = service.GetLabel(new Label { LabelID = View.Model.CurLabel.LabelID }).First();

                View.Model.CurLabel.CurrQty = dbLabel.CurrQty + 1;
                View.Model.RemainingQty++;

                //Va al sistema a eliminar el label
                service.DeleteLabel((Label)lineObject);
                service.UpdateLabel(View.Model.CurLabel);
            }

            View.UniqueTrackList.Items.Refresh();

            if (View.Model.MaxTrackQty != 1)
                View.TxtQtyTrack.Text = View.Model.RemainingQty.ToString();

            if (View.Model.RemainingQty > 0)
                View.BtnAddTrack.IsEnabled = true;

        }


        #region OLD

        /*
         * 
        private void EnterNewTrackData()
        {
            Unit selectedUnit = View.Model.CurUnit; 

            //Cantidad pendiente por poner el tracking
            if (View.Model.MaxTrackQty == 1)
            { 
                //Si es unique debe pedir 1 a 1 hasta todas las unidades basicas
                View.Model.RemainingQty = View.Model.QtyPerPack * View.Model.QtyToTrack * (Int32)selectedUnit.BaseAmount;
                View.TxtQtyTrack.Text = View.Model.MaxTrackQty.ToString();
                View.Model.TrackUnit = View.Model.Product.BaseUnit;
                View.TxtQtyTrack.IsEnabled = false;
            }
            else
            {
                View.Model.RemainingQty = View.Model.QtyPerPack * View.Model.QtyToTrack;
                View.TxtQtyTrack.Text = View.Model.RemainingQty.ToString();
                View.Model.TrackUnit = selectedUnit;
                View.TxtQtyTrack.IsEnabled = true;
            }

            //Inicia con el total de Qty
            View.Model.ManualTrackList = new List<Label>();
            View.BtnAddTrack.IsEnabled = true;
        }         
         * 
         * 
               //23 mArzo 09 - se separo comp un procedo atomico,para reuso.
        //permite cargar las opciones de track del producto actual, cuando elproducto esta
        //recibido carga las opciones de los recibidos.
        //APLica solo para Picking 12/SEP/2009
        public void SetupManualTrackOption()
        {
            //DEFINICIONES PARA PICKING
            if (View.Model.TrackType == 1)
            {
                View.UcProduct.Visibility = Visibility.Collapsed;
                View.StkPrdDesc.Visibility = Visibility.Visible;
                View.BtnTrackRemove.Visibility = Visibility.Visible;
                View.StkAddTrack.Visibility = Visibility.Visible;
                View.TxtMsgQty.Text = "Qty to Pick";
            }


            if (View.Model.Product == null) //Solo controla el producto en picking
            {
                Util.ShowError("Product not Selected.");
                return;
            }

            ProcessWindow("Loading Tracking Options ...", false);

            //loading track Options
            //Slo muestra las opciones que tenga is required, esas son de datos a entrar las demas son de alertas.
            //View.Model.TrackData = View.Model.Product.ProductTrack.Where(f=>f.IsRequired == true).ToList();

            //Load Label List Columns
            IList<ProductTrackRelation> listTrack;



            //1. Se arma el subcontrol que pide dato y el listview que muestra la info.

            System.Windows.Controls.GridViewColumn[] grid =
                new System.Windows.Controls.GridViewColumn[((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Count];
            ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.CopyTo(grid, 0);

            int curColumn = 0;
            int offset = 3; //Numero de columnas fijas
            //Arma le grid que pedira los datos de Tracking
            foreach (System.Windows.Controls.GridViewColumn col in grid)
            {

                //si es el barcode pero estamos adicionando se remueve el barcode de las columnas
                if (col.Header.ToString().Contains("Barcode"))
                {
                    //if (View.Model.CurQtyPending > 0 ) {
                    //    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Remove(col);
                    //    offset--;
                    //}
                    continue;
                }

                //si es la columan product, continua porque siempre se meustra
                if (col.Header.ToString().Contains("Product"))
                    continue;

                //si es la columan qty, continua porque siempre se meustra
                if (col.Header.ToString().Contains("Quantity"))
                    continue;


                listTrack = View.Model.TrackData; //.Where(f => f.TrackOption.Name == col.Header.ToString()).ToList();


                if (curColumn < View.Model.TrackData.Count)
                {
                    //Cambia el Header  y el with
                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset].Header = listTrack[curColumn].TrackOption.DisplayName;

                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns[curColumn + offset].Width = 130;

                }
                else  //Si ya estan todos los elementos del trackData Remueve las demas columnas.
                    ((System.Windows.Controls.GridView)View.ManualTrackList.View).Columns.Remove(col);


                curColumn++;
            }


            //2. Detectanto si hay alguna track unique que restrinja el maximo qty a entrar en 1 each.
            for (int i = 0; i < View.Model.TrackData.Count; i++)
                if (View.Model.TrackData[i].IsUnique == true)
                {
                    View.Model.MaxTrackQty = 1;
                    break;
                }


            //Proceso cuando hay cantidad pendiente - debe recibir los labels que faltan,
            if (View.Model.CurQtyPending > 0)
            {
                EnterNewTrackData();
                pw.Close();
                return;
            }

            //Si no hay cantidad pendiente debe cargar el listado de labels con los labels missing
            //Y ejecutar el proceso sobre estos labels.
            UpdateTrackData();

            pw.Close();

        }
         * 
         * 
         * 
        //Ingresa nuevos track options a la lista de labels para luego ser recibidos.
        private void AddReceivingManualTrackToList(int qty)
        {
            Label curLabel;
            IList<LabelTrackOption> curLabelTrack;
            LabelTrackOption curTrack;

            //Si es unique pasa la unidad basica a Each, Si no usa la que ingreso el user
            Unit selectedUnit = (View.Model.MaxTrackQty == 1) ? View.Model.Product.BaseUnit : View.Model.CurUnit; //(Unit)View.ComboUnit.SelectedItem;

            //for (int i = 0; i < qty; i++)
            //{

            curLabel = new Label
            {
                Product = View.Model.Product,
                Unit = selectedUnit, //View.Model.TrackUnit,
                Bin = View.Model.Bin,
                IsLogistic = false,
                LabelType = new DocumentType { DocTypeID = LabelType.ProductLabel },
                Node = new Node { NodeID = NodeType.PreLabeled },
                Printed = View.Model.MaxTrackQty == 1 ? true : false,
                StartQty = qty * selectedUnit.BaseAmount,
                CurrQty = qty * selectedUnit.BaseAmount,
                //UnitBaseFactor = View.Model.CurUnit.BaseAmount, //((Unit)View.ComboUnit.SelectedItem).BaseAmount,
                Status = new Status { StatusID = EntityStatus.Active },
                Barcode = "",
                Name = "",
                CreatedBy = App.curUser.UserName,
                CreationDate = DateTime.Now
            };

            curLabelTrack = new List<LabelTrackOption>();

            //Asigna los valores ingresados de las TrackOption al correspodiente campo del label
            foreach (ProductTrackRelation trackRel in View.Model.TrackData)
            {
                //curLabel.GetType().GetProperty(trackRel.TrackOption.Name).SetValue(curLabel, trackRel.TempValue, null);
                curTrack = new LabelTrackOption
                {
                    Label = curLabel,
                    CreatedBy = App.curUser.UserName,
                    CreationDate = DateTime.Now,
                    TrackOption = trackRel.TrackOption,
                    TrackValue = trackRel.TempValue.Trim()
                };

                curLabelTrack.Add(curTrack);
            }

            //Assignado las track options del label
            curLabel.TrackOptions = curLabelTrack.ToList();
            View.Model.ManualTrackList.Add(curLabel);
            //}

        }
        */
        #endregion

    }
}
