using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfFront.IQ.Models
{
    public class ListViewData
    {
        public ListViewData()
        {
            // default constructor
        }

        public ListViewData(string col1, string col2)
        {
            Col1 = col1;
            Col2 = col2;
        }

        public string Col1 { get; set; }
        public string Col2 { get; set; }

    }
}
