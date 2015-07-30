using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.General;
using Entities.Trace;
using Entities.Master;
using Entities.Profile;
using Integrator.Dao;
using Entities.Process;

namespace Integrator
{
    public class WmsTypes
    {
        private DaoFactory Factory { get; set; }

        public WmsTypes()
        {
            Factory = new DaoFactory();
        }

        public WmsTypes(DaoFactory factory)
        {
            Factory = factory;
        }

        public DocumentType GetLabelType(DocumentType data)
        {
            return Factory.DaoDocumentType().Select(data).First();
        }


        public Status GetStatus(Status data)
        {
            return Factory.DaoStatus().Select(data).First();
        }


        public SysUser GetUser(SysUser data)
        {
          return Factory.DaoSysUser().Select(data).First();
        }

        public DocumentConcept GetDocumentConcept(DocumentConcept data)
        {
            return Factory.DaoDocumentConcept().Select(data).First();
        }

        public DocumentType GetDocumentType(DocumentType data)
        {
           return Factory.DaoDocumentType().Select(data).First();
        }


        public Node GetNode(Node data)
        {
           return Factory.DaoNode().SelectById(data);
        }


        public AccountType GetAccountType(AccountType data)
        {
            return Factory.DaoAccountType().Select(data).First();
        }


        public Account GetAccount(Account data)
        {
           return Factory.DaoAccount().Select(data).First();
        }


        public AccountAddress GetAccountAddress(AccountAddress data)
        {
            return Factory.DaoAccountAddress().Select(data).FirstOrDefault();
        }


        public DocumentConcept GetDefaultConcept(DocumentClass docClass)
        {
            try
            {
                DocumentConcept data = new DocumentConcept();
                data.DocClass = docClass;
                data.Name = "Default";
                return Factory.DaoDocumentConcept().Select(data).First();
            }
            catch { return new DocumentConcept {  DocConceptID = 401 }; } //task by default
        }


        public Location GetDefaultLocation()
        {
            Company company = new Company();
            company.IsDefault = true;
            company = Factory.DaoCompany().Select(company).First();

            Location data = new Location();
            data.IsDefault = true;
            data.Company = company;
            return Factory.DaoLocation().Select(data).First();
        }


        public Company GetDefaultCompany()
        {
            Company data = new Company();
            data.IsDefault = true;
            return Factory.DaoCompany().Select(data).First();
        }


        public Unit GetUnit(Unit unit)
        {
            return Factory.DaoUnit().Select(unit).First();
        }


        public KitAssembly GetKitAssembly(KitAssembly data)
        {
            return Factory.DaoKitAssembly().Select(data,0).First();
        }


        public Product GetProduct(Product data)
        {
            return Factory.DaoProduct().Select(data,0).First();
        }

        public Location GetLocation(Location data)
        {
            return Factory.DaoLocation().Select(data).First();
        }


        //public Bin GetDefaultBin()
        //{
        //    Bin bin = new Bin { Location = GetDefaultLocation(), BinCode = "MAIN" };

        //    return Factory.DaoBin().Select(bin).First();
        //}


        public ProductCategory GetProductCategory(ProductCategory data)
        {
            return Factory.DaoProductCategory().Select(data).First();
        }

        public Bin GetBin(Bin data)
        {
            return Factory.DaoBin().Select(data).First();
        }

        public ShippingMethod GetShippingMethod(ShippingMethod data)
        {
            return Factory.DaoShippingMethod().Select(data).First();
        }


        public object GetCompanyOption(Company company, string code)
        {
            try
            {
                return Factory.DaoConfigOptionByCompany().Select(new ConfigOptionByCompany
                {
                    Company = company,
                    ConfigOption = new ConfigOption { Code = code }
                }).First().Value;
            }
            catch { return ""; }
        }

        public CustomProcess GetCustomProcess(CustomProcess data)
        {
            return Factory.DaoCustomProcess().Select(data).First();
        }


        public IList<ConnectionErpSetup> GetConnectionErpSetup(ConnectionErpSetup data)
        {
            return Factory.DaoConnectionErpSetup().Select(data);
        }


        public void UpdateDocument(Document document)
        {
            Factory.DaoDocument().UpdateTranx(document);
        }
    }
}
