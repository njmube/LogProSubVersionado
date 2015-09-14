using System;
using System.Collections;
using System.Collections.Generic;
using Integrator.Config;
using NHibernate;

namespace Integrator.Dao
{
    public abstract class DaoService
    {
        protected IList<Object[]> Parms { get; set; }

        private DaoFactory factory;



        public DaoFactory Factory
        {
            get { return factory; }
            set { factory = value; }
        }


        protected DaoService(DaoFactory factory)
        {
            Factory = factory;
        }


        public Object Save(Object data)
        {
            try
            { 
                factory.Session.Save(data); 
            
            }
            catch (Exception ex)
            {
                factory.Rollback();
                throw new Exception("Problem creating the record. please check if already exists.\n" 
                    + WriteLog.GetTechMessage(ex));
            }

            if (!factory.IsTransactional)
                factory.Commit();

            return data;

        }


        public Boolean Update(Object data)
        {
            try { factory.Session.Update(data); }
            catch (Exception ex)
            {
                factory.Rollback();
                //throw;
                throw new Exception("Problem updating the record. Please check message below.\n" 
                    + WriteLog.GetTechMessage(ex));

            }

            if (!factory.IsTransactional)
                factory.Commit();

            return true;    
        }


        public Boolean UpdateTranx(Object data)
        {
            try { factory.Session.Update(data); }
            catch (Exception ex)
            {
                throw new Exception("Problem updating the record. Please check message below.\n"
                    + WriteLog.GetTechMessage(ex));
            }

            return true;
        }


        public Boolean Delete(Object data)
        {
            try { factory.Session.Delete(data); }
            catch (Exception ex)
            {
                factory.Rollback();
                //throw;
                throw new Exception("Problem deleting the record. Please check message below.\n" + WriteLog.GetTechMessage(ex));

            }

            if (!factory.IsTransactional)
                factory.Commit();

            return true;    
        }


        public Object SelectById(Object data)
        {
            try { data = GetHsql(data).UniqueResult(); }
            catch
            {
                //factory.Rollback();
                throw;
            }
            if (!factory.IsTransactional)
                factory.Commit();

            return data;    
        }


        public abstract IQuery GetHsql(Object data);


        public void SetParameters(IQuery query)
        {
            if (Parms != null && Parms.Count > 0)
                foreach (IList item in Parms)
                {
                    if (item[1] is Int16)
                        query.SetInt16(item[0].ToString(), (Int16)item[1]);
                    else if (item[1] is Int32)
                        query.SetInt32(item[0].ToString(), (Int32)item[1]);
                    else if (item[1] is Int64)
                        query.SetInt64(item[0].ToString(), (Int64)item[1]);
                    else if (item[1] is String)
                        query.SetString(item[0].ToString(), (String)item[1]);
                    else if (item[1] is Double)
                        query.SetDouble(item[0].ToString(), (Double)item[1]);
                    else if (item[1] is DateTime)
                        query.SetDateTime(item[0].ToString(), (DateTime)item[1]);
                    else if (item[1] is Guid)
                        query.SetGuid(item[0].ToString(), (Guid)item[1]);
                    else if (item[1] is Boolean)
                        query.SetBoolean(item[0].ToString(), (Boolean)item[1]);
                    else if (item[1] is Byte)
                        query.SetByte(item[0].ToString(), (Byte)item[1]);
                    else if (item[1] is Decimal)
                        query.SetDecimal(item[0].ToString(), (Decimal)item[1]);
                    else if (item[1] is Char)
                        query.SetCharacter(item[0].ToString(), (Char)item[1]);
                }
        }
    }
}
