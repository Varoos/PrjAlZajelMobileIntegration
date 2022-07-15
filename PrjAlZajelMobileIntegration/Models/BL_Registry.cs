using Focus.Common.DataStructs;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace PrjAlZajelMobileIntegration.Models
{
    public class BL_Registry
    {
        public DataSet GetData(string strSelQry, int CompId, ref string error)
        {
            error = "";
            try
            {
                Database obj = Focus.DatabaseFactory.DatabaseWrapper.GetDatabase(CompId);
                return (obj.ExecuteDataSet(CommandType.Text, strSelQry));
            }
            catch (Exception e)
            {
                error = e.Message;
                FConvert.LogFile("AlZajelMobileIntegration.log", "err : " + "[" + System.DateTime.Now + "] - GetData :" + error + "---" + strSelQry);
                return null;
            }
        }

        public int GetExecute(string strSelQry, int CompId, ref string error)
        {
            error = "";
            try
            {
                Database obj = Focus.DatabaseFactory.DatabaseWrapper.GetDatabase(CompId);
                return (obj.ExecuteNonQuery(CommandType.Text, strSelQry));
            }
            catch (Exception e)
            {
                error = e.Message;
                FConvert.LogFile("AlZajelMobileIntegration.log", DateTime.Now.ToString() + " GetExecute :" + error + "---" + strSelQry);
                return 0;
            }
        }

        public class SaveMasterByNamesResult
        {
            public int lResult { get; set; }
            public string sValue { get; set; }
        }
        public class MRootObject
        {
            public SaveMasterByNamesResult SaveMasterByNamesResult { get; set; }
        }
        public class HashData
        {
            //public string url { get; set; }
            public List<Hashtable> data { get; set; }
            public int result { get; set; }
            public string message { get; set; }
        }
        public void SetLog(string content)
        {
            StreamWriter objSw = null;
            try
            {
                string sFilePath = System.IO.Path.GetTempPath() + "AlZajelMobileIntegrationLog" + DateTime.Now.Date.ToString("ddMMyyyy") + ".txt";
                objSw = new StreamWriter(sFilePath, true);
                objSw.WriteLine(DateTime.Now.ToString() + " " + content + Environment.NewLine);
            }
            catch (Exception ex)
            {
                //SetLog("Error -" + ex.Message);
            }
            finally
            {
                if (objSw != null)
                {
                    objSw.Flush();
                    objSw.Dispose();
                }
            }
        }

        public void SetSuccessLog(string LogName, string content)
        {
            StreamWriter objSw = null;
            try
            {
                string sFilePath = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine) + @"\" + LogName + DateTime.Now.Date.ToString("ddMMyyyy") + ".txt";
                objSw = new StreamWriter(sFilePath, true);
                objSw.WriteLine(DateTime.Now.ToString() + " " + content + Environment.NewLine);
            }
            catch (Exception ex)
            {
                //SetLog("Error -" + ex.Message);
            }
            finally
            {
                if (objSw != null)
                {
                    objSw.Flush();
                    objSw.Dispose();
                }
            }
        }

        public void SetErrorLog(string LogName, string content)
        {
            StreamWriter objSw = null;
            try
            {
                string sFilePath = System.Web.HttpContext.Current.Server.MapPath("~/Temp") + LogName + DateTime.Now.Date.ToString("ddMMyyyy") + ".txt";
                objSw = new StreamWriter(sFilePath, true);
                objSw.WriteLine(DateTime.Now.ToString() + " " + content + Environment.NewLine);
            }
            catch (Exception ex)
            {
                //SetLog("Error -" + ex.Message);
            }
            finally
            {
                if (objSw != null)
                {
                    objSw.Flush();
                    objSw.Dispose();
                }
            }
        }

        //public void SetLog2(string content)
        //{
        //    StreamWriter objSw = null;
        //    try
        //    {
        //        string sFilePath = AppDomain.CurrentDomain.BaseDirectory + "AlZajelMobileIntegrationLog-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
        //        //string sFilePath = System.IO.Path.GetTempPath() + "CleancoLog" + DateTime.Now.Date.ToString("ddMMyyyy") + ".txt";
        //        objSw = new StreamWriter(sFilePath, true);
        //        objSw.WriteLine(DateTime.Now.ToString() + " " + content + Environment.NewLine);
        //    }
        //    catch (Exception ex)
        //    {
        //        //SetLog("Error -" + ex.Message);
        //    }
        //    finally
        //    {
        //        if (objSw != null)
        //        {
        //            objSw.Flush();
        //            objSw.Dispose();
        //        }
        //    }
        //}

        public void SetLog2(string LogName, string content)
        {
            string str = "Logs/" + LogName + ".txt";
            FileStream stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory.ToString() + str, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            writer.BaseStream.Seek(0L, SeekOrigin.End);
            writer.WriteLine(DateTime.Now.ToString() + " - " + content);
            writer.Flush();
            writer.Close();
        }
    }
}