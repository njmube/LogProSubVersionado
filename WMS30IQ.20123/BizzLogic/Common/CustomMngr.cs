using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.General;
using Entities.Master;
using Integrator.Dao;
using Entities.Trace;
using Entities;
using Integrator;
using Entities.Profile;
using System.Data;
using Entities.Report;
using System.IO;
using System.Reflection;
using System.Xml;
using ErpConnect;
using System.Data.SqlClient;
using System.Threading;
using Entities.Process;

namespace BizzLogic.Logic
{

    public partial class BasicMngr
    {

        public IList<C_CasNumberRule> GetCasNumberRule(C_CasNumberRule data)
        {
            //Obtiene la lisata de el Regulator a Mirar //MetaMaster
            IList<MMaster> regulatorList = Factory.DaoMMaster().Select(
                new MMaster { MetaType = new MType { Code = data.Rule.MetaType.Code } })
                .OrderBy(f=>f.NumOrder).ToList();

            //Obtiene la lista de las Regulaciones existentes
            IList<C_CasNumberRule> casNumberList = Factory.DaoC_CasNumberRule().Select(data);

            try
            {
                //Left Outer Join entregando Regualciones FULL
                casNumberList = (from ori in regulatorList
                                 join rule in casNumberList on ori.MetaMasterID equals rule.Rule.MetaMasterID into g
                                 from rule in g.DefaultIfEmpty()
                                 select new C_CasNumberRule { 
                                     Rule = ori, 
                                     CasNumber = data.CasNumber, 
                                     RuleValue = rule == null ? "" : rule.RuleValue,
                                     CreatedBy = rule == null ? "" : rule.CreatedBy,
                                     CreationDate = rule == null ? null : rule.CreationDate, 
                                     RowID = rule == null ? 0 : rule.RowID })
                                 .ToList();
            }
            catch { }

            return casNumberList;

        }
    }
}
