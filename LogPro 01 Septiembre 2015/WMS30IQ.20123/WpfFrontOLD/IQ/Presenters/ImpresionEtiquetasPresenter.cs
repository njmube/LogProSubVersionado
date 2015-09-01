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

    public interface IImpresionEtiquetasPresenter
    {
        IImpresionEtiquetasView View { get; set; }
        ToolWindow Window { get; set; }
    }
    public class ImpresionEtiquetasPresenter : IImpresionEtiquetasPresenter
    {
        public IImpresionEtiquetasView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        public ImpresionEtiquetasPresenter(IUnityContainer container, IImpresionEtiquetasView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<ImpresionEtiquetasModel>();

            #region Metodos

            #region Busqueda

            View.LoadData += new EventHandler<EventArgs>(this.OnLoadData);

            #endregion

            #region Eventos Botones

            View.Delete += new EventHandler<EventArgs>(this.OnDelete);
            View.Save += new EventHandler<EventArgs>(this.OnSave);

            #endregion

            #endregion

            #region Datos

            //Obtengo el cliente
            View.Model.RecordCliente = service.GetLocation(new Location { LocationID = App.curLocation.LocationID }).First();
            //Obtengo el listado de etiquetas
            View.Model.ListadoEtiquetas = service.GetMMaster(new MMaster { MetaType = new MType { MetaTypeID = 130 }, Code2 = View.Model.RecordCliente.ErpCode });
            //Inicio las variables
            View.Model.ListaEquipos = new DataTable { Columns = { "Label", "Serial", "Etiqueta" } };
            //View.Model.ListaEquipos.PrimaryKey = new DataColumn[] { View.Model.ListaEquipos.Columns["Label"] };
            View.Model.ListaEquiposAuxiliar = new DataTable { Columns = { "Label", "Serial", "Etiqueta" } };
            View.Model.ListaEquiposAuxiliar.PrimaryKey = new DataColumn[] { View.Model.ListaEquiposAuxiliar.Columns["Label"] };

            #endregion
        }

        #region Busqueda

        private void OnLoadData(object sender, EventArgs e)
        {
            //Variables Auxiliares
            WpfFront.WMSBusinessService.Label Equipo;
            DataRow Registro = View.Model.ListaEquipos.NewRow();
            DataRow RegistroAuxiliar = View.Model.ListaEquiposAuxiliar.NewRow();

            //Evaluo si el serial existe en el sistema y obtengo sus datos
            try
            {
                Equipo = service.GetLabel(new WpfFront.WMSBusinessService.Label { LabelCode = View.GetNumeroBusqueda.Text }).First();
            }
            catch 
            {
                Util.ShowError("El equipo no existe en el sistema");
                return;
            }

            //Adiciono los datos al registro
            try
            {
                Registro[0] = Equipo.LabelID;
                Registro[1] = Equipo.LabelCode;
                Registro[2] = ((MMaster)View.GetTipoBusqueda.SelectedItem).Code;
                RegistroAuxiliar[0] = Equipo.LabelID;
                RegistroAuxiliar[1] = Equipo.LabelCode;
                RegistroAuxiliar[2] = ((MMaster)View.GetTipoBusqueda.SelectedItem).Code;

                //Adiciono el registro al listado
                View.Model.ListaEquiposAuxiliar.Rows.Add(RegistroAuxiliar);
                View.Model.ListaEquipos.Rows.Add(Registro);
            }
            catch 
            {
                Util.ShowError("El serial ya existe en el listado");
                return;
            }

            //Actualizo el listado
            View.GetSerialesEscaneados.Items.Refresh();

            //Limpio los datos
            View.GetNumeroBusqueda.Text = "";
        }

        #endregion

        #region Eventos Botones

        private void OnDelete(object sender, EventArgs e)
        {
            //Variables Auxiliares
            DataRow Registro = View.Model.ListaEquipos.NewRow();

            //Evaluo si hay seleccionado algun registro para eliminar del listado
            if (View.GetSerialesEscaneados.SelectedItems.Count == 0)
                return;

            //Recorro los registros seleccionados para eliminarlos
            foreach (DataRowView dr in View.GetSerialesEscaneados.SelectedItems)
            {
                View.Model.ListaEquiposAuxiliar.Rows.Find(dr.Row[0]).Delete();
            }

            //Asigno el nuevo listado a la variable
            View.Model.ListaEquipos.Clear();
            foreach (DataRow dr in View.Model.ListaEquiposAuxiliar.Rows)
            {
                Registro = View.Model.ListaEquipos.NewRow();
                Registro[0] = dr[0];
                Registro[1] = dr[1];
                Registro[2] = dr[2];
                View.Model.ListaEquipos.Rows.Add(Registro);
            }

            //Actualizo el listado
            View.GetSerialesEscaneados.Items.Refresh();
        }

        private void OnSave(object sender, EventArgs e)
        {
            //Variables auxiliares
            string Query = "";
            string NombreTabla = "";
            Connection Conexion = service.GetConnection(new Connection { Name = "LOCAL" }).First();

            //Evaluo si hay registros en el listado para guardar
            if (View.Model.ListaEquipos.Rows.Count == 0)
                return;

            try
            {
                //Recorro el listado de equipos scaneados para realizar el save
                foreach (DataRow dr in View.Model.ListaEquipos.Rows)
                {
                    //Construyo el nombre de la tabla
                    NombreTabla = "PRINT_" + View.Model.RecordCliente.ErpCode + "_" + dr[2];

                    //Creo la cadena para realizar la validacion si existe la tabla donde se guardara el registro
                    Query = "EXEC dbo.spAdminDynamicData 3, NULL, NULL, NULL, '" + NombreTabla + "'";

                    //Ejecuto el servicio para validar la tabla
                    service.DirectSQLNonQuery(Query, Conexion);

                    //Creo la cadena para realizar el save del registro en la DB
                    Query = "INSERT INTO " + NombreTabla + "(LabelID,LabelCode1,CreatedBy,CreationDate) VALUES(" + dr[0] + ",'" + dr[1] + "','" + App.curUser.UserName + "',GETDATE())";

                    //Guardo el registro
                    service.DirectSQLNonQuery(Query, Conexion);
                }

                //Limpio el listado de seriales
                View.Model.ListaEquipos.Rows.Clear();
                View.Model.ListaEquiposAuxiliar.Rows.Clear();

                //Actualizo el listado
                View.GetSerialesEscaneados.Items.Refresh();

                //Muestro el mensaje de confirmacion
                Util.ShowMessage("Los seriales fueron guardados satisfactoriamente");
            }
            catch 
            {
                //Muestro el mensaje de error
                Util.ShowError("Hubo un error durante el proceso, por favor revisar");
                return;
            }
        }

        #endregion

    }
}