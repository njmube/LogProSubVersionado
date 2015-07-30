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

    public interface IAdminInformationConsultaPresenter
    {
       IAdminInformationConsultaView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class AdminInformationConsultaPresenter : IAdminInformationConsultaPresenter
    {
        public IAdminInformationConsultaView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        public AdminInformationConsultaPresenter(IUnityContainer container, IAdminInformationConsultaView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<AdminInformationConsultaModel>();

            #region Metodos

            #region Busqueda

            View.LoadData += new EventHandler<DataEventArgs<ClassEntity>>(this.OnLoadData);
            View.ShowFields += new EventHandler<DataEventArgs<ClassEntity>>(this.OnShowFields);

            #endregion

            #region Datos Estaticos

            View.AsignarProducto += new EventHandler<DataEventArgs<Product>>(this.OnAsignarProducto);
            View.CargarDatos += new EventHandler<DataEventArgs<Location>>(this.OnCargarDatos);

            #endregion

            #region Datos Generales

            #endregion

            #region Eventos Botones

            View.UpdateData += new EventHandler<EventArgs>(this.OnUpdateData);
            View.DeleteData += new EventHandler<EventArgs>(this.OnDeleteData);

            #endregion

            #endregion

            #region Datos

            //Obtengo el cliente
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            //Obtengo las opciones de Documento y Label
            View.Model.SearchTypeList = service.GetClassEntity(new ClassEntity())
                .Where(f => f.ClassEntityID == EntityID.Document || f.ClassEntityID == EntityID.Label).ToList();
            //Obtengo las ubicaciones del cliente
            View.Model.BinList = service.GetBin(new Bin { Location = View.Model.RecordCliente });
            //Consulto los productos definidos para el cliente
            //View.Model.ProductsLocationList = service.GetProduct(new Product { Reference = View.Model.RecordCliente.LocationID.ToString() });
            //View.Model.ProductsLocationList.Insert(0, service.GetProduct(new Product { ProductCode = WmsSetupValues.DEFAULT }).First());
            //Obtengo los estados de los equipos
            View.Model.StatusList = App.EntityStatusList;

            #endregion
        }

        #region Busqueda

        private void OnLoadData(object sender, DataEventArgs<ClassEntity> e)
        {
            //Variables Auxiliares
            bool NumeroEncontrado = false;
            Location LocationCreTerminal;

            //Evaluo si selecciono un tipo de busqueda(Document, Label)
            if (View.GetTipoBusqueda.SelectedIndex == -1)
            {
                Util.ShowError("Por favor seleccionar el tipo de busqueda que desea realizar");
                return;
            }

            //Evaluo si el serial1 esta habilitado y fue digitado
            if (View.GetStackSearchSerial1.Visibility == Visibility.Visible && View.GetSearchSerial1.Text == "")
            {
                Util.ShowError("Por favor digitar un valor en el campo del serial que desea buscar");
                return;
            }

            //Evaluo si el serial2 esta habilitado y fue digitado
            /*if (View.GetStackSearchSerial2.Visibility == Visibility.Visible && View.GetSearchSerial2.Text == "")
            {
                Util.ShowError("Por favor digitar un valor en el campo del serial que desea buscar");
                return;
            }*/

            //Evaluo si el serial3 esta habilitado y fue digitado
            /*if (View.GetStackSearchSerial3.Visibility == Visibility.Visible && View.GetSearchSerial3.Text == "")
            {
                Util.ShowError("Por favor digitar un valor en el campo del serial que desea buscar");
                return;
            }*/

            //Limpio los datos dinamicos
            View.GetStackPanelDinamicos.Children.Clear();

            //Oculto los campos con los datos y los botones
            View.GetStackDatosEstaticos.Visibility = Visibility.Collapsed;
            View.GetStackDatos.Visibility = Visibility.Collapsed;
            View.GetStackButtons.Visibility = Visibility.Collapsed;
            View.GetButtonAnular.Visibility = Visibility.Collapsed;
            View.GetListClientes.Visibility = Visibility.Collapsed;
            View.GetClientes.Visibility = Visibility.Collapsed;

            //Realizo la busqueda del documento
            if (e.Value.ClassEntityID == EntityID.Document)
            {
                try
                {
                    View.Model.DocumentSearch = service.GetDocument(new Document
                    {
                        DocNumber = View.GetSearchSerial1.Text,
                        Location = View.Model.RecordCliente
                    }).First();
                    View.Model.DataInformationSearch = service.GetDataInformation(new DataInformation { EntityRowID = View.Model.DocumentSearch.DocID }).First();
                    NumeroEncontrado = true;
                }
                catch
                {
                    Util.ShowError("El documento requerido no existe");
                    return;
                }
                //Muestro el cliente
                View.GetClientes.Visibility = Visibility.Visible;
            }

            //Realizo la busqueda del equipo por su serial
            if (e.Value.ClassEntityID == EntityID.Label)
            {
                try
                {
                    View.Model.LabelSearch = service.GetLabel(new WpfFront.WMSBusinessService.Label
                    {
                        LabelCode = (View.GetStackSearchSerial1.Visibility == Visibility.Visible) ? View.GetSearchSerial1.Text : null,
                        PrintingLot = (View.GetStackSearchSerial2.Visibility == Visibility.Visible && View.GetSearchSerial2.Text != "") ? View.GetSearchSerial2.Text : null,
                        Manufacturer = (View.GetStackSearchSerial3.Visibility == Visibility.Visible && View.GetSearchSerial3.Text != "") ? View.GetSearchSerial3.Text : null,
                        Bin = new Bin { Location = View.Model.RecordCliente }
                    }).OrderBy(f => f.Status.StatusID).First();
                    NumeroEncontrado = true;
                }
                catch
                {
                    Util.ShowError("El equipo requerido no existe");
                    return;
                }
                try
                {
                    View.Model.DataInformationSearch = service.GetDataInformation(new DataInformation { EntityRowID = Int32.Parse(View.Model.LabelSearch.LabelID.ToString()) }).First();
                }
                catch { View.Model.DataInformationSearch = new DataInformation { Entity = new ClassEntity { ClassEntityID = EntityID.Label } }; }

                //Obtengo los Location en los cuales el equipo ha estado
                LocationCreTerminal = null;
                try
                {
                    if (!String.IsNullOrEmpty(View.Model.LabelSearch.CreTerminal))
                        LocationCreTerminal = service.GetLocation(new Location { LocationID = Int32.Parse(View.Model.LabelSearch.CreTerminal) }).First();
                }
                catch { }

                View.Model.LocationList = service.GetNodeTrace(new NodeTrace { Label = View.Model.LabelSearch })
                    //.Where(f => f.Bin.Location.Name != "DESPACHOS")
                    .Select(f => f.Bin.Location).Distinct().ToList();

                if (LocationCreTerminal != null && !View.Model.LocationList.Any(f => f.LocationID == LocationCreTerminal.LocationID))
                    View.Model.LocationList.Add(LocationCreTerminal);

                //Muestro el listado de clientes y asigno el cliente actual
                View.GetListClientes.Visibility = Visibility.Visible;
                View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();

                //Oculto el boton de eliminar label
                View.GetButtonAnular.Visibility = Visibility.Collapsed;
                View.GetButtonUpdate.Visibility = Visibility.Collapsed;
                //Coloco ineditable El Cbx Cliente
                View.GetListClientes.IsEnabled =false;
            }

            //Cargo los campos y muestro en el caso de que la busqueda haya arrojado un resultado
            if (NumeroEncontrado)
            {
                //Cargo los datos estaticos
                CargarDatosEstaticos(e.Value.ClassEntityID);
                //Cargo los datos dinamicos si el equipo los tiene
                CargarDatosDinamicos(View.Model.RecordCliente);
                //Muestro los campos con los datos y los botones
                View.GetStackDatosEstaticos.Visibility = Visibility.Visible;
                View.GetStackDatos.Visibility = Visibility.Visible;
                View.GetStackButtons.Visibility = Visibility.Visible;
            }
        }

        private void OnShowFields(object sender, DataEventArgs<ClassEntity> e)
        {
            //Oculto los campos y limpio los datos
            View.GetStackSearchSerial1.Visibility = Visibility.Collapsed;
            View.GetSearchSerial1.Text = "";
            View.GetStackSearchSerial2.Visibility = Visibility.Collapsed;
            View.GetSearchSerial2.Text = "";
            View.GetStackSearchSerial3.Visibility = Visibility.Collapsed;
            View.GetSearchSerial3.Text = "";

            //Evaluo si fue seleccionado el tipo documento
            if (e.Value.ClassEntityID == EntityID.Document)
            {
                //Coloco visible el primer campo para digitar
                View.GetStackSearchSerial1.Visibility = Visibility.Visible;

                //Coloco el titulo del campo
                View.GetTextSearchSerial1.Text = "Documento";

                //Coloco el cursor en el primer campo
                View.GetSearchSerial1.Focus();
            }

            //Evaluo si fue seleccionado el tipo label
            if (e.Value.ClassEntityID == EntityID.Label)
            {
                //Cargo los campos definidos para el cliente
                CargarSeriales();
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
                        View.GetStackSearchSerial1.Visibility = Visibility.Visible;
                        View.GetTextSearchSerial1.Text = DataDefinitionByBin.DataDefinition.DisplayName;
                        break;
                    case "SERIAL2":
                        View.GetStackSearchSerial2.Visibility = Visibility.Visible;
                        View.GetTextSearchSerial2.Text = DataDefinitionByBin.DataDefinition.DisplayName;
                        break;
                    case "SERIAL3":
                        View.GetStackSearchSerial3.Visibility = Visibility.Visible;
                        View.GetTextSearchSerial3.Text = DataDefinitionByBin.DataDefinition.DisplayName;
                        break;
                }
            }

            //Coloco el cursor en el primer campo serial
            View.GetSearchSerial1.Focus();
        }

        #endregion

        #region Datos Estaticos

        private void OnAsignarProducto(object sender, DataEventArgs<Product> e)
        {
            View.Model.LabelSearch.Product = e.Value;
        }

        private void CargarDatosEstaticos(int ClassEntity)
        {
            //Evaluo si los datos a cargar pertenecen a un documento
            if (ClassEntity == EntityID.Document)
            {
                //Muestro los datos para el documento
                View.GetStackEstaticosDocumento.Visibility = Visibility.Visible;
            }

            //Evaluo si los datos a cargar pertenecen a un serial
            if (ClassEntity == EntityID.Label)
            {
                //Consulto los seriales activos para el cliente actual
                View.Model.SerialesActivosList = service.GetDataDefinition(new DataDefinition
                {
                    Location = View.Model.RecordCliente,
                    IsHeader = false,
                    IsSerial = true
                }).OrderBy(f => f.Code).ToList();

                //Recorro los seriales para mostrar los que han sido definidos
                foreach (DataDefinition DataDefinition in View.Model.SerialesActivosList)
                {
                    switch (DataDefinition.Code)
                    {
                        case "SERIAL1":
                            View.GetStackSerial1.Visibility = Visibility.Visible;
                            View.GetTextSerial1.Text = DataDefinition.DisplayName;
                            break;
                        case "SERIAL2":
                            View.GetStackSerial2.Visibility = Visibility.Visible;
                            View.GetTextSerial2.Text = DataDefinition.DisplayName;
                            break;
                        case "SERIAL3":
                            View.GetStackSerial3.Visibility = Visibility.Visible;
                            View.GetTextSerial3.Text = DataDefinition.DisplayName;
                            break;
                    }
                }

                //Asigno el producto seleccionado
                View.GetProductLabel.Product = View.Model.LabelSearch.Product;
                View.GetProductLabel.Text = View.Model.LabelSearch.Product.Name;

                //Muestro los datos para el label
                View.GetStackEstaticosLabel.Visibility = Visibility.Visible;
                // Dejo Los Campos No  Editables
                View.GetStackEstaticosLabel.IsEnabled = false;
            }
        }

        #endregion

        #region Datos Dinamicos

        private void OnCargarDatos(object sender, DataEventArgs<Location> e)
        {
            //Limpio los datos actuales
            View.GetStackPanelDinamicos.Children.Clear();

            //Cargo los datos estaticos
            CargarDatosEstaticos(EntityID.Label);

            //Cargo los datos dinamicos
            CargarDatosDinamicos(e.Value);
        }

        private void CargarDatosDinamicos(Location Cliente)
        {
            //Variables Auxiliares
            IList<ShowData> DetailDataList = null;
            ShowData DetailData = null;

            //Limpio los datos dinamicos
            View.GetStackPanelDinamicos.Children.Clear();

            //Deserializo el Xml para obtener el Showdata
            DetailDataList = Util.DeserializeMetaDataWF(View.Model.DataInformationSearch.XmlData);

            //Evaluo si el tipo de dato a editar es documento
            if (View.Model.DataInformationSearch.Entity.ClassEntityID == EntityID.Document)
            {
                View.Model.CamposDinamicosEditList = service.GetDataDefinitionByBin(new DataDefinitionByBin
                {
                    DataDefinition = new DataDefinition
                    {
                        //Location = service.GetDocument(new Document { DocID = View.Model.DataInformationSearch.EntityRowID }).First().Location 
                        Location = Cliente
                    }
                }).OrderBy(f => f.DataDefinition.NumOrder).ToList();
            }

            //Evaluo si el tipo de dato a editar es un label
            if (View.Model.DataInformationSearch.Entity.ClassEntityID == EntityID.Label)
            {
                View.Model.CamposDinamicosEditList = service.GetDataDefinitionByBin(new DataDefinitionByBin
                {
                    DataDefinition = new DataDefinition
                    {
                        Location = Cliente,
                        IsHeader = false,
                        IsSerial = false
                    }
                });

                //Recorro la lista para filtrar los campos que estan repetidos
                IList<DataDefinitionByBin> controList = new List<DataDefinitionByBin>();
                foreach (DataDefinitionByBin dfx in View.Model.CamposDinamicosEditList)
                {
                    if (controList.Any(f => f.DataDefinition.Code == dfx.DataDefinition.Code))
                        continue;
                    else
                        controList.Add(dfx);
                }

                View.Model.CamposDinamicosEditList = controList.OrderBy(f => f.DataDefinition.NumOrder).ToList();
            }

            string ucType; //Tipo del User Control
            Assembly assembly;
            UserControl someObject = null;
            IList<MMaster> listCombo = null;
            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.CamposDinamicosEditList)
            {
                try
                {
                    DetailData = DetailDataList.Where(f => f.DataKey == DataDefinitionByBin.DataDefinition.Code).First();
                }
                catch { continue; }

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
                    if (DetailData.DataValue != "")
                        try
                        {
                            someObject.GetType().GetProperty("UcValue").SetValue(someObject, DateTime.Parse(DetailData.DataValue), null);
                        }
                        catch { }
                }
                else
                {
                    if (ucType.Contains("WFCheckBox"))
                    {
                        if (DetailData.DataValue != "")
                            someObject.GetType().GetProperty("UcValue").SetValue(someObject, Boolean.Parse(DetailData.DataValue), null);
                        else
                            someObject.GetType().GetProperty("UcValue").SetValue(someObject, false, null);
                    }
                    else
                        someObject.GetType().GetProperty("UcValue").SetValue(someObject, DetailData.DataValue, null);
                    
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
                //someObject.IsEnabled = false;
                View.GetStackPanelDinamicos.Children.Add(someObject);
                View.GetStackPanelDinamicos.IsEnabled = false;
            }
        }

        #endregion

        #region Eventos Botones

        private void OnUpdateData(object sender, EventArgs e)
        {
            Object ChildrenValue, ChildrenLabel;
            ShowData HeaderDataSave;
            string XmlData;
            string CodigoCampo;
            DataDefinition IsRequiredField;
            bool ControlIsRequired;
            IList<ShowData> DetailDataList;
            //Deserializo el Xml para obtener el Showdata
            DetailDataList = Util.DeserializeMetaDataWF(View.Model.DataInformationSearch.XmlData);
            //Inicializo la lista de los datos a convertir en Xml
            View.Model.ListDataSave = new List<ShowData>();
            //Obtengo los datos de los campos
            foreach (UIElement UIElement in View.GetStackPanelDinamicos.Children)
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
                    /*if (IsRequiredField.IsRequired == true && String.IsNullOrEmpty(ChildrenValue == null ? "" : ChildrenValue.ToString()))
                        ControlIsRequired = false;
                    else*/
                    ControlIsRequired = true;
                    //Evaluo si puedo continuar dependiendo de si el dato era requerido y fue digitado o no
                    if (ControlIsRequired)
                    {
                        //Evaluo si existe el dato que estoy leyendo para quitarlo del listado anterior
                        try
                        {
                            HeaderDataSave = DetailDataList.Where(f => f.DataKey == CodigoCampo.ToString()).First();
                            DetailDataList.Remove(HeaderDataSave);
                        }
                        //En caso de no existir no quito nada
                        catch { }
                        //Creo el ShowData con los datos de ChildrenLabel y ChildrenValue
                        HeaderDataSave = new ShowData
                        {
                            DataKey = CodigoCampo.ToString(),
                            DataValue = ChildrenValue == null ? "" : ChildrenValue.ToString()
                        };
                        //Adiciono el ShowData al listado para crear el Xml
                        View.Model.ListDataSave.Add(HeaderDataSave);
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
                //Concateno las listas de ShowData, la que estaba guardada y la nueva
                View.Model.ListDataSave = View.Model.ListDataSave.Concat(DetailDataList).ToList();
                //Convierto el listado de datos a un Xml
                XmlData = Util.XmlSerializerWF(View.Model.ListDataSave);
                //Evaluo el tipo de dato a editar(Document o Label)
                if (View.Model.DataInformationSearch.Entity.ClassEntityID == EntityID.Document)
                    //Guardo los datos para el documento
                    UpdateDocumentData(XmlData);
                if (View.Model.DataInformationSearch.Entity.ClassEntityID == EntityID.Label)
                    //Guardo los datos para el label
                    UpdateLabelData(XmlData);
                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Los datos fueron editados correctamente");
            }
            catch (Exception Ex) { Util.ShowError("Hubo un error durante el proceso: " + Ex.Message); }
        }

        private void UpdateDocumentData(string XmlData)
        {
            //Asigno los datos del documento para actualizar
            View.Model.DocumentSearch.ModifiedBy = App.curUser.UserName;
            View.Model.DocumentSearch.ModDate = DateTime.Now;
            //Actualizo el Documento
            service.UpdateDocument(View.Model.DocumentSearch);
            //Actualizo los datos Xml del DataInformation
            View.Model.DataInformationSearch.XmlData = XmlData;
            View.Model.DataInformationSearch.ModDate = DateTime.Now;
            View.Model.DataInformationSearch.ModifiedBy = App.curUser.UserName;
            //Actualizo el Xml en la tabla DataInformation
            service.UpdateDataInformation(View.Model.DataInformationSearch);
        }

        private void UpdateLabelData(string XmlData)
        {
            //Asigno los datos del label para actualizar
            View.Model.LabelSearch.ModifiedBy = App.curUser.UserName;
            View.Model.LabelSearch.ModDate = DateTime.Now;
            //Actualizo el label
            service.UpdateLabel(View.Model.LabelSearch);
            //Actualizo los datos Xml del DataInformation
            View.Model.DataInformationSearch.XmlData = XmlData;
            View.Model.DataInformationSearch.ModDate = DateTime.Now;
            View.Model.DataInformationSearch.ModifiedBy = App.curUser.UserName;
            //Actualizo el Xml en la tabla DataInformation
            service.UpdateDataInformation(View.Model.DataInformationSearch);
        }

        private void OnDeleteData(object sender, EventArgs e)
        {
            //Actualizo el labelcode
            View.Model.LabelSearch.LabelCode = "VOID_" + DateTime.Now + "_" + View.Model.LabelSearch.LabelCode;

            //Coloco el label inactivo
            View.Model.LabelSearch.Status = new Status { StatusID = 1002 };
            View.Model.LabelSearch.Node = new Node { NodeID = NodeType.Voided };

            try
            {
                //Actualizo el label con los nuevos datos para inactivarlo del sistema y poder utilizarlo nuevamente
                service.UpdateLabel(View.Model.LabelSearch);

                //Elimino el label del datainformation
                service.DeleteDataInformation(new DataInformation { EntityRowID = Int32.Parse(View.Model.LabelSearch.LabelID.ToString()) });

                //Muestro la ventana de confirmacion
                Util.ShowMessage("Equipo eliminado satisfactoriamente");
            }
            catch { Util.ShowError("Hubo un error tratando de eliminar el equipo del sistema. Por favor volver a intentarlo"); }
        }

        #endregion

    }
}