using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Master;
using Entities.General;
using System.Diagnostics;
using System.IO;
using Entities.Process;
using System.Reflection;
using Entities;

namespace UtilTool.PrintFile
{
    public class PDF : PrintFileFactory
    {
        private ProcessEntityResource Resource { get; set; }

        public PDF(ProcessEntityResource resource)
        {
            Resource = resource;
        }

        public override void PrintFile()
        {
            //Manda a Imprimir

           // try
            //{

                Process command = new Process();
                String pdfFile = Guid.NewGuid() + "_" + Resource.File.ImageName;

                //Directorio depende si es web o app donde va a queda rel PDF creado.
                String appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!appPath.Contains(WmsSetupValues.WebServer))
                    appPath = Path.Combine(appPath, WmsSetupValues.WebServer);

                appPath = Path.Combine(appPath, WmsSetupValues.PrintReportDir);

                //PDF file.
                pdfFile = Path.Combine(appPath, pdfFile);

                //Save File to Disk Before Print.
                //file.
                FileStream oFileStream = new FileStream(pdfFile, FileMode.Create);
                oFileStream.Write(Resource.File.Image, 0, Resource.File.Image.Length);
                oFileStream.Close();


                // Configuration.PathToAcrobatReader 
                //command.StartInfo.FileName = @"C:\Archivos de programa\Adobe\Reader 8.0\Reader\acrord32.exe";
                command.StartInfo.FileName = Path.Combine(Resource.File.FileType.CnnString, "acrord32.exe");

                //command.StartInfo.Arguments = "/p /h " + @"c:\testInv.pdf"; // Path.GetFullPath("testInv.pdf");
                command.StartInfo.Arguments = "/t \"" + pdfFile + "\" \"" + Resource.Printer.Name + "\" "; // Other Printer

                command.EnableRaisingEvents = false;


                command.StartInfo.CreateNoWindow = true;
                command.StartInfo.RedirectStandardOutput = true;
                command.StartInfo.UseShellExecute = false;
                command.Start();
                command.StandardOutput.ReadToEnd();
                //command.Kill();
                command.WaitForExit();
                command.Close();

                //myProcessStartInfo.Arguments = " /t " & """" & p.sourceFolder & p.sourceFileName & """" & " " & """" & p.destination & """"                   
                //Process.Start(@"C:\Program Files\Adobe\Acrobat 7.0\Reader\AcroRd32.exe", @"/t ""C:\acrobat.pdf"" ""HP LaserJet 6P"" """" """"");
            //}
            //catch { }

        }
    }
}
