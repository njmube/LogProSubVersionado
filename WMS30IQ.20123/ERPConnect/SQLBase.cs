using System;
using System.Collections.Generic;
using System.Text;
using Entities.General;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Integrator;
using Entities;


namespace ErpConnect
{
    public class SQLBase
    {
        
        //Conexiones a la Base de Datos
        public SqlConnection Connection;
        public SqlCommand Command;

        /// <summary>
        /// Constuctor
        /// </summary>
        public SQLBase()
        {
            Connection = new SqlConnection(); //GetCnnString()
            Command = new SqlCommand();

            //try
            //{
            //    Connection.Open();
            //    Connection.Close();
            //}
            //catch (SqlException ex)
            //{ throw new Exception("Connection Failed: " + ex.Message); }

        }

        #region Connection



        public void OpenConnection()
        {
            if (Connection.State == System.Data.ConnectionState.Closed)
                Connection.Open();
        }


        public void CloseConnection()
        {
            if (Connection.State == System.Data.ConnectionState.Open)
                Connection.Close();
        }

        #endregion


        #region AddParameter methods
        public void AddParameter(string ParameterName, string ParameterValue)
        {
            Command.Parameters.AddWithValue(ParameterName, ParameterValue);
        }

        public void AddParameter(string ParameterName, int ParameterValue)
        {
            Command.Parameters.AddWithValue(ParameterName, ParameterValue);
        }

        public void AddParameter(string ParameterName, DateTime ParameterValue)
        {
            Command.Parameters.AddWithValue(ParameterName, ParameterValue);
        }

        public void AddParameter(string ParameterName, double ParameterValue)
        {
            Command.Parameters.AddWithValue(ParameterName, ParameterValue);
        }

        public void AddParameter(string ParameterName, float ParameterValue)
        {
            Command.Parameters.AddWithValue(ParameterName, ParameterValue);
        }

        public void AddParameter(string ParameterName, long ParameterValue)
        {
            Command.Parameters.AddWithValue(ParameterName, ParameterValue);
        }

        public void AddParameter(string ParameterName, bool ParameterValue)
        {
            Command.Parameters.AddWithValue(ParameterName, ParameterValue);
        }
        #endregion


        public static DataSet ReturnDataSet(string Query, string sWhere, string tableName,  SqlConnection connection)
        {
            try
            {
                DataSet ds = new DataSet();

                sWhere = string.IsNullOrEmpty(sWhere) ? sWhere : " AND " + sWhere;

                SqlDataAdapter objAdapter = new SqlDataAdapter(Query + sWhere, connection);

                //Console.WriteLine(Query + sWhere);

                objAdapter.Fill(ds, tableName);
                return ds;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("ReturnDataSet: " + Query + sWhere, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                return null;
            }
        }


        public static String ReturnScalar(string Query, string sWhere, SqlConnection connection)
        {
            try
            {

                sWhere = string.IsNullOrEmpty(sWhere) ? sWhere : " AND " + sWhere;

                SqlCommand xCommand = new SqlCommand();
                xCommand.Connection = connection;
                xCommand.CommandText = Query + sWhere;

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string rs = "";

                try { rs = xCommand.ExecuteScalar().ToString(); }
                catch { }

                return (rs == null) ? "" : rs;

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("ReturnScalar:", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                return "";
            }
            finally
            {
                    connection.Close();
            }
        }

     
        public static void ExecuteQuery(string Query, SqlConnection connection)
        {
            try
            {

                SqlCommand xCommand = new SqlCommand();
                xCommand.Connection = connection;
                xCommand.CommandText = Query;

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                try { xCommand.ExecuteNonQuery(); }
                catch { }

            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("ExecuteQuery:", ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
            }
            //finally { connection.Close(); }
        }


        public DataTable SaveDataSet(DataSet objDSXML, string strTable, string userName)
        {
            SqlDataAdapter objAdapter;
            //DataRow objDBRow;
            DataSet objDSDBTable = new DataSet(strTable);
            SqlCommandBuilder ObjCmdBuilder;
            objAdapter = new SqlDataAdapter("SELECT * FROM " + strTable + " WHERE 1 = 2 ", Connection);
            objAdapter.Fill(objDSDBTable, strTable);


            string strProcedure = "sp" + strTable;

            try
            {

                //Empty Table
                Command.Connection = Connection;
                Command.CommandText = strProcedure;
                Command.CommandType = CommandType.StoredProcedure;
                Command.Parameters.Clear();
                Command.Parameters.AddWithValue("@numOption", 1);
                Connection.Open();
                Command.ExecuteNonQuery();


                //Cargue del XML
                foreach (DataRow objDataRow in objDSXML.Tables[0].Rows)
                {
                    DataRow objDBRow = objDSDBTable.Tables[0].NewRow();
                    for (int i = 0; i < objDataRow.Table.Columns.Count; i++)
                    {
                        //objDBRow[i] = objDataRow[i];
                        objDBRow[objDataRow.Table.Columns[i].ColumnName] = objDataRow[objDataRow.Table.Columns[i].ColumnName].ToString().Trim();
                    }

                    objDSDBTable.Tables[0].Rows.Add(objDBRow);
                    ObjCmdBuilder = new SqlCommandBuilder(objAdapter);
                    objAdapter.Update(objDSDBTable, strTable);
                }


                //Populating Final Table
                Command.CommandText = strProcedure;
                Command.Parameters.Clear();
                Command.Parameters.AddWithValue("@numOption", 2);
                Command.Parameters.AddWithValue("@user", userName);

                SqlDataAdapter objDataAdapter = new SqlDataAdapter(Command);
                DataTable objDataTable = new DataTable();
                objDataAdapter.Fill(objDataTable);
                return objDataTable;

                //objCommand.ExecuteNonQuery();
                //return "";
                //return null;
            }
            catch (SqlException ex)
            {
                throw new Exception("Error:" + ex.Message);
                //return ex.Message;
            }
            finally
            {
                Connection.Close();
            }
        }


        public static DataTable ReturnDataTable(string Query, string sWhere, string tableName, SqlConnection connection)
        {
            try
            {

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                DataTable ds = new DataTable(tableName);

                sWhere = string.IsNullOrEmpty(sWhere) ? sWhere : " AND " + sWhere;

                SqlDataAdapter objAdapter = new SqlDataAdapter(Query + sWhere, connection);

                //Console.WriteLine(Query + sWhere);

                objAdapter.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("ReturnDataTable: " + Query + sWhere, ListValues.EventType.Error, ex, null, ListValues.ErrorCategory.ErpConnection);
                return null;
            }
            finally { connection.Close();  }
        }

    }
}
