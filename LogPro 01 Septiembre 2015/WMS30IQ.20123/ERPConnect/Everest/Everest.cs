using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErpConnect;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.GP.eConnect;
using Microsoft.Dynamics.GP.eConnect.Serialization;
using System.Configuration;
using System.Data;
using Entities.Master;


namespace ErpConnect.Everest
{
    public class Everest : ConnectFactory
    {

        public override IDocumentService Documents()
        { return new DocumentService(FactoryCompany); }


        public override IReferenceService References()
        { return new ReferenceService(FactoryCompany); }

    }
}
