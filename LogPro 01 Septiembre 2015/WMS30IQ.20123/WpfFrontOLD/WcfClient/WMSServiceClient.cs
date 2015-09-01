using WpfFront.WMSBusinessService;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using WpfFront.Common;
using System;
using System.Data;
using System.Collections.Specialized;


namespace WpfFront.Services
{



    public class WMSServiceClient
    {

        public WMSProcessClient SerClient;


        public WMSServiceClient()
        {
            //if (this.SerClient  == null)
                this.SerClient = new WMSProcessClient();
          

            //if (this.SerClient.State == CommunicationState.Faulted)
            //    this.SerClient.Abort();


            //if (this.SerClient.State != CommunicationState.Opened)
            //    this.SerClient.Open();
        }


        public void SetService() {

            this.SerClient = new WMSProcessClient();

            if (this.SerClient.State == CommunicationState.Faulted)
                this.SerClient.Abort();

            if (this.SerClient.State != CommunicationState.Opened)
                this.SerClient.Open();
        }



        public IList<Product> GetDocumentProductFromLines(IList<DocumentLine> docLines)
        {
            if (docLines == null || docLines.Count == 0)
                return null;

            try
            {
                return docLines.Where(f => f.LineStatus.StatusID != DocStatus.Cancelled)
                           .Select(f => f.Product).Distinct().Where(f => f.Status.StatusID == EntityStatus.Active)
                           .ToList();
            }
            catch
            {
                return null;
            }
        }


        public IList<Document> SearchDocument(string data, DocumentType docType)
        {
            IList<Document> docList = new List<Document>();

            if (data.Length < WmsSetupValues.SearchLength)
                return null;

            try
            {
                SetService();

                //Si coincide el docNo
                docList = SerClient.GetDocument(new Document { 
                    Search = data, 
                    DocType = docType, 
                    Company = App.curCompany, Location = App.curLocation });

                
                ///Adiciona si coincide el Vendor, Customer
                if (docType.DocClass != null && docType.DocClass.DocClassID == SDocClass.Receiving)
                    docList = docList.Union<Document>(
                        SerClient.GetDocument(new Document
                        {
                            DocType = docType,
                            Location = App.curLocation,
                            Vendor = new Account { Name = data, Company = App.curCompany }
                        })
                     ).ToList<Document>();

            
                ///Adiciona si coincide el Customer
                if (docType.DocClass != null &&  docType.DocClass.DocClassID == SDocClass.Shipping)
                    docList = docList.Union<Document>(
                        SerClient.GetDocument(new Document
                        {
                            DocType = docType,
                            Location = App.curLocation,
                            Customer = new Account { Name = data, Company = App.curCompany }
                        })
                     ).ToList<Document>();


                return docList;
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }



        }


        public IList<Product> SearchProduct(string data)
        {
            if (data.Length < WmsSetupValues.SearchLength)
                return null;

            IList<Product> productList = new List<Product>();

            try
            {
                SetService();
                //Busca por Nombre
                productList = SerClient.GetProductApp(new Product { Name = data, Company = App.curCompany }, WmsSetupValues.NumRegs);

                //Buscar por Vendor ItemNumber

                return productList;
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }

        }

        public IList<Product> SearchProduct(string data, string reference)
        {
            if (data.Length < WmsSetupValues.SearchLength)
                return null;

            IList<Product> productList = new List<Product>();

            try
            {
                SetService();
                //Busca por Nombre
                productList = SerClient.GetProductApp(new Product { Name = data, Reference = reference, Company = App.curCompany }, WmsSetupValues.NumRegs);

                //Buscar por Vendor ItemNumber

                return productList;
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }

        }


        public IList<Product> SearchProduct(string data, Account account)
        {
            if (data.Length < WmsSetupValues.SearchLength)
                return null;

            IList<Product> productList = new List<Product>();

            try
            {
                SetService();
                //Busca por Nombre
                productList = SerClient.GetProduct(new Product { Name = data, Company = App.curCompany });

                //Buscar por Vendor ItemNumber
                IList<ProductAccountRelation> itemAccount = SerClient.GetProductAccountRelation(
                    new ProductAccountRelation { ItemNumber = data, Account = account });

                if (itemAccount != null && itemAccount.Count > 0)
                    productList = productList.Union(itemAccount.Select(f => f.Product)).ToList();

                return productList;
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }

        }


        public IList<Account> SearchVendor(string data)
        {
            IList<Account> list = new List<Account>();

            try
            {
                SetService();

                if (data.Length == 0)
                {
                    list = SerClient.GetVendorAccount(new Account { Company = App.curCompany });
                    return list;
                }


                //if (data.Length < WmsSetupValues.SearchLength)
                //    return null;

                //Busca por Nombre

                list = SerClient.GetVendorAccount(new Account { Name = data, Company = App.curCompany });
                return list.Distinct().ToList();

            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }

        }


        public IList<Account> SearchCustomer(string data)
        {
            IList<Account> list = new List<Account>();

            SetService();

            try
            {

                if (data.Length == 0)
                {
                    list = SerClient.GetCustomerAccount(new Account { Company = App.curCompany });
                    return list;

                }

                //if (data.Length < WmsSetupValues.SearchLength)
                //    return null;

                //Busca por Nombre
                list = SerClient.GetCustomerAccount(new Account { Name = data, Company = App.curCompany });
                return list.Distinct().ToList();

            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }

        }


        public IList<Bin> SearchBin(string data)
        {
            SetService();

            try
            {
                IList<Bin> list = new List<Bin>();
                list = SerClient.GetBin(new Bin { BinCode = data, Location = App.curLocation });
                return list;
            }
            finally
            {
                SerClient.Close();

                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }

        }


        //Obtiene un Bilocation a partir del bincode, si es putawy retorna el putawy
        public Bin GetBinLocation(string binLocation, bool? putAway)
        {

            SetService();

            string BinCode;

            if (putAway == true)
                BinCode = DefaultBin.PUTAWAY;
            else
            {
                //View.BinLocation.Text = View.BinLocation.Text.ToUpper();

                //Checking if valid destination location
                if (string.IsNullOrEmpty(binLocation.Trim()))
                    return null;

                BinCode = binLocation;
            }


            try
            {
                return SerClient.GetBin(new Bin { BinCode = BinCode, Location = App.curLocation }).First();
            }
            catch
            { return null; }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }
        }


        public Label GetLocationData(string data, bool onlyLogistic)
        {
            SetService();

            try
            {
                IList<Label> result;
                DocumentType lType;

                //Busca por Bin si longitud es menor que 10
                if (data.Length <= WmsSetupValues.MaxBinLength)
                {
                    lType = new DocumentType { DocTypeID = LabelType.BinLocation };

                    result = SerClient.GetLabel(new Label { LabelCode = data, LabelType = lType, Bin = new Bin { Location = App.curLocation } });

                    if (result.Count > 0)
                        return result.First();
                }


                //Busca por label entire code o el numero de label
                //try { data = long.Parse(data).ToString(); }
                //catch { Util.ShowError("Location Entered is not valid Location."); return null; }


                lType = new DocumentType { DocTypeID = LabelType.ProductLabel };
                Label label = new Label { LabelCode = data, LabelType = lType, Bin = new Bin { Location = App.curLocation } };

                //if (onlyLogistic)
                //    label.IsLogistic = true;

                result = SerClient.GetLabel(label);

                if (result.Count > 0)
                    return result.OrderBy(f=>f.LabelType.DocTypeID).First();



                return null;
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                {
                    SerClient.Abort();
                }
            }
        }


        public Document ConfirmKitAssemblyOrder(Document data, Location location)
        {
            try
            {
                SetService();
                return SerClient.ConfirmKitAssemblyOrder(data, location);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        //####################### GENERADAS AUTOMATICAMENTE



        public IList<Unit> GetProductUnit(Product data)
        {
            try {
            SetService();  return SerClient.GetProductUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Account> GetVendorAccount(Account data)
        {
            try {
            SetService();  return SerClient.GetVendorAccount(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Account> GetCustomerAccount(Account data)
        {
            try {
            SetService();  return SerClient.GetCustomerAccount(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public DocumentTypeSequence GetNextDocSequence(Company company, DocumentType docType)
        {
            try {
            SetService();  return SerClient.GetNextDocSequence(company, docType); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void AssignBinToProduct(Product product, ZoneBinRelation zoneBin)
        {
            try {
                SetService(); SerClient.AssignBinToProduct(product, zoneBin);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Product> GetProduct(Product data)
        {
            try {
            SetService();  return SerClient.GetProduct(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }


        public IList<Product> GetProductApp(Product data, int showRegs)
        {
            try
            {
                SetService(); return SerClient.GetProductApp(data, showRegs);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public Product SaveProduct(Product data)
        {
            try {
            SetService();  return SerClient.SaveProduct(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateProduct(Product data)
        {
            try {
            SetService();  SerClient.UpdateProduct(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteProduct(Product data)
        {
            try {
            SetService();  SerClient.DeleteProduct(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<TrackOption> GetTrackOption(TrackOption data)
        {
            try {
            SetService();  return SerClient.GetTrackOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public TrackOption SaveTrackOption(TrackOption data)
        {
            try {
            SetService();  return SerClient.SaveTrackOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteTrackOption(TrackOption data)
        {
            try {
            SetService();  SerClient.DeleteTrackOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateTrackOption(TrackOption data)
        {
            try {
            SetService();  SerClient.UpdateTrackOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ProductTrackRelation> GetProductTrackRelation(ProductTrackRelation data)
        {
            try {
            SetService();  return SerClient.GetProductTrackRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ProductTrackRelation SaveProductTrackRelation(ProductTrackRelation data)
        {
            try {
            SetService();  return SerClient.SaveProductTrackRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteProductTrackRelation(ProductTrackRelation data)
        {
            try {
            SetService();  SerClient.DeleteProductTrackRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateProductTrackRelation(ProductTrackRelation data)
        {
            try {
            SetService();  SerClient.UpdateProductTrackRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Account> GetAccount(Account data)
        {
            try {
            SetService();  return SerClient.GetAccount(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Account SaveAccount(Account data)
        {
            try {
            SetService();  return SerClient.SaveAccount(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateAccount(Account data)
        {
            try {
            SetService();  SerClient.UpdateAccount(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteAccount(Account data)
        {
            try {
            SetService();  SerClient.DeleteAccount(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<AccountAddress> GetAccountAddress(AccountAddress data)
        {
            try {
            SetService();  return SerClient.GetAccountAddress(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public AccountAddress SaveAccountAddress(AccountAddress data)
        {
            try {
            SetService();  return SerClient.SaveAccountAddress(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateAccountAddress(AccountAddress data)
        {
            try {
            SetService();  SerClient.UpdateAccountAddress(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteAccountAddress(AccountAddress data)
        {
            try {
            SetService();  SerClient.DeleteAccountAddress(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<AccountTypeRelation> GetAccountTypeRelation(AccountTypeRelation data)
        {
            try {
            SetService();  return SerClient.GetAccountTypeRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public AccountTypeRelation SaveAccountTypeRelation(AccountTypeRelation data)
        {
            try {
            SetService();  return SerClient.SaveAccountTypeRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateAccountTypeRelation(AccountTypeRelation data)
        {
            try {
            SetService();  SerClient.UpdateAccountTypeRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteAccountTypeRelation(AccountTypeRelation data)
        {
            try {
            SetService();  SerClient.DeleteAccountTypeRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Bin> GetBin(Bin data)
        {
            try {
            SetService();  
                return SerClient.GetBin(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Bin SaveBin(Bin data)
        {
            try {
            SetService();  return SerClient.SaveBin(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateBin(Bin data)
        {
            try {
            SetService();  SerClient.UpdateBin(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteBin(Bin data)
        {
            try {
            SetService();  SerClient.DeleteBin(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Company> GetCompany(Company data)
        {
            try {
            SetService();  return SerClient.GetCompany(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Company SaveCompany(Company data)
        {
            try {
            SetService();  return SerClient.SaveCompany(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateCompany(Company data)
        {
            try {
            SetService();  SerClient.UpdateCompany(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteCompany(Company data)
        {
            try {
            SetService();  SerClient.DeleteCompany(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Contact> GetContact(Contact data)
        {
            try {
            SetService();  return SerClient.GetContact(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Contact SaveContact(Contact data)
        {
            try {
            SetService();  return SerClient.SaveContact(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateContact(Contact data)
        {
            try {
            SetService();  SerClient.UpdateContact(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteContact(Contact data)
        {
            try {
            SetService();  SerClient.DeleteContact(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ContactEntityRelation> GetContactEntityRelation(ContactEntityRelation data)
        {
            try {
            SetService();  return SerClient.GetContactEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ContactEntityRelation SaveContactEntityRelation(ContactEntityRelation data)
        {
            try {
            SetService();  return SerClient.SaveContactEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateContactEntityRelation(ContactEntityRelation data)
        {
            try {
            SetService();  SerClient.UpdateContactEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteContactEntityRelation(ContactEntityRelation data)
        {
            try {
            SetService();  SerClient.DeleteContactEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ContactPosition> GetContactPosition(ContactPosition data)
        {
            try {
            SetService();  return SerClient.GetContactPosition(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ContactPosition SaveContactPosition(ContactPosition data)
        {
            try {
            SetService();  return SerClient.SaveContactPosition(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateContactPosition(ContactPosition data)
        {
            try {
            SetService();  SerClient.UpdateContactPosition(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteContactPosition(ContactPosition data)
        {
            try {
            SetService();  SerClient.DeleteContactPosition(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Location> GetLocation(Location data)
        {
            try {
            SetService();  return SerClient.GetLocation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Location SaveLocation(Location data)
        {
            try {
            SetService();  return SerClient.SaveLocation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateLocation(Location data)
        {
            try {
            SetService();  SerClient.UpdateLocation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteLocation(Location data)
        {
            try {
            SetService();  SerClient.DeleteLocation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ShippingMethod> GetShippingMethod(ShippingMethod data)
        {
            try {
            SetService();  return SerClient.GetShippingMethod(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ShippingMethod SaveShippingMethod(ShippingMethod data)
        {
            try {
            SetService();  return SerClient.SaveShippingMethod(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateShippingMethod(ShippingMethod data)
        {
            try {
            SetService();  SerClient.UpdateShippingMethod(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteShippingMethod(ShippingMethod data)
        {
            try {
            SetService();  SerClient.DeleteShippingMethod(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Terminal> GetTerminal(Terminal data)
        {
            try {
            SetService();  return SerClient.GetTerminal(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Terminal SaveTerminal(Terminal data)
        {
            try {
            SetService();  return SerClient.SaveTerminal(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateTerminal(Terminal data)
        {
            try {
            SetService();  SerClient.UpdateTerminal(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteTerminal(Terminal data)
        {
            try {
            SetService();  SerClient.DeleteTerminal(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Unit> GetUnit(Unit data)
        {
            try {
            SetService();  return SerClient.GetUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Unit SaveUnit(Unit data)
        {
            try {
            SetService();  return SerClient.SaveUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateUnit(Unit data)
        {
            try {
            SetService();  SerClient.UpdateUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteUnit(Unit data)
        {
            try {
            SetService();  SerClient.DeleteUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<UnitProductEquivalence> GetUnitProductEquivalence(UnitProductEquivalence data)
        {
            try {
            SetService();  return SerClient.GetUnitProductEquivalence(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public UnitProductEquivalence SaveUnitProductEquivalence(UnitProductEquivalence data)
        {
            try {
            SetService();  return SerClient.SaveUnitProductEquivalence(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateUnitProductEquivalence(UnitProductEquivalence data)
        {
            try {
            SetService();  SerClient.UpdateUnitProductEquivalence(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteUnitProductEquivalence(UnitProductEquivalence data)
        {
            try {
            SetService();  SerClient.DeleteUnitProductEquivalence(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<UnitProductLogistic> GetUnitProductLogistic(UnitProductLogistic data)
        {
            try {
            SetService();  return SerClient.GetUnitProductLogistic(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public UnitProductLogistic SaveUnitProductLogistic(UnitProductLogistic data)
        {
            try {
            SetService();  return SerClient.SaveUnitProductLogistic(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateUnitProductLogistic(UnitProductLogistic data)
        {
            try {
            SetService();  SerClient.UpdateUnitProductLogistic(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteUnitProductLogistic(UnitProductLogistic data)
        {
            try {
            SetService();  SerClient.DeleteUnitProductLogistic(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<UnitProductRelation> GetUnitProductRelation(UnitProductRelation data)
        {
            try {
            SetService();  return SerClient.GetUnitProductRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public UnitProductRelation SaveUnitProductRelation(UnitProductRelation data)
        {
            try {
            SetService();  return SerClient.SaveUnitProductRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateUnitProductRelation(UnitProductRelation data)
        {
            try {
            SetService();  SerClient.UpdateUnitProductRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteUnitProductRelation(UnitProductRelation data)
        {
            try {
            SetService();  SerClient.DeleteUnitProductRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Vehicle> GetVehicle(Vehicle data)
        {
            try {
            SetService();  return SerClient.GetVehicle(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Vehicle SaveVehicle(Vehicle data)
        {
            try {
            SetService();  return SerClient.SaveVehicle(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateVehicle(Vehicle data)
        {
            try {
            SetService();  SerClient.UpdateVehicle(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteVehicle(Vehicle data)
        {
            try {
            SetService();  SerClient.DeleteVehicle(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Zone> GetZone(Zone data)
        {
            try {
            SetService();  return SerClient.GetZone(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Zone SaveZone(Zone data)
        {
            try {
            SetService();  return SerClient.SaveZone(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateZone(Zone data)
        {
            try {
            SetService();  SerClient.UpdateZone(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteZone(Zone data)
        {
            try {
            SetService();  SerClient.DeleteZone(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ZoneBinRelation> GetZoneBinRelation(ZoneBinRelation data)
        {
            try {
            SetService();  return SerClient.GetZoneBinRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ZoneBinRelation SaveZoneBinRelation(ZoneBinRelation data)
        {
            try {
            SetService();  return SerClient.SaveZoneBinRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateZoneBinRelation(ZoneBinRelation data)
        {
            try {
            SetService();  SerClient.UpdateZoneBinRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteZoneBinRelation(ZoneBinRelation data)
        {
            try {
            SetService();  SerClient.DeleteZoneBinRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ZonePickerRelation> GetZonePickerRelation(ZonePickerRelation data)
        {
            try {
            SetService();  return SerClient.GetZonePickerRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ZonePickerRelation SaveZonePickerRelation(ZonePickerRelation data)
        {
            try {
            SetService();  return SerClient.SaveZonePickerRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateZonePickerRelation(ZonePickerRelation data)
        {
            try {
            SetService();  SerClient.UpdateZonePickerRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteZonePickerRelation(ZonePickerRelation data)
        {
            try {
            SetService();  SerClient.DeleteZonePickerRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ZoneEntityRelation> GetZoneEntityRelation(ZoneEntityRelation data)
        {
            try {
            SetService();  return SerClient.GetZoneEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ZoneEntityRelation SaveZoneEntityRelation(ZoneEntityRelation data)
        {
            try {
            SetService();  return SerClient.SaveZoneEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateZoneEntityRelation(ZoneEntityRelation data)
        {
            try {
            SetService();  SerClient.UpdateZoneEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteZoneEntityRelation(ZoneEntityRelation data)
        {
            try {
            SetService();  SerClient.DeleteZoneEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<AccountType> GetAccountType(AccountType data)
        {
            try {
            SetService();  return SerClient.GetAccountType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public AccountType SaveAccountType(AccountType data)
        {
            try {
            SetService();  return SerClient.SaveAccountType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateAccountType(AccountType data)
        {
            try {
            SetService();  SerClient.UpdateAccountType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteAccountType(AccountType data)
        {
            try {
            SetService();  SerClient.DeleteAccountType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ClassEntity> GetClassEntity(ClassEntity data)
        {
            try {
            SetService();  return SerClient.GetClassEntity(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ClassEntity SaveClassEntity(ClassEntity data)
        {
            try {
            SetService();  return SerClient.SaveClassEntity(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateClassEntity(ClassEntity data)
        {
            try {
            SetService();  SerClient.UpdateClassEntity(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteClassEntity(ClassEntity data)
        {
            try {
            SetService();  SerClient.DeleteClassEntity(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<DocumentClass> GetDocumentClass(DocumentClass data)
        {
            try {
            SetService();  return SerClient.GetDocumentClass(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public DocumentClass SaveDocumentClass(DocumentClass data)
        {
            try {
            SetService();  return SerClient.SaveDocumentClass(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateDocumentClass(DocumentClass data)
        {
            try {
            SetService();  SerClient.UpdateDocumentClass(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteDocumentClass(DocumentClass data)
        {
            try {
            SetService();  SerClient.DeleteDocumentClass(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<DocumentConcept> GetDocumentConcept(DocumentConcept data)
        {
            try {
            SetService();  return SerClient.GetDocumentConcept(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public DocumentConcept SaveDocumentConcept(DocumentConcept data)
        {
            try {
            SetService();  return SerClient.SaveDocumentConcept(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateDocumentConcept(DocumentConcept data)
        {
            try {
            SetService();  SerClient.UpdateDocumentConcept(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteDocumentConcept(DocumentConcept data)
        {
            try {
            SetService();  SerClient.DeleteDocumentConcept(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<DocumentType> GetDocumentType(DocumentType data)
        {
            try {
            SetService();  
                return SerClient.GetDocumentType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public DocumentType SaveDocumentType(DocumentType data)
        {
            try {
            SetService();  return SerClient.SaveDocumentType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateDocumentType(DocumentType data)
        {
            try {
            SetService();  SerClient.UpdateDocumentType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteDocumentType(DocumentType data)
        {
            try {
            SetService();  SerClient.DeleteDocumentType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }



        public IList<PickMethod> GetPickMethod(PickMethod data)
        {
            try
            {
                SetService(); return SerClient.GetPickMethod(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public PickMethod SavePickMethod(PickMethod data)
        {
            try
            {
                SetService(); return SerClient.SavePickMethod(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdatePickMethod(PickMethod data)
        {
            try
            {
                SetService(); SerClient.UpdatePickMethod(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeletePickMethod(PickMethod data)
        {
            try
            {
                SetService(); SerClient.DeletePickMethod(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        public IList<DocumentTypeSequence> GetDocumentTypeSequence(DocumentTypeSequence data)
        {
            try {
            SetService();  return SerClient.GetDocumentTypeSequence(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public DocumentTypeSequence SaveDocumentTypeSequence(DocumentTypeSequence data)
        {
            try {
            SetService();  return SerClient.SaveDocumentTypeSequence(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateDocumentTypeSequence(DocumentTypeSequence data)
        {
            try {
            SetService();  SerClient.UpdateDocumentTypeSequence(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteDocumentTypeSequence(DocumentTypeSequence data)
        {
            try {
            SetService();  SerClient.DeleteDocumentTypeSequence(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<GroupCriteria> GetGroupCriteria(GroupCriteria data)
        {
            try {
            SetService();  return SerClient.GetGroupCriteria(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public GroupCriteria SaveGroupCriteria(GroupCriteria data)
        {
            try {
            SetService();  return SerClient.SaveGroupCriteria(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateGroupCriteria(GroupCriteria data)
        {
            try {
            SetService();  SerClient.UpdateGroupCriteria(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteGroupCriteria(GroupCriteria data)
        {
            try {
            SetService();  SerClient.DeleteGroupCriteria(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<GroupCriteriaDetail> GetGroupCriteriaDetail(GroupCriteriaDetail data)
        {
            try {
            SetService();  return SerClient.GetGroupCriteriaDetail(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public GroupCriteriaDetail SaveGroupCriteriaDetail(GroupCriteriaDetail data)
        {
            try {
            SetService();  return SerClient.SaveGroupCriteriaDetail(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateGroupCriteriaDetail(GroupCriteriaDetail data)
        {
            try {
            SetService();  SerClient.UpdateGroupCriteriaDetail(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteGroupCriteriaDetail(GroupCriteriaDetail data)
        {
            try {
            SetService();  SerClient.DeleteGroupCriteriaDetail(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<GroupCriteriaRelation> GetGroupCriteriaRelation(GroupCriteriaRelation data)
        {
            try {
            SetService();  return SerClient.GetGroupCriteriaRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public GroupCriteriaRelation SaveGroupCriteriaRelation(GroupCriteriaRelation data)
        {
            try {
            SetService();  return SerClient.SaveGroupCriteriaRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateGroupCriteriaRelation(GroupCriteriaRelation data)
        {
            try {
            SetService();  SerClient.UpdateGroupCriteriaRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteGroupCriteriaRelation(GroupCriteriaRelation data)
        {
            try {
            SetService();  SerClient.DeleteGroupCriteriaRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<GroupCriteriaRelationData> GetGroupCriteriaRelationData(GroupCriteriaRelationData data)
        {
            try {
            SetService();  return SerClient.GetGroupCriteriaRelationData(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public GroupCriteriaRelationData SaveGroupCriteriaRelationData(GroupCriteriaRelationData data)
        {
            try {
            SetService();  return SerClient.SaveGroupCriteriaRelationData(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateGroupCriteriaRelationData(GroupCriteriaRelationData data)
        {
            try {
            SetService();  SerClient.UpdateGroupCriteriaRelationData(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteGroupCriteriaRelationData(GroupCriteriaRelationData data)
        {
            try {
            SetService();  SerClient.DeleteGroupCriteriaRelationData(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<LabelMapping> GetLabelMapping(LabelMapping data)
        {
            try {
            SetService();  return SerClient.GetLabelMapping(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public LabelMapping SaveLabelMapping(LabelMapping data)
        {
            try {
            SetService();  return SerClient.SaveLabelMapping(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateLabelMapping(LabelMapping data)
        {
            try {
            SetService();  SerClient.UpdateLabelMapping(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteLabelMapping(LabelMapping data)
        {
            try {
            SetService();  SerClient.DeleteLabelMapping(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public LabelTemplate SaveLabelTemplate(LabelTemplate data)
        {
            try {
            SetService();  return SerClient.SaveLabelTemplate(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateLabelTemplate(LabelTemplate data)
        {
            try {
            SetService();  SerClient.UpdateLabelTemplate(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteLabelTemplate(LabelTemplate data)
        {
            try {
            SetService();  SerClient.DeleteLabelTemplate(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<LogError> GetLogError(LogError data)
        {
            try {
            SetService();  return SerClient.GetLogError(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public LogError SaveLogError(LogError data)
        {
            try {
            SetService();  return SerClient.SaveLogError(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateLogError(LogError data)
        {
            try {
            SetService();  SerClient.UpdateLogError(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteLogError(LogError data)
        {
            try {
            SetService();  SerClient.DeleteLogError(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<MeasureType> GetMeasureType(MeasureType data)
        {
            try {
            SetService();  return SerClient.GetMeasureType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public MeasureType SaveMeasureType(MeasureType data)
        {
            try {
            SetService();  return SerClient.SaveMeasureType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateMeasureType(MeasureType data)
        {
            try {
            SetService();  SerClient.UpdateMeasureType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteMeasureType(MeasureType data)
        {
            try {
            SetService();  SerClient.DeleteMeasureType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<MeasureUnit> GetMeasureUnit(MeasureUnit data)
        {
            try {
            SetService();  return SerClient.GetMeasureUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public MeasureUnit SaveMeasureUnit(MeasureUnit data)
        {
            try {
            SetService();  return SerClient.SaveMeasureUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateMeasureUnit(MeasureUnit data)
        {
            try {
            SetService();  SerClient.UpdateMeasureUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteMeasureUnit(MeasureUnit data)
        {
            try {
            SetService();  SerClient.DeleteMeasureUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<MeasureUnitConvertion> GetMeasureUnitConvertion(MeasureUnitConvertion data)
        {
            try {
            SetService();  return SerClient.GetMeasureUnitConvertion(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public MeasureUnitConvertion SaveMeasureUnitConvertion(MeasureUnitConvertion data)
        {
            try {
            SetService();  return SerClient.SaveMeasureUnitConvertion(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateMeasureUnitConvertion(MeasureUnitConvertion data)
        {
            try {
            SetService();  SerClient.UpdateMeasureUnitConvertion(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteMeasureUnitConvertion(MeasureUnitConvertion data)
        {
            try {
            SetService();  SerClient.DeleteMeasureUnitConvertion(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Status> GetStatus(Status data)
        {
            try {
            SetService();  return SerClient.GetStatus(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Status SaveStatus(Status data)
        {
            try {
            SetService();  return SerClient.SaveStatus(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateStatus(Status data)
        {
            try {
            SetService();  SerClient.UpdateStatus(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteStatus(Status data)
        {
            try {
            SetService();  SerClient.DeleteStatus(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<StatusType> GetStatusType(StatusType data)
        {
            try {
            SetService();  return SerClient.GetStatusType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public StatusType SaveStatusType(StatusType data)
        {
            try {
            SetService();  return SerClient.SaveStatusType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateStatusType(StatusType data)
        {
            try {
            SetService();  SerClient.UpdateStatusType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteStatusType(StatusType data)
        {
            try {
            SetService();  SerClient.DeleteStatusType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Document SaveDocument(Document data)
        {
            try {
            SetService();  return SerClient.SaveDocument(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteDocument(Document data)
        {
            try {
            SetService();  SerClient.DeleteDocument(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<DocumentAddress> GetDocumentAddress(DocumentAddress data)
        {
            try {
            SetService();  return SerClient.GetDocumentAddress(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public DocumentAddress SaveDocumentAddress(DocumentAddress data)
        {
            try {
            SetService();  return SerClient.SaveDocumentAddress(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateDocumentAddress(DocumentAddress data)
        {
            try {
            SetService();  SerClient.UpdateDocumentAddress(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteDocumentAddress(DocumentAddress data)
        {
            try {
            SetService();  SerClient.DeleteDocumentAddress(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        //public IList<DocumentHistory> GetDocumentHistory(DocumentHistory data)
        //{
        //    try {
        //    SetService();  return SerClient.GetDocumentHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public DocumentHistory SaveDocumentHistory(DocumentHistory data)
        //{
        //    try {
        //    SetService();  return SerClient.SaveDocumentHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public void UpdateDocumentHistory(DocumentHistory data)
        //{
        //    try {
        //    SetService();  SerClient.UpdateDocumentHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public void DeleteDocumentHistory(DocumentHistory data)
        //{
        //    try {
        //    SetService();  SerClient.DeleteDocumentHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        public DocumentLine SaveDocumentLine(DocumentLine data)
        {
            try {
            SetService();  return SerClient.SaveDocumentLine(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateDocumentLine(DocumentLine data)
        {
            try {
            SetService();  SerClient.UpdateDocumentLine(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteDocumentLine(DocumentLine data)
        {
            try {
            SetService();  SerClient.DeleteDocumentLine(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        //public IList<DocumentLineHistory> GetDocumentLineHistory(DocumentLineHistory data)
        //{
        //    try {
        //    SetService();  return SerClient.GetDocumentLineHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public DocumentLineHistory SaveDocumentLineHistory(DocumentLineHistory data)
        //{
        //    try {
        //    SetService();  return SerClient.SaveDocumentLineHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public void UpdateDocumentLineHistory(DocumentLineHistory data)
        //{
        //    try {
        //    SetService();  SerClient.UpdateDocumentLineHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public void DeleteDocumentLineHistory(DocumentLineHistory data)
        //{
        //    try {
        //    SetService();  SerClient.DeleteDocumentLineHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        public void UpdateLabel(Label data)
        {
            try {
            SetService();  SerClient.UpdateLabel(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteLabel(Label data)
        {
            try {
            SetService();  SerClient.DeleteLabel(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        //public IList<LabelHistory> GetLabelHistory(LabelHistory data)
        //{
        //    try {
        //    SetService();  return SerClient.GetLabelHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public LabelHistory SaveLabelHistory(LabelHistory data)
        //{
        //    try {
        //    SetService();  return SerClient.SaveLabelHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public void UpdateLabelHistory(LabelHistory data)
        //{
        //    try {
        //    SetService();  SerClient.UpdateLabelHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public void DeleteLabelHistory(LabelHistory data)
        //{
        //    try {
        //    SetService();  SerClient.DeleteLabelHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        public IList<Node> GetNode(Node data)
        {
            try {
            SetService();  return SerClient.GetNode(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Node SaveNode(Node data)
        {
            try {
            SetService();  return SerClient.SaveNode(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateNode(Node data)
        {
            try {
            SetService();  SerClient.UpdateNode(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteNode(Node data)
        {
            try {
            SetService();  SerClient.DeleteNode(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<NodeDocumentType> GetNodeDocumentType(NodeDocumentType data)
        {
            try {
            SetService();  return SerClient.GetNodeDocumentType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public NodeDocumentType SaveNodeDocumentType(NodeDocumentType data)
        {
            try {
            SetService();  return SerClient.SaveNodeDocumentType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateNodeDocumentType(NodeDocumentType data)
        {
            try {
            SetService();  SerClient.UpdateNodeDocumentType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteNodeDocumentType(NodeDocumentType data)
        {
            try {
            SetService();  SerClient.DeleteNodeDocumentType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<NodeExtension> GetNodeExtension(NodeExtension data)
        {
            try {
            SetService();  return SerClient.GetNodeExtension(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public NodeExtension SaveNodeExtension(NodeExtension data)
        {
            try {
            SetService();  return SerClient.SaveNodeExtension(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateNodeExtension(NodeExtension data)
        {
            try {
            SetService();  SerClient.UpdateNodeExtension(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteNodeExtension(NodeExtension data)
        {
            try {
            SetService();  SerClient.DeleteNodeExtension(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<NodeExtensionTrace> GetNodeExtensionTrace(NodeExtensionTrace data)
        {
            try {
            SetService();  return SerClient.GetNodeExtensionTrace(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public NodeExtensionTrace SaveNodeExtensionTrace(NodeExtensionTrace data)
        {
            try {
            SetService();  return SerClient.SaveNodeExtensionTrace(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateNodeExtensionTrace(NodeExtensionTrace data)
        {
            try {
            SetService();  SerClient.UpdateNodeExtensionTrace(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteNodeExtensionTrace(NodeExtensionTrace data)
        {
            try {
            SetService();  SerClient.DeleteNodeExtensionTrace(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<NodeRoute> GetNodeRoute(NodeRoute data)
        {
            try {
            SetService();  return SerClient.GetNodeRoute(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public NodeRoute SaveNodeRoute(NodeRoute data)
        {
            try {
            SetService();  return SerClient.SaveNodeRoute(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateNodeRoute(NodeRoute data)
        {
            try {
            SetService();  SerClient.UpdateNodeRoute(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteNodeRoute(NodeRoute data)
        {
            try {
            SetService();  SerClient.DeleteNodeRoute(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<NodeTrace> GetNodeTrace(NodeTrace data)
        {
            try
            {
                SetService(); 
                return SerClient.GetNodeTrace(data);
            }
            catch { return null;  }
            finally
            {
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort(); 
                else
                    SerClient.Close();
            }
        }

        public NodeTrace SaveNodeTrace(NodeTrace data)
        {
            try {
            SetService();  return SerClient.SaveNodeTrace(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateNodeTrace(NodeTrace data)
        {
            try {
            SetService();  SerClient.UpdateNodeTrace(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteNodeTrace(NodeTrace data)
        {
            try {
            SetService();  SerClient.DeleteNodeTrace(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        //public IList<NodeTraceHistory> GetNodeTraceHistory(NodeTraceHistory data)
        //{
        //    try {
        //    SetService();  return SerClient.GetNodeTraceHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public NodeTraceHistory SaveNodeTraceHistory(NodeTraceHistory data)
        //{
        //    try {
        //    SetService();  return SerClient.SaveNodeTraceHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public void UpdateNodeTraceHistory(NodeTraceHistory data)
        //{
        //    try {
        //    SetService();  SerClient.UpdateNodeTraceHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        //public void DeleteNodeTraceHistory(NodeTraceHistory data)
        //{
        //    try {
        //    SetService();  SerClient.DeleteNodeTraceHistory(data); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        public IList<TaskDocumentRelation> GetTaskDocumentRelation(TaskDocumentRelation data)
        {
            try {
            SetService();  return SerClient.GetTaskDocumentRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public TaskDocumentRelation SaveTaskDocumentRelation(TaskDocumentRelation data)
        {
            try {
            SetService();  return SerClient.SaveTaskDocumentRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateTaskDocumentRelation(TaskDocumentRelation data)
        {
            try {
            SetService();  SerClient.UpdateTaskDocumentRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteTaskDocumentRelation(TaskDocumentRelation data)
        {
            try {
            SetService();  SerClient.DeleteTaskDocumentRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Document> GetTaskByUser(TaskByUser data)
        {
            try {
            SetService();  return SerClient.GetTaskByUser(data, App.curLocation); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public TaskByUser SaveTaskByUser(TaskByUser data)
        {
            try {
            SetService();  return SerClient.SaveTaskByUser(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateTaskByUser(TaskByUser data)
        {
            try {
            SetService();  SerClient.UpdateTaskByUser(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteTaskByUser(TaskByUser data)
        {
            try {
            SetService();  SerClient.DeleteTaskByUser(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Connection> GetConnection(Connection data)
        {
            try {
            SetService();  return SerClient.GetConnection(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Connection SaveConnection(Connection data)
        {
            try {
            SetService();  return SerClient.SaveConnection(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateConnection(Connection data)
        {
            try {
            SetService();  SerClient.UpdateConnection(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteConnection(Connection data)
        {
            try {
            SetService();  SerClient.DeleteConnection(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ProductAccountRelation> GetProductAccountRelation(ProductAccountRelation data)
        {
            try {
            SetService();  return SerClient.GetProductAccountRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ProductAccountRelation SaveProductAccountRelation(ProductAccountRelation data)
        {
            try {
            SetService();  return SerClient.SaveProductAccountRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteProductAccountRelation(ProductAccountRelation data)
        {
            try {
            SetService();  SerClient.DeleteProductAccountRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateProductAccountRelation(ProductAccountRelation data)
        {
            try {
            SetService();  SerClient.UpdateProductAccountRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ImageEntityRelation> GetImageEntityRelation(ImageEntityRelation data)
        {
            try {
            SetService();  return SerClient.GetImageEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ImageEntityRelation SaveImageEntityRelation(ImageEntityRelation data)
        {
            try {
            SetService();  return SerClient.SaveImageEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteImageEntityRelation(ImageEntityRelation data)
        {
            try {
            SetService();  SerClient.DeleteImageEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateImageEntityRelation(ImageEntityRelation data)
        {
            try {
            SetService();  SerClient.UpdateImageEntityRelation(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }



        public IList<ConnectionType> GetConnectionType(ConnectionType data)
        {
            try {
            SetService();  return SerClient.GetConnectionType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Document> GetDocument(Document document)
        {
            try {
            SetService();  return SerClient.GetDocument(document); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Document CreateNewDocument(Document data, Boolean autocommit)
        {
            try {
            SetService();  return SerClient.CreateNewDocument(data, autocommit); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Boolean UpdateDocument(Document data)
        {
            try {
            SetService();  
                return SerClient.UpdateDocument(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        //                public Document CreateNewDocumentTask(IList<Document> docList, Document taskDoc)
        //{
        //try {
        //SetService();  return SerClient.CreateNewDocumentTask(docList, taskDoc); }
        //finally
        //{
        //SerClient.Close();
        //if (SerClient.State == CommunicationState.Faulted)
        //{SerClient.Abort();}
        //}
        //}

        public IList<DocumentBalance> GetDocumentBalance(DocumentBalance docBalance, bool isCrossDock)
        {
            try {
                SetService(); return SerClient.GetDocumentBalance(docBalance, isCrossDock);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Document> GetCrossDockSalesOrders(Document data)
        {
            try {
            SetService();  return SerClient.GetCrossDockSalesOrders(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<DocumentBalance> GetCrossDockBalance(DocumentBalance purchaseBalance, IList<Document> salesDocs)
        {
            try {
            SetService();  return SerClient.GetCrossDockBalance(purchaseBalance, salesDocs.ToList()); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Document ConfirmCrossDockProcess(IList<DocumentBalance> crossDockBalance, string user)
        {
            try {
            SetService();  return SerClient.ConfirmCrossDockProcess(crossDockBalance.ToList(), user); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }



        public IList<DocumentLine> GetDocumentLine(DocumentLine data)
        {
            try {
            SetService();  return SerClient.GetDocumentLine(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Product> GetDocumentProduct(Document data, Product product)
        {
            try {
            SetService();  return SerClient.GetDocumentProduct(data, product); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Unit> GetDocumentUnit(DocumentLine data)
        {
            try {
            SetService();  return SerClient.GetDocumentUnit(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Document> GetPendingDocument(Document document, int daysAgo, int records)
        {
            try {
                SetService(); 
                return SerClient.GetPendingDocument(document, daysAgo, records);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<DocumentBalance> GetDocumentPostingBalance(DocumentBalance docBalance)
        {
            try {
            SetService();  
                return SerClient.GetDocumentPostingBalance(docBalance); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<DocumentBalance> GetDocumentBalanceForEmpty(DocumentBalance docBalance)
        {
            try {
                SetService();  
                return SerClient.GetDocumentBalanceForEmpty(docBalance); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ProductStock> GetDocumentStock(Document document, bool printed)
        {
            try {
               SetService();  return SerClient.GetDocumentStock(document, printed); 
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ZoneBinRelation> GetProductAssignedZone(Product product, Location location)
        {
            try {
            SetService();  return SerClient.GetProductAssignedZone(product, location); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Label> GetLabel(Label data)
        {
            try {
            SetService();  return SerClient.GetLabel(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Label SaveLabel(Label data)
        {
            try {
            SetService();  return SerClient.SaveLabel(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<DocumentType> GetLabelType()
        {
            try {
            SetService();  
                return SerClient.GetLabelType(); 
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }


        public IList<Label> GetDocumentLabelAvailable(Document data, Label searchLabel)
        {
            try
            {
                SetService();
                return SerClient.GetDocumentLabelAvailable(data, searchLabel);
            }
            catch { throw; }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public SysUser UserAuthentication(SysUser data)
        {
            try {
            SetService();  return SerClient.UserAuthentication(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }



        public IList<ShowData> GetDomainList()
        {
            try {
                SetService();  
                return SerClient.GetDomainList(); 
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ShowData> GetCustomersList()
        {
            try
            {
                SetService();
                return SerClient.GetCustomersList();
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public IList<ConfigOption> GetConfigOption(ConfigOption data)
        {
            try {
            SetService();  return SerClient.GetConfigOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ConfigOption SaveConfigOption(ConfigOption data)
        {
            try {
            SetService();  return SerClient.SaveConfigOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateConfigOption(ConfigOption data)
        {
            try {
            SetService();  SerClient.UpdateConfigOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteConfigOption(ConfigOption data)
        {
            try {
            SetService();  SerClient.DeleteConfigOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<ConfigType> GetConfigType(ConfigType data)
        {
            try {
            SetService();  return SerClient.GetConfigType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ConfigType SaveConfigType(ConfigType data)
        {
            try {
            SetService();  return SerClient.SaveConfigType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateConfigType(ConfigType data)
        {
            try {
            SetService();  SerClient.UpdateConfigType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteConfigType(ConfigType data)
        {
            try {
            SetService();  SerClient.DeleteConfigType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<MenuOption> GetMenuOption(MenuOption data)
        {
            try {
            SetService();  return SerClient.GetMenuOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public MenuOption SaveMenuOption(MenuOption data)
        {
            try {
            SetService();  return SerClient.SaveMenuOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateMenuOption(MenuOption data)
        {
            try {
            SetService();  SerClient.UpdateMenuOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteMenuOption(MenuOption data)
        {
            try {
            SetService();  SerClient.DeleteMenuOption(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<MenuOptionByRol> GetMenuOptionByRol(MenuOptionByRol data)
        {
            try {
            SetService();  return SerClient.GetMenuOptionByRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public MenuOptionByRol SaveMenuOptionByRol(MenuOptionByRol data)
        {
            try {
            SetService();  return SerClient.SaveMenuOptionByRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateMenuOptionByRol(MenuOptionByRol data)
        {
            try {
            SetService();  SerClient.UpdateMenuOptionByRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteMenuOptionByRol(MenuOptionByRol data)
        {
            try {
            SetService();  SerClient.DeleteMenuOptionByRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<MenuOptionType> GetMenuOptionType(MenuOptionType data)
        {
            try {
            SetService();  return SerClient.GetMenuOptionType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public MenuOptionType SaveMenuOptionType(MenuOptionType data)
        {
            try {
            SetService();  return SerClient.SaveMenuOptionType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateMenuOptionType(MenuOptionType data)
        {
            try {
            SetService();  SerClient.UpdateMenuOptionType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteMenuOptionType(MenuOptionType data)
        {
            try {
            SetService();  SerClient.DeleteMenuOptionType(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<Rol> GetRol(Rol data)
        {
            try {
            SetService();  return SerClient.GetRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Rol SaveRol(Rol data)
        {
            try {
            SetService();  return SerClient.SaveRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateRol(Rol data)
        {
            try {
            SetService();  SerClient.UpdateRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteRol(Rol data)
        {
            try {
            SetService();  SerClient.DeleteRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<SysUser> GetSysUser(SysUser data)
        {
            try {
            SetService();  return SerClient.GetSysUser(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public SysUser SaveSysUser(SysUser data)
        {
            try {
            SetService();  return SerClient.SaveSysUser(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateSysUser(SysUser data)
        {
            try {
            SetService();  SerClient.UpdateSysUser(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteSysUser(SysUser data)
        {
            try {
            SetService();  SerClient.DeleteSysUser(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public IList<UserByRol> GetUserByRol(UserByRol data)
        {
            try {
            SetService();  return SerClient.GetUserByRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public UserByRol SaveUserByRol(UserByRol data)
        {
            try {
            SetService();  return SerClient.SaveUserByRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateUserByRol(UserByRol data)
        {
            try {
            SetService();  SerClient.UpdateUserByRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteUserByRol(UserByRol data)
        {
            try {
            SetService();  SerClient.DeleteUserByRol(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        //                public IList<UserTransactionLog> GetUserTransactionLog(UserTransactionLog data) 
        //{
        //try {
        //SetService();  return SerClient.GetUserTransactionLog( data) ; }
        //finally
        //{
        //SerClient.Close();
        //if (SerClient.State == CommunicationState.Faulted)
        //{SerClient.Abort();}
        //}
        //}

        //                public UserTransactionLog SaveUserTransactionLog(UserTransactionLog data) 
        //{
        //try {
        //SetService();  return SerClient.SaveUserTransactionLog( data) ; }
        //finally
        //{
        //SerClient.Close();
        //if (SerClient.State == CommunicationState.Faulted)
        //{SerClient.Abort();}
        //}
        //}

        //                public void UpdateUserTransactionLog(UserTransactionLog data) 
        //{
        //try {
        //SetService();  SerClient.UpdateUserTransactionLog( data) ; }
        //finally
        //{
        //SerClient.Close();
        //if (SerClient.State == CommunicationState.Faulted)
        //{SerClient.Abort();}
        //}
        //}

        //                public void DeleteUserTransactionLog(UserTransactionLog data) 
        //{
        //try {
        //SetService();  SerClient.DeleteUserTransactionLog( data) ; }
        //finally
        //{
        //SerClient.Close();
        //if (SerClient.State == CommunicationState.Faulted)
        //{SerClient.Abort();}
        //}
        //}

        public IList<ConfigOptionByCompany> GetConfigOptionByCompany(ConfigOptionByCompany data)
        {
            try {
            SetService();  return SerClient.GetConfigOptionByCompany(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public ConfigOptionByCompany SaveConfigOptionByCompany(ConfigOptionByCompany data)
        {
            try {
            SetService();  return SerClient.SaveConfigOptionByCompany(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void DeleteConfigOptionByCompany(ConfigOptionByCompany data)
        {
            try {
            SetService();  SerClient.DeleteConfigOptionByCompany(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void UpdateConfigOptionByCompany(ConfigOptionByCompany data)
        {
            try {
            SetService();  SerClient.UpdateConfigOptionByCompany(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public DocumentLine SaveAdjustmentTransaction(DocumentLine data, Label label)
        {
            try {
            SetService();  return SerClient.SaveAdjustmentTransaction(data, label, true); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Boolean CheckAdjustmentLine(DocumentLine data, Label label)
        {
            try {
            SetService();  return SerClient.CheckAdjustmentLine(data, label); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Label ChangeLabelUbication(Label labelSource, Label labelDest)
        {
            try {
            SetService();  return SerClient.ChangeLabelUbication(labelSource, labelDest,"", App.curUser); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Label ChangeLabelLocationV2(Label labelSource, Label labelDest, Document document)
        {
            try
            {
                SetService(); return SerClient.ChangeLabelLocationV2(labelSource, labelDest, document);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public DocumentLine ChangeProductUbication(Label labelSource, DocumentLine changeLine, Label labelDest)
        {
            try {
            SetService();  return SerClient.ChangeProductUbication(labelSource, changeLine, labelDest,""); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Boolean ReceiveProduct(DocumentLine receivingLine, Unit logisticUnit, Bin destLocation, Node recNode)
        {
            try {
            SetService();  
            return SerClient.ReceiveProduct(receivingLine, logisticUnit, destLocation, recNode); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void ReceiveLabels(Document document, List<Label> labels, Bin destLocation, Node recNode, Boolean defaultBin, Location location)
        {
            try
            {
                SetService(); 
                SerClient.ReceiveLabels(document,labels, destLocation, recNode, defaultBin, location);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public Label ReceiveLabel(Document document, Label label, Bin destLocation, Node recNode)
        {
            try {
            SetService();  return SerClient.ReceiveLabel(document, label, destLocation, recNode); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void ReceiptAtOnce(Document document, Bin destLocation, Node recNode)
        {
            try {
            SetService();  SerClient.ReceiptAtOnce(document, destLocation, recNode); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Boolean PickProduct(DocumentLine line, Label sourceLocation, Node node, Label packageLabel)
        {
            try {
                SetService();
                return SerClient.PickProduct(line, sourceLocation, node, packageLabel, App.curUser, null);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Label PickLabel(Document document, Label label, Node node, Label packageLabel)
        {
            try {
            SetService();
                //Modified Enero 10 /2009 - Bin in null
            return SerClient.PickLabel(document, label, node, packageLabel, App.curUser, null);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void PickAtOnce(Document document, Label sourceLocation, Node node)
        {
            try {
            SetService();  SerClient.PickAtOnce(document, sourceLocation, node, App.curUser); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void PickCrossDockProduct(Document purchase, IList<DocumentBalance> crossDockBalance, SysUser picker)
        {
            try {
            SetService();  SerClient.PickCrossDockProduct(purchase, crossDockBalance.ToList(), picker); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void ReverseReceiptNodeTraceByLabels(List<NodeTrace> nodeTraceList, SysUser user, DocumentType docType)
        {
            try {
                SetService(); SerClient.ReverseReceiptNodeTraceByLabels(nodeTraceList, user, docType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }


        public void ReversePickingNodeTraceByLabels(List<NodeTrace> nodeTraceList, SysUser user, Bin restockBin)
        {
            try
            {
                SetService(); SerClient.ReversePickingNodeTraceByLabels(nodeTraceList, user, restockBin);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public void ReverseReceiptNodeTraceByQty(DocumentBalance docBalance, int quantity, SysUser user)
        {
            try {
                SetService(); SerClient.ReverseReceiptNodeTraceByQty(docBalance, quantity, user);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void TestConnection(Company data)
        {
            try {
            SetService();  SerClient.TestConnection(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }



        public void CreateInventoryAdjustment(Document data)
        {
            try {
            SetService();  SerClient.CreateInventoryAdjustment(data, true); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void ReverseInventoryAdjustment(Document data)
        {
            try {
            SetService();  SerClient.ReverseInventoryAdjustment(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public Document CreatePurchaseReceipt(Document data)
        {
            try {
            SetService();  return SerClient.CreatePurchaseReceipt(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void ReversePurchaseReceipt(Document data)
        {
            try {
            SetService();  SerClient.ReversePurchaseReceipt(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public void ReverseShipmentDocument(Document data, Bin binRestore)
        {
            try {
                SetService(); SerClient.ReverseShipmentDocument(data, binRestore);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }




        //public String GetReplacedTemplate(IList<DocumentBalance> printList, LabelTemplate template, String printLot, UserByRol userByRol)
        //{
        //    try {
        //    SetService();  return SerClient.GetReplacedTemplate(printList.ToList(), template, printLot, userByRol); }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //        SerClient.Abort(); 
        //    }
        //}

        public IList<LabelTemplate> GetLabelTemplate(LabelTemplate data)
        {
            try {
            SetService();  return SerClient.GetLabelTemplate(data); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }

        public String GetReplacedTemplateForLabels(IList<Label> labelList, LabelTemplate template, string printLot)
        {
            try
            {
                SetService(); return SerClient.GetReplacedTemplateForLabels(labelList.ToList(), template, printLot);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public IList<Label> GenerateLabelsToPrint(IList<DocumentBalance> printList, String printLot, UserByRol userByRol)
        {
            try
            {
                SetService(); return SerClient.GenerateLabelsToPrint(printList.ToList(), printLot, userByRol);
            }
            catch {
                throw;
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }


        public ReportHeaderFormat GetReportInformation(Document data, string template)
        {
            try {
            SetService();  return SerClient.GetReportInformation(data, template); }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                SerClient.Abort(); 
            }
        }


        public Document CreateShipmentDocument(Document data)
        {
            try
            {
                SetService(); 
                return SerClient.CreateShipmentDocument(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        //Abr 8 2009
        public IList<ProductStock> GetReplanishmentList(ProductStock data, Location location, short selector, 
            bool showEmpty, string bin1, string bin2)
        {
            try
            {
                SetService();
                return SerClient.GetReplanishmentList(data, location, selector, showEmpty, bin1, bin2);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        //Abril 12 2009
        public Document CreateReplenishOrder(IList<ProductStock> lines, String user, Location location) {
            try
            {
                SetService();
                return SerClient.CreateReplenishOrder(lines.ToList(), user, location);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        //Abril 14 de 2009 - tracking Options
        public IList<LabelTrackOption> GetLabelTrackOption(LabelTrackOption data)
        {
            try
            {
                SetService(); return SerClient.GetLabelTrackOption(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public LabelTrackOption SaveLabelTrackOption(LabelTrackOption data)
        {
            try
            {
                SetService(); return SerClient.SaveLabelTrackOption(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateLabelTrackOption(LabelTrackOption data)
        {
            try
            {
                SetService(); SerClient.UpdateLabelTrackOption(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteLabelTrackOption(LabelTrackOption data)
        {
            try
            {
                SetService(); SerClient.DeleteLabelTrackOption(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public IList<DataType> GetDataType(DataType data)
        {
            try
            {
                SetService(); return SerClient.GetDataType(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public IList<ProductStock> GetBinStock(ProductStock data)
        {
            try
            {
                SetService(); return SerClient.GetBinStock(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        public DataSet GetReportObject(MenuOption data, IList<String> rpParam, Location location)
        {
            try
            {
                SetService(); return 
                SerClient.GetReportObject(data, rpParam.ToList(), location);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public IList<OptionType> GetOptionType(OptionType data)
        {
            try
            {
                SetService(); 
                return SerClient.GetOptionType(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        //Abril 22 de 2009 - Menu Extension
        public IList<MenuOptionExtension> GetMenuOptionExtension(MenuOptionExtension data)
        {
            try
            {
                SetService(); return SerClient.GetMenuOptionExtension(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public MenuOptionExtension SaveMenuOptionExtension(MenuOptionExtension data)
        {
            try
            {
                SetService(); return SerClient.SaveMenuOptionExtension(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateMenuOptionExtension(MenuOptionExtension data)
        {
            try
            {
                SetService(); SerClient.UpdateMenuOptionExtension(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteMenuOptionExtension(MenuOptionExtension data)
        {
            try
            {
                SetService(); SerClient.DeleteMenuOptionExtension(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }




        public IList<MessageRuleExtension> GetMessageRuleExtension(MessageRuleExtension data)
        {
            try
            {
                SetService(); return SerClient.GetMessageRuleExtension(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public MessageRuleExtension SaveMessageRuleExtension(MessageRuleExtension data)
        {
            try
            {
                SetService(); return SerClient.SaveMessageRuleExtension(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateMessageRuleExtension(MessageRuleExtension data)
        {
            try
            {
                SetService(); SerClient.UpdateMessageRuleExtension(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteMessageRuleExtension(MessageRuleExtension data)
        {
            try
            {
                SetService(); SerClient.DeleteMessageRuleExtension(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }




        //public IList<ReportDocument> GetReportDocument(ReportDocument data)
        //{
        //    try
        //    {
        //        SetService(); return SerClient.GetReportDocument(data);
        //    }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //            SerClient.Abort();
        //    }
        //}

        //public ReportDocument SaveReportDocument(ReportDocument data)
        //{
        //    try
        //    {
        //        SetService(); return SerClient.SaveReportDocument(data);
        //    }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //            SerClient.Abort();
        //    }
        //}

        //public void UpdateReportDocument(ReportDocument data)
        //{
        //    try
        //    {
        //        SetService(); SerClient.UpdateReportDocument(data);
        //    }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //            SerClient.Abort();
        //    }
        //}

        //public void DeleteReportDocument(ReportDocument data)
        //{
        //    try
        //    {
        //        SetService(); SerClient.DeleteReportDocument(data);
        //    }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //            SerClient.Abort();
        //    }
        //}





        public IList<MessageRuleByCompany> GetMessageRuleByCompany(MessageRuleByCompany data)
        {
            try
            {
                SetService(); return SerClient.GetMessageRuleByCompany(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public MessageRuleByCompany SaveMessageRuleByCompany(MessageRuleByCompany data)
        {
            try
            {
                SetService(); return SerClient.SaveMessageRuleByCompany(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateMessageRuleByCompany(MessageRuleByCompany data)
        {
            try
            {
                SetService(); SerClient.UpdateMessageRuleByCompany(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteMessageRuleByCompany(MessageRuleByCompany data)
        {
            try
            {
                SetService(); SerClient.DeleteMessageRuleByCompany(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public IList<ProductStock> GetProductStock(ProductStock productStock, PickMethod pickMethod)
        {
            try
            {
                SetService();
                return SerClient.GetProductStock(productStock, pickMethod);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }




        public IList<ProductAlternate> GetProductAlternate(ProductAlternate data)
        {
            try
            {
                SetService(); return SerClient.GetProductAlternate(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public ProductAlternate SaveProductAlternate(ProductAlternate data)
        {
            try
            {
                SetService(); return SerClient.SaveProductAlternate(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateProductAlternate(ProductAlternate data)
        {
            try
            {
                SetService(); SerClient.UpdateProductAlternate(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteProductAlternate(ProductAlternate data)
        {
            try
            {
                SetService(); SerClient.DeleteProductAlternate(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }




        public IList<CustomProcessContextByEntity> GetCustomProcessContextByEntity(CustomProcessContextByEntity data)
        {
            try
            {
                SetService(); return SerClient.GetCustomProcessContextByEntity(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public CustomProcessContextByEntity SaveCustomProcessContextByEntity(CustomProcessContextByEntity data)
        {
            try
            {
                SetService(); return SerClient.SaveCustomProcessContextByEntity(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateCustomProcessContextByEntity(CustomProcessContextByEntity data)
        {
            try
            {
                SetService(); SerClient.UpdateCustomProcessContextByEntity(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteCustomProcessContextByEntity(CustomProcessContextByEntity data)
        {
            try
            {
                SetService(); SerClient.DeleteCustomProcessContextByEntity(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }





        public IList<CustomProcess> GetCustomProcess(CustomProcess data)
        {
            try
            {
                SetService(); return SerClient.GetCustomProcess(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public CustomProcess SaveCustomProcess(CustomProcess data)
        {
            try
            {
                SetService(); return SerClient.SaveCustomProcess(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateCustomProcess(CustomProcess data)
        {
            try
            {
                SetService(); SerClient.UpdateCustomProcess(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteCustomProcess(CustomProcess data)
        {
            try
            {
                SetService(); SerClient.DeleteCustomProcess(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }







        public IList<CustomProcessActivity> GetCustomProcessActivity(CustomProcessActivity data)
        {
            try
            {
                SetService(); return SerClient.GetCustomProcessActivity(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public CustomProcessActivity SaveCustomProcessActivity(CustomProcessActivity data)
        {
            try
            {
                SetService(); return SerClient.SaveCustomProcessActivity(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateCustomProcessActivity(CustomProcessActivity data)
        {
            try
            {
                SetService(); SerClient.UpdateCustomProcessActivity(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteCustomProcessActivity(CustomProcessActivity data)
        {
            try
            {
                SetService(); SerClient.DeleteCustomProcessActivity(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }







        public IList<CustomProcessTransition> GetCustomProcessTransition(CustomProcessTransition data)
        {
            try
            {
                SetService(); return SerClient.GetCustomProcessTransition(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public CustomProcessTransition SaveCustomProcessTransition(CustomProcessTransition data)
        {
            try
            {
                SetService(); return SerClient.SaveCustomProcessTransition(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateCustomProcessTransition(CustomProcessTransition data)
        {
            try
            {
                SetService(); SerClient.UpdateCustomProcessTransition(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteCustomProcessTransition(CustomProcessTransition data)
        {
            try
            {
                SetService(); SerClient.DeleteCustomProcessTransition(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }





        public IList<CustomProcessContext> GetCustomProcessContext(CustomProcessContext data)
        {
            try
            {
                SetService(); return SerClient.GetCustomProcessContext(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public CustomProcessContext SaveCustomProcessContext(CustomProcessContext data)
        {
            try
            {
                SetService(); return SerClient.SaveCustomProcessContext(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateCustomProcessContext(CustomProcessContext data)
        {
            try
            {
                SetService(); SerClient.UpdateCustomProcessContext(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteCustomProcessContext(CustomProcessContext data)
        {
            try
            {
                SetService(); SerClient.DeleteCustomProcessContext(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }







        public IList<CustomProcessRoute> GetCustomProcessRoute(CustomProcessRoute data)
        {
            try
            {
                SetService(); return SerClient.GetCustomProcessRoute(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public CustomProcessRoute SaveCustomProcessRoute(CustomProcessRoute data)
        {
            try
            {
                SetService(); return SerClient.SaveCustomProcessRoute(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateCustomProcessRoute(CustomProcessRoute data)
        {
            try
            {
                SetService(); SerClient.UpdateCustomProcessRoute(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteCustomProcessRoute(CustomProcessRoute data)
        {
            try
            {
                SetService(); SerClient.DeleteCustomProcessRoute(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }





        public IList<IqColumn> GetIqColumn(IqColumn data)
        {
            try
            {
                SetService(); return SerClient.GetIqColumn(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public IqColumn SaveIqColumn(IqColumn data)
        {
            try
            {
                SetService(); return SerClient.SaveIqColumn(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateIqColumn(IqColumn data)
        {
            try
            {
                SetService(); SerClient.UpdateIqColumn(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteIqColumn(IqColumn data)
        {
            try
            {
                SetService(); SerClient.DeleteIqColumn(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        public DataSet GetIqReportDataSet(string dataQuery, DataSet rpParams)
        {
            try
            {
                SetService();
                return SerClient.GetIqReportDataSet(dataQuery, rpParams);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        //public IqQueryParameter SaveIqQueryParameter(IqQueryParameter data)
        //{
        //    try
        //    {
        //        SetService(); return SerClient.SaveIqQueryParameter(data);
        //    }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //            SerClient.Abort();
        //    }
        //}

        //public void UpdateIqQueryParameter(IqQueryParameter data)
        //{
        //    try
        //    {
        //        SetService(); SerClient.UpdateIqQueryParameter(data);
        //    }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //            SerClient.Abort();
        //    }
        //}

        //public void DeleteIqQueryParameter(IqQueryParameter data)
        //{
        //    try
        //    {
        //        SetService(); SerClient.DeleteIqQueryParameter(data);
        //    }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //            SerClient.Abort();
        //    }
        //}




        public IList<IqReport> GetIqReport(IqReport data)
        {
            try
            {
                SetService(); return SerClient.GetIqReport(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public IqReport SaveIqReport(IqReport data)
        {
            try
            {
                SetService(); return SerClient.SaveIqReport(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateIqReport(IqReport data)
        {
            try
            {
                SetService(); SerClient.UpdateIqReport(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteIqReport(IqReport data)
        {
            try
            {
                SetService(); SerClient.DeleteIqReport(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }






        public IList<IqReportColumn> GetIqReportColumn(IqReportColumn data)
        {
            try
            {
                SetService(); return SerClient.GetIqReportColumn(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public IqReportColumn SaveIqReportColumn(IqReportColumn data)
        {
            try
            {
                SetService(); return SerClient.SaveIqReportColumn(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateIqReportColumn(IqReportColumn data)
        {
            try
            {
                SetService(); SerClient.UpdateIqReportColumn(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteIqReportColumn(IqReportColumn data)
        {
            try
            {
                SetService(); SerClient.DeleteIqReportColumn(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }











        public IList<IqReportTable> GetIqReportTable(IqReportTable data)
        {
            try
            {
                SetService(); return SerClient.GetIqReportTable(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public IqReportTable SaveIqReportTable(IqReportTable data)
        {
            try
            {
                SetService(); return SerClient.SaveIqReportTable(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateIqReportTable(IqReportTable data)
        {
            try
            {
                SetService(); SerClient.UpdateIqReportTable(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteIqReportTable(IqReportTable data)
        {
            try
            {
                SetService(); SerClient.DeleteIqReportTable(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }




        public IList<IqTable> GetIqTable(IqTable data)
        {
            try
            {
                SetService(); return SerClient.GetIqTable(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public IqTable SaveIqTable(IqTable data)
        {
            try
            {
                SetService(); return SerClient.SaveIqTable(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateIqTable(IqTable data)
        {
            try
            {
                SetService(); SerClient.UpdateIqTable(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteIqTable(IqTable data)
        {
            try
            {
                SetService(); SerClient.DeleteIqTable(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        public IList<DocumentPackage> GetDocumentPackage(DocumentPackage data)
        {
            try
            {
                SetService(); return SerClient.GetDocumentPackage(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public DocumentPackage SaveDocumentPackage(DocumentPackage data)
        {
            try
            {
                SetService(); return SerClient.SaveDocumentPackage(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateDocumentPackage(DocumentPackage data)
        {
            try
            {
                SetService(); SerClient.UpdateDocumentPackage(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteDocumentPackage(DocumentPackage data)
        {
            try
            {
                SetService(); SerClient.DeleteDocumentPackage(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal int PrintPackageLabels(Document shipment)
        {
            try
            {
                SetService();
                return SerClient.PrintPackageLabels(shipment, null, null,"");
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        //internal void PrintShipmentDocs(Document shipment)
        //{
        //    try
        //    {
        //        SetService();
        //        SerClient.PrintShipmentDocs(shipment, null, null);
        //    }
        //    finally
        //    {
        //        SerClient.Close();
        //        if (SerClient.State == CommunicationState.Faulted)
        //            SerClient.Abort();
        //    }
        //}


        internal IList<ProductStock> GetLabelStock(Label label)
        {
            try
            {
                SetService();
                return SerClient.GetLabelStock(label);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal DocumentPackage CreateNewPackage(Document document, SysUser sysUser, bool isOpen, 
            DocumentPackage parent, string packType)
        {
            try
            {
                SetService();
                return SerClient.CreateNewPackage(document, sysUser, isOpen, parent, packType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void MoveQtyBetweenPackages(DocumentPackage curPack, DocumentPackage newPack, Product product, 
            double qty)
        {
            try
            {
                SetService();
                SerClient.MoveQtyBetweenPackages(curPack, newPack, product, qty);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal IList<ProductStock> GetStockComparation(ProductStock data, bool detailed, 
            Company company)
        {
            try
            {
                SetService();
                return SerClient.GetStockComparation(data, detailed, company);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }




        public IList<BinByTask> GetBinByTask(BinByTask data)
        {
            try
            {
                SetService(); return SerClient.GetBinByTask(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public BinByTask SaveBinByTask(BinByTask data)
        {
            try
            {
                SetService(); return SerClient.SaveBinByTask(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateBinByTask(BinByTask data)
        {
            try
            {
                SetService(); SerClient.UpdateBinByTask(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteBinByTask(BinByTask data)
        {
            try
            {
                SetService(); SerClient.DeleteBinByTask(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        public IList<BinByTaskExecution> GetBinByTaskExecution(BinByTaskExecution data)
        {
            try
            {
                SetService(); return SerClient.GetBinByTaskExecution(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public BinByTaskExecution SaveBinByTaskExecution(BinByTaskExecution data)
        {
            try
            {
                SetService(); return SerClient.SaveBinByTaskExecution(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateBinByTaskExecution(BinByTaskExecution data)
        {
            try
            {
                SetService(); SerClient.UpdateBinByTaskExecution(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteBinByTaskExecution(BinByTaskExecution data)
        {
            try
            {
                SetService(); SerClient.DeleteBinByTaskExecution(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal IList<CountTaskBalance> GetCountSummary(Document document, bool summary)
        {
            try
            {
                SetService();
                return SerClient.GetCountSummary(document, summary);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal Document ConfirmCountingTaskDocument(Document countTask, List<CountTaskBalance> taskList, string user)
        {
            try
            {
                SetService();
                return SerClient.ConfirmCountingTaskDocument(countTask, taskList, user);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal void PrintLabelsFromDevice(string printer, string rdlTemplateName, List<Label> listOfLabels)
        {
            try
            {
                SetService();
                SerClient.PrintLabelsFromDevice(printer, rdlTemplateName, listOfLabels, "");
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal void CancelCountingTask(Document countTask, string user)
        {
            try
            {
                SetService();
                SerClient.CancelCountingTask(countTask, user);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        public IList<Contract> GetContract(Contract data)
        {
            try
            {
                SetService(); return SerClient.GetContract(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public Contract SaveContract(Contract data)
        {
            try
            {
                SetService(); return SerClient.SaveContract(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateContract(Contract data)
        {
            try
            {
                SetService(); SerClient.UpdateContract(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteContract(Contract data)
        {
            try
            {
                SetService(); SerClient.DeleteContract(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        public Label PickProductWithTrack(Document document, Label label, double qtyToPick, Node node, SysUser picker, Label packLabel)
        {
            try
            {
                SetService();
                return SerClient.PickProductWithTrack(document, label, qtyToPick, node, picker, packLabel);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void ReceiveReturn(Document document, IList<ProductStock> retProduct, SysUser sysUser, double retTotalQty, Node recNode)
        {
            try
            {
                SetService();
                SerClient.ReceiveReturn(document, retProduct.ToList(), sysUser, retTotalQty, recNode);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdatePackageMovedLabels(IList<Label> movedLabels)
        {
            try
            {
                SetService();
                SerClient.UpdatePackageMovedLabels(movedLabels.ToList());
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void PrintDocumentsInBatch(List<Document> documentList, string printer, CustomProcess process )
        {
            try
            {
                SetService();
                SerClient.PrintDocumentsInBatch(documentList, null, printer, process);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal Label CreateUniqueTrackLabel(Label label, string labelCode)
        {
            try
            {
                SetService();
                return SerClient.CreateUniqueTrackLabel(label, labelCode);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal void ReceiptAcknowledge(Document document, double numLabels, SysUser sysUser)
        {
            try
            {
                SetService();
                SerClient.ReceiptAcknowledge(document, numLabels, sysUser, null);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal Label UpdateLabelTracking(Label curLabel, TrackOption trackOption, string trackValue, string user)
        {
            try
            {
                SetService();
                return SerClient.UpdateLabelTracking(curLabel, trackOption, trackValue, user);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }




        public IList<ProcessEntityResource> GetProcessEntityResource(ProcessEntityResource data)
        {
            try
            {
                SetService(); return SerClient.GetProcessEntityResource(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public ProcessEntityResource SaveProcessEntityResource(ProcessEntityResource data)
        {
            try
            {
                SetService(); return SerClient.SaveProcessEntityResource(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteProcessEntityResource(ProcessEntityResource data)
        {
            try
            {
                SetService(); SerClient.DeleteProcessEntityResource(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateProcessEntityResource(ProcessEntityResource data)
        {
            try
            {
                SetService(); SerClient.UpdateProcessEntityResource(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal IList<ShowData> GetDocumentAccount(Document document, short accountType, bool pending)
        {
            try
            {
                SetService(); 
                return SerClient.GetDocumentAccount(document, accountType, pending);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal Document CreateMergedDocument(Document document, List<DocumentLine> dtLines, List<SysUser> pickers, List<DocumentAddress> addresses)
        {
            try
            {
                SetService();
                return SerClient.CreateMergedDocument(document, dtLines, pickers, addresses);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal Document CreateMergedDocumentV2(Document document, List<DocumentLine> dtLines, List<SysUser> pickers, List<DocumentAddress> addresses)
        {
            try
            {
                SetService();
                return SerClient.CreateMergedDocumentV2(document, dtLines, pickers, addresses);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal string CreateMergedDocumentForBackOrder(Document document, List<DocumentLine> dtLines, List<SysUser> pickers, 
            List<DocumentAddress> addresses, int process)
        {
            try
            {
                SetService();
                return SerClient.CreateMergedDocumentForBackOrders(document, dtLines, pickers, addresses, process);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public IList<ProductCategory> GetProductCategory(ProductCategory data)
        {
            try
            {
                SetService(); return SerClient.GetProductCategory(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public ProductCategory SaveProductCategory(ProductCategory data)
        {
            try
            {
                SetService(); return SerClient.SaveProductCategory(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteProductCategory(ProductCategory data)
        {
            try
            {
                SetService(); SerClient.DeleteProductCategory(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateProductCategory(ProductCategory data)
        {
            try
            {
                SetService(); SerClient.UpdateProductCategory(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal IList<ProductStock> GetDocumentProductStock(Document document, Product product)
        {
            try
            {
                SetService();
                return SerClient.GetDocumentProductStock(document, product);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal IList<ProductInventory> GetProductInventoryByProduct(ProductInventory productInventory, List<int> productList)
        {
            try
            {
                SetService();
                return SerClient.GetProductInventoryByProduct(productInventory, productList);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateProductInventory(ProductInventory productInventory)
        {
            try
            {
                SetService();
                SerClient.UpdateProductInventory(productInventory);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal ProductInventory SaveProductInventory(ProductInventory productInventory)
        {
            try
            {
                SetService();
                return SerClient.SaveProductInventory(productInventory);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
              
        }

        internal void PersistProductInUse(ProductInventory productInventory)
        {
            try
            {
                SetService();
                SerClient.PersistProductInUse(productInventory);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void ResetQtyInUse(ProductInventory productInventory)
        {
            try
            {
                SetService();
                SerClient.ResetQtyInUse(productInventory);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        public IList<CountSchedule> GetCountSchedule(CountSchedule data)
        {
            try
            {
                SetService(); return SerClient.GetCountSchedule(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public CountSchedule SaveCountSchedule(CountSchedule data)
        {
            try
            {
                SetService(); return SerClient.SaveCountSchedule(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteCountSchedule(CountSchedule data)
        {
            try
            {
                SetService(); SerClient.DeleteCountSchedule(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateCountSchedule(CountSchedule data)
        {
            try
            {
                SetService();
                SerClient.UpdateCountSchedule(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal Label PickUniqueLabel(Document document, Node node, Product product, string serialLabel, SysUser picker, Label packLabel)
        {
            try
            {
                SetService();
                return SerClient.PickUniqueLabel(document, node, product, serialLabel, picker, packLabel);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UnPickUniqueLabel(Document document, Label label, SysUser picker)
        {
            try
            {
                SetService();
                SerClient.UnPickUniqueLabel(document, label, picker);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal IList<Label> GetUniqueTrackLabels(Label label)
        {
            try
            {
                SetService();
                return SerClient.GetUniqueTrackLabels(label);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void CloseDocumentPackage(DocumentPackage newPack)
        {
            try
            {
                SetService();
                SerClient.CloseDocumentPackage(newPack);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal IList<ShowData> GetCustomList(string databaseObject, string field, string strWhere)
        {
            try
            {
                SetService();
                return SerClient.GetCustomList(databaseObject, field, strWhere);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal IList<ProductStock> GetProductInUseForMerged(List<int> list, Location location)
        {
           try
            {
                SetService();
                return SerClient.GetProductInUseForMerged(list, location);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
            return null;
        }


        internal void CancelMergerOrder(Document document, DocumentLine docLine)
        {
            try
            {
                SetService();
                SerClient.CancelMergerOrder(document, docLine);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        internal void FullfilSalesOrder(Document document)
        {
            try
            {
                SetService();
                SerClient.FullfilSalesOrder(document);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal string ProcessFile(CustomProcess process, string stream)
        {
            try
            {
                SetService();
                return SerClient.ProcessFile(process, stream, App.curUser);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        public IList<KitAssembly> GetKitAssembly(KitAssembly data, int showRegs)
        {
            try
            {
                SetService();
                return SerClient.GetKitAssembly(data, showRegs);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public KitAssembly SaveKitAssembly(KitAssembly data)
        {
            try
            {
                SetService(); return SerClient.SaveKitAssembly(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateKitAssembly(KitAssembly data)
        {
            try
            {
                SetService(); SerClient.UpdateKitAssembly(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteKitAssembly(KitAssembly data)
        {
            try
            {
                SetService(); SerClient.DeleteKitAssembly(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public IList<KitAssemblyFormula> GetKitAssemblyFormula(KitAssemblyFormula data)
        {
            try
            {
                SetService();
                return SerClient.GetKitAssemblyFormula(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public KitAssemblyFormula SaveKitAssemblyFormula(KitAssemblyFormula data)
        {
            try
            {
                SetService(); return SerClient.SaveKitAssemblyFormula(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void UpdateKitAssemblyFormula(KitAssemblyFormula data)
        {
            try
            {
                SetService(); SerClient.UpdateKitAssemblyFormula(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        public void DeleteKitAssemblyFormula(KitAssemblyFormula data)
        {
            try
            {
                SetService(); SerClient.DeleteKitAssemblyFormula(data);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal DocumentLine SaveUpdateDocumentLine(DocumentLine docLine, bool removeLine)
        {
            try
            {
                SetService(); 
                return SerClient.SaveUpdateDocumentLine(docLine, removeLine);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void PrintKitAssemblyLabels(Document ssDocument, int qtyType)
        {
            try
            {
                SetService();
                SerClient.PrintKitAssemblyLabels(ssDocument, qtyType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal void ConfirmPicking(Document document, string user)
        {
            try
            {
                SetService();
                SerClient.ConfirmPicking(document, user);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal IList<ProductStock> GetNoCountSummary(Location location)
        {
            try
            {
                SetService();
                return SerClient.GetNoCountSummary(location);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal Document ProcessNoCount(List<ProductStock> list, string username, bool erp)
        {
            try
            {
                SetService(); 
                return  SerClient.ProcessNoCount(list, username, erp);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void ProcessNoCountToBin(List<ProductStock> listNoCount, string username, Location location, Bin bin)
        {
            try
            {
                SetService();
                SerClient.ProcessNoCountToBin(listNoCount, username, location,bin);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        internal void DirectSQLNonQuery(string query, Connection localSQL)
        {
            try
            {
                SetService();
                SerClient.DirectSQLNonQuery(query, localSQL);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        internal DataTable DirectSQLQuery(string query, string swhere, string tableName, Connection connection)
        {
            try
            {
                SetService();
                return SerClient.DirectSQLQuery(query, swhere, tableName, connection);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal DataSet DirectSQLQueryDS(string query, string swhere, string tableName, Connection connection)
        {
            try
            {
                SetService();
                return SerClient.DirectSQLQueryDS(query, swhere, tableName, connection);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal IList<ShowData> GetLanguage(string culture)
        {
            try
            {
                SetService();
                return SerClient.GetLanguage(culture);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal string GetObject(String hql, object fieldID)
        {
            try
            {
                SetService();
                return SerClient.GetObject(hql, fieldID, true);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal void UpdateIsMainProductAccount(ProductAccountRelation pa)
        {
            try
            {
                SetService();
                SerClient.UpdateIsMainProductAccount(pa);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void ReSendInventoryAdjustmentToERP(Document document)
        {
            try
            {
                SetService();
                SerClient.ReSendInventoryAdjustmentToERP(document);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal IList<C_CasNumber> GetC_CasNumber(C_CasNumber c_CasNumber)
        {
            try
            {
                SetService();
                return SerClient.GetC_CasNumber(c_CasNumber);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal C_CasNumber SaveC_CasNumber(C_CasNumber c_CasNumber)
        {
            try
            {
                SetService();
                return SerClient.SaveC_CasNumber(c_CasNumber);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateC_CasNumber(C_CasNumber c_CasNumber)
        {
            try
            {
                SetService();
                SerClient.UpdateC_CasNumber(c_CasNumber);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteC_CasNumber(C_CasNumber c_CasNumber)
        {
            try
            {
                SetService();
                SerClient.DeleteC_CasNumber(c_CasNumber);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }


        internal IList<C_CasNumberFormula> GetC_CasNumberFormula(C_CasNumberFormula c_CasNumberFormula)
        {
            try
            {
                SetService();
                return SerClient.GetC_CasNumberFormula(c_CasNumberFormula);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal C_CasNumberFormula SaveC_CasNumberFormula(C_CasNumberFormula c_CasNumberFormula)
        {
            try
            {
                SetService();
                return SerClient.SaveC_CasNumberFormula(c_CasNumberFormula);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateC_CasNumberFormula(C_CasNumberFormula c_CasNumberFormula)
        {
            try
            {
                SetService();
                SerClient.UpdateC_CasNumberFormula(c_CasNumberFormula);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteC_CasNumberFormula(C_CasNumberFormula c_CasNumberFormula)
        {
            try
            {
                SetService();
                SerClient.DeleteC_CasNumberFormula(c_CasNumberFormula);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }
        


        
        internal IList<C_CasNumberRule> GetC_CasNumberRule(C_CasNumberRule C_CasNumberRule)
        {
            try
            {
                SetService();
                return SerClient.GetC_CasNumberRule(C_CasNumberRule);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal C_CasNumberRule SaveC_CasNumberRule(C_CasNumberRule C_CasNumberRule)
        {
            try
            {
                SetService();
                return SerClient.SaveC_CasNumberRule(C_CasNumberRule);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateC_CasNumberRule(C_CasNumberRule C_CasNumberRule)
        {
            try
            {
                SetService();
                SerClient.UpdateC_CasNumberRule(C_CasNumberRule);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteC_CasNumberRule(C_CasNumberRule C_CasNumberRule)
        {
            try
            {
                SetService();
                SerClient.DeleteC_CasNumberRule(C_CasNumberRule);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }
    



        
        internal IList<MType> GetMType(MType MType)
        {
            try
            {
                SetService();
                return SerClient.GetMType(MType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal MType SaveMType(MType MType)
        {
            try
            {
                SetService();
                return SerClient.SaveMType(MType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateMType(MType MType)
        {
            try
            {
                SetService();
                SerClient.UpdateMType(MType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteMType(MType MType)
        {
            try
            {
                SetService();
                SerClient.DeleteMType(MType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }
    



        
        internal IList<MMaster> GetMMaster(MMaster MMaster)
        {
            try
            {
                SetService();
                return SerClient.GetMMaster(MMaster);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal MMaster SaveMMaster(MMaster MMaster)
        {
            try
            {
                SetService();
                return SerClient.SaveMMaster(MMaster);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateMMaster(MMaster MMaster)
        {
            try
            {
                SetService();
                SerClient.UpdateMMaster(MMaster);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteMMaster(MMaster MMaster)
        {
            try
            {
                SetService();
                SerClient.DeleteMMaster(MMaster);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }






        internal IList<EntityExtraData> GetEntityExtraData(EntityExtraData EntityExtraData)
        {
            try
            {
                SetService();
                return SerClient.GetEntityExtraData(EntityExtraData);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal EntityExtraData SaveEntityExtraData(EntityExtraData EntityExtraData)
        {
            try
            {
                SetService();
                return SerClient.SaveEntityExtraData(EntityExtraData);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateEntityExtraData(EntityExtraData EntityExtraData)
        {
            try
            {
                SetService();
                SerClient.UpdateEntityExtraData(EntityExtraData);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteEntityExtraData(EntityExtraData EntityExtraData)
        {
            try
            {
                SetService();
                SerClient.DeleteEntityExtraData(EntityExtraData);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }



        internal Document ConsolidateOrdersInNewDocument(Document document, List<Document> docList)
        {
            try
            {
                SetService();
                return SerClient.ConsolidateOrdersInNewDocument(document, docList);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal IList<ShowData> GetCustomListV2(string query)
        {
            try
            {
                SetService();
                return SerClient.GetCustomListV2(query);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal IList<DocumentLine> CreateAssemblyOrderLines(Document Document, Product Product, Double Quantity)
        {
            try
            {
                SetService();
                return SerClient.CreateAssemblyOrderLines(Document, Product, Quantity);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        #region Metodos IQ

        #region BinRoute

        internal IList<BinRoute> GetBinRoute(BinRoute BinRoute)
        {
            try
            {
                SetService();
                return SerClient.GetBinRoute(BinRoute);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal BinRoute SaveBinRoute(BinRoute BinRoute)
        {
            try
            {
                SetService();
                return SerClient.SaveBinRoute(BinRoute);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateBinRoute(BinRoute BinRoute)
        {
            try
            {
                SetService();
                SerClient.UpdateBinRoute(BinRoute);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteBinRoute(BinRoute BinRoute)
        {
            try
            {
                SetService();
                SerClient.DeleteBinRoute(BinRoute);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        #endregion

        #region DataDefinition

        internal IList<DataDefinition> GetDataDefinition(DataDefinition DataDefinition)
        {
            try
            {
                SetService();
                return SerClient.GetDataDefinition(DataDefinition);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal DataDefinition SaveDataDefinition(DataDefinition DataDefinition)
        {
            try
            {
                SetService();
                return SerClient.SaveDataDefinition(DataDefinition);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateDataDefinition(DataDefinition DataDefinition)
        {
            try
            {
                SetService();
                SerClient.UpdateDataDefinition(DataDefinition);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteDataDefinition(DataDefinition DataDefinition)
        {
            try
            {
                SetService();
                SerClient.DeleteDataDefinition(DataDefinition);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        #endregion

        #region DataDefinitionByBin

        internal IList<DataDefinitionByBin> GetDataDefinitionByBin(DataDefinitionByBin DataDefinitionByBin)
        {
            try
            {
                SetService();
                return SerClient.GetDataDefinitionByBin(DataDefinitionByBin);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal DataDefinitionByBin SaveDataDefinitionByBin(DataDefinitionByBin DataDefinitionByBin)
        {
            try
            {
                SetService();
                return SerClient.SaveDataDefinitionByBin(DataDefinitionByBin);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateDataDefinitionByBin(DataDefinitionByBin DataDefinitionByBin)
        {
            try
            {
                SetService();
                SerClient.UpdateDataDefinitionByBin(DataDefinitionByBin);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteDataDefinitionByBin(DataDefinitionByBin DataDefinitionByBin)
        {
            try
            {
                SetService();
                SerClient.DeleteDataDefinitionByBin(DataDefinitionByBin);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        #endregion

        #region DataInformation

        internal IList<DataInformation> GetDataInformation(DataInformation DataInformation)
        {
            try
            {
                SetService();
                return SerClient.GetDataInformation(DataInformation);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal DataInformation SaveDataInformation(DataInformation DataInformation)
        {
            try
            {
                SetService();
                return SerClient.SaveDataInformation(DataInformation);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateDataInformation(DataInformation DataInformation)
        {
            try
            {
                SetService();
                SerClient.UpdateDataInformation(DataInformation);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteDataInformation(DataInformation DataInformation)
        {
            try
            {
                SetService();
                SerClient.DeleteDataInformation(DataInformation);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        #endregion

        #region WFDataType

        internal IList<WFDataType> GetWFDataType(WFDataType WFDataType)
        {
            try
            {
                SetService();
                return SerClient.GetWFDataType(WFDataType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal WFDataType SaveWFDataType(WFDataType WFDataType)
        {
            try
            {
                SetService();
                return SerClient.SaveWFDataType(WFDataType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void UpdateWFDataType(WFDataType WFDataType)
        {
            try
            {
                SetService();
                SerClient.UpdateWFDataType(WFDataType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        internal void DeleteWFDataType(WFDataType WFDataType)
        {
            try
            {
                SetService();
                SerClient.DeleteWFDataType(WFDataType);
            }
            finally
            {
                SerClient.Close();
                if (SerClient.State == CommunicationState.Faulted)
                    SerClient.Abort();
            }
        }

        #endregion

        #endregion


    }





    

}
