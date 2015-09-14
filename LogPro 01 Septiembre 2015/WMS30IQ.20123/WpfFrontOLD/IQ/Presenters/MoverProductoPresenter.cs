using System;
using WpfFront.Models;
using WpfFront.Views;
using Assergs.Windows;
using WMComposite.Events;
using Microsoft.Practices.Unity;
using WpfFront.WMSBusinessService;
using WpfFront.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using WpfFront.Services;
using System.Linq;
using System.Data;
using System.Reflection;
using Microsoft.Windows.Controls;
using WpfFront.Common.Windows;
using System.Text;

namespace WpfFront.Presenters
{

    public interface IMoverProductoPresenter
    {
        IMoverProductoView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class MoverProductoPresenter : IMoverProductoPresenter
    {
        public IMoverProductoView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }
        public int offset = 3; //# columnas que no se debe replicar porque son fijas. 
        //Crear Nuevo obejto Etiqueta
        private Common.Tools.Etiquetas Etiquetas = new Common.Tools.Etiquetas();
        //Varibles Auxiliares
        private string Default_Value { get; set; }
        WpfFront.WMSBusinessService.Label EvalSerialLabel;


        public MoverProductoPresenter(IUnityContainer container, IMoverProductoView view)
        {

            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<MoverProductoModel>();

            #region Metodos

            #region Header

            View.LoadBinFrom += new EventHandler<DataEventArgs<Location>>(this.OnLoadBinFrom);
            View.LoadLocationTo += new EventHandler<DataEventArgs<Bin>>(this.OnLoadLocationTo);
            View.LoadBinTo += new EventHandler<DataEventArgs<Location>>(this.OnLoadBinTo);
            View.LoadDocumentData += new EventHandler<DataEventArgs<BinRoute>>(this.OnLoadDocumentData);
            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.NewBasicData += new EventHandler<EventArgs>(this.OnNewBasicData);
            View.CancelBasicData += new EventHandler<EventArgs>(this.OnCancelBasicData);
            View.Cbx_Etiqueta1_SelectedValue += new EventHandler<DataEventArgs<MMaster>>(this.OnCbx_Etiqueta1_SelectedValue);
            #endregion

            #region Serial

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);
            View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);

            #endregion

            #region Details

            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ReplicateDetails += new EventHandler<EventArgs>(View_ReplicateDetails);
            View.MostrarInformacion += new EventHandler<EventArgs>(this.OnMostrarInformacion); //MostrarInformacion.
            View.Impresion_1 += new EventHandler<EventArgs>(this.Impresion_Etiqueta1);
            View.Impresion_2 += new EventHandler<EventArgs>(this.Impresion_Etiqueta2);

            #endregion

            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document { CreatedBy = App.curUser.UserName };
            View.Model.LocationFromList = App.curUser.UserRols.Select(f => f.Location).Distinct().ToList();
            View.Model.ListLabelScann = new List<WpfFront.WMSBusinessService.Label>();
            View.Model.ListDataInformation = new List<DataInformation>();

