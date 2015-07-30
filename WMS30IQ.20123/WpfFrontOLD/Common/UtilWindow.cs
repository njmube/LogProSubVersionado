using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using WpfFront.WMSBusinessService;

namespace WpfFront.Common
{
    public class UtilWindow
    {
        public static string ConfirmWindow(string condition)
        {
            ConfirmationWindow cw = new ConfirmationWindow();
            cw.hdrWindow.Header = condition;
            bool? res = cw.ShowDialog();

            if (res == true)
                return cw.MessageBoxText.Text;
            else
                return "";
        }



        public static bool? ConfirmOK(string message)
        {
            ConfirmationOK cw = new ConfirmationOK();
            cw.MessageBoxText.Text = message;
            return cw.ShowDialog();
        }


        public static bool AuthorizateWindow(string MenuOption)
        {
            //return true;
            AuthorizationWindow aw = new AuthorizationWindow(MenuOption);
            return aw.ShowDialog().Value;
        }


        public static bool ConfirmResult(string customMsg)
        {
            ResultWindow cw = new ResultWindow();
            cw.txtMesage.Text = customMsg;
            bool? res = cw.ShowDialog();

            if (res == true)
                return cw.checkBox1.IsChecked == true ? false : true;
            else
                return true;
        }

        public static void ShowError(string customMsg)
        {
            ErrWindow cw = new ErrWindow();
            cw.txtMesage.Text = customMsg;
            cw.Title = "LogPro Express :: Error Message !";

            if (customMsg.Length > 100)
            {
                cw.Height = 300;
                cw.Width = 700;
            }

            cw.ShowDialog();
        }


        public static void ShowMessage(string customMsg)
        {

            MsgWindow cw = new MsgWindow();
            cw.txtMesage.Text = customMsg;
            cw.Title = "LogPro Express :: Process Message !";

            if (customMsg.Length > 100)
            {
                cw.Height = 300;
                cw.Width = 700;
            }

            cw.ShowDialog();
        }


        public static void ShowDocument(LabelTemplate rptCode, int docID, string printer, bool showBtnPrint)
        {
            ViewDocument fv = new ViewDocument(rptCode, docID, printer, showBtnPrint, null);
            fv.Show();
        }

        public static void ShowLabelsToPrint(LabelTemplate rptCode, bool showBtnPrint, IList<Label> labelList)
        {
            ViewDocument fv = new ViewDocument(rptCode, 0, null, showBtnPrint, labelList);
            fv.Show();
        }

        //public static void ShowPopUpShell(RegionNames regionName, object view, string viewName, bool clear)
        //{
        //    PopUpShell popUp = new PopUpShell();
        //    popUp.ShowViewInShell(regionName, view, viewName, clear);
        //    popUp.ShowDialog();
        //}

        internal static void ShowFile(ImageEntityRelation file)
        {
            string urlDocument = Util.GetFilePath(file);
            
            if (string.IsNullOrEmpty(urlDocument))
                return;

            ViewDocumentBrowser fv = new ViewDocumentBrowser(urlDocument);
            fv.Show();
        }
    }
}
