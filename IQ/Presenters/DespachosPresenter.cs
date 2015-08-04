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

namespace WpfFront.Presenters
{

    public interface IDespachosPresenter
    {
       IDespachosView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class DespachosPresenter : IDespachosPresenter
    {
        public IDespachosView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        //Variables Auxiliares
        public Bin BinDespachos;
        public Product ProductDefault;
        public Location LocationDespachos;

        public DespachosPresenter(IUnityContainer container, IDespachosView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<DespachosModel>();

            #region Metodos

            #region Header

            View.CargarHeader += new EventHandler<DataEventArgs<Bin>>(this.OnCargarHeader);
            View.ConfirmBasicData += new EventHandler<EventArgs>(this.OnConfirmBasicData);
            View.CancelBasicData += new EventHandler<EventArgs>(this.OnCancelBasicData);
            View.NewBasicData += new EventHandler<EventArgs>(this.OnNewBasicData);

            #endregion

            #region Serial

            View.AddLine += new EventHandler<EventArgs>(this.OnAddLine);

            #endregion

            #region Details

            View.SaveDetails += new EventHandler<EventArgs>(this.OnSaveDetails);

            #endregion

            #endregion

            #region Datos

            View.Model.HeaderDocument = new Document();
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            View.Model.ListBinDespachosAlmacen = service.GetBin(new Bin { LevelCode = "D", Location = View.Model.RecordCliente });
            //BinDespachos = service.GetBin(new Bin { BinCode = View.Model.RecordCliente.AddressLine2, Location = View.Model.RecordCliente }).First();
            ProductDefault = service.GetProduct(new Product { ProductCode = WmsSetupValues.DEFAULT }).First();
            LocationDespachos = service.GetLocation(new Location { ErpCode = "DESPACHOS" }).First();
            View.Model.ListLabelScann = new List<WpfFront.WMSBusinessService.Label>();
            View.Model.ListDataInformation = new List<DataInformation>();
            //CargarDatosHeader();

            #endregion
        }

        #region Header

        private void CargarDatosHeader()
        {
            IList<DataDefinitionByBin> CamposHeader;
            CamposHeader = service.GetDataDefinitionByBin(new DataDefinitionByBin
            {
                DataDefinition = new DataDefinition { Location = View.Model.RecordCliente, IsHeader = true },
                Bin = BinDespachos
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
                Bin = new Bin { BinCode = View.Model.RecordCliente.AddressLine2, Location = View.Model.RecordCliente }
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
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
            Txt.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Id"));
            Columna.Header = "Id";
            Columna.CellTemplate = new DataTemplate();
            Columna.CellTemplate.VisualTree = Txt;
            View.GetGridViewDetails.Columns.Add(Columna);
            View.Model.ListRecords.Columns.Add("Id");

            //Adiciono el campo de ProductID en el DataTable(tamano 0 para que no se vea en el listado)
            Columna = new GridViewColumn();
            Txt = new FrameworkElementFactory(typeof(TextBlock));
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
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
            Txt.SetValue(TextBlock.MinWidthProperty, (double)100);
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
            }

            //Obtener los campos que voy a mostrar
            View.Model.CamposDetails = service.GetDataDefinitionByBin(new DataDefinitionByBin
            {
                DataDefinition = new DataDefinition { Location = View.Model.RecordCliente, IsHeader = false, IsSerial = false },
                Bin = new Bin { BinCode = View.Model.RecordCliente.AddressLine2, Location = View.Model.RecordCliente }
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
                        Txt.SetValue(ComboBox.MinWidthProperty, (double)150);
                        break;
                    case "System.Windows.Controls.TextBox":
                        Txt.SetValue(TextBox.MinWidthProperty, (double)100);
                        Txt.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding(DataDefinitionByBin.DataDefinition.Code));
                        break;
                    case "Microsoft.Windows.Controls.DatePicker":
                        //Revisar como obtener el dato o realizar el binding con el DataTable
                        Txt.SetValue(Microsoft.Windows.Controls.DatePicker.MinWidthProperty, (double)100);
                        //Txt.SetValue(DatePicker.FirstDayOfWeekProperty, "Monday");
                        Txt.SetBinding(Microsoft.Windows.Controls.DatePicker.SelectedDateProperty, new System.Windows.Data.Binding(DataDefinitionByBin.DataDefinition.Code));
                        break;
                    case "System.Windows.Controls.CheckBox":
                        Txt.SetValue(CheckBox.MinWidthProperty, (double)100);
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

        private void OnCargarHeader(object sender, DataEventArgs<Bin> e)
        {
            View.GetStackPanelHeader.Children.Clear();
            BinDespachos = e.Value;
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
                View.Model.HeaderDocument.Location = LocationDespachos;
                View.Model.HeaderDocument.DocType = new DocumentType { DocTypeID = SDocType.SalesOrder };
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
                    CreatedBy = App.curUser.UserName
                };
                //Guardo el Xml en la tabla DataInformation
                View.Model.DataInformationHeader = service.SaveDataInformation(View.Model.DataInformationHeader);
                //Inhabilito el boton de Guardar, muestro Cancelar y oculto Nuevo
                View.GetButtonConfirmar.IsEnabled = false;
                View.GetButtonNuevo.Visibility = Visibility.Collapsed;
                View.GetButtonCancelar.Visibility = Visibility.Visible;
                //Inhabilito el combobox de bin de ingreso
                View.GetListBinInicio.IsEnabled = false;
                //Muestro el boton para mostrar los datos del header y oculto header
                View.GetTextHideShowHeader.Visibility = Visibility.Visible;
                View.GetBorderHeader.Visibility = Visibility.Collapsed;
                //Cargo los datos para el detalle, los seriales y muestro
                CargarSeriales();
                CargarDatosDetails();
                View.BorderDetails.Visibility = Visibility.Visible;
                //Muestro el mensaje de confirmacion
                Util.ShowMessage("El documento se guardo satisfactoriamente");
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error durante el proceso: " + Ex.Message); }
        }

        private void OnCancelBasicData(object sender, EventArgs e)
        {
            //Util.ShowMessage("Cancelar el documento");
            Util.ShowMessage("Regitros guardados exitosamente. \nCierre la pestaña y genere un nuevo despacho.");
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

        private void OnAddLine(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataRow dr = View.Model.ListRecords.NewRow();
            WpfFront.WMSBusinessService.Label EvalSerialLabel = new WpfFront.WMSBusinessService.Label
            {
                Bin = new Bin { Location = View.Model.RecordCliente }
            };
            DataInformation DataInformationLabel;
            List<ShowData> ShowDataList;
            ShowData ShowData;
            bool EvalSerial1 = true, EvalSerial2 = true, EvalSerial3 = true;

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

            //Analizo si el serial esta en el sistema y esta en el Bin Despachos
            EvalSerialLabel.Bin = BinDespachos;
            try
            {
                //Obtengo el Label
                EvalSerialLabel = service.GetLabel(EvalSerialLabel).First();
                //Obtengo el DataInformation asociado al Label
                DataInformationLabel = service.GetDataInformation(new DataInformation
                {
                    Entity = new ClassEntity { ClassEntityID = 20 },
                    EntityRowID = Int32.Parse(EvalSerialLabel.LabelID.ToString())
                }).First();
            }
            catch
            {
                Util.ShowError("El serial no se encuentra en el estado " + BinDespachos.BinCode);
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

            //Limpio los seriales para digitar nuevos datos
            View.GetSerial1.Text = "";
            View.GetSerial2.Text = "";
            View.GetSerial3.Text = "";
            View.GetSerial1.Focus();
        }

        #endregion

        #region Details

        private void OnSaveDetails(object sender, EventArgs e)
        {
            //Validacion si no existen datos para guardar
            if (View.Model.ListRecords.Rows.Count == 0)
                return;

            //Variables Auxiliares
            DataInformation DataInformationSerial;
            Object ChildrenValue, ChildrenLabel;
            ShowData DetailDataSave;
            IList<ShowData> DetailDataList;
            string XmlData;
            WpfFront.WMSBusinessService.Label LabelAux;
            DataDefinitionByBin DataDefinitionControlIsRequired;
            bool ControlIsRequired;

            try
            {
                //Recorro los registros de la lista para obtener los datos y hacer un join con los anteriores y guardarlos
                foreach (DataRow DataRow in View.Model.ListRecords.Rows)
                {
                    //Obtengo el DataInformation asociado al registro
                    DataInformationSerial = View.Model.ListDataInformation.Where
                        (f => f.Entity.ClassEntityID == EntityID.Label &&
                              f.EntityRowID == Int32.Parse(DataRow[0].ToString())).ToList().First();

                    //Deserializo el Xml para obtener el Showdata
                    DetailDataList = Util.DeserializeMetaDataWF(DataInformationSerial.XmlData);

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
                            //Evaluo si existe el dato que estoy leyendo para quitarlo del listado anterior
                            try
                            {
                                DetailDataSave = DetailDataList.Where(f => f.DataKey == c.ColumnName).First();
                                DetailDataList.Remove(DetailDataSave);
                            }
                            //En caso de no existir no quito nada
                            catch { }

                            //Creo el ShowData con el dato
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
                    }

                    //Concateno las listas de ShowData, la que estaba guardada y la nueva
                    View.Model.ListDetailsDataSave = View.Model.ListDetailsDataSave.Concat(DetailDataList).ToList();

                    //Convierto el listado de datos a un Xml
                    XmlData = Util.XmlSerializerWF(View.Model.ListDetailsDataSave);

                    //Modifico el DataInformation con el nuevo Xml
                    DataInformationSerial.XmlData = XmlData;
                    DataInformationSerial.ModDate = DateTime.Now;
                    DataInformationSerial.ModifiedBy = App.curUser.UserName;

                    //Actualizo el DataInformation
                    service.UpdateDataInformation(DataInformationSerial);
                }

                LabelAux = service.GetLabel(new WpfFront.WMSBusinessService.Label
                {
                    Bin = new Bin { BinCode = "MAIN", Location = LocationDespachos },
                    LabelType = new DocumentType { DocTypeID = LabelType.BinLocation }
                }).First();

                //Recorro el listado de Labels para actualizar sus datos
                foreach (WpfFront.WMSBusinessService.Label Label in View.Model.ListLabelScann)
                {
                    Label.Notes += (View.Model.IsCheckedCommentsSerial) ? (" " + View.Model.HeaderDocument.Comment) : "";
                    try { service.ChangeLabelLocationV2(Label, LabelAux, View.Model.HeaderDocument); }
                    catch { }
                }

                //Reinicio las variables
                View.Model.ListRecords.Rows.Clear();
                View.Model.ListLabelScann = new List<WpfFront.WMSBusinessService.Label>();
                View.Model.ListDataInformation = new List<DataInformation>();

                //Muestro el mensaje de guardado
                Util.ShowMessage("Datos Guardados exitosamente.");
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error guardando los datos, por favor vuelva a intentarlo: " + Ex.Message); }
        }

        #endregion
                
    }
}