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
using WpfFront.Common.WFUserControls;
using Microsoft.Windows.Controls;
using WpfFront.Common.Windows;
using System.Windows.Input;

namespace WpfFront.Presenters
{

    public interface IEntradaAlmacenPresenter
    {
        IEntradaAlmacenView View { get; set; }
        ToolWindow Window { get; set; }
    }


    public class EntradaAlmacenPresenter : IEntradaAlmacenPresenter
    {
        public IEntradaAlmacenView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares
        public Bin BinRecibo;
        public Product ProductDefault;
        public int offset = 4; //# columnas que no se debe replicar porque son fijas. 
        public Connection Local;
        public int TotalRegistrosCargar = 100;
        //nueva variable auxiliar
        WMSBusinessService.Document DocumentoConsultado;

        public EntradaAlmacenPresenter(IUnityContainer container, IEntradaAlmacenView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<EntradaAlmacenModel>();

            #region Metodos

            #region Header

            View.CargarHeader += new EventHandler<DataEventArgs<Bin>>(this.OnCargarHeader);
            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.CancelBasicData += new EventHandler<EventArgs>(this.OnCancelBasicData);
            View.NewBasicData += new EventHandler<EventArgs>(this.OnNewBasicData);
            View.MostrarInformacion += new EventHandler<EventArgs>(this.OnMostrarInformacion); //MostrarInformacion.

            #endregion

            #region Serial

            View.EvaluarTipoProducto += new EventHandler<DataEventArgs<Product>>(this.OnEvaluarTipoProducto);
            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);

            #endregion

            #region Details

            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);
            View.ReplicateDetails += new EventHandler<EventArgs>(this.OnReplicateDetails);
            View.Imprimir += new EventHandler<EventArgs>(this.OnImprimir);

            #endregion

            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinEntradaAlmacen = service.GetBin(new Bin { LevelCode = "R", Location = View.Model.RecordCliente });
            ProductDefault = service.GetProduct(new Product { ProductCode = WmsSetupValues.DEFAULT }).First();
            //string fullyqualifiedname = new DateTime().GetType().AssemblyQualifiedName;
            try { Local = service.GetConnection(new Connection { Name = "LOCAL" }).First(); }
            catch { }

