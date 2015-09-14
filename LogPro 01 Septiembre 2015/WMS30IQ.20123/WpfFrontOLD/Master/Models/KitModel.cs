using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfFront.WMSBusinessService;
using Core.BusinessEntity;
using WpfFront.Common;

namespace WpfFront.Models
{
    public interface IKitModel
    {
       
        KitAssembly Record { get; set; }
        IList<KitAssembly> EntityList { get; set; }
        IList<KitAssemblyFormula> FormulaList { get; set; }
       
    }

    public class KitModel : BusinessEntityBase, IKitModel
    {
        private KitAssembly record;
        public KitAssembly Record
        {
            get { return record; }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        private IList<KitAssembly> entityList;
        public IList<KitAssembly> EntityList
        {
            get
            { return entityList; }
            set
            {
                entityList = value;
                OnPropertyChanged("EntityList");
            }
        } 
        
        private IList<KitAssemblyFormula> formulaList;
        public IList<KitAssemblyFormula> FormulaList
        {
            get
            { return formulaList; }
            set
            {
                formulaList = value;
                OnPropertyChanged("FormulaList");
            }
        }       
         
    }
}
