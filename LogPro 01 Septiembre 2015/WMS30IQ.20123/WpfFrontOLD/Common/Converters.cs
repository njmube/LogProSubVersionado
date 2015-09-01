using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using WpfFront.WMSBusinessService;

namespace WpfFront.Common
{
    public class ConverterObj2Visibility : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EvaluateValue(value);
        }

        private object EvaluateValue(object value)
        {
            if (value == null)
                return Visibility.Collapsed;

            else if (value.GetType().Equals(typeof(bool)) && bool.Parse(value.ToString()) == false)
                return Visibility.Collapsed;

            else if (value.GetType().Equals(typeof(int)) && int.Parse(value.ToString()) == 0)
                return Visibility.Collapsed;

            else
                return Visibility.Visible;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }



    public class ConverterNegation : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           //retorna la negacion de lo que entra
          return !(bool)value; 
        } 


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }



    public class GetPackageImage : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            DocumentPackage package = (DocumentPackage)value;


            if (package.PackID == 0)
                return "/WpfFront;component/Images/Icons/48x48/OrderManager.png";

            //if (package.ParentPackage == null && package.ChildPackages != null && package.ChildPackages.Count > 0)
            if (package.PackageType == "P")
                return "/WpfFront;component/Images/Pallet.png";

            return "/WpfFront;component/Images/openbox.png";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }


}