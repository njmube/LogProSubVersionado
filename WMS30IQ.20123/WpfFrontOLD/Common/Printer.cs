using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfFront.Common
{
    public class Printer
    {
        public String PrinterName { get; set; }
        public String PrinterPath { get; set; }
        public Boolean? IsDefault { get; set; }
        public Boolean? FromServer { get; set; }
    }

}
