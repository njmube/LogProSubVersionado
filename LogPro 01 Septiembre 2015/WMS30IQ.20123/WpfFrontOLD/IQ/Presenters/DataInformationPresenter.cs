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

namespace WpfFront.Presenters
{

    public interface IDataInformationPresenter
    {
       IDataInformationView View { get; set; }
       ToolWindow Window { get; set; }
    }


    public class DataInformationPresenter : IDataInformationPresenter
    {
        public IDataInformationView View { get; set; }
        private readonly IUnityContainer container;
        private readonly WMSServiceClient service;
        public ToolWindow Window { get; set; }

        public DataInformationPresenter(IUnityContainer container, IDataInformationView view)
        {
            View = view;
            this.container = container;
            this.service = new WMSServiceClient();
            View.Model = this.container.Resolve<DataInformationModel>();

            //Metodos Principal
            View.LoadLocation += new EventHandler<DataEventArgs<Location>>(this.OnLoadLocation);
            //Metodos DataDefinition
            View.LoadSearchDataDefinition += new EventHandler<DataEventArgs<string>>(this.OnLoadSearchDataDefinition);
            View.NewDataDefinition += new EventHandler<EventArgs>(this.OnNewDataDefinition);
            View.LoadDataDefinition += new EventHandler<DataEventArgs<DataDefinition>>(this.OnLoadDataDefinition);
            View.SaveDataDefinition += new EventHandler<EventArgs>(this.OnSaveDataDefinition);
            View.DeleteDataDefinition += new EventHandler<EventArgs>(this.OnDeleteDataDefinition);
            View.ShowHideMetaType += new EventHandler<EventArgs>(this.OnShowHideMetaType);
            //Metodos DataDefinitionByBin
            View.LoadDataDefinitionByBin += new EventHandler<DataEventArgs<Bin>>(this.OnLoadDataDefinitionByBin);
            View.AddDataDefinitionByBin += new EventHandler<EventArgs>(this.OnAddDataDefinitionByBin);
            View.DeleteDataDefinitionByBin += new EventHandler<EventArgs>(this.OnDeleteDataDefinitionByBin);
            //Metodos BinRoute
            View.LoadBinToList += new EventHandler<DataEventArgs<Location>>(this.OnLoadBinToList);
            View.SaveBinRoute += new EventHandler<EventArgs>(this.OnSaveBinRoute);
            View.DeleteBinRoute += new EventHandler<EventArgs>(this.OnDeleteBinRoute);

            //Inicializacion de datos
            View.Model.Record = new Location();
            View.Model.LocationList = service.GetLocation(new Location());
            View.Model.RecordBinRoute = new BinRoute
            {
                BinFrom = new Bin(),
                BinTo = new Bin(),
                RequireData = false
            };
            View.Model.WFDataTypeList = service.GetWFDataType(new WFDataType());
            View.Model.MetaTypeList = service.GetMType(new MType());

        }

        #region Location

        private void OnLoadLocation(object sender, DataEventArgs<Location> e)
        {
            //Principal
            View.Model.Record = e.Value;
            View.GetStackMenu.Visibility = Visibility.Visible;
            View.GetBorderDataDefinition.Visibility = Visibility.Collapsed;
            //DataDefinition
            View.Model.DataDefinitionList = service.GetDataDefinition(new DataDefinition { Location = new Location { LocationID = e.Value.LocationID } });
            //DataDefinitionByBin
            View.Model.BinList = service.GetBin(new Bin { Location = new Location { LocationID = e.Value.LocationID } });
            View.Model.DataDefinitionListNotUse = new List<DataDefinition>();
            View.Model.DataDefinitionByBinListUsed = new List<DataDefinitionByBin>();
            //BinRoute
            View.GetBinRouteLocationToList.SelectedItem = View.Model.Record;
            View.Model.BinRouteList = service.GetBinRoute(new BinRoute { LocationFrom = new Location { LocationID = View.Model.Record.LocationID } });
        }

        #endregion

        #region DataDefinition

        private void RefreshList()
        {
            View.Model.DataDefinitionList = service.GetDataDefinition(new DataDefinition { Location = new Location { LocationID = View.Model.Record.LocationID } });
            View.GetBorderDataDefinition.Visibility = Visibility.Collapsed;
        }

        private void OnLoadSearchDataDefinition(object sender, DataEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(e.Value))
            {
                View.Model.DataDefinitionList = service.GetDataDefinition(new DataDefinition { Location = new Location { LocationID = View.Model.Record.LocationID } });
                return;
            }

            if (e.Value.Length < WmsSetupValues.SearchLength)
                return;

            //Busca por Nombre
            View.Model.DataDefinitionList = service.GetDataDefinition(new DataDefinition { DisplayName = e.Value, Location = new Location { LocationID = View.Model.Record.LocationID } });
        }

        private void OnNewDataDefinition(object sender, EventArgs e)
        {
            View.Model.RecordDataDefinition = new DataDefinition();
            View.Model.RecordDataDefinition.IsHeader = View.Model.RecordDataDefinition.IsSerial = false;
            View.Model.RecordDataDefinition.Location = View.Model.Record;
            View.Model.RecordDataDefinition.DataType = new WFDataType();
            View.Model.RecordDataDefinition.MetaType = new MType();          
            View.GetBorderDataDefinition.Visibility = Visibility.Visible;
        }

        private void OnLoadDataDefinition(object sender, DataEventArgs<DataDefinition> e)
        {
            View.Model.RecordDataDefinition = e.Value;
            View.GetBorderDataDefinition.Visibility = Visibility.Visible;
        }

        private void OnSaveDataDefinition(object sender, EventArgs e)
        {
            //Evaluo si el codigo digitado ya existe o no en el sistema
            IList<DataDefinition> ValidacionCodigo = service.GetDataDefinition(new DataDefinition
            {
                Code = View.GetCode.Text.Trim(),
                Location = View.Model.Record
            });
            if (ValidacionCodigo.Count() > 0 && ValidacionCodigo.First().RowID != View.Model.RecordDataDefinition.RowID)
            {
                Util.ShowError("El codigo: " + View.GetCode.Text.Trim() + ", ya existe.");
                return;
            }
            
            View.Model.RecordDataDefinition.Entity = service.GetClassEntity(new ClassEntity { ClassEntityID = EntityID.Document }).First();
            
            
            if (View.GetStackPanelMetaType.Visibility == Visibility.Collapsed)
            {
                View.Model.RecordDataDefinition.MetaType = null;
                View.GetMetaType.SelectedIndex = -1;
            }
            else
            {
                View.Model.RecordDataDefinition.MetaType = (MType)View.GetMetaType.SelectedItem;
            }



            try
            {
                View.Model.RecordDataDefinition.Code = View.Model.RecordDataDefinition.Code.Trim();

                if (View.Model.RecordDataDefinition.RowID == Guid.Empty)
                {
                    View.Model.RecordDataDefinition.CreatedBy = App.curUser.UserName;
                    View.Model.RecordDataDefinition.CreationDate = DateTime.Now;
                    View.Model.RecordDataDefinition.RowID = Guid.NewGuid();
                    View.Model.RecordDataDefinition = service.SaveDataDefinition(View.Model.RecordDataDefinition);
                    Util.ShowMessage("Definicion guardada satisfactoriamente");
                }
                else
                {
                    View.Model.RecordDataDefinition.ModifiedBy = App.curUser.UserName;
                    View.Model.RecordDataDefinition.ModDate = DateTime.Now;
                    
                    service.UpdateDataDefinition(View.Model.RecordDataDefinition);
                    Util.ShowMessage("Definicion actualizada satisfactoriamente");
                }
                RefreshList();
            }
            catch { Util.ShowError("Hubo un error durante el proceso, por favor vuelva a intentarlo"); }
        }

        private void OnDeleteDataDefinition(object sender, EventArgs e)
        {
            try
            {
                service.DeleteDataDefinition(View.Model.RecordDataDefinition);
                Util.ShowMessage("Definicion eliminada satisfactoriamente");
                RefreshList();
            }
            catch { Util.ShowError("Hubo un error durante el proceso, por favor vuelva a intentarlo"); }
        }

        private void OnShowHideMetaType(object sender, EventArgs e)
        {
            if (View.GetDataType.SelectedItem != null && ((WFDataType)View.GetDataType.SelectedItem).DataTypeID == 10)
            {
                View.GetStackPanelMetaType.Visibility = Visibility.Visible;
            }
            else
            {
                View.GetStackPanelMetaType.Visibility = Visibility.Collapsed;
                View.GetMetaType.SelectedIndex = -1;
            }
        }

        #endregion

        #region DataDefinitionByBin