            #endregion
        }



        #region Header

        private void CargarDatosHeader()
        {
            IList<DataDefinitionByBin> CamposHeader;
            CamposHeader = service.GetDataDefinitionByBin(new DataDefinitionByBin
            {
                DataDefinition = new DataDefinition { Location = View.Model.RecordCliente, IsHeader = true },
                Bin = BinRecibo
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

        private void CargarSeriales()
        {
            //Obtengo los seriales
            View.Model.CamposSeriales = service.GetDataDefinitionByBin(new DataDefinitionByBin
            {
                DataDefinition = new DataDefinition { Location = View.Model.RecordCliente, IsHeader = false, IsSerial = true },
                Bin = new Bin { BinCode = View.Model.RecordCliente.AddressLine1, Location = View.Model.RecordCliente }
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

            //Evaluo si el un serial automatico para colocar el valor
            if (View.Model.RecordCliente.AddressLine3 == "1")
            {
                Int32 Consecutivo = Int32.Parse(View.Model.RecordCliente.City) + 1;
                View.Model.RecordCliente.City = Consecutivo.ToString();
                View.GetSerial1.Text = Consecutivo.ToString();
                service.UpdateLocation(View.Model.RecordCliente);
            }

            //Coloco el focus en el serial1
            View.GetSerial1.Focus();
        }

        private void CargarDatosDetails()
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

            //Adiciono el campo de Cantidad en el DataTable
            Columna = new GridViewColumn();
            Txt = new FrameworkElementFactory(typeof(TextBlock));
            //Txt.SetValue(TextBlock.MaxWidthProperty, (double)50);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Cantidad"));
            Columna.Header = "Cantidad";
            Columna.Width = 55;
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.GetGridViewDetails.Columns.Add(Columna);
            View.Model.ListRecords.Columns.Add("Cantidad");

            //Recorro la lista de los seriales disponibles para generarlos en el DataTable
            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
            {
                Columna = new GridViewColumn();
                Txt = new FrameworkElementFactory(typeof(TextBlock));

                //Defino los datos del campo para serial
                Txt.SetValue(TextBlock.MaxWidthProperty, (double)110);
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
                DataDefinition = new DataDefinition { Location = View.Model.RecordCliente, IsHeader = false, IsSerial = false },
                Bin = BinRecibo
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
                        Txt.SetValue(TextBox.TabIndexProperty, (int)0);//Interruption Point
                        Txt.SetValue(TextBox.IsTabStopProperty, true);
                        Txt.AddHandler(System.Windows.Controls.TextBox.KeyDownEvent, new KeyEventHandler(OnValidarTeclaEnter)); //NUEVO EVENTO
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
                }

                // add textbox template
                Columna.CellTemplate = new DataTemplate();
                Columna.CellTemplate.VisualTree = Txt;
                View.GetGridViewDetails.Columns.Add(Columna); //Creacion de la columna en el GridView
                View.Model.ListRecords.Columns.Add(DataDefinitionByBin.DataDefinition.Code); //Creacion de la columna en el DataTable
            }
        }

        private void OnValidarTeclaEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                for (int i = 0; i < View.Model.ListRecords.Rows.Count; i++)
                {
                    View.LvDocumentMaster.SelectedIndex = View.LvDocumentMaster.SelectedIndex + 1; //GOOD, now it rest FOCUS THE TXT!
                    //View.LvDocumentMaster.Items.GetItemAt(5); Pequena prueba. = no funcional.
                    return; //Esto es para que se salga del FOR y no siga recorriendo las filas hasta el final.
                }
            }
        }

        private void OnCargarHeader(object sender, DataEventArgs<Bin> e)
        {
            View.GetStackPanelHeader.Children.Clear();
            BinRecibo = e.Value;
            CargarDatosHeader();
        }

        private void OnConfirmBasicData(object sender, EventArgs e)
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
                        //Creo el ShowData con los datos de ChildrenLabel y ChildrenValue
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
                View.Model.HeaderDocument.Location = View.Model.RecordCliente;
                View.Model.HeaderDocument.DocType = new DocumentType { DocTypeID = SDocType.ReceivingTask };
                View.Model.HeaderDocument.IsFromErp = false;
                View.Model.HeaderDocument.CrossDocking = false;
                View.Model.HeaderDocument.Vendor = service.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT }).First();
                View.Model.HeaderDocument.Customer = service.GetAccount(new Account { AccountCode = WmsSetupValues.DEFAULT }).First();
                View.Model.HeaderDocument.Date1 = DateTime.Now;
                View.Model.HeaderDocument.CreatedBy = App.curUser.UserName;
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
                    CreatedBy = App.curUser.UserName,
                };
                //Guardo el Xml en la tabla DataInformation
                View.Model.DataInformationHeader = service.SaveDataInformation(View.Model.DataInformationHeader);
                //Ejecuto el proceso para cargar los datos a las tablas
                CargarDatosXML(View.Model.DataInformationHeader);
                //Inhabilito el boton de Guardar, muestro Cancelar y oculto Nuevo
                View.GetButtonConfirmar.IsEnabled = false;
                View.GetButtonConfirmar.Visibility = Visibility.Collapsed;
                View.GetButtonNuevo.Visibility = Visibility.Collapsed;
                View.GetButtonCancelar.Visibility = Visibility.Visible;
                //Inhabilito el combobox de bin de ingreso
                View.GetListBinInicio.IsEnabled = false;
                //Muestro el boton para mostrar los datos del header y oculto header
                View.GetTextHideShowHeader.Visibility = Visibility.Visible;
                View.GetBorderHeader.Visibility = Visibility.Visible;
                //Cargo los datos para el detalle, los seriales y muestro
                CargarSeriales();
                CargarDatosDetails();
                View.BorderDetails.Visibility = Visibility.Visible;
                //Muestro el mensaje de confirmacion
                //Util.ShowMessage("El documento se guardo satisfactoriamente");

                //valido el rol pare realizar el cargue masivo
                if (App.curRol.Rol.RolCode.Equals("MASIVO"))
                {
                    View.GetStackUploadFile.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error durante el proceso: " + Ex.Message); }
        }

        private void OnCancelBasicData(object sender, EventArgs e)
        {
            Util.ShowMessage("Regitros guardados exitosamente. \nCierre la pestaña y genere una nueva entrada al almacen.");
        }

        private void OnNewBasicData(object sender, EventArgs e)
        {
            //Reinicio el documento
            View.Model.HeaderDocument = new Document();
            //Habilito el boton de Guardar, oculto Cancelar y Nuevo
            View.GetButtonConfirmar.IsEnabled = true;
            View.GetButtonCancelar.Visibility = Visibility.Collapsed;
            View.GetButtonNuevo.Visibility = Visibility.Visible;
            //Habilito el combobox del listado de bin de ingreso
            View.GetListBinInicio.IsEnabled = true;
            //Elimino los campos del Header
            View.GetStackPanelHeader.Children.Clear();
            //Creo los campos del Header
            CargarDatosHeader();
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

        #endregion

        #region Serial

        private void OnEvaluarTipoProducto(object sender, DataEventArgs<Product> e)
        {
            if (View.GetProductLocation.Product != null)
            {
                //Asigno el producto seleccionado a la variable del modelo
                View.Model.ProductoSerial = e.Value;
                //Evaluo si el producto es serializable o no para habilitar el campo o no. 1 => Serializable, 0 => no Serializable
                if (View.Model.ProductoSerial.ErpTrackOpt == 1)
                {
                    View.GetCantidadProducto.Text = "1";
                    View.GetCantidadProducto.IsEnabled = false;
                    View.GetSerial1.IsEnabled = View.GetSerial2.IsEnabled = View.GetSerial3.IsEnabled = View.GetUpLoadFile.IsEnabled = true;
                    View.GetSerial1.Focus();
                }
                else
                {
                    View.GetCantidadProducto.Text = "";
                    View.GetCantidadProducto.IsEnabled = true;
                    View.GetCantidadProducto.Focus();
                    View.GetSerial1.IsEnabled = View.GetSerial2.IsEnabled = View.GetSerial3.IsEnabled = View.GetUpLoadFile.IsEnabled = false;
                }
            }
        }

        #region Version Antigua OnAddLine

        private void OnAddLine(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();

            //WpfFront.WMSBusinessService.Label EvalSerialSave = new WpfFront.WMSBusinessService.Label { Bin = new Bin { Location = View.Model.RecordCliente } };
            bool EvalSerial1 = false, EvalSerial2 = false, EvalSerial3 = false;
            DataSet ValidarSeriales;

            //Validacion si fue seleccionado el producto
            if (View.Model.ProductoSerial == null)
            {
                Util.ShowError("Por favor seleccionar un producto");
                return;
            }

            //Validacion si el producto no es serializable y no se coloco un valor en la cantidad
            if (View.Model.ProductoSerial.ErpTrackOpt == 0 && String.IsNullOrEmpty(View.GetCantidadProducto.Text))
            {
                Util.ShowError("Por favor digitar la cantidad a ingresar del producto.");
                return;
            }

            //Genero la variable del Label para adicionar el dato
            WpfFront.WMSBusinessService.Label SerialLabel = new WpfFront.WMSBusinessService.Label
            {
                LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel },
                Status = new Status { StatusID = EntityStatus.Active },
                StartQty = Int32.Parse(View.GetCantidadProducto.Text),
                CurrQty = Int32.Parse(View.GetCantidadProducto.Text),
                Node = new Node { NodeID = NodeType.PreLabeled },
                Bin = BinRecibo,
                Printed = (View.Model.ProductoSerial.ErpTrackOpt == 0) ? false : true,
                Notes = (View.Model.IsCheckedCommentsSerial) ? View.Model.HeaderDocument.Comment : "Doc# " + View.Model.HeaderDocument.DocNumber,
                ReceivingDate = DateTime.Now,
                CreatedBy = App.curUser.UserName,
                CreationDate = DateTime.Now,
                ReceivingDocument = View.Model.HeaderDocument,
                Product = View.Model.ProductoSerial,
                Unit = View.Model.ProductoSerial.BaseUnit,
                CreTerminal = View.Model.RecordCliente.LocationID.ToString()
            };

            //Si el producto es serializable hago las evaluaciones, en caso contrario omito estos pasos
            if (View.Model.ProductoSerial.ErpTrackOpt == 1)
            {
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

                            //Consulto el serial1 en el sistema
                            ValidarSeriales = service.DirectSQLQueryDS("EXEC dbo.spUtil 1, '" + View.Model.RecordCliente.LocationID + "', '" + View.GetSerial1.Text + "'", "", "Trace.Label", Local);

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            if (ValidarSeriales.Tables[0].Rows.Count > 0)
                            {
                                Util.ShowError("El serial " + View.GetSerial1.Text + " ya existe en el sistema");
                                return;
                            }

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            if (ValidarSeriales.Tables[1].Rows.Count > 0)
                            {
                                Util.ShowError("El serial " + View.GetSerial1.Text + " ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Tables[1].Rows.Count + " oportunidad(es).");
                            }

                            //Consulto el serial2 en la bodega de despachos para saber si es un reingreso
                            /*ValidarSeriales = service.DirectSQLQuery("EXEC dbo.spUtil 1, '453', '" + View.GetSerial2.Text + "'", "", "Trace.Label", Local);

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            if (ValidarSeriales.Rows.Count > 0)
                            {
                                Util.ShowError("El serial " + View.GetSerial2.Text + " ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Rows.Count + " oportunidad(es).");
                            }*/

                            //Asigno el serial para evaluar si ya existe en el sistema
                            //EvalSerialSave.PrintingLot = View.GetSerial2.Text;

                            //Evaluo si existe la columna para validar el serial existente en el listado actual
                            if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
                            {
                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial1 = DataRow[DataDefinitionByBin.DataDefinition.Code].ToString() == View.GetSerial2.Text ? true : false;
                                    if (EvalSerial1)
                                        break;
                                }
                            }

                            //Evaluo si existe la columna del serial1 para validar la existencia del serial digitado en serial2
                            if (View.Model.ListRecords.Columns.Contains("SERIAL2") && !EvalSerial1)
                            {
                                //Evaluo que el serial1 sea diferente al serial2
                                if (View.GetSerial1.Text == View.GetSerial2.Text)
                                {
                                    Util.ShowError("No pueden haber seriales repetidos.");
                                    return;
                                }

                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial1 = DataRow["SERIAL2"].ToString() == View.GetSerial1.Text ? true : false;
                                    if (EvalSerial1)
                                        break;
                                }
                            }

                            //Evaluo si existe la columna del serial3 para validar la existencia del serial digitado en serial2
                            if (View.Model.ListRecords.Columns.Contains("SERIAL3") && !EvalSerial1)
                            {
                                //Evaluo que el serial1 sea diferente al serial3
                                if (View.GetSerial1.Text == View.GetSerial3.Text)
                                {
                                    Util.ShowError("No pueden haber seriales repetidos.");
                                    return;
                                }

                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial2 = DataRow["SERIAL3"].ToString() == View.GetSerial2.Text ? true : false;
                                    if (EvalSerial2)
                                        break;
                                }
                            }

                            break;

                        /*case "SERIAL2":
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

                            //Consulto el serial2 en el sistema
                            ValidarSeriales = service.DirectSQLQueryDS("EXEC dbo.spUtil 1, '" + View.Model.RecordCliente.LocationID + "', '" + View.GetSerial2.Text + "'", "", "Trace.Label", Local);

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            if (ValidarSeriales.Tables[0].Rows.Count > 0)
                            {
                                Util.ShowError("El serial " + View.GetSerial2.Text + " ya existe en el sistema");
                                return;
                            }

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            if (ValidarSeriales.Tables[1].Rows.Count > 0)
                            {
                                Util.ShowError("El serial " + View.GetSerial2.Text + " ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Tables[1].Rows.Count + " oportunidad(es).");
                            }

                            //Consulto el serial2 en la bodega de despachos para saber si es un reingreso
                            //ValidarSeriales = service.DirectSQLQuery("EXEC dbo.spUtil 1, '453', '" + View.GetSerial2.Text + "'", "", "Trace.Label", Local);

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            //if (ValidarSeriales.Rows.Count > 0)
                            //{
                            //    Util.ShowError("El serial " + View.GetSerial2.Text + " ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Rows.Count + " oportunidad(es).");
                            //}

                            //Asigno el serial para evaluar si ya existe en el sistema
                            //EvalSerialSave.PrintingLot = View.GetSerial2.Text;

                            //Evaluo si existe la columna para validar el serial existente en el listado actual
                            if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
                            {
                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial2 = DataRow[DataDefinitionByBin.DataDefinition.Code].ToString() == View.GetSerial2.Text ? true : false;
                                    if (EvalSerial2)
                                        break;
                                }
                            }

                            //Evaluo si existe la columna del serial1 para validar la existencia del serial digitado en serial2
                            if (View.Model.ListRecords.Columns.Contains("SERIAL1") && !EvalSerial2)
                            {
                                //Evaluo que el serial2 sea diferente al serial1
                                if (View.GetSerial2.Text == View.GetSerial1.Text)
                                {
                                    Util.ShowError("No pueden haber seriales repetidos.");
                                    return;
                                }

                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial2 = DataRow["SERIAL1"].ToString() == View.GetSerial2.Text ? true : false;
                                    if (EvalSerial2)
                                        break;
                                }
                            }

                            //Evaluo si existe la columna del serial3 para validar la existencia del serial digitado en serial2
                            if (View.Model.ListRecords.Columns.Contains("SERIAL3") && !EvalSerial2)
                            {
                                //Evaluo que el serial2 sea diferente al serial3
                                if (View.GetSerial2.Text == View.GetSerial3.Text)
                                {
                                    Util.ShowError("No pueden haber seriales repetidos.");
                                    return;
                                }

                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial2 = DataRow["SERIAL3"].ToString() == View.GetSerial2.Text ? true : false;
                                    if (EvalSerial2)
                                        break;
                                }
                            }

                            break;*/
                        /*case "SERIAL3":
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

                            //Consulto el serial3 en el sistema
                            ValidarSeriales = service.DirectSQLQueryDS("EXEC dbo.spUtil 1, '" + View.Model.RecordCliente.LocationID + "', '" + View.GetSerial3.Text + "'", "", "Trace.Label", Local);

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            if (ValidarSeriales.Tables[0].Rows.Count > 0)
                            {
                                Util.ShowError("El serial " + View.GetSerial3.Text + " ya existe en el sistema");
                                return;
                            }

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            if (ValidarSeriales.Tables[1].Rows.Count > 0)
                            {
                                Util.ShowError("El serial " + View.GetSerial3.Text + " ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Tables[1].Rows.Count + " oportunidad(es).");
                            }

                            //Consulto el serial3 en la bodega de despachos para saber si es un reingreso
                            //ValidarSeriales = service.DirectSQLQuery("EXEC dbo.spUtil 1, '453', '" + View.GetSerial3.Text + "'", "", "Trace.Label", Local);

                            //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
                            //if (ValidarSeriales.Rows.Count > 0)
                            //{
                            //    Util.ShowError("El serial " + View.GetSerial3.Text + " ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Rows.Count + " oportunidad(es).");
                            //}

                            //Asigno el serial para evaluar si ya existe en el sistema
                            //EvalSerialSave.Manufacturer = View.GetSerial3.Text;

                            //Evaluo si existe la columna para validar el serial existente en el listado actual
                            if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
                            {
                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial3 = DataRow[DataDefinitionByBin.DataDefinition.Code].ToString() == View.GetSerial3.Text ? true : false;
                                    if (EvalSerial3)
                                        break;
                                }
                            }

                            //Evaluo si existe la columna del serial1 para validar la existencia del serial digitado en serial3
                            if (View.Model.ListRecords.Columns.Contains("SERIAL1") && !EvalSerial3)
                            {
                                //Evaluo que el serial3 sea diferente al serial1
                                if (View.GetSerial3.Text == View.GetSerial1.Text)
                                {
                                    Util.ShowError("No pueden haber seriales repetidos.");
                                    return;
                                }

                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial3 = DataRow["SERIAL1"].ToString() == View.GetSerial3.Text ? true : false;
                                    if (EvalSerial3)
                                        break;
                                }
                            }

                            //Evaluo si existe la columna del serial2 para validar la existencia del serial digitado en serial3
                            if (View.Model.ListRecords.Columns.Contains("SERIAL2") && !EvalSerial3)
                            {
                                //Evaluo que el serial3 sea diferente al serial2
                                if (View.GetSerial3.Text == View.GetSerial2.Text)
                                {
                                    Util.ShowError("No pueden haber seriales repetidos.");
                                    return;
                                }

                                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                                {
                                    EvalSerial3 = DataRow["SERIAL2"].ToString() == View.GetSerial3.Text ? true : false;
                                    if (EvalSerial3)
                                        break;
                                }
                            }

                            break;*/
                    }
                }

                //Analizo si el serial ya esta en el sistema
                //EvalSerialSave.Bin = new Bin { Location = View.Model.RecordCliente };

                /*if (service.GetLabel(EvalSerialSave).Count() > 0)
                {
                    Util.ShowError("El serial ya existe en el sistema");
                    return;
                }*/

                //Analizo si el serial ya esta en la lista
                if (EvalSerial1 && EvalSerial2 && EvalSerial3)
                {
                    Util.ShowError("El serial ya existe en el listado de ingresos");
                    return;
                }

                //Evaluo cuales seriales estan visibles para adicionarlos al registro
                int cont = 4;
                foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
                {
                    switch (DataDefinitionByBin.DataDefinition.Code)
                    {
                        case "SERIAL1":
                            dr[cont] = View.GetSerial1.Text;
                            SerialLabel.LabelCode = View.GetSerial1.Text;
                            break;
                        case "SERIAL2":
                            dr[cont] = View.GetSerial2.Text;
                            SerialLabel.PrintingLot = View.GetSerial2.Text;
                            break;
                        case "SERIAL3":
                            dr[cont] = View.GetSerial3.Text;
                            SerialLabel.Manufacturer = View.GetSerial3.Text;
                            break;
                    }
                    cont++;
                }

                //offset = cont;
            }
            //Caso en que el producto no es serializable
            else
            {
                SerialLabel.LabelCode = Guid.NewGuid().ToString();
            }

            if (View.Model.ProductoSerial.ErpTrackOpt == 0)
            {
                //Realizo un control si hay algun error guardando los datos
                try
                {
                    //Define Document, Product, Unit and Qty to send to receiving transaction
                    DocumentLine receivingLine = new DocumentLine
                    {
                        Document = View.Model.HeaderDocument,
                        Product = SerialLabel.Product,
                        Unit = SerialLabel.Product.BaseUnit,
                        Quantity = SerialLabel.StartQty,
                        QtyPending = SerialLabel.StartQty,
                        QtyAllocated = 0,
                        CreatedBy = App.curUser.UserName,
                        Note = SerialLabel.Notes
                    };
                    service.ReceiveProduct(receivingLine, SerialLabel.Product.BaseUnit, SerialLabel.Bin, new Node { NodeID = NodeType.Received });
                }
                catch (Exception Ex)
                {
                    Util.ShowError("1. Hubo un error intentando recibir el producto, por favor volver a intentarlo. Error: " + Ex.Message);
                    return;
                }
            }
            else
            {
                //Realizo un control si hay algun error guardando los datos
                try
                {
                    //Guardo el serial en Label y asigno el Id
                    SerialLabel = service.SaveLabel(SerialLabel);
                    service.ReceiveLabel(View.Model.HeaderDocument, SerialLabel, SerialLabel.Bin, new Node { NodeID = NodeType.Received });

                    //Valido si el Label fue guardado correctamente para continuar, en caso contrario muestro mensaje de error y aborto el save
                    if (SerialLabel.LabelID == 0)
                    {
                        Util.ShowError("2. Hubo un error intentando recibir el producto no serializado, por favor volver a intentarlo.");
                        return;
                    }
                }
                catch
                {
                    //Util.ShowMessage("El sistema esta ocupado. Oprima enter para continuar con la carga.");
                    ////Guardo el serial en Label y asigno el Id
                    //SerialLabel = service.SaveLabel(SerialLabel);
                    //service.ReceiveLabel(View.Model.HeaderDocument, SerialLabel, SerialLabel.Bin, new Node { NodeID = NodeType.Received });

                    //Ciclo que espera por un recurso en la BD para guardar un registro.
                    Boolean Control = true;
                    //Util.ShowMessage("El sistema esta ocupado. Oprima enter para continuar con la carga.");
                    while (Control)
                    {
                        try
                        {
                            SerialLabel = service.SaveLabel(SerialLabel);
                            //service.SaveLabel(SerialLabel);
                            service.ReceiveLabel(View.Model.HeaderDocument, SerialLabel, SerialLabel.Bin, new Node { NodeID = NodeType.Received });
                            Control = false;
                        }
                        catch { Control = true; }
                    }
                }
            }

            #region Agregar los campos a la grilla

            //Asigno al campo 0 el LabelID
            dr[0] = SerialLabel.LabelID;

            //Asigno al campo 1 ProductID, campo 2 ProducName, campo 3 Cantidad
            dr[1] = View.Model.ProductoSerial.ProductID;
            dr[2] = View.Model.ProductoSerial.Name;
            dr[3] = View.GetCantidadProducto.Text;

            //Asigno a cada campo su valor definido por defecto
            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposDetails)
            {
                dr[DataDefinitionByBin.DataDefinition.Code] = DataDefinitionByBin.DataDefinition.DefaultValue;
            }
            View.Model.ListRecords.Rows.Add(dr);

            //Limpio los campos para digitar nuevos valores
            if (View.Model.ProductoSerial.ErpTrackOpt == 1)
            {
                //Limpio los seriales para digitar nuevos datos
                View.GetSerial1.Text = (View.Model.RecordCliente.AddressLine3 == "1") ? View.Model.RecordCliente.City : "";
                View.GetSerial2.Text = "";
                View.GetSerial3.Text = "";
                View.GetSerial1.Focus();
            }
            else
            {
                //Limpio la cantidad de producto a ingresar
                View.GetCantidadProducto.Text = "";
                View.GetCantidadProducto.Focus();
            }
            #endregion
        }

        #endregion

        //#region actualizado OnAddLine
        //private void OnAddLine(object sender, EventArgs e)
        //{
        //    //Variables Auxiliares           
        //    DataRow dr = View.Model.ListRecords.NewRow();

        //    //Validacion si fue seleccionado el producto
        //    if (View.Model.ProductoSerial == null)
        //    {
        //        Util.ShowError("Por favor seleccionar un producto");
        //        return;
        //    }

        //    //Validacion si el producto no es serializable y no se coloco un valor en la cantidad
        //    if (View.Model.ProductoSerial.ErpTrackOpt == 0 && String.IsNullOrEmpty(View.GetCantidadProducto.Text))
        //    {
        //        Util.ShowError("Por favor digitar la cantidad a ingresar del producto.");
        //        return;
        //    }

        //    //Evaluo cuales seriales estan visibles para adicionarlos al registro
        //    int cont = 4;
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
        //                else if (View.GetSerial1.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
        //                {
        //                    Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
        //                    return;
        //                }
        //                //Agrego El Valor A la columna
        //                dr[cont] = View.GetSerial1.Text;

        //                break;
        //            case "SERIAL2":
        //                if (View.GetSerial2.Text == "")
        //                {
        //                    Util.ShowError("Por favor digite el serial en " + DataDefinitionByBin.DataDefinition.DisplayName);
        //                    return;
        //                }
        //                else if (View.GetSerial2.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
        //                {
        //                    Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
        //                    return;
        //                }
        //                //Agrego El Valor A la columna
        //                dr[cont] = View.GetSerial2.Text;
        //                break;
        //            case "SERIAL3":
        //                if (View.GetSerial3.Text == "")
        //                {
        //                    Util.ShowError("Por favor digite el serial en " + DataDefinitionByBin.DataDefinition.DisplayName);
        //                    return;
        //                }
        //                else if (View.GetSerial3.Text.Length != DataDefinitionByBin.DataDefinition.Size && DataDefinitionByBin.DataDefinition.Size != 0)
        //                {
        //                    Util.ShowError("El serial en el campo " + DataDefinitionByBin.DataDefinition.DisplayName + " debe tener " + DataDefinitionByBin.DataDefinition.Size + " digitos.");
        //                    return;
        //                }
        //                //Agrego El Valor A la columna
        //                dr[cont] = View.GetSerial3.Text;
        //                break;
        //        }
        //        cont++;
        //    }
        //    #region Agregar los campos a la grilla


        //    double LabelID = 0;
        //    if (View.Model.ListRecords.Rows.Count == 0)
        //    {
        //        LabelID = double.Parse(service.DirectSQLQueryDS("SELECT MAX(LabelID)+1 AS Mayor FROM Trace.Label", "", "Trace.Label", Local).Tables[0].Rows[0][0].ToString());
        //    }
        //    else
        //    {
        //        LabelID = double.Parse(View.Model.ListRecords.Rows[View.Model.ListRecords.Rows.Count - 1][0].ToString()) + 1;
        //    }
        //    dr[0] = LabelID;
        //    //Asigno al campo 1 ProductID, campo 2 ProducName, campo 3 Cantidad
        //    dr[1] = View.Model.ProductoSerial.ProductID;
        //    dr[2] = View.Model.ProductoSerial.Name;
        //    dr[3] = View.GetCantidadProducto.Text;

        //    //Asigno a cada campo su valor definido por defecto
        //    foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposDetails)
        //    {
        //        dr[DataDefinitionByBin.DataDefinition.Code] = DataDefinitionByBin.DataDefinition.DefaultValue;
        //    }
        //    View.Model.ListRecords.Rows.Add(dr);

        //    //Limpio los campos para digitar nuevos valores
        //    if (View.Model.ProductoSerial.ErpTrackOpt == 1)
        //    {
        //        //Limpio los seriales para digitar nuevos datos
        //        View.GetSerial1.Text = (View.Model.RecordCliente.AddressLine3 == "1") ? View.Model.RecordCliente.City : "";
        //        View.GetSerial2.Text = "";
        //        View.GetSerial3.Text = "";
        //        View.GetSerial1.Focus();
        //    }
        //    else
        //    {
        //        //Limpio la cantidad de producto a ingresar
        //        View.GetCantidadProducto.Text = "";
        //        View.GetCantidadProducto.Focus();
        //    }
        //    #endregion
        //}
        //#endregion


        #endregion

        #region Details

        #region Version Actualizada OnSaveDetails

        //private void OnSaveDetails(object sender, EventArgs e)
        //{
        //    //Validacion si no existen datos para guardar
        //    if (View.Model.ListRecords.Rows.Count == 0)
        //        return;

        //    //Variables Auxiliares
        //    DataInformation DataInformationSerial;
        //    DataDefinitionByBin DataDefinitionControlIsRequired;
        //    Object ChildrenValue, ChildrenLabel;
        //    ShowData DetailDataSave;
        //    string XmlData;
        //    bool ControlIsRequired;
        //    DataTable Dt_res = new DataTable();
        //    bool Permiso = true;
        //    double Registrados = 0;


        //    string[] Columas = { "TIPO", "# SERIAL", "DESCRIPCION" };
        //    foreach (string Col in Columas)
        //    {
        //        Dt_res.Columns.Add(Col);
        //    }

        //    try
        //    {
        //        foreach (DataRow DataRow in View.Model.ListRecords.Rows)
        //        {
        //            //Inicializo la lista de los datos a convertir en Xml
        //            View.Model.ListDetailsDataSave = new List<ShowData>();

        //            #region Codigo Juan

        //            bool EvalSerial1 = false, EvalSerial2 = false, EvalSerial3 = false;
        //            DataSet ValidarSeriales;


        //            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
        //            {
        //                switch (DataDefinitionByBin.DataDefinition.Code)
        //                {
        //                    case "SERIAL1":

        //                        //Consulto el serial1 en el sistema
        //                        ValidarSeriales = service.DirectSQLQueryDS("EXEC dbo.spUtil 1, '" + View.Model.RecordCliente.LocationID + "', '" + DataRow["SERIAL1"].ToString() + "'", "", "Trace.Label", Local);

        //                        //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
        //                        if (ValidarSeriales.Tables[0].Rows.Count > 0)
        //                        {
        //                            Dt_res.Rows.Add(new object[] { "SERIAL1", DataRow["SERIAL1"].ToString(), "Ya existe en el sistema" });
        //                            Permiso = false;
        //                            break;
        //                        }

        //                        //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
        //                        if (ValidarSeriales.Tables[1].Rows.Count > 0)
        //                        {
        //                            Dt_res.Rows.Add(new object[] { "SERIAL1", DataRow["SERIAL1"].ToString(), "ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Tables[1].Rows.Count + " oportunidad(es)." });
        //                            Permiso = false;
        //                        }

        //                        //Evaluo si existe la columna para validar el serial existente en el listado actual
        //                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
        //                        {
        //                            //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
        //                            EvalSerial1 = Validar_serial_Lista_actual(DataDefinitionByBin.DataDefinition.Code, DataRow["SERIAL1"].ToString());
        //                        }

        //                        //Evaluo si existe la columna del serial1 para validar la existencia del serial digitado en serial2
        //                        if (View.Model.ListRecords.Columns.Contains("SERIAL2") && !EvalSerial1)
        //                        {
        //                            //Evaluo que el serial1 sea diferente al serial2
        //                            if (DataRow["SERIAL1"].ToString().Equals(DataRow["SERIAL2"].ToString()))
        //                            {
        //                                Dt_res.Rows.Add(new object[] { "SERIAL1", DataRow["SERIAL1"].ToString(), "No puede ser igual al SERIAL2." });
        //                                Permiso = false;
        //                                break;
        //                            }
        //                            ////Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
        //                            //EvalSerial1 = Validar_serial_Lista_actual("SERIAL2", DataRow["SERIAL1"].ToString());

        //                        }

        //                        //Evaluo si existe la columna del serial3 para validar la existencia del serial digitado en serial2
        //                        if (View.Model.ListRecords.Columns.Contains("SERIAL3") && !EvalSerial1)
        //                        {
        //                            //Evaluo que el serial1 sea diferente al serial3
        //                            if (DataRow["SERIAL1"].ToString().Equals(DataRow["SERIAL3"].ToString()))
        //                            {
        //                                Dt_res.Rows.Add(new object[] { "SERIAL1", DataRow["SERIAL1"].ToString(), "No puede ser igual al SERIAL3." });
        //                                Permiso = false;
        //                                break;
        //                            }
        //                            ////Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido                                                
        //                            //EvalSerial2 = Validar_serial_Lista_actual("SERIAL3", DataRow["SERIAL1"].ToString());
        //                        }
        //                        break;

        //                    case "SERIAL2":

        //                        //Consulto el serial2 en el sistema
        //                        ValidarSeriales = service.DirectSQLQueryDS("EXEC dbo.spUtil 1, '" + View.Model.RecordCliente.LocationID + "', '" + DataRow["SERIAL2"].ToString() + "'", "", "Trace.Label", Local);

        //                        //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
        //                        if (ValidarSeriales.Tables[0].Rows.Count > 0)
        //                        {
        //                            Dt_res.Rows.Add(new object[] { "SERIAL2", DataRow["SERIAL2"].ToString(), "Ya existe en el sistema" });
        //                            Permiso = false;
        //                            break;
        //                        }

        //                        //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
        //                        if (ValidarSeriales.Tables[1].Rows.Count > 0)
        //                        {
        //                            Dt_res.Rows.Add(new object[] { "SERIAL2", DataRow["SERIAL2"].ToString(), "El serial ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Tables[1].Rows.Count + " oportunidad(es)." });
        //                            Permiso = false;
        //                            break;
        //                        }

        //                        //Evaluo si existe la columna para validar el serial existente en el listado actual
        //                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
        //                        {
        //                            //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido                                                
        //                            EvalSerial2 = Validar_serial_Lista_actual(DataDefinitionByBin.DataDefinition.Code.ToString(), DataRow["SERIAL2"].ToString());
        //                        }


        //                        //Evaluo si existe la columna del serial1 para validar la existencia del serial digitado en serial2
        //                        if (View.Model.ListRecords.Columns.Contains("SERIAL1") && !EvalSerial2)
        //                        {
        //                            //Evaluo que el serial2 sea diferente al serial1
        //                            if (DataRow["SERIAL2"].ToString().Equals(DataRow["SERIAL1"].ToString()))
        //                            {
        //                                Dt_res.Rows.Add(new object[] { "SERIAL2", DataRow["SERIAL2"].ToString(), "No puede ser igual al SERIAL1." });
        //                                Permiso = false;
        //                                break;
        //                            }
        //                            //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
        //                            //EvalSerial2 = Validar_serial_Lista_actual("SERIAL1", DataRow["SERIAL2"].ToString());

        //                        }

        //                        //Evaluo si existe la columna del serial3 para validar la existencia del serial digitado en serial2
        //                        if (View.Model.ListRecords.Columns.Contains("SERIAL3") && !EvalSerial2)
        //                        {
        //                            //Evaluo que el serial2 sea diferente al serial3
        //                            if (DataRow["SERIAL2"].ToString().Equals(DataRow["SERIAL3"].ToString()))
        //                            {
        //                                Dt_res.Rows.Add(new object[] { "SERIAL2", DataRow["SERIAL2"].ToString(), "No puede ser igual al SERIAL3." });
        //                                Permiso = false;
        //                                break;
        //                            }
        //                            //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
        //                            //EvalSerial2 = Validar_serial_Lista_actual("SERIAL1", DataRow["SERIAL2"].ToString());
        //                        }
        //                        break;
        //                    case "SERIAL3":

        //                        //Consulto el serial3 en el sistema
        //                        ValidarSeriales = service.DirectSQLQueryDS("EXEC dbo.spUtil 1, '" + View.Model.RecordCliente.LocationID + "', '" + DataRow["SERIAL3"].ToString() + "'", "", "Trace.Label", Local);

        //                        //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
        //                        if (ValidarSeriales.Tables[0].Rows.Count > 0)
        //                        {
        //                            Dt_res.Rows.Add(new object[] { "SERIAL3", DataRow["SERIAL3"].ToString(), "Ya existe en el sistema" });
        //                            Permiso = false;
        //                            break;
        //                        }

        //                        //Evaluo si la consulta me trajo serial alguno para mostrar el mensaje de error
        //                        if (ValidarSeriales.Tables[1].Rows.Count > 0)
        //                        {
        //                            Dt_res.Rows.Add(new object[] { "SERIAL3", View.GetSerial3.Text, "El serial " + DataRow["SERIAL3"].ToString() + " ya fue procesado y despachado anteriormente. Ha sido procesado en " + ValidarSeriales.Tables[1].Rows.Count + " oportunidad(es)." });
        //                            Permiso = false;
        //                        }

        //                        //Evaluo si existe la columna para validar el serial existente en el listado actual
        //                        if (View.Model.ListRecords.Columns.Contains(DataDefinitionByBin.DataDefinition.Code))
        //                        {
        //                            EvalSerial3 = Validar_serial_Lista_actual(DataDefinitionByBin.DataDefinition.Code, DataRow["SERIAL3"].ToString());
        //                        }

        //                        //Evaluo si existe la columna del serial1 para validar la existencia del serial digitado en serial3
        //                        if (View.Model.ListRecords.Columns.Contains("SERIAL1") && !EvalSerial3)
        //                        {
        //                            //Evaluo que el serial3 sea diferente al serial1
        //                            if (DataRow["SERIAL3"].ToString().Equals(DataRow["SERIAL1"].ToString()))
        //                            {
        //                                Dt_res.Rows.Add(new object[] { "SERIAL3", DataRow["SERIAL3"].ToString(), "No puede ser igual al SERIAL1." });
        //                                Permiso = false;
        //                                break;
        //                            }

        //                            //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
        //                            EvalSerial3 = Validar_serial_Lista_actual("SERIAL1", DataRow["SERIAL3"].ToString());
        //                        }

        //                        //Evaluo si existe la columna del serial2 para validar la existencia del serial digitado en serial3
        //                        if (View.Model.ListRecords.Columns.Contains("SERIAL2") && !EvalSerial3)
        //                        {
        //                            //Evaluo que el serial3 sea diferente al serial2
        //                            if (DataRow["SERIAL3"].ToString().Equals(DataRow["SERIAL2"].ToString()))
        //                            {
        //                                Dt_res.Rows.Add(new object[] { "SERIAL3", DataRow["SERIAL3"].ToString(), "El serial3 No puede ser igual al SERIAL2." });
        //                                Permiso = false;
        //                                break;
        //                            }
        //                            //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
        //                            //EvalSerial3 = Validar_serial_Lista_actual("SERIAL2", DataRow["SERIAL3"].ToString());
        //                        }
        //                        break;
        //                }
        //            }
        //            if (EvalSerial1 || EvalSerial2 || EvalSerial3)
        //            {
        //                StringBuilder Seriales = new StringBuilder(); ;
        //                if (EvalSerial1)
        //                {
        //                    Seriales.AppendLine(DataRow["SERIAL1"].ToString());
        //                }
        //                if (EvalSerial2)
        //                {
        //                    Seriales.AppendLine("," + DataRow["SERIAL2"].ToString());
        //                }
        //                if (EvalSerial2)
        //                {
        //                    Seriales.AppendLine("," + DataRow["SERIAL3"].ToString());
        //                }
        //                Dt_res.Rows.Add(new object[] { Seriales, "El serial ya existe en el listado de ingresos" });
        //                Permiso = false;
        //            }

        //            if (Permiso)
        //            {



        //                //Genero la variable del Label para adicionar el dato
        //                WpfFront.WMSBusinessService.Label SerialLabel = new WpfFront.WMSBusinessService.Label
        //                {
        //                    LabelType = new DocumentType { DocTypeID = LabelType.UniqueTrackLabel },
        //                    Status = new Status { StatusID = EntityStatus.Active },
        //                    StartQty = Int32.Parse(View.GetCantidadProducto.Text),
        //                    CurrQty = Int32.Parse(View.GetCantidadProducto.Text),
        //                    Node = new Node { NodeID = NodeType.PreLabeled },
        //                    Bin = BinRecibo,
        //                    Printed = (View.Model.ProductoSerial.ErpTrackOpt == 0) ? false : true,
        //                    Notes = (View.Model.IsCheckedCommentsSerial) ? View.Model.HeaderDocument.Comment : "Doc# " + View.Model.HeaderDocument.DocNumber,
        //                    ReceivingDate = DateTime.Now,
        //                    CreatedBy = App.curUser.UserName,
        //                    CreationDate = DateTime.Now,
        //                    ReceivingDocument = View.Model.HeaderDocument,
        //                    Product = View.Model.ProductoSerial,
        //                    Unit = View.Model.ProductoSerial.BaseUnit,
        //                    CreTerminal = View.Model.RecordCliente.LocationID.ToString()
        //                };


        //                //Evaluo cuales seriales estan visibles para adicionarlos al registro
        //                int cont = 4;
        //                foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposSeriales)
        //                {
        //                    switch (DataDefinitionByBin.DataDefinition.Code)
        //                    {
        //                        case "SERIAL1":
        //                            SerialLabel.LabelCode = DataRow["SERIAL1"].ToString();
        //                            break;
        //                        case "SERIAL2":
        //                            SerialLabel.PrintingLot = DataRow["SERIAL2"].ToString();
        //                            break;
        //                        case "SERIAL3":
        //                            SerialLabel.Manufacturer = DataRow["SERIAL3"].ToString();
        //                            break;
        //                    }
        //                    cont++;
        //                }


        //            #endregion

        //                //Obtengo los datos de cada campo con su nombre
        //                foreach (DataColumn c in View.Model.ListRecords.Columns)
        //                {

        //                    //Obtengo el label y el valor digitado
        //                    ChildrenLabel = c.ColumnName;
        //                    ChildrenValue = DataRow[c.ColumnName].ToString();

        //                    //Evaluo si el campo es obligatorio
        //                    ControlIsRequired = true;
        //                    DataDefinitionControlIsRequired = (View.Model.CamposDetails.Where(f => f.DataDefinition.Code == ChildrenLabel.ToString()).Count() > 0) ? View.Model.CamposDetails.Where(f => f.DataDefinition.Code == ChildrenLabel).First() : null;
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
        //                        //Creo el ShowData con los datos de ChildrenLabel y ChildrenValue
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
        //                    View.Model.UltimosProcesados = View.Model.UltimosProcesados + ChildrenValue.ToString() + " \t ";

        //                }

        //                #region  Labe XML
        //                try
        //                {
        //                    //Guardo el serial en Label y asigno el Id
        //                    SerialLabel = service.SaveLabel(SerialLabel);
        //                    service.ReceiveLabel(View.Model.HeaderDocument, SerialLabel, SerialLabel.Bin, new Node { NodeID = NodeType.Received });
        //                    //Valido si el Label fue guardado correctamente para continuar, en caso contrario muestro mensaje de error y aborto el save
        //                    if (SerialLabel.LabelID == 0)
        //                    {
        //                        Util.ShowError("2. Hubo un error intentando recibir el producto no serializado, por favor volver a intentarlo.");
        //                        return;
        //                    }
        //                }
        //                catch
        //                {
        //                    //Ciclo que espera por un recurso en la BD para guardar un registro.
        //                    Boolean Control = true;
        //                    Util.ShowMessage("El sistema esta ocupado. Oprima enter para continuar con la carga.");
        //                    while (Control)
        //                    {
        //                        try
        //                        {
        //                            SerialLabel = service.SaveLabel(SerialLabel);
        //                            //service.SaveLabel(SerialLabel);
        //                            service.ReceiveLabel(View.Model.HeaderDocument, SerialLabel, SerialLabel.Bin, new Node { NodeID = NodeType.Received });
        //                            Control = false;
        //                        }
        //                        catch { Control = true; }
        //                    }
        //                }
        //                #endregion

        //                #region  Datainformation XML
        //                //Convierto el listado de datos a un Xml
        //                XmlData = Util.XmlSerializerWF(View.Model.ListDetailsDataSave);
        //                //Creo el DataInformation del Serial para el Xml
        //                DataInformationSerial = new DataInformation
        //                {
        //                    Entity = new ClassEntity { ClassEntityID = 20 },
        //                    EntityRowID = Int32.Parse(DataRow[0].ToString()),
        //                    XmlData = XmlData,
        //                    CreationDate = DateTime.Now,
        //                    CreatedBy = App.curUser.UserName
        //                };

        //                //Guardo el Xml en la tabla DataInformation
        //                DataInformationSerial = service.SaveDataInformation(DataInformationSerial);

        //                View.Model.UltimosProcesados = View.Model.UltimosProcesados + "\n";
        //                #endregion
        //                try
        //                {
        //                    //COMPLETAR DOCUMENTO
        //                    service.CreatePurchaseReceipt(View.Model.HeaderDocument);


        //                    #region Nuevo Codigo

        //                    DocumentoConsultado = service.GetDocument(new Document { DocNumber = View.NumeroDelDocumento.Text }).Where(f => f.DocNumber == View.NumeroDelDocumento.Text).ToList().First();
        //                    View.Model.ListaSerialesNoCargados = service.GetLabel(new WMSBusinessService.Label
        //                    {
        //                        Node = new Node { NodeID = NodeType.PreLabeled },
        //                        CreTerminal = View.Model.RecordCliente.LocationID.ToString(),
        //                        ReceivingDocument = DocumentoConsultado
        //                    }).Where(f => f.Node.NodeID == 1 && f.ReceivingDocument == DocumentoConsultado).ToList();
        //                    #endregion

        //                }
        //                catch (Exception Ex)
        //                {
        //                    Util.ShowError("Hubo un error guardando los datos, por favor vuelva a intentarlo: " + Ex.Message);
        //                }

        //                Registrados++;
        //            }
        //            else
        //            {
        //                Permiso = true;
        //            }

        //        }

        //        int No_Cargados = View.Model.ListaSerialesNoCargados == null ? 0 : View.Model.ListaSerialesNoCargados.Count;
        //        Util.ShowMessage("Los equipos cargados fueron guardados satisfactoriamente. \n"
        //              + "Equipos cargados: " + Registrados + " \n"
        //              + "Equipos No cargador por error en la comunicacion con el servidor principal: " + (No_Cargados));
        //        if (Dt_res.Rows.Count > 0)
        //        {
        //            Util.ShowMsgGrilla(Dt_res);

        //        }
        //        //Inicializo los campos de ingreso
        //        LimpiarDatosIngresoSeriales();
        //    }
        //    catch (Exception ex)
        //    { Util.ShowError(ex.Message); }
        //}

        private bool Validar_serial_Lista_actual(string Code, string Serial)
        {
            bool result = false;
            int Con = 0;
            //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
            foreach (DataRow DataRow in View.Model.ListRecords.Rows)
            {
                //Recorro el listado de seriales adicionados para validar que no este ingresando un serial repetido
                Con = DataRow[Code].ToString() == Serial ? Con++ : 0;
                result = Con > 1 ? true : false;
            }
            return result;
        }
        #endregion

        #region Versio Desactualizada OnSaveDetails
        private void OnSaveDetails(object sender, EventArgs e)
        {
            //Validacion si no existen datos para guardar
            if (View.Model.ListRecords.Rows.Count == 0)
                return;

            //Variables Auxiliares
            DataInformation DataInformationSerial;
            DataDefinitionByBin DataDefinitionControlIsRequired;
            Object ChildrenValue, ChildrenLabel;
            ShowData DetailDataSave;
            string XmlData;
            bool ControlIsRequired;

            try
            {
                try
                {
                    foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                    {
                        //Inicializo la lista de los datos a convertir en Xml
                        View.Model.ListDetailsDataSave = new List<ShowData>();
                        //Obtengo los datos de cada campo con su nombre
                        foreach (DataColumn c in View.Model.ListRecords.Columns)
                        {
                            //Obtengo el label y el valor digitado
                            ChildrenLabel = c.ColumnName;
                            ChildrenValue = DataRow[c.ColumnName].ToString();

                            //Evaluo si el campo es obligatorio
                            ControlIsRequired = true;
                            DataDefinitionControlIsRequired = (View.Model.CamposDetails.Where(f => f.DataDefinition.Code == ChildrenLabel).Count() > 0) ? View.Model.CamposDetails.Where(f => f.DataDefinition.Code == ChildrenLabel).First() : null;
                            if (DataDefinitionControlIsRequired != null)
                            {
                                if (DataDefinitionControlIsRequired.DataDefinition.IsRequired == true && String.IsNullOrEmpty(ChildrenValue.ToString()))
                                    ControlIsRequired = false;
                                else
                                    ControlIsRequired = true;
                            }
                            //Evaluo si puedo continuar dependiendo de si el dato era requerido y fue digitado o no
                            if (ControlIsRequired)
                            {
                                //Creo el ShowData con los datos de ChildrenLabel y ChildrenValue
                                DetailDataSave = new ShowData
                                {
                                    DataKey = ChildrenLabel.ToString(),
                                    DataValue = ChildrenValue.ToString()
                                };
                                //Adiciono cada dato a la lista
                                View.Model.ListDetailsDataSave.Add(DetailDataSave);
                            }
                            else
                            {
                                Util.ShowError("El campo " + ChildrenLabel.ToString() + " no puede ser vacio.");
                                return;
                            }

                            //Cargo el dato para la variable de ultimos procesados
                            View.Model.UltimosProcesados = View.Model.UltimosProcesados + ChildrenValue.ToString() + " \t ";

                        }
                        //Convierto el listado de datos a un Xml
                        XmlData = Util.XmlSerializerWF(View.Model.ListDetailsDataSave);
                        //Creo el DataInformation del Serial para el Xml
                        DataInformationSerial = new DataInformation
                        {
                            Entity = new ClassEntity { ClassEntityID = 20 },
                            EntityRowID = Int32.Parse(DataRow[0].ToString()),
                            XmlData = XmlData,
                            CreationDate = DateTime.Now,
                            CreatedBy = App.curUser.UserName,
                            ModDate = DateTime.Now
                        };
                        //Guardo el Xml en la tabla DataInformation
                        DataInformationSerial = service.SaveDataInformation(DataInformationSerial);

                        //Ejecuto el proceso para cargar los datos a las tablas
                        CargarDatosXML(DataInformationSerial);

                        View.Model.UltimosProcesados = View.Model.UltimosProcesados + "\n";
                    }
                }
                catch (Exception ex)
                { Util.ShowError(ex.Message); }


                //COMPLETAR DOCUMENTO
                service.CreatePurchaseReceipt(View.Model.HeaderDocument);

                WindowInfo confirm = new WindowInfo();
                confirm.Txt_Mensaje.Text = View.Model.UltimosProcesados;
                confirm.ShowDialog();

                //Mensaje de confirmacion
                Util.ShowMessage("Datos Guardados exitosamente.");

                #region Nuevo Codigo

                DocumentoConsultado = service.GetDocument(new Document { DocNumber = View.NumeroDelDocumento.Text }).Where(f => f.DocNumber == View.NumeroDelDocumento.Text).ToList().First();
                View.Model.ListaSerialesNoCargados = service.GetLabel(new WMSBusinessService.Label
                {
                    Node = new Node { NodeID = NodeType.PreLabeled },
                    CreTerminal = View.Model.RecordCliente.LocationID.ToString(),
                    ReceivingDocument = DocumentoConsultado
                }).Where(f => f.Node.NodeID == 1 && f.ReceivingDocument == DocumentoConsultado).ToList();

                Util.ShowMessage("Los equipos cargados fueron guardados satisfactoriamente. \n"
                    + "Equipos cargados: " + View.Model.ListRecords.Rows.Count + " \n"
                    + "Equipos No cargador por error en la comunicacion con el servidor principal: " + View.Model.ListaSerialesNoCargados.Count);

                #endregion
                //Inicializo los campos de ingreso
                LimpiarDatosIngresoSeriales();
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error guardando los datos, por favor vuelva a intentarlo: " + Ex.Message); }
        }
        #endregion



        /// <summary>
        /// Mostrar la informacion de la lista del modulo de Entrada de Almacen en una ventana emergente.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMostrarInformacion(object sender, EventArgs e)
        {
            WindowInfo confirm = new WindowInfo();
            confirm.Txt_Mensaje.Text = View.Model.UltimosProcesados;
            confirm.ShowDialog();
        }

        private void LimpiarDatosIngresoSeriales()
        {
            //Limpio los registros de la lista
            View.Model.ListRecords.Rows.Clear();
            //Inicializo el producto seleccionado
            View.GetProductLocation.Product = null;
            View.GetProductLocation.ProductDesc = "";
            View.GetProductLocation.Text = "";
            //Limpio la cantidad de producto ingresado
            View.GetCantidadProducto.Text = "";
            //Inicializo la variable modelo del producto seleccionado
            View.Model.ProductoSerial = null;
            //Habilito los campos de seriales y cantidades para el producto
            View.GetCantidadProducto.IsEnabled = View.GetSerial1.IsEnabled = View.GetSerial2.IsEnabled = View.GetSerial3.IsEnabled = View.GetUpLoadFile.IsEnabled = true;
        }

        private void OnReplicateDetails(object sender, EventArgs e)
        {
            //Recorre la primera linea y con esa setea el valor de las demas lineas.
            for (int i = 1; i < View.Model.ListRecords.Rows.Count; i++)
                for (int z = offset; z < View.Model.ListRecords.Columns.Count; z++)
                    View.Model.ListRecords.Rows[i][z] = View.Model.ListRecords.Rows[0][z];
        }

        private void OnImprimir(object sender, EventArgs e)
        {
            try
            {
                Common.Tools.PrinterControl.Imprimir_certificados(View.Model.ListRecords, View.Model.HeaderDocument.DocNumber);
            }
            catch (Exception ex)
            {
                Util.ShowError(ex.ToString());
            }

        }

        public void CargarDatosXML(DataInformation Registro)
        {
            //Variables Auxiliares
            Document Document;
            WMSBusinessService.Label Label;
            string NombreTabla = "";
            string updQuery = "";
            IList<ShowData> metaData;


            try
            {
                if (Registro.Entity.ClassEntityID == EntityID.Document)
                {
                    //Obtengo los datos del documento para tener el nombre de la bodega
                    Document = service.GetDocument(new Document { DocID = Registro.EntityRowID }).First();
                    //Obtengo el nombre de la bodega que pertenece el registro y creo el nombre de la tabla
                    NombreTabla = "Datos_" + Document.Location.ErpCode;
                }
                else if (Registro.Entity.ClassEntityID == EntityID.Label)
                {
                    //Obtengo los datos del label para tener el nombre de la bodega
                    Label = service.GetLabel(new WMSBusinessService.Label { LabelID = Registro.EntityRowID }).First();
                    try
                    {
                        Location location = service.GetLocation(new Location { LocationID = int.Parse(Label.CreTerminal) }).First();
                        NombreTabla = "Datos_" + location.ErpCode;
                    }
                    catch
                    {
                        //Obtengo el nombre de la bodega que pertenece el registro y creo el nombre de la tabla
                        NombreTabla = "Datos_" + Label.Bin.Location.ErpCode;
                    }
                }
                //Parte incial del update
                updQuery = "UPDATE dbo." + NombreTabla + " SET ModDate = GETDATE(), RowID = " + Registro.EntityRowID.ToString() + " ";
                //Obtiene la lista de campos a actualizar segun la bodega
                //Es decir los codgios de campos que son tus nombres de columna
                metaData = Util.DeserializeMetaDataWF(Registro.XmlData);

                if (metaData.Count == 0)
                {
                    //Registro.ModTerminal = null;
                    //service.UpdateDataInformation(Registro);
                    return;
                }

                //Crear el Update
                //Aqui va contacenando nombre columna y valor para el update
                List<string> sColumns = new List<string>();

                for (int i = 0; i < metaData.Count; i++)
                {
                    if (metaData[i].DataKey.ToLower().Equals("id"))
                        continue;

                    if (metaData[i].DataKey.ToLower().Equals("productid"))
                        continue;

                    if (metaData[i].DataKey.ToLower().Equals("producto"))
                        continue;

                    if (metaData[i].DataKey.ToLower().Equals("cantidad"))
                        continue;


                    if (!sColumns.Contains(metaData[i].DataKey))
                    {
                        updQuery += "," + metaData[i].DataKey + " = '" + metaData[i].DataValue + "' \n";
                        sColumns.Add(metaData[i].DataKey);
                    }

                }

                //parte final del update
                updQuery += " WHERE  InstanceID = " + Registro.RowID.ToString();


                //Intenta crear el ID por si no existe
                //Esto lo hace por si el registro que vpy a actualizar no existe, entonces
                ///primero se crea un registro en blano en la tabla para que el update funcione
                ///el ID  del registro deberia ser el LabelID para elc aso de los labels y el docuemntid en los docuemntos
                try { service.DirectSQLNonQuery("EXEC dbo.spAdminDynamicData 2, " + Registro.RowID.ToString() + ",'" + NombreTabla + "'," + Registro.Entity.ClassEntityID.ToString(), Local); }
                catch { }

                //Ejecutando el query
                service.DirectSQLNonQuery(updQuery, Local);

                //POniendo la entidad como actualizada
                /*Registro.ModTerminal = null;
                Factory.DaoDataInformation().Update(di);
                Console.WriteLine("OK => " + di.EntityRowID + ". " + di.RowID);*/

            }
            catch (Exception ex)
            {
                //report the mistake.
                /*ExceptionMngr.WriteEvent("Routines: ", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Business);
                Console.WriteLine("  ERR => " + di.EntityRowID + ". " + di.RowID + ". " + ex.Message);*/
            }
        }

        #endregion

    }
}