            #endregion

        }


        void View_ReplicateDetails(object sender, EventArgs e)
        {
            //Recorre la primera linea y con esa setea el valor de las demas lineas.
            for (int i = 1; i < View.Model.ListRecords.Rows.Count; i++)
                for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                    View.Model.ListRecords.Rows[i][z] = View.Model.ListRecords.Rows[0][z];
        }


        #region Header

        private void GuardarDocumentoDataInformation()
        {
            Object ChildrenValue, ChildrenLabel;
            ShowData HeaderDataSave;
            string XmlData;
            string CodigoCampo;
            DataDefinition IsRequiredField;
            bool ControlIsRequired;
            //Inicializo la lista de los datos a convertir en Xml
            View.Model.ListHeaderDataSave = new List<ShowData>();
            //Obtengo los datos de los campos
            foreach (UIElement UIElement in View.GetStackPanelHeader.Children)
            {
                try
                {
                    //Obtengo el label y el valor digitado
                    ChildrenLabel = UIElement.GetType().GetProperty("UcLabel").GetValue(UIElement, null);
                    ChildrenValue = UIElement.GetType().GetProperty("UcValue").GetValue(UIElement, null);
                    //Obtengo el codigo del campo
                    CodigoCampo = UIElement.GetType().GetProperty("Name").GetValue(UIElement, null).ToString();
                    CodigoCampo = CodigoCampo.Replace("f_", "");
                    //Traigo sus datos de creacion
                    IsRequiredField = service.GetDataDefinition(new DataDefinition { Code = CodigoCampo }).First();
                    //Evaluo si el campo es obligatorio
                    if (IsRequiredField.IsRequired == true && String.IsNullOrEmpty(ChildrenValue == null ? "" : ChildrenValue.ToString()))
                        ControlIsRequired = false;
                    else
                        ControlIsRequired = true;
                    //Evaluo si puedo continuar dependiendo de si el dato era requerido y fue digitado o no
                    if (ControlIsRequired)
                    {
                        //Creo el ShowData con los datos de CodigoCampo y ChildrenValue
                        HeaderDataSave = new ShowData
                        {
                            DataKey = CodigoCampo.ToString(),
                            DataValue = ChildrenValue == null ? "" : ChildrenValue.ToString()
                        };
                        //Adiciono el ShowData al listado para crear el Xml
                        View.Model.ListHeaderDataSave.Add(HeaderDataSave);
                    }
                    else
                    {
                        Util.ShowError("El campo " + ChildrenLabel.ToString() + " no puede ser vacio.");
                        return;
                    }
                }
                catch { continue; }
            }
            try
            {
                //Convierto el listado de datos a un Xml
                XmlData = Util.XmlSerializerWF(View.Model.ListHeaderDataSave);
                //Asigno los datos del documento para guardar
                View.Model.HeaderDocument.Location = View.Model.LocationTo;
                View.Model.HeaderDocument.DocType = new DocumentType { DocTypeID = SDocType.ReceivingTask };
                View.Model.HeaderDocument.IsFromErp = false;
                View.Model.HeaderDocument.CrossDocking = false;
                View.Model.HeaderDocument.Vendor = service.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT }).First();
                View.Model.HeaderDocument.Customer = service.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT }).First();
                View.Model.HeaderDocument.Date1 = DateTime.Now;
                View.Model.HeaderDocument.Company = new Company { CompanyID = App.curCompany.CompanyID };
                //Guardo el Documento
                View.Model.HeaderDocument = service.CreateNewDocument(View.Model.HeaderDocument, true);
                //Creo el DataInformation del Header para el Xml
                View.Model.DataInformationHeader = new DataInformation
                {
                    Entity = new ClassEntity { ClassEntityID = 6 },
                    EntityRowID = View.Model.HeaderDocument.DocID,
                    XmlData = XmlData,
                    CreationDate = DateTime.Now,
                    CreatedBy = App.curUser.UserName
                };
                //Guardo el Xml en la tabla DataInformation
                View.Model.DataInformationHeader = service.SaveDataInformation(View.Model.DataInformationHeader);

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("El documento se guardo satisfactoriamente");
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error durante el proceso: " + Ex.Message); }
        }

        private void CargarDatosHeader(Location Location, Bin Bin)
        {
            IList<DataDefinitionByBin> CamposHeader;
            CamposHeader = service.GetDataDefinitionByBin(new DataDefinitionByBin
            {
                DataDefinition = new DataDefinition { Location = Location, IsHeader = true },
                Bin = Bin
            }).OrderBy(f => f.DataDefinition.NumOrder).ToList();

            string ucType; //Tipo del User Control
            Assembly assembly;
            UserControl someObject = null;
            IList<MMaster> listCombo = null;
            foreach (DataDefinitionByBin DataDefinitionByBin in CamposHeader)
            {
                if (DataDefinitionByBin.DataDefinition.DataType.UIControl != null)
                {
                    assembly = Assembly.GetAssembly(Type.GetType(DataDefinitionByBin.DataDefinition.DataType.UIControl));
                    //El primer elemento del assembly antes de la coma
                    ucType = DataDefinitionByBin.DataDefinition.DataType.UIControl.Split(",".ToCharArray())[0];
                    someObject = (UserControl)Activator.CreateInstance(assembly.GetType(ucType));
                }
                else
                {
                    assembly = Assembly.GetAssembly(Type.GetType("WpfFront.Common.WFUserControls.WFStringText, WpfFront, Version=1.0.3598.33141, Culture=neutral, PublicKeyToken=null"));
                    someObject = (UserControl)Activator.CreateInstance(assembly.GetType("WpfFront.Common.WFUserControls.WFStringText"));
                    ucType = "WpfFront.Common.WFUserControls.WFStringText";
                }
                someObject.GetType().GetProperty("UcLabel").SetValue(someObject, DataDefinitionByBin.DataDefinition.DisplayName, null);
                if (ucType.Contains("WFDateTime"))
                {
                    if (DataDefinitionByBin.DataDefinition.DefaultValue != "")
                        try
                        {
                            someObject.GetType().GetProperty("UcValue").SetValue(someObject, DateTime.Parse(DataDefinitionByBin.DataDefinition.DefaultValue), null);
                        }
                        catch { }
                }
                else
                {
                    if (ucType.Contains("WFCheckBox"))
                    {
                        if (DataDefinitionByBin.DataDefinition.DefaultValue != "")
                            someObject.GetType().GetProperty("UcValue").SetValue(someObject, Boolean.Parse(DataDefinitionByBin.DataDefinition.DefaultValue), null);
                        else
                            someObject.GetType().GetProperty("UcValue").SetValue(someObject, false, null);
                    }
                    else
                        someObject.GetType().GetProperty("UcValue").SetValue(someObject, DataDefinitionByBin.DataDefinition.DefaultValue, null);
                }
                someObject.GetType().GetProperty("Name").SetValue(someObject, "f_" + DataDefinitionByBin.DataDefinition.Code.Replace(" ", "").ToString(), null);
                try
                {
                    listCombo = null;
                    if (DataDefinitionByBin.DataDefinition.MetaType != null)
                    {
                        listCombo = service.GetMMaster(new MMaster { MetaType = DataDefinitionByBin.DataDefinition.MetaType }).OrderBy(f => f.NumOrder).ToList();
                        listCombo.Add(new MMaster { Code = "", Name = "..." });

                        someObject.GetType().GetProperty("UcList").SetValue(someObject, listCombo, null);

                        try { someObject.GetType().GetProperty("DefaultList").SetValue(someObject, listCombo, null); }
                        catch { }
                    }
                }
                catch { }
                View.GetStackPanelHeader.Children.Add(someObject);
            }
        }

        private void CargarSeriales(Location Location, Bin Bin)
        {
            //Obtengo los seriales
            View.Model.CamposSeriales = service.GetDataDefinitionByBin(new DataDefinitionByBin
            {
                DataDefinition = new DataDefinition { Location = Location, IsHeader = false, IsSerial = true },
                Bin = Bin
            }).OrderBy(f => f.DataDefinition.Code).ToList();

            //Recorro los seriales para mostrar los que han sido definidos
            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
            {
                switch (DataDefinitionByBin.DataDefinition.Code)
                {
                    case "SERIAL1":
                        View.GetStackSerial1.Visibility = Visibility.Visible;
                        View.GetTextSerial1.Text = DataDefinitionByBin.DataDefinition.DisplayName;
                        break;
                    case "SERIAL2":
                        View.GetStackSerial2.Visibility = Visibility.Visible;
                        View.GetTextSerial2.Text = DataDefinitionByBin.DataDefinition.DisplayName;
                        break;
                    case "SERIAL3":
                        View.GetStackSerial3.Visibility = Visibility.Visible;
                        View.GetTextSerial3.Text = DataDefinitionByBin.DataDefinition.DisplayName;
                        break;
                }
            }
            View.GetSerial1.Focus();
        }

        private void CargarDatosDetails(Location Location, Bin Bin)
        {
            //Variables Auxiliares
            GridViewColumn Columna;
            FrameworkElementFactory Txt;
            Assembly assembly;
            string TipoDato;
            IList<MMaster> DatosCombo;

            //Inicio el DataTable para guardar los detalles
            View.Model.ListRecords = new DataTable();

            //Adiciono el campo de ID en el DataTable
            Columna = new GridViewColumn();
            Txt = new FrameworkElementFactory(typeof(TextBlock));
            //Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Id"));
            Columna.Header = "Id";
            Columna.Width = 55;
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.GetGridViewDetails.Columns.Add(Columna);
            View.Model.ListRecords.Columns.Add("Id");

            //Adiciono el campo de ProductID en el DataTable(tamano 0 para que no se vea en el listado)
            Columna = new GridViewColumn();
            Txt = new FrameworkElementFactory(typeof(TextBlock));
            //Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("ProductID"));
            Columna.Header = "ProductID";
            Columna.Width = 0;
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.GetGridViewDetails.Columns.Add(Columna);
            View.Model.ListRecords.Columns.Add("ProductID");

            //Adiciono el campo de ProductName en el DataTable
            Columna = new GridViewColumn();
            Txt = new FrameworkElementFactory(typeof(TextBlock));
            Txt.SetValue(TextBlock.WidthProperty, (double)200);
            Txt.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Producto"));
            Columna.Header = "Producto";
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.GetGridViewDetails.Columns.Add(Columna);
            View.Model.ListRecords.Columns.Add("Producto");

            //Recorro la lista de los seriales disponibles para generarlos en el DataTable
            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
            {
                Columna = new GridViewColumn();
                Txt = new FrameworkElementFactory(typeof(TextBlock));

                //Defino los datos del campo para serial
                Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
                Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(DataDefinitionByBin.DataDefinition.Code));
                Columna.Header = DataDefinitionByBin.DataDefinition.DisplayName;
                Columna.CellTemplate = new DataTemplate();
                Columna.CellTemplate.VisualTree = Txt;

                //Adiciono el campo del serial al GridView y DataTable
                View.GetGridViewDetails.Columns.Add(Columna);
                View.Model.ListRecords.Columns.Add(DataDefinitionByBin.DataDefinition.Code);

                //Aumento el control de la cantidad de columnas fijas para no replicar estos datos
                offset++;
            }

            //Obtener los campos que voy a mostrar
            View.Model.CamposDetails = service.GetDataDefinitionByBin(new DataDefinitionByBin
            {
                DataDefinition = new DataDefinition { Location = Location, IsHeader = false, IsSerial = false },
                Bin = Bin
            }).OrderBy(f => f.DataDefinition.NumOrder).ToList();

            //Recorrer cada campo y crearlo en DataTable y en Grid
            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposDetails)
            {
                Columna = new GridViewColumn();
                assembly = Assembly.GetAssembly(Type.GetType(DataDefinitionByBin.DataDefinition.DataType.UIListControl));
                TipoDato = DataDefinitionByBin.DataDefinition.DataType.UIListControl.Split(",".ToCharArray())[0];

                Columna.Header = DataDefinitionByBin.DataDefinition.DisplayName;

                Txt = new FrameworkElementFactory(assembly.GetType(TipoDato));

                switch (TipoDato)
                {
                    case "System.Windows.Controls.ComboBox":
                        DatosCombo = service.GetMMaster(new MMaster { MetaType = new MType { MetaTypeID = DataDefinitionByBin.DataDefinition.MetaType.MetaTypeID } });
                        Txt.SetValue(ComboBox.ItemsSourceProperty, DatosCombo);
                        Txt.SetValue(ComboBox.DisplayMemberPathProperty, "Name");
                        //Txt.SetValue(ComboBox.SelectedValuePathProperty, "MetaMasterID");
                        Txt.SetValue(ComboBox.SelectedValuePathProperty, "Code");
                        Txt.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding(DataDefinitionByBin.DataDefinition.Code));
                        Txt.SetValue(ComboBox.WidthProperty, (double)110);
                        break;
                    case "System.Windows.Controls.TextBox":
                        Txt.SetValue(TextBox.MinWidthProperty, (double)100);
                        Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding(DataDefinitionByBin.DataDefinition.Code));
                        break;
                    case "Microsoft.Windows.Controls.DatePicker":
                        //Revisar como obtener el dato o realizar el binding con el DataTable
                        Txt.SetValue(Microsoft.Windows.Controls.DatePicker.MinWidthProperty, (double)130);
                        //Txt.SetValue(DatePicker.FirstDayOfWeekProperty, "Monday");
                        Txt.SetBinding(Microsoft.Windows.Controls.DatePicker.SelectedDateProperty, new System.Windows.Data.Binding(DataDefinitionByBin.DataDefinition.Code));
                        break;
                    case "System.Windows.Controls.CheckBox":
                        Txt.SetValue(CheckBox.MinWidthProperty, (double)60);
                        Txt.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding(DataDefinitionByBin.DataDefinition.Code));
                        break;
                    case "System.Windows.Controls.TextBlock":
                        Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
                        Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(DataDefinitionByBin.DataDefinition.Code));
                        break;
                }

                // add textbox template
                Columna.CellTemplate = new DataTemplate();
                Columna.CellTemplate.VisualTree = Txt;
                View.GetGridViewDetails.Columns.Add(Columna); //Creacion de la columna en el GridView
                View.Model.ListRecords.Columns.Add(DataDefinitionByBin.DataDefinition.Code); //Creacion de la columna en el DataTable
            }
        }

        private void OnLoadBinFrom(object sender, DataEventArgs<Location> e)
        {
            View.Model.BinFromList = service.GetBinRoute(new BinRoute { LocationFrom = e.Value }).Where(f => f.LocationTo.Name != "DESPACHOS").Select(f => f.BinFrom).Distinct().ToList();
            View.expDetails.IsEnabled = false;
            //Seleccion de etiqueta por bodega
            View.Model.ListadoEtiquetas = service.GetMMaster(new MMaster { MetaType = new MType { Code = "ETIQUETASI" } }).Where(f => f.Code.Contains(e.Value.ErpCode)).ToList();
        }

        private void OnLoadLocationTo(object sender, DataEventArgs<Bin> e)
        {
            if (e.Value != null)
            {
                try
                {
                    View.Model.LocationToList = service.GetBinRoute(new BinRoute
                    {
                        LocationFrom = (Location)View.GetListLocationFrom.SelectedItem,
                        BinFrom = e.Value
                    }).Where(f => f.LocationTo.Name != "DESPACHOS").Select(f => f.LocationTo).Distinct().ToList();
                }
                catch { View.Model.LocationToList = new List<Location>(); }
            }
            else
                View.Model.LocationToList = new List<Location>();
            View.expDetails.IsEnabled = false;
        }

        private void OnLoadBinTo(object sender, DataEventArgs<Location> e)
        {
            if (e.Value != null)
                View.Model.BinToList = service.GetBinRoute(new BinRoute
                {
                    LocationFrom = (Location)View.GetListLocationFrom.SelectedItem,
                    BinFrom = (Bin)View.GetListBinFrom.SelectedItem,
                    LocationTo = e.Value
                });
            else
            {
                View.Model.BinToList = new List<BinRoute>();
            }
            View.expDetails.IsEnabled = false;
        }

        private void OnLoadDocumentData(object sender, DataEventArgs<BinRoute> e)
        {
            if (e.Value != null)
            {
                //Si realizo movimientos entre Location diferentes, cargo los datos para el documento
                if (((Location)View.GetListLocationFrom.SelectedItem).LocationID != ((Location)View.GetListLocationTo.SelectedItem).LocationID)
                {
                    View.GetStackPanelHeader.Children.Clear();
                    CargarDatosHeader((Location)View.GetListLocationTo.SelectedItem, e.Value.BinTo);
                    View.expDetails.IsEnabled = true;
                }
                else
                    View.expDetails.IsEnabled = false;
            }
            else
                View.expDetails.IsEnabled = false;
        }

        private void OnConfirmBasicData(object sender, EventArgs e)
        {
            //Evaluo si los ComboBox han sido seleccionados
            if (View.GetListLocationFrom.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar un cliente origen");
                return;
            }
            if (View.GetListBinFrom.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar un estado origen");
                return;
            }
            if (View.GetListLocationTo.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar un cliente destino");
                return;
            }
            if (View.GetListBinTo.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar un estado destino");
                return;
            }

            //Asigno los datos seleccionados a las variables del modelo
            View.Model.LocationFrom = (Location)View.GetListLocationFrom.SelectedItem;
            View.Model.BinFrom = (Bin)View.GetListBinFrom.SelectedItem;
            View.Model.LocationTo = (Location)View.GetListLocationTo.SelectedItem;
            View.Model.BinTo = ((BinRoute)View.GetListBinTo.SelectedItem).BinTo;

            //Guardo el Documento y el DataInformation(Si los Location son diferentes)
            if (View.Model.LocationFrom.LocationID != View.Model.LocationTo.LocationID)
                GuardarDocumentoDataInformation();

            //Inhabilito el boton de Guardar, muestro Cancelar y oculto Nuevo
            View.GetButtonConfirmar.IsEnabled = false;
            View.GetButtonNuevo.Visibility = Visibility.Collapsed;
            View.GetButtonCancelar.Visibility = Visibility.Visible;

            //Nuevas Lineas
            View.GetButtonCancelar.Visibility = Visibility.Visible;


            //Cargo los datos para el detalle, los seriales
            CargarSeriales(View.Model.LocationTo, View.Model.BinTo);
            CargarDatosDetails(View.Model.LocationTo, View.Model.BinTo);

            //Muestro los campos de seriales y detalles
            View.BorderDetails.Visibility = Visibility.Visible;

            //valido el rol pare realizar el cargue masivo
            if (App.curRol.Rol.RolCode.Equals("MASIVO"))
            {
                View.GetStackUploadFile.Visibility = Visibility.Collapsed;
            }
        }

        private void OnNewBasicData(object sender, EventArgs e)
        {
            //Reinicio el documento
            View.Model.HeaderDocument = new Document { CreatedBy = App.curUser.UserName };

            //Habilito el boton de Guardar, oculto Cancelar y Nuevo
            View.GetButtonConfirmar.IsEnabled = true;
            View.GetButtonCancelar.Visibility = Visibility.Collapsed;
            View.GetButtonNuevo.Visibility = Visibility.Visible;

            //Quito lo seleccionado en los ComboBox de LocationFrom, BinFro, LocationTo, BinTo
            View.GetListLocationFrom.SelectedIndex = -1;
            View.GetListBinFrom.SelectedIndex = -1;
            View.GetListLocationTo.SelectedIndex = -1;
            View.GetListBinTo.SelectedIndex = -1;

            //Elimino los campos creados dinamicamente para el documento(En caso de necesitarse)
            View.GetStackPanelHeader.Children.Clear();

            //Oculto los bloques de seriales y detalles. Oculto los campos de los seriales
            View.GetStackSerial1.Visibility = Visibility.Collapsed;
            View.GetStackSerial2.Visibility = Visibility.Collapsed;
            View.GetStackSerial3.Visibility = Visibility.Collapsed;
            View.BorderDetails.Visibility = Visibility.Collapsed;

            //Limpio los seriales
            View.GetSerial1.Text = "";
            View.GetSerial2.Text = "";
            View.GetSerial3.Text = "";

            //Limpio las columnas del GridView y DataTable
            View.GetGridViewDetails.Columns.Clear();
            if (View.Model.ListRecords != null)
            {
                View.Model.ListRecords.Columns.Clear();
            }
        }

        private void OnCancelBasicData(object sender, EventArgs e)
        {
            //Nuevas Lineas
            View.GetButtonNuevo.Visibility = Visibility.Collapsed;
            View.GetButtonCancelar.Visibility = Visibility.Visible;
            Util.ShowMessage("Regitros guardados exitosamente. \nCierre la pestaña y genere un nuevo traslado.");
        }

        #endregion

        #region Serial

        #region OnAddLine Descatualizado
        private void OnAddLine(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            WpfFront.WMSBusinessService.Label EvalSerialLabel = new WpfFront.WMSBusinessService.Label
            {
                Bin = new Bin { Location = service.GetLocation(new Location { LocationID = View.Model.LocationFrom.LocationID }).First() }
            };
            IList<DataInformation> DataInformationLabelList;
            DataInformation DataInformationLabel;
            List<ShowData> ShowDataList;
            ShowData ShowData;
            bool EvalSerial1 = true, EvalSerial2 = true, EvalSerial3 = true;
            IList<WpfFront.WMSBusinessService.Label> EvalSerialLabelList;
            //Agrego las columnas...


            //Validacion de los seriales(tamanos, datos y existencia en el sistema)
            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
            {
                switch (DataDefinitionByBin.DataDefinition.Code)
                {
                    case "SERIAL1":
                        if (View.GetSerial1.Text == "")
                        {
                            Util.ShowError("Por favor digite el serial en " + DataDefinitionByBin.DataDefinition.DisplayName);
                            return;
                        }
                        if (View.GetSerial1.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
                        {
                            Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
                            return;
                        }

                        //Asigno el serial para evaluar si existe en el sistema
                        EvalSerialLabel.LabelCode = View.GetSerial1.Text;

                        //Evaluo si existe la columna para validar el serial existente en el listado actual
                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
                            foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                            {
                                EvalSerial1 = DataRow[DataDefinitionByBin.DataDefinition.Code].ToString() == View.GetSerial1.Text ? true : false;
                            }
                        break;
                    case "SERIAL2":
                        if (View.GetSerial2.Text == "")
                        {
                            Util.ShowError("Por favor digite el serial en " + DataDefinitionByBin.DataDefinition.DisplayName);
                            return;
                        }
                        if (View.GetSerial2.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
                        {
                            Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
                            return;
                        }

                        //Asigno el serial para evaluar si existe en el sistema
                        EvalSerialLabel.PrintingLot = View.GetSerial2.Text;

                        //Evaluo si existe la columna para validar el serial existente en el listado actual
                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
                            foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                            {
                                EvalSerial2 = DataRow[DataDefinitionByBin.DataDefinition.Code].ToString() == View.GetSerial2.Text ? true : false;
                            }
                        break;
                    case "SERIAL3":
                        if (View.GetSerial3.Text == "")
                        {
                            Util.ShowError("Por favor digite el serial en " + DataDefinitionByBin.DataDefinition.DisplayName);
                            return;
                        }
                        if (View.GetSerial3.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
                        {
                            Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
                            return;
                        }

                        //Asigno el serial para evaluar si existe en el sistema
                        EvalSerialLabel.Manufacturer = View.GetSerial3.Text;

                        //Evaluo si existe la columna para validar el serial existente en el listado actual
                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
                            foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                            {
                                EvalSerial3 = DataRow[DataDefinitionByBin.DataDefinition.Code].ToString() == View.GetSerial3.Text ? true : false;
                            }
                        break;
                }
            }

            //Analizo si el serial ya esta en la lista y el listado a lo sumo tiene un elemento
            if (EvalSerial1 && EvalSerial2 && EvalSerial3 && View.Model.ListRecords.Rows.Count > 0)
            {
                Util.ShowError("El serial ya existe en el listado de ingresos");
                return;
            }

            //Analizo si el serial esta en el sistema
            if (service.GetLabel(EvalSerialLabel).Count() == 0)
            {
                Util.ShowError("El serial no existe en el sistema");
                return;
            }

            //Analizo si el serial esta en el sistema y esta en el Bin actual
            EvalSerialLabel.Bin = View.Model.BinFrom;

            //Evaluo el tipo de equipo, si fue cargado automaticamente el serial o es digitado
            if (View.Model.LocationFrom.AddressLine3 == "1")
            {
                //Obtengo los equipos con el label
                EvalSerialLabelList = service.GetLabel(EvalSerialLabel);

                //Evaluo si obtuve registros para continuar
                if (EvalSerialLabelList.Count == 0)
                {
                    Util.ShowError("El serial no se encuentra en el estado " + View.Model.BinFrom.BinCode);
                    return;
                }

                //Recorro cada equipo para cargar sus datos
                foreach (WpfFront.WMSBusinessService.Label Equipo in EvalSerialLabelList)
                {
                    try
                    {
                        //Obtengo el DataInformation asociado al Label
                        DataInformationLabel = service.GetDataInformation(new DataInformation
                        {
                            Entity = new ClassEntity { ClassEntityID = 20 },
                            EntityRowID = Int32.Parse(Equipo.LabelID.ToString())
                        }).First();

                        //Inicio la variable por cada equipo
                        dr = View.Model.ListRecords.NewRow();

                        //Guardo el Label y DataInformation en el listado para control de actualizacion
                        View.Model.ListLabelScann.Add(Equipo);
                        View.Model.ListDataInformation.Add(DataInformationLabel);

                        //Evaluo cuales seriales estan visibles para adicionarlos al registro
                        int cont = 3;
                        foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
                        {
                            switch (DataDefinitionByBin.DataDefinition.Code)
                            {
                                case "SERIAL1":
                                    dr[cont] = Equipo.LabelCode;
                                    break;
                                case "SERIAL2":
                                    dr[cont] = Equipo.PrintingLot;
                                    break;
                                case "SERIAL3":
                                    dr[cont] = Equipo.Manufacturer;
                                    break;
                            }
                            cont++;
                        }

                        //Asigno al campo 0 el LabelID
                        dr[0] = Equipo.LabelID;

                        //Asigno al campo 1 ProductID, campo 2 ProducName
                        dr[1] = Equipo.Product.ProductID;
                        dr[2] = Equipo.Product.Name;

                        //Deserializo el Xml para obtener los datos guardados anteriormente en el DataInformation
                        ShowDataList = Util.DeserializeMetaDataWF(DataInformationLabel.XmlData);

                        //Asigno a cada campo su valor definido por defecto o asigno el dato guardado anteriormente
                        foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposDetails)
                        {
                            try
                            {
                                ShowData = ShowDataList.Where(f => f.DataKey == DataDefinitionByBin.DataDefinition.Code).ToList().First();
                            }
                            catch { ShowData = null; }
                            if (ShowData != null)
                            {
                                dr[ShowData.DataKey] = ShowData.DataValue;
                            }
                            else
                            {
                                dr[DataDefinitionByBin.DataDefinition.Code] = DataDefinitionByBin.DataDefinition.DefaultValue;
                            }
                        }

                        //Adiciono el registro a la lista
                        View.Model.ListRecords.Rows.Add(dr);
                    }
                    catch { }
                }
            }
            else
            {
                try
                {
                    //Obtengo el Label
                    EvalSerialLabel = service.GetLabel(EvalSerialLabel).First();
                    //Obtengo el DataInformation asociado al Label, si es un producto pickeado, coloco la variable en vacio
                    DataInformationLabelList = service.GetDataInformation(new DataInformation
                    {
                        Entity = new ClassEntity { ClassEntityID = 20 },
                        EntityRowID = Int32.Parse(EvalSerialLabel.LabelID.ToString())
                    });
                    /*if (DataInformationLabelList.Count > 0)*/
                    DataInformationLabel = DataInformationLabelList.First();
                    /*else
                        DataInformationLabel = new DataInformation { Entity = new ClassEntity { ClassEntityID = EntityID.Label } };*/
                }
                catch
                {
                    Util.ShowError("El serial no se encuentra en el estado " + View.Model.BinFrom.BinCode);
                    return;
                }

                //Guardo el Label y DataInformation en el listado para control de actualizacion
                View.Model.ListLabelScann.Add(EvalSerialLabel);
                View.Model.ListDataInformation.Add(DataInformationLabel);

                //Evaluo cuales seriales estan visibles para adicionarlos al registro
                int cont = 3;
                foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
                {
                    switch (DataDefinitionByBin.DataDefinition.Code)
                    {
                        case "SERIAL1":
                            dr[cont] = EvalSerialLabel.LabelCode;
                            break;
                        case "SERIAL2":
                            dr[cont] = EvalSerialLabel.PrintingLot;
                            break;
                        case "SERIAL3":
                            dr[cont] = EvalSerialLabel.Manufacturer;
                            break;
                    }
                    cont++;
                }

                //Asigno al campo 0 el LabelID
                dr[0] = EvalSerialLabel.LabelID;

                //Asigno al campo 1 ProductID, campo 2 ProducName
                dr[1] = EvalSerialLabel.Product.ProductID;
                dr[2] = EvalSerialLabel.Product.Name;

                //Deserializo el Xml para obtener los datos guardados anteriormente en el DataInformation
                ShowDataList = Util.DeserializeMetaDataWF(DataInformationLabel.XmlData);

                //Asigno a cada campo su valor definido por defecto o asigno el dato guardado anteriormente
                foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposDetails)
                {
                    try
                    {
                        ShowData = ShowDataList.Where(f => f.DataKey == DataDefinitionByBin.DataDefinition.Code).ToList().First();
                    }
                    catch { ShowData = null; }
                    if (ShowData != null)
                    {
                        dr[ShowData.DataKey] = ShowData.DataValue;
                    }
                    else
                    {
                        dr[DataDefinitionByBin.DataDefinition.Code] = DataDefinitionByBin.DataDefinition.DefaultValue;
                    }
                }


                //Adiciono el registro a la lista
                View.Model.ListRecords.Rows.Add(dr);

            }

            //Limpio los seriales para digitar nuevos datos
            View.GetSerial1.Text = "";
            View.GetSerial2.Text = "";
            View.GetSerial3.Text = "";
            View.GetSerial1.Focus();
        }
        #endregion

        //#region OnAddLine Actualizado
        //private void OnAddLine(object sender, EventArgs e)
        //{
        //    //Variables Auxiliares
        //    DataRow dr = View.Model.ListRecords.NewRow();
        //    EvalSerialLabel = new WpfFront.WMSBusinessService.Label
        //    {
        //        Bin = new Bin { Location = service.GetLocation(new Location { LocationID = View.Model.LocationFrom.LocationID }).First() }
        //    };
        //    IList<DataInformation> DataInformationLabelList;
        //    DataInformation DataInformationLabel;
        //    List<ShowData> ShowDataList;
        //    ShowData ShowData;
        //    bool EvalSerial1 = true, EvalSerial2 = true, EvalSerial3 = true;
        //    IList<WpfFront.WMSBusinessService.Label> EvalSerialLabelList;
        //    //Agrego las columnas...


        //    //Validacion de los seriales(tamanos, datos y existencia en el sistema)
        //    foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
        //    {
        //        switch (DataDefinitionByBin.DataDefinition.Code)
        //        {
        //            case "SERIAL1":
        //                if (View.GetSerial1.Text == "")
        //                {
        //                    Util.ShowError("Por favor digite el serial en " + DataDefinitionByBin.DataDefinition.DisplayName);
        //                    return;
        //                }
        //                if (View.GetSerial1.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
        //                {
        //                    Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
        //                    return;
        //                }
        //                //Asigno el serial para evaluar si existe en el sistema
        //                EvalSerialLabel.LabelCode = View.GetSerial1.Text;

        //                break;
        //            case "SERIAL2":
        //                if (View.GetSerial2.Text == "")
        //                {
        //                    Util.ShowError("Por favor digite el serial en " + DataDefinitionByBin.DataDefinition.DisplayName);
        //                    return;
        //                }
        //                if (View.GetSerial2.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
        //                {
        //                    Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
        //                    return;
        //                }

        //                //Asigno el serial para evaluar si existe en el sistema
        //                EvalSerialLabel.PrintingLot = View.GetSerial2.Text;

        //                //Evaluo si existe la columna para validar el serial existente en el listado actual
        //                if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
        //                    foreach (DataRow DataRow in View.Model.ListRecords.Rows)
        //                    {
        //                        EvalSerial2 = DataRow[DataDefinitionByBin.DataDefinition.Code].ToString() == View.GetSerial2.Text ? true : false;
        //                    }
        //                break;
        //            case "SERIAL3":
        //                if (View.GetSerial3.Text == "")
        //                {
        //                    Util.ShowError("Por favor digite el serial en " + DataDefinitionByBin.DataDefinition.DisplayName);
        //                    return;
        //                }
        //                if (View.GetSerial3.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
        //                {
        //                    Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
        //                    return;
        //                }

        //                //Asigno el serial para evaluar si existe en el sistema
        //                EvalSerialLabel.Manufacturer = View.GetSerial3.Text;

        //                //Evaluo si existe la columna para validar el serial existente en el listado actual
        //                if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
        //                    foreach (DataRow DataRow in View.Model.ListRecords.Rows)
        //                    {
        //                        EvalSerial3 = DataRow[DataDefinitionByBin.DataDefinition.Code].ToString() == View.GetSerial3.Text ? true : false;
        //                    }
        //                break;
        //        }
        //    }

        //    //Analizo si el serial ya esta en la lista y el listado a lo sumo tiene un elemento
        //    //if (EvalSerial1 && EvalSerial2 && EvalSerial3 && View.Model.ListRecords.Rows.Count > 0)
        //    //{
        //    //    Util.ShowError("El serial ya existe en el listado de ingresos");
        //    //    return;
        //    //}

        //    //Analizo si el serial esta en el sistema
        //    //if (service.GetLabel(EvalSerialLabel).Count() == 0)
        //    //{
        //    //    Util.ShowError("El serial no existe en el sistema");
        //    //    return;
        //    //}

        //    ////Analizo si el serial esta en el sistema y esta en el Bin actual
        //    //EvalSerialLabel.Bin = View.Model.BinFrom;

        //    //Evaluo el tipo de equipo, si fue cargado automaticamente el serial o es digitado
        //    //if (View.Model.LocationFrom.AddressLine3 == "1")
        //    //{
        //    //Obtengo los equipos con el label
        //    EvalSerialLabelList = service.GetLabel(EvalSerialLabel);

        //    //Evaluo si obtuve registros para continuar
        //    //if (EvalSerialLabelList.Count == 0)
        //    //{
        //    //    Util.ShowError("El serial no se encuentra en el estado " + View.Model.BinFrom.BinCode);
        //    //    return;
        //    //}

        //    //Recorro cada equipo para cargar sus datos
        //    foreach (WpfFront.WMSBusinessService.Label Equipo in EvalSerialLabelList)
        //    {
        //        try
        //        {
        //            //Obtengo el DataInformation asociado al Label
        //            DataInformationLabel = service.GetDataInformation(new DataInformation
        //            {
        //                Entity = new ClassEntity { ClassEntityID = 20 },
        //                EntityRowID = Int32.Parse(Equipo.LabelID.ToString())
        //            }).First();

        //            //Inicio la variable por cada equipo
        //            dr = View.Model.ListRecords.NewRow();

        //            //Guardo el Label y DataInformation en el listado para control de actualizacion
        //            View.Model.ListLabelScann.Add(Equipo);
        //            View.Model.ListDataInformation.Add(DataInformationLabel);

        //            //Evaluo cuales seriales estan visibles para adicionarlos al registro
        //            int cont = 3;
        //            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
        //            {
        //                switch (DataDefinitionByBin.DataDefinition.Code)
        //                {
        //                    case "SERIAL1":
        //                        dr[cont] = Equipo.LabelCode;
        //                        break;
        //                    case "SERIAL2":
        //                        dr[cont] = Equipo.PrintingLot;
        //                        break;
        //                    case "SERIAL3":
        //                        dr[cont] = Equipo.Manufacturer;
        //                        break;
        //                }
        //                cont++;
        //            }

        //            //Asigno al campo 0 el LabelID
        //            dr[0] = Equipo.LabelID;

        //            //Asigno al campo 1 ProductID, campo 2 ProducName
        //            dr[1] = Equipo.Product.ProductID;
        //            dr[2] = Equipo.Product.Name;

        //            //Deserializo el Xml para obtener los datos guardados anteriormente en el DataInformation
        //            ShowDataList = Util.DeserializeMetaDataWF(DataInformationLabel.XmlData);

        //            //Asigno a cada campo su valor definido por defecto o asigno el dato guardado anteriormente
        //            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposDetails)
        //            {
        //                try
        //                {
        //                    ShowData = ShowDataList.Where(f => f.DataKey == DataDefinitionByBin.DataDefinition.Code).ToList().First();
        //                }
        //                catch { ShowData = null; }
        //                if (ShowData != null)
        //                {
        //                    dr[ShowData.DataKey] = ShowData.DataValue;
        //                }
        //                else
        //                {
        //                    dr[DataDefinitionByBin.DataDefinition.Code] = DataDefinitionByBin.DataDefinition.DefaultValue;
        //                }
        //            }

        //            //Adiciono el registro a la lista
        //            View.Model.ListRecords.Rows.Add(dr);
        //        }
        //        catch { }
        //        //}
        //    }
        //    //else
        //    //{
        //    //try
        //    //{
        //    //    //Obtengo el Label
        //    //    EvalSerialLabel = service.GetLabel(EvalSerialLabel).First();
        //    //    //Obtengo el DataInformation asociado al Label, si es un producto pickeado, coloco la variable en vacio
        //    //    DataInformationLabelList = service.GetDataInformation(new DataInformation
        //    //    {
        //    //        Entity = new ClassEntity { ClassEntityID = 20 },
        //    //        EntityRowID = Int32.Parse(EvalSerialLabel.LabelID.ToString())
        //    //    });
        //    //    /*if (DataInformationLabelList.Count > 0)*/
        //    //    DataInformationLabel = DataInformationLabelList.First();
        //    //    /*else
        //    //        DataInformationLabel = new DataInformation { Entity = new ClassEntity { ClassEntityID = EntityID.Label } };*/
        //    //}
        //    //catch
        //    //{
        //    //    Util.ShowError("El serial no se encuentra en el estado " + View.Model.BinFrom.BinCode);
        //    //    return;
        //    //}

        //    ////Guardo el Label y DataInformation en el listado para control de actualizacion
        //    ////View.Model.ListLabelScann.Add(EvalSerialLabel);
        //    ////View.Model.ListDataInformation.Add(DataInformationLabel);

        //    ////Evaluo cuales seriales estan visibles para adicionarlos al registro
        //    //int cont = 3;
        //    //foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
        //    //{
        //    //    switch (DataDefinitionByBin.DataDefinition.Code)
        //    //    {
        //    //        case "SERIAL1":
        //    //            dr[cont] = EvalSerialLabel.LabelCode;
        //    //            break;
        //    //        case "SERIAL2":
        //    //            dr[cont] = EvalSerialLabel.PrintingLot;
        //    //            break;
        //    //        case "SERIAL3":
        //    //            dr[cont] = EvalSerialLabel.Manufacturer;
        //    //            break;
        //    //    }
        //    //    cont++;
        //    //}

        //    ////Asigno al campo 0 el LabelID
        //    //dr[0] = EvalSerialLabel.LabelID;

        //    ////Asigno al campo 1 ProductID, campo 2 ProducName
        //    //dr[1] = EvalSerialLabel.Product.ProductID;
        //    //dr[2] = EvalSerialLabel.Product.Name;

        //    ////Deserializo el Xml para obtener los datos guardados anteriormente en el DataInformation
        //    //ShowDataList = Util.DeserializeMetaDataWF(DataInformationLabel.XmlData);

        //    ////Asigno a cada campo su valor definido por defecto o asigno el dato guardado anteriormente
        //    //foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposDetails)
        //    //{
        //    //    try
        //    //    {
        //    //        ShowData = ShowDataList.Where(f => f.DataKey == DataDefinitionByBin.DataDefinition.Code).ToList().First();
        //    //    }
        //    //    catch { ShowData = null; }
        //    //    if (ShowData != null)
        //    //    {
        //    //        dr[ShowData.DataKey] = ShowData.DataValue;
        //    //    }
        //    //    else
        //    //    {
        //    //        dr[DataDefinitionByBin.DataDefinition.Code] = DataDefinitionByBin.DataDefinition.DefaultValue;
        //    //    }
        //    ////}


        //    ////Adiciono el registro a la lista
        //    //View.Model.ListRecords.Rows.Add(dr);

        //    //Limpio los seriales para digitar nuevos datos
        //    View.GetSerial1.Text = "";
        //    View.GetSerial2.Text = "";
        //    View.GetSerial3.Text = "";
        //    View.GetSerial1.Focus();
        //}



        //#endregion

        /// <summary>
        /// Nuevas lineas de codigo "evaluar tipo de producto".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEvaluarTipoProducto(object sender, DataEventArgs<Product> e)
        {
            if (View.GetProductLocation.Product != null)
            {
                //Asigno el producto seleccionado a la variable del modelo
                View.Model.ProductoSerial = e.Value;
                //Evaluo si el producto es serializable o no para habilitar el campo o no. 1 => Serializable, 0 => no Serializable
                //Aqui se valida que sea cero, porque estos productos al no ser serializables se podran mover por cantidades.
                if (View.Model.ProductoSerial.ErpTrackOpt == 0)
                {
                    View.GetStackCantidad.IsEnabled = View.GetUpLoadFile.IsEnabled = true;
                    View.GetStackCantidad.Focus();
                }
                else
                {
                    View.GetStackCantidad.IsEnabled = View.GetUpLoadFile.IsEnabled = false;
                }
            }
        }

        #endregion

        #region Details

        #region OnSaveDetails Desactualizada
        private void OnSaveDetails(object sender, EventArgs e)
        {
            ////Validacion si no existen datos para guardar
            if (View.Model.ListRecords.Rows.Count == 0)
                return;



            ////Variables Auxiliares
            IList<DataInformation> DataInformationSerialList;
            DataInformation DataInformationSerial = null;
            Object ChildrenValue, ChildrenLabel;
            ShowData DetailDataSave;
            IList<ShowData> DetailDataList;
            string XmlData;
            WpfFront.WMSBusinessService.Label LabelAux;
            DataDefinitionByBin DataDefinitionControlIsRequired;
            bool ControlIsRequired;

            try
            {
                ////Recorro los registros de la lista para obtener los datos y hacer un join con los anteriores y guardarlos
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    ////Validacion si hay errores leyendo las cadenas XML
                    try
                    {
                        ////Evaluo si el registro viene de un picking o si ya esta en el sistema
                        DataInformationSerialList = View.Model.ListDataInformation.Where
                            (f => f.Entity.ClassEntityID == EntityID.Label &&
                                  f.EntityRowID == Int32.Parse(DataRow[0].ToString())).ToList();

                        ////Si el registro tiene datainformation, lo obtengo, en caso contrario creo uno nuevo
                        if (DataInformationSerialList.Count > 0)
                            ////Obtengo el DataInformation asociado al registro
                            DataInformationSerial = View.Model.ListDataInformation.Where
                                (f => f.Entity.ClassEntityID == EntityID.Label &&
                                      f.EntityRowID == Int32.Parse(DataRow[0].ToString())).First();
                        /*else
                            Creo un nuevo DataInformation
                            DataInformationSerial = new DataInformation
                            {
                                Entity = new ClassEntity { ClassEntityID = 20 },
                                EntityRowID = Int32.Parse(DataRow[0].ToString()),
                                CreationDate = DateTime.Now,
                                CreatedBy = App.curUser.UserName
                            };*/
                    }
                    catch
                    {
                        Util.ShowError("Hubo un error durante el proceso de grabacion. Por favor intentar nuevamente.");
                        return;
                    }

                    ////Deserializo el Xml para obtener el Showdata
                    DetailDataList = Util.DeserializeMetaDataWF(DataInformationSerial.XmlData);

                    ////Inicializo la lista de los datos a convertir en Xml
                    View.Model.ListDetailsDataSave = new List<ShowData>();

                    ////Obtengo los datos de cada campo con su nombre
                    foreach (DataColumn c in View.Model.ListRecords.Columns)
                    {
                        ////Obtengo el label y el valor digitado
                        ChildrenLabel = c.ColumnName;
                        ChildrenValue = DataRow[c.ColumnName].ToString();

                        ////Evaluo si el campo es obligatorio
                        ControlIsRequired = true;
                        DataDefinitionControlIsRequired = (View.Model.CamposDetails.Where(f => f.DataDefinition.Code == ChildrenLabel).Count() > 0) ? View.Model.CamposDetails.Where(f => f.DataDefinition.Code == ChildrenLabel).First() : null;
                        if (DataDefinitionControlIsRequired != null)
                        {
                            if (DataDefinitionControlIsRequired.DataDefinition.IsRequired == true && String.IsNullOrEmpty(ChildrenValue.ToString()))
                                ControlIsRequired = false;
                            else
                                ControlIsRequired = true;
                        }

                        ////Evaluo si puedo continuar dependiendo de si el dato era requerido y fue digitado o no
                        if (ControlIsRequired)
                        {
                            ////Evaluo si existe el dato que estoy leyendo para quitarlo del listado anterior
                            try
                            {
                                DetailDataSave = DetailDataList.Where(f => f.DataKey == c.ColumnName).First();
                                DetailDataList.Remove(DetailDataSave);
                            }
                            ////En caso de no existir no quito nada
                            catch { }

                            ////Creo el ShowData con el dato
                            DetailDataSave = new ShowData
                            {
                                DataKey = ChildrenLabel.ToString(),
                                DataValue = ChildrenValue.ToString()
                            };

                            ////Adiciono cada dato a la lista
                            View.Model.ListDetailsDataSave.Add(DetailDataSave);
                        }
                        else
                        {
                            Util.ShowError("El campo " + ChildrenLabel.ToString() + " no puede ser vacio.");
                            return;
                        }

                        ////Cargo el dato para la variable de ultimos procesados
                        View.Model.UltimosProcesadosMP = View.Model.UltimosProcesadosMP + ChildrenValue.ToString() + " \t ";
                    }

                    ////Concateno las listas de ShowData, la que estaba guardada y la nueva
                    View.Model.ListDetailsDataSave = View.Model.ListDetailsDataSave.Concat(DetailDataList).ToList();

                    ////Convierto el listado de datos a un Xml
                    XmlData = Util.XmlSerializerWF(View.Model.ListDetailsDataSave);

                    ////Modifico el DataInformation con el nuevo Xml
                    DataInformationSerial.XmlData = XmlData;

                    ////Actualizo el DataInformation o creo uno nuevo en el caso de que sea un producto de picking
                    if (DataInformationSerialList.Count > 0)
                    {
                        DataInformationSerial.ModDate = DateTime.Now;
                        DataInformationSerial.ModifiedBy = App.curUser.UserName;
                        service.UpdateDataInformation(DataInformationSerial);
                    }
                    else
                        service.SaveDataInformation(DataInformationSerial);

                    ////Paso #2.
                    View.Model.UltimosProcesadosMP = View.Model.UltimosProcesadosMP + "\n";
                }

                LabelAux = service.GetLabel(new WpfFront.WMSBusinessService.Label
                {
                    Bin = View.Model.BinTo,
                    LabelType = new DocumentType { DocTypeID = LabelType.BinLocation }
                }).First();

                ////Recorro el listado de Labels para actualizar sus datos
                foreach (WpfFront.WMSBusinessService.Label Label in View.Model.ListLabelScann)
                {
                    Label.Notes += (View.Model.IsCheckedCommentsSerial) ? (" " + View.Model.HeaderDocument.Comment) : "";
                    try { service.ChangeLabelLocationV2(Label, LabelAux, View.Model.HeaderDocument); }
                    catch { }
                }

                ////Reinicio las variables
                View.Model.ListRecords.Rows.Clear();
                View.Model.ListLabelScann = new List<WpfFront.WMSBusinessService.Label>();
                View.Model.ListDataInformation = new List<DataInformation>();

                ////Muestro el mensaje de guardado 
                Util.ShowMessage("Datos Guardados exitosamente.");
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error guardando los datos, por favor vuelva a intentarlo: " + Ex.Message); }
        }
        #endregion

        //#region OnSaveDetails Actualizada
        //private void OnSaveDetails(object sender, EventArgs e)
        //{
        //    //Validacion si no existen datos para guardar
        //    if (View.Model.ListRecords.Rows.Count == 0)
        //        return;



        //    IList<DataInformation> DataInformationSerialList = null;
        //    //IList<DataInformation> DataInformationLabelList;
        //    //DataInformation DataInformationLabel;
        //    IList<WpfFront.WMSBusinessService.Label> EvalSerialLabelList;
        //    DataInformation DataInformationSerial = null;
        //    Object ChildrenValue, ChildrenLabel;
        //    ShowData DetailDataSave;
        //    IList<ShowData> DetailDataList;
        //    string XmlData;
        //    WpfFront.WMSBusinessService.Label LabelAux;
        //    DataDefinitionByBin DataDefinitionControlIsRequired;
        //    bool ControlIsRequired, EvalSerial1 = true, EvalSerial2 = true, EvalSerial3 = true, Res = true;
        //    DataTable Dt_Res = new DataTable("Resultados");

        //    //Agrego Columnas
        //    Dt_Res.Columns.Add("SERIAL");
        //    Dt_Res.Columns.Add("# SERIAL");
        //    Dt_Res.Columns.Add("DESCRIPCION");

        //    try
        //    {
        //        //Recorro los registros de la lista para obtener los datos y hacer un join con los anteriores y guardarlos
        //        foreach (DataRow DataRow in View.Model.ListRecords.Rows)
        //        {
        //            //Variables Auxiliares
        //            EvalSerialLabel = new WpfFront.WMSBusinessService.Label { Bin = new Bin { Location = service.GetLocation(new Location { LocationID = View.Model.LocationFrom.LocationID }).First() } };
        //            #region Nuevo Codigo

        //            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
        //            {
        //                switch (DataDefinitionByBin.DataDefinition.Code)
        //                {
        //                    case "SERIAL1":
        //                        //Asigno el serial para evaluar si existe en el sistema
        //                        EvalSerialLabel.LabelCode = DataRow["SERIAL1"].ToString();
        //                        //Evaluo si existe la columna para validar el serial existente en el listado actual
        //                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
        //                        {
        //                            EvalSerial1 = Validar_serial_Lista_actual(DataDefinitionByBin.DataDefinition.Code, DataRow["SERIAL1"].ToString());
        //                        }
        //                        break;
        //                    case "SERIAL2":
        //                        //Asigno el serial para evaluar si existe en el sistema
        //                        EvalSerialLabel.PrintingLot = DataRow["SERIAL2"].ToString();
        //                        //Evaluo si existe la columna para validar el serial existente en el listado actual
        //                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
        //                        {
        //                            EvalSerial2 = Validar_serial_Lista_actual(DataDefinitionByBin.DataDefinition.Code, DataRow["SERIAL2"].ToString());
        //                        }
        //                        break;
        //                    case "SERIAL3":
        //                        //Asigno el serial para evaluar si existe en el sistema
        //                        EvalSerialLabel.PrintingLot = DataRow["SERIAL3"].ToString();
        //                        //Evaluo si existe la columna para validar el serial existente en el listado actual
        //                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
        //                        {
        //                            EvalSerial3 = Validar_serial_Lista_actual(DataDefinitionByBin.DataDefinition.Code, DataRow["SERIAL3"].ToString());
        //                        }
        //                        break;
        //                }
        //            }
        //            //Analizo si el serial ya esta en la lista y el listado a lo sumo tiene un elemento
        //            if (EvalSerial1 && EvalSerial2 && EvalSerial3 && View.Model.ListRecords.Rows.Count > 0)
        //            {
        //                Dt_Res.Rows.Add(new object[] { "SERIAL1", EvalSerialLabel.LabelCode, "El serial ya existe en el listado de ingresos" });
        //                Res = false;
        //                break;
        //            }

        //            //Analizo si el serial esta en el sistema
        //            if (service.GetLabel(EvalSerialLabel).Count() == 0)
        //            {
        //                Dt_Res.Rows.Add(new object[] { "SERIAL1", EvalSerialLabel.LabelCode, "El serial no existe en el sistema" });
        //                Res = false;
        //                break;
        //            }

        //            ////Analizo si el serial esta en el sistema y esta en el Bin actual
        //            EvalSerialLabel.Bin = View.Model.BinFrom;

        //            if (View.Model.LocationFrom.AddressLine3 == "1")
        //            {
        //                //Obtengo los equipos con el label
        //                EvalSerialLabelList = service.GetLabel(EvalSerialLabel);

        //                //Evaluo si obtuve registros para continuar
        //                if (EvalSerialLabelList.Count == 0)
        //                {
        //                    Dt_Res.Rows.Add(new object[] { "SERIAL1", EvalSerialLabel.LabelCode, "El serial no se encuentra en el estado " + View.Model.BinFrom.BinCode });
        //                    Res = false;
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    //Obtengo el Label
        //                    EvalSerialLabel = service.GetLabel(EvalSerialLabel).First();
        //                    //Obtengo el DataInformation asociado al Label, si es un producto pickeado, coloco la variable en vacio
        //                    DataInformationSerialList = service.GetDataInformation(new DataInformation
        //                    {
        //                        Entity = new ClassEntity { ClassEntityID = 20 },
        //                        EntityRowID = Int32.Parse(EvalSerialLabel.LabelID.ToString())
        //                    });
        //                    /*if (DataInformationLabelList.Count > 0)*/
        //                    DataInformationSerial = DataInformationSerialList.First();
        //                    /*else
        //                        DataInformationLabel = new DataInformation { Entity = new ClassEntity { ClassEntityID = EntityID.Label } };*/
        //                }
        //                catch
        //                {
        //                    Dt_Res.Rows.Add(new object[] { "SERIAL1", EvalSerialLabel.LabelCode, "El serial no se encuentra en el estado " + View.Model.BinFrom.BinCode });
        //                    Res = false;
        //                    DataInformationSerialList = null;
        //                    DataInformationSerial = null;

        //                }
        //            }

        //            #endregion

        //            if (Res)
        //            {
        //                //Validacion si hay errores leyendo las cadenas XML
        //                try
        //                {
        //                    //Evaluo si el registro viene de un picking o si ya esta en el sistema
        //                    DataInformationSerialList = View.Model.ListDataInformation.Where
        //                        (f => f.Entity.ClassEntityID == EntityID.Label &&
        //                              f.EntityRowID == Int32.Parse(DataRow[0].ToString())).ToList();

        //                    //Si el registro tiene datainformation, lo obtengo, en caso contrario creo uno nuevo
        //                    //if (DataInformationSerialList.Count > 0)
        //                    //Obtengo el DataInformation asociado al registro
        //                    DataInformationSerial = View.Model.ListDataInformation.Where
        //                        (f => f.Entity.ClassEntityID == EntityID.Label &&
        //                              f.EntityRowID == Int32.Parse(DataRow[0].ToString())).First();
        //                    /*else
        //                        //Creo un nuevo DataInformation
        //                        DataInformationSerial = new DataInformation
        //                        {
        //                            Entity = new ClassEntity { ClassEntityID = 20 },
        //                            EntityRowID = Int32.Parse(DataRow[0].ToString()),
        //                            CreationDate = DateTime.Now,
        //                            CreatedBy = App.curUser.UserName
        //                        };*/
        //                }
        //                catch
        //                {
        //                    Util.ShowError("Hubo un error durante el proceso de grabacion. Por favor intentar nuevamente.");
        //                    return;
        //                }




        //                //Deserializo el Xml para obtener el Showdata
        //                DetailDataList = Util.DeserializeMetaDataWF(DataInformationSerial.XmlData);

        //                //Inicializo la lista de los datos a convertir en Xml
        //                View.Model.ListDetailsDataSave = new List<ShowData>();

        //                //Obtengo los datos de cada campo con su nombre
        //                foreach (DataColumn c in View.Model.ListRecords.Columns)
        //                {
        //                    //Obtengo el label y el valor digitado
        //                    ChildrenLabel = c.ColumnName;
        //                    ChildrenValue = DataRow[c.ColumnName].ToString();

        //                    //Evaluo si el campo es obligatorio
        //                    ControlIsRequired = true;
        //                    DataDefinitionControlIsRequired = (View.Model.CamposDetails.Where(f => f.DataDefinition.Code == ChildrenLabel).Count() > 0) ? View.Model.CamposDetails.Where(f => f.DataDefinition.Code == ChildrenLabel).First() : null;
        //                    if (DataDefinitionControlIsRequired != null)
        //                    {
        //                        if (DataDefinitionControlIsRequired.DataDefinition.IsRequired == true && String.IsNullOrEmpty(ChildrenValue.ToString()))
        //                            ControlIsRequired = false;
        //                        else
        //                            ControlIsRequired = true;
        //                    }

        //                    //Evaluo si puedo continuar dependiendo de si el dato era requerido y fue digitado o no
        //                    if (ControlIsRequired)
        //                    {
        //                        //Evaluo si existe el dato que estoy leyendo para quitarlo del listado anterior
        //                        try
        //                        {
        //                            DetailDataSave = DetailDataList.Where(f => f.DataKey == c.ColumnName).First();
        //                            DetailDataList.Remove(DetailDataSave);
        //                        }
        //                        //En caso de no existir no quito nada
        //                        catch { }

        //                        //Creo el ShowData con el dato
        //                        DetailDataSave = new ShowData
        //                        {
        //                            DataKey = ChildrenLabel.ToString(),
        //                            DataValue = ChildrenValue.ToString()
        //                        };

        //                        //Adiciono cada dato a la lista
        //                        View.Model.ListDetailsDataSave.Add(DetailDataSave);
        //                    }
        //                    else
        //                    {
        //                        Util.ShowError("El campo " + ChildrenLabel.ToString() + " no puede ser vacio.");
        //                        return;
        //                    }

        //                    //Cargo el dato para la variable de ultimos procesados
        //                    View.Model.UltimosProcesadosMP = View.Model.UltimosProcesadosMP + ChildrenValue.ToString() + " \t ";
        //                }
        //                //Concateno las listas de ShowData, la que estaba guardada y la nueva
        //                View.Model.ListDetailsDataSave = View.Model.ListDetailsDataSave.Concat(DetailDataList).ToList();

        //                //Convierto el listado de datos a un Xml
        //                XmlData = Util.XmlSerializerWF(View.Model.ListDetailsDataSave);

        //                //Modifico el DataInformation con el nuevo Xml
        //                DataInformationSerial.XmlData = XmlData;

        //                //Actualizo el DataInformation o creo uno nuevo en el caso de que sea un producto de picking
        //                if (DataInformationSerialList.Count > 0)
        //                {
        //                    DataInformationSerial.ModDate = DateTime.Now;
        //                    DataInformationSerial.ModifiedBy = App.curUser.UserName;
        //                    service.UpdateDataInformation(DataInformationSerial);
        //                }
        //                else
        //                    service.SaveDataInformation(DataInformationSerial);

        //                //Paso #2.
        //                View.Model.UltimosProcesadosMP = View.Model.UltimosProcesadosMP + "\n";
        //            }
        //        }//cierre datarow

        //        if (Res)
        //        {
        //            LabelAux = service.GetLabel(new WpfFront.WMSBusinessService.Label
        //       {
        //           Bin = View.Model.BinTo,
        //           LabelType = new DocumentType { DocTypeID = LabelType.BinLocation }
        //       }).First();

        //            //Recorro el listado de Labels para actualizar sus datos
        //            foreach (WpfFront.WMSBusinessService.Label Label in View.Model.ListLabelScann)
        //            {
        //                Label.Notes += (View.Model.IsCheckedCommentsSerial) ? (" " + View.Model.HeaderDocument.Comment) : "";
        //                try { service.ChangeLabelLocationV2(Label, LabelAux, View.Model.HeaderDocument); }
        //                catch { }
        //            }

        //            //Reinicio las variables
        //            View.Model.ListRecords.Rows.Clear();
        //            View.Model.ListLabelScann = new List<WpfFront.WMSBusinessService.Label>();
        //            View.Model.ListDataInformation = new List<DataInformation>();

        //            //Muestro el mensaje de guardado 
        //            Util.ShowMessage("Datos Guardados exitosamente.");
        //        }

        //        if (!Res)
        //        {
        //            Util.ShowMsgGrilla(Dt_Res);
        //        }
        //    }

        //    catch (Exception Ex) { Util.ShowError("Hubo un error guardando los datos, por favor vuelva a intentarlo: " + Ex.Message); }
        //}


        //private bool Validar_serial_Lista_actual(string Code, string Serial)
        //{
        //    bool result = false;
        //    int Con = 0;
        //    //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
        //    foreach (DataRow DataRow in View.Model.ListRecords.Rows)
        //    {
        //        //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
        //        Con = DataRow[Code].ToString() == Serial ? Con++ : 0;
        //        result = Con > 1 ? true : false;
        //    }
        //    return result;
        //}

        //#endregion


        /// <summary>
        /// Mostrar la informacion de la lista del modulo de "Mover Producto" en una ventana emergente.
        /// OK.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMostrarInformacion(object sender, EventArgs e)
        {
            WindowInfo confirm = new WindowInfo();
            confirm.Txt_Mensaje.Text = View.Model.UltimosProcesadosMP;
            confirm.ShowDialog();
        }

        public void OnCbx_Etiqueta1_SelectedValue(object sender, DataEventArgs<MMaster> e)
        {
            if (e.Value != null)
            {
                //Etiquetas.Tipo = e.Value.Code2;
                Etiquetas.Parametros = e.Value.Code2;
                Etiquetas.Codigo_PL = new StringBuilder(e.Value.DefValue);
                Default_Value = String.IsNullOrEmpty(e.Value.Description) ? Default_Value = "" : Default_Value = e.Value.Description;
            }
        }


        public void Impresion_Etiqueta1(object sender, EventArgs e)
        {
            if (Default_Value.Equals("Impresora1"))
            {
                Enviar_Impresora("0");
            }
            else
            {
                Util.ShowError("Este tipo de etiqueta no corresponde a la impresora");
            }
        }


        public void Impresion_Etiqueta2(object sender, EventArgs e)
        {
            if (Default_Value.Equals("Impresora2"))
            {
                Enviar_Impresora("1");
            }
            else
            {
                Util.ShowError("Este tipo de etiqueta no corresponde a la impresora");
            }
        }


        private void Enviar_Impresora(string Btn)
        {

            //Variables Auxiliares
            Product Producto;
            char[] delimiterChars = { ',' };
            //string[] words = text.Split(delimiterChars);
            if (!Etiquetas.Codigo_PL.Equals(""))
            {
                DataTable Dt = View.Model.ListRecords;
                foreach (DataRow row in Dt.Rows)
                {
                    //varible auxiliar
                    int Col = 4;
                    if (View.GetListCbx_Etiqueta1.Text.Contains("Caja1") || View.GetListCbx_Etiqueta1.Text.Contains("Caja2"))
                    {
                        Col = 5;
                    }
                    Producto = service.GetLabel(new WMSBusinessService.Label { LabelID = long.Parse(row[0].ToString()) }).Select(f => f.Product).First();
                    Etiquetas.Imprimir(Producto.ProductCode, row[3].ToString(), row[Col].ToString(), Btn);
                }

            }
            else
            {
                Util.ShowMessage("Debe de seleccionar un tipo de etiqueta");
            }
        }
        #endregion
    }
}