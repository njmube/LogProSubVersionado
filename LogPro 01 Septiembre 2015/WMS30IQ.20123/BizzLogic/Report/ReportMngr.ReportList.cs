using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Integrator.Dao;
using Entities.Report;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Entities.Trace;
using Integrator;
using Entities;
using Entities.Master;
using System.Reflection;
using Entities.General;
using Microsoft.Reporting.WinForms;
using System.Threading;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using Entities.Profile;

namespace BizzLogic.Logic
{
    /// <summary>
    /// Maneja el modulo de despliegue de reportes documentos - Tickets y otros
    /// </summary>
    public partial class ReportMngr : BasicMngr
    {


        public DataSet GetReportObject(MenuOption report, IList<String> rpParams, Location location)
        {
            //obtine el extension de ese menu.
            MenuOptionExtension moe;
            try
            {
                 moe = Factory.DaoMenuOptionExtension().Select(new MenuOptionExtension { MenuOption = report }).First();
            }
            catch
            {
                throw new Exception("No query extension defined for this report.");
            }

            string selectQuery = moe.Custom1;
            selectQuery= selectQuery.Replace("__LOCATION", location.LocationID.ToString());

            String[] colList = moe.Custom2.Split(',');

            IList<Object[]> list = Factory.DaoMenuOptionExtension().GetReportObject(selectQuery, rpParams);

            if (list == null || list.Count == 0)
                return null;

           DataTable dt = GetDataTableSchema(colList, "dt0", list);

           DataSet ds = new DataSet("dsResult");
           ds.Tables.Add(dt);
           return ds;

        }

   }
}
