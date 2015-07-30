using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace WpfFront.Common
{

    public class UIMenu
    {
        public UIMenu() { }

        public string Name { get; set; }
        public IList<UIMenuOption> Options { get; set; }
    }


    public class UIMenuOption
    {
        public UIMenuOption() { }

        public string Name { get; set; }
        public Type PresenterType { get; set; }
        public BitmapImage Icon { get; set; }
    }


}
