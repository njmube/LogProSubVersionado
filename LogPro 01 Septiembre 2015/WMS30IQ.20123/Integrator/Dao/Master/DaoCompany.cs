using System;
using System.Collections.Generic;
using System.Text;
using Entities;
using Integrator.Config;
using NHibernate;
using Entities.Master;

namespace Integrator.Dao.Master
{
    public class DaoCompany : DaoService
    {
        public DaoCompany(DaoFactory factory) : base(factory) { }

        public Company Save(Company data)
        {
            return (Company)base.Save(data);
        }


        public Boolean Update(Company data)
        {
            return base.Update(data);
        }


        public Boolean Delete(Company data)
        {
            return base.Delete(data);
        }


        public Company SelectById(Company data)
        {
            return (Company)base.SelectById(data);
        }


        public IList<Company> Select(Company data)
        {

                IList<Company> datos = new List<Company>();

                datos = GetHsql(data).List<Company>();
                if (!Factory.IsTransactional)
                    Factory.Commit();
                return datos;
            
        }



        public override IQuery GetHsql(Object data)
        {
            StringBuilder sql = new StringBuilder("select a from Company a    where  ");
            Company company = (Company)data;
            if (company != null)
            {
                Parms = new List<Object[]>();
                if (company.CompanyID != 0)
                {
                    sql.Append(" a.CompanyID = :id     and   ");
                    Parms.Add(new Object[] { "id", company.CompanyID });
                }

                if (!String.IsNullOrEmpty(company.Name))
                {
                    sql.Append(" a.Name = :nom     and   "); 
                    Parms.Add(new Object[] { "nom", company.Name });
                }

                if (!String.IsNullOrEmpty(company.ErpCode))
                {
                    sql.Append(" a.ErpCode = :nom1     and   "); 
                    Parms.Add(new Object[] { "nom1", company.ErpCode });
                }


                if (!String.IsNullOrEmpty(company.Email))
                {
                    sql.Append(" a.Email = :nom2     and   "); 
                    Parms.Add(new Object[] { "nom2", company.Email });
                }

                if (!String.IsNullOrEmpty(company.ContactPerson))
                {
                    sql.Append(" a.ContactPerson = :nom3     and   "); 
                    Parms.Add(new Object[] { "nom3", company.ContactPerson });
                }

                if (!String.IsNullOrEmpty(company.Website))
                {
                    sql.Append(" a.Website = :nom4     and   "); 
                    Parms.Add(new Object[] { "nom4", company.Website });
                }

                if (company.Status != null && company.Status.StatusID != 0)
                {
                    sql.Append(" a.Status.StatusID = :id1     and   ");
                    Parms.Add(new Object[] { "id1", company.Status.StatusID });
                }

                if (company.IsDefault != null)
                {
                    sql.Append(" a.IsDefault = :nom5     and   ");
                    Parms.Add(new Object[] { "nom5", company.IsDefault });
                }

                if (company.ErpConnection != null && company.ErpConnection.ConnectionID != 0)
                {
                    sql.Append(" a.ErpConnection.ConnectionID = :id7  and  ");
                    Parms.Add(new Object[] { "id7", company.ErpConnection.ConnectionID });
                }

            }

            sql = new StringBuilder(sql.ToString());
            sql.Append(" 1=1 order by a.CompanyID asc ");
            IQuery query = Factory.Session.CreateQuery(sql.ToString());
            SetParameters(query);
            return query;
        }


    }
}