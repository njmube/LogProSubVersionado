using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using Entities.Master;
using Entities.General;
using Entities.Process;

namespace UtilTool.PrintFile
{
    public abstract class PrintFileFactory
    {
        //Methods to implement.
        public abstract void PrintFile();

        public static PrintFileFactory getFactory(ProcessEntityResource resource)
        {
            switch (resource.File.FileType.Name)
            {
                case SFileType.PDF:
                    return new PDF(resource);

                default:
                    return null;
            }
        }
    }
}