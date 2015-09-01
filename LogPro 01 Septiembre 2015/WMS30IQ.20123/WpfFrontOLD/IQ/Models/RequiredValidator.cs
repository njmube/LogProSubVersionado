using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfFront.IQ.Models
{
    //class RequiredValidator : ValidationRule
    //{
    public class RequiredValidator : IDataErrorInfo
    {
        public string nroPedido { get; set; }
        public string Consecutivo { get; set; }
        public string TipoRecoleccion { get; set; }
        public string TipoOrigen { get; set; }
        public string Origen { get; set; }
        public string Direccion { get; set; }
        public string NombreContacto { get; set; }
        public string CelularContacto { get; set; }
        public string filename { get; set; }

        #region IDataErrorInfo Members


        public string Error
        {
            get { throw new NotImplementedException(); }
        }


        public string this[string columnName]
        {



            get
            {
                string result = null;

                if (columnName == "nroPedido")
                {
                    if (string.IsNullOrEmpty(nroPedido))
                        result = "Por favor ingrese el numero de pedido";
                }
                if (columnName == "Consecutivo")
                {
                    if (string.IsNullOrEmpty(Consecutivo))
                        result = "Por favor ingrese el consecutivo";
                }
                if (columnName == "TipoRecoleccion")
                {
                    if (string.IsNullOrEmpty(TipoRecoleccion))
                        result = "Por favor ingrese el tipo de recoleccion";
                }
                if (columnName == "TipoOrigen")
                {
                    if (string.IsNullOrEmpty(TipoOrigen))
                        result = "Por favor ingrese el tipo de origen";
                }
                if (columnName == "Origen")
                {
                    if (string.IsNullOrEmpty(Origen))
                        result = "Por favor ingrese el origen";
                }
                if (columnName == "Direccion")
                {
                    if (string.IsNullOrEmpty(Direccion))
                        result = "Por favor ingrese la direccion";
                }
                if (columnName == "NombreContacto")
                {
                    if (string.IsNullOrEmpty(NombreContacto))
                        result = "Por favor ingrese el nombre del contacto";
                }
                if (columnName == "CelularContacto")
                {
                    if (string.IsNullOrEmpty(CelularContacto))
                        result = "Por favor ingrese el celular del contacto";
                }
                if (columnName == "filename")
                {
                    if (string.IsNullOrEmpty(filename))
                        result = "Por favor cargue el archivo";
                }

                return result;
            }
        }

        #endregion

    }
}
//}