        private void LoadListsDataDefinitionByBin(Bin Bin)
        {
            View.Model.DataDefinitionByBinListUsed = service.GetDataDefinitionByBin(new DataDefinitionByBin
            {
                Bin = new Bin { BinID = Bin.BinID },
                DataDefinition = new DataDefinition { Location = View.Model.Record }
            });
            View.Model.DataDefinitionListNotUse = service.GetDataDefinition(new DataDefinition
            {
                Location = View.Model.Record
            });
            DataDefinition DataDefinitionControl;
            foreach (DataDefinitionByBin DataDefinitionByBin in View.Model.DataDefinitionByBinListUsed)
            {
                DataDefinitionControl = View.Model.DataDefinitionListNotUse.Where(f => f.RowID == DataDefinitionByBin.DataDefinition.RowID).First();
                if (DataDefinitionControl != null)
                    View.Model.DataDefinitionListNotUse.Remove(DataDefinitionControl);
            }
            View.GetDataDefinitionListNotUse.Items.Refresh();
            View.GetDataDefinitionByBinListUsed.Items.Refresh();
        }

        private void OnLoadDataDefinitionByBin(object sender, DataEventArgs<Bin> e)
        {
            View.Model.RecordBin = e.Value;
            LoadListsDataDefinitionByBin(e.Value);
        }

        private void OnAddDataDefinitionByBin(object sender, EventArgs e)
        {
            DataDefinitionByBin DataDefinitionByBinControl;
            foreach (DataDefinition DataDefinition in View.GetDataDefinitionListNotUse.SelectedItems)
            {
                try
                {
                    DataDefinitionByBinControl = new DataDefinitionByBin
                    {
                        DataDefinition = DataDefinition,
                        Bin = View.Model.RecordBin,
                        CreatedBy = App.curUser.UserName,
                        CreationDate = DateTime.Now
                    };
                    DataDefinitionByBinControl = service.SaveDataDefinitionByBin(DataDefinitionByBinControl);
                }
                catch { }
            }
            LoadListsDataDefinitionByBin(View.Model.RecordBin);
        }

        private void OnDeleteDataDefinitionByBin(object sender, EventArgs e)
        {
            foreach (DataDefinitionByBin DataDefinitionByBin in View.GetDataDefinitionByBinListUsed.SelectedItems)
            {
                try
                {
                    service.DeleteDataDefinitionByBin(DataDefinitionByBin);
                }
                catch { }
            }
            LoadListsDataDefinitionByBin(View.Model.RecordBin);
        }

        #endregion

        #region BinRoute

        private void RefreshBinRouteList()
        {
            View.Model.BinRouteList = service.GetBinRoute(new BinRoute { LocationFrom = new Location { LocationID = View.Model.Record.LocationID } });
            View.GetBinRouteList.Items.Refresh();
        }

        private void OnLoadBinToList(object sender, DataEventArgs<Location> e)
        {
            View.Model.BinToList = service.GetBin(new Bin { Location = new Location { LocationID = e.Value.LocationID } });
        }

        private void OnSaveBinRoute(object sender, EventArgs e)
        {
            try
            {
                View.Model.RecordBinRoute.LocationFrom = View.Model.Record;
                View.Model.RecordBinRoute.LocationTo = (Location)View.GetBinRouteLocationToList.SelectedItem;
                /*View.Model.RecordBinRoute.BinFrom = (Bin)View.GetBinFromList.SelectedItem;
                View.Model.RecordBinRoute.BinTo = (Bin)View.GetBinToList.SelectedItem;*/
                View.Model.RecordBinRoute.CreatedBy = App.curUser.UserName;
                View.Model.RecordBinRoute.CreationDate = DateTime.Now;
                View.Model.RecordBinRoute = service.SaveBinRoute(View.Model.RecordBinRoute);
                //Recargo la lista
                RefreshBinRouteList();
                //Limpio los campos para crear nuevos
                View.Model.RecordBinRoute = new BinRoute
                {
                    BinFrom = new Bin(),
                    BinTo = new Bin(),
                    RequireData = false
                };
                View.GetBinFromList.SelectedIndex = -1;
                View.GetBinToList.SelectedIndex = -1;
            }
            catch { }
        }

        private void OnDeleteBinRoute(object sender, EventArgs e)
        {
            foreach (BinRoute BinRoute in View.GetBinRouteList.SelectedItems)
            {
                try
                {
                    service.DeleteBinRoute(BinRoute);
                }
                catch { }
            }
            RefreshBinRouteList();
        }

        #endregion
    }
}