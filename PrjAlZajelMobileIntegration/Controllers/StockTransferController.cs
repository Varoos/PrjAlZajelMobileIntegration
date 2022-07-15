using Focus.Common.DataStructs;
using Newtonsoft.Json;
using PrjAlZajelMobileIntegration.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace PrjAlZajelMobileIntegration.Controllers
{
    public class StockTransferController : Controller
    {
        string AccessToken = "";
        string AccessTokenURL = ConfigurationManager.AppSettings["AccessTokenURL"];
        string PostingURL = ConfigurationManager.AppSettings["PostingURL"];
        string serverip = ConfigurationManager.AppSettings["ServerIP"];
        string serveripp = ConfigurationManager.AppSettings["ServerIPP"];
        string CompanyCode = ConfigurationManager.AppSettings["CompanyCode"];
        string Identifier = ConfigurationManager.AppSettings["Identifier"];
        string Secret = ConfigurationManager.AppSettings["Secret"];
        string Lng = ConfigurationManager.AppSettings["Lng"];
        BL_Registry objreg = new BL_Registry();
        // GET: StockTransfer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Posting(int CompanyId, string SessionId, int LoginId, int vtype, string DocNo)
        {
            objreg.SetLog2("AlZajelMobileIntegrationLog","Posting Method");
            string errors1 = "";
            string Message = "";
            try
            {
                List<StockDetails> Listsd = new List<StockDetails>();

                #region BaseVoucherDetails
                string Basequery = $@"select iTransactionId
                                from tCore_Header_0 h 
                                join tCore_Data_0 d on h.iHeaderId=d.iHeaderId
                                where iVoucherType=5383 and sVoucherNo= '{DocNo}'";
                DataSet baseds = objreg.GetData(Basequery, CompanyId, ref errors1);

                foreach (DataRow row in baseds.Tables[0].Rows)
                {
                    StockDetails c = new StockDetails();
                    c.iTransactionId = Convert.ToInt32(row["iTransactionId"]);
                    Listsd.Add(c);
                }
                #endregion
                if (Listsd.Count() == 0)
                {
                    var sMessage = "No Data Found";
                    objreg.SetLog(sMessage);
                    objreg.SetLog2("AlZajelMobileIntegrationLog", sMessage);
                    return Json(new { success = sMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var LinkType = "";
                    int LRCount = 0;
                    int MRCount = 0;

                    #region loadRequestSection
                    string lrquery = $@"select count(*) [Count] from tCore_Links_0 where iLinkId = 520361223 and bBase = 0 and iTransactionId= '{Listsd.First().iTransactionId}'";
                    DataSet lrds = objreg.GetData(lrquery, CompanyId, ref errors1);

                    foreach (DataRow row in lrds.Tables[0].Rows)
                    {
                        LRCount = Convert.ToInt32(row["Count"]);
                        LinkType = "Load Request";
                    }
                    #endregion

                    if (LRCount == 0)
                    {
                        #region MobileOthersSection
                        string moquery = $@"select count(*) [Count] from tCore_Links_0 where iLinkId = 520426759 and bBase = 0 and iTransactionId= '{Listsd.First().iTransactionId}'";
                        DataSet mods = objreg.GetData(moquery, CompanyId, ref errors1);

                        foreach (DataRow row in mods.Tables[0].Rows)
                        {
                            MRCount = Convert.ToInt32(row["Count"]);
                            LinkType = "Mobile Others";
                        }
                        #endregion
                    }
                    objreg.SetLog2("AlZajelMobileIntegrationLog", "LinkType"+ LinkType);

                    if (LinkType == "Load Request")
                    {
                        #region LoadReq
                        AccessToken = GetAccessToken();
                        if (AccessToken == "")
                        {
                            Message = "Invalid Token";
                            objreg.SetLog("Invalid Token");
                            objreg.SetLog2("AlZajelMobileIntegrationLog", Message);
                            //FConvert.LogFile("AlZajelMenuCategoryFailed.log", DateTime.Now.ToString() + " : Invalid Token");
                            var sMessage = "Token Should not be Empty";
                            objreg.SetLog(sMessage);
                            objreg.SetLog2("AlZajelMobileIntegrationLog", sMessage);
                            return Json(new { Message = Message }, JsonRequestBehavior.AllowGet);
                        }

                        StockList clist = new StockList();
                        clist.AccessToken = AccessToken;
                        clist.ObjectType = "vanStock";

                        #region VoucherDetails
                        string query = $@"select iTransactionId,ouf.sCode [OutletFrom],outt.sCode [OutletTo],iProduct,u.sCode [Productunit] ,sBatchNo,
                                            case when b.iExpiryDate=0 then '0' else convert(varchar,dbo.IntToDate(b.iExpiryDate),23) end [ExpiryDate],
                                            abs(fQuantity) Quantity,ISNULL(sNarration,'') Narration,GETDATE() [TransactionDate] from tCore_Header_0 h 
                                            join tCore_Data_0 d on h.iHeaderId=d.iHeaderId
                                            join tCore_Indta_0 i on i.iBodyId=d.iBodyId
                                            join tCore_HeaderData2055_0 hd on hd.iHeaderId=h.iHeaderId
                                            join tCore_Data_Tags_0 dt on dt.iBodyId=d.iBodyId
											join mCore_OutletFrom ouf on ouf.iMasterId=iTag3028
											join mCore_OutletTo outt on outt.iMasterId=iTag3029
                                            join mCore_Units u on u.iMasterId=iUnit
                                            left join tCore_Batch_0 b on b.iBodyId=d.iBodyId
                                            where iVoucherType=2055 and d.iTransactionId in(
                                            select ll.iTransactionId from tCore_Header_0 h join tCore_Data_0 d on h.iHeaderId=d.iHeaderId
                                            join tCore_Links_0 l on l.iTransactionId=d.iBodyId and bBase = 1 
                                            join tCore_Links_0 ll on ll.iRefId=l.iRefId and ll.bBase=0
                                            where iVoucherType=5383 and sVoucherNo='" + DocNo + "')";
                        List<StockDetailsData> lc = new List<StockDetailsData>();
                        DataSet ds = objreg.GetData(query, CompanyId, ref errors1);

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            var CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
                            StockDetailsData c = new StockDetailsData();
                            c.SourceStockCode = Convert.ToString(row["OutletFrom"]);
                            c.DestinationStockCode = Convert.ToString(row["OutletTo"]);
                            c.ProductId = Convert.ToInt32(row["iProduct"]);
                            c.ProductUnit = Convert.ToString(row["Productunit"]);   
                            c.BatchId = Convert.ToString(row["sBatchNo"]);
                            if (Convert.ToString(row["ExpiryDate"]) == "0")
                            {
                                c.ExpiryDate = null;
                            }
                            else
                            {
                                c.ExpiryDate = Convert.ToString(row["ExpiryDate"]);
                            }
                            c.Qty = Convert.ToDecimal(row["Quantity"]);
                            c.TransactionDateTime = Convert.ToString(CurrentDate);
                            c.Comments = Convert.ToString(row["Narration"]);
                            c.Type = 2;
                            lc.Add(c);
                        }
                        #endregion

                        clist.ItemList = lc;

                        if (clist.ItemList.Count() > 0)
                        {
                            #region PostingSection

                            var sContent = new JavaScriptSerializer().Serialize(clist);
                            using (WebClient client = new WebClient())
                            {
                                client.Headers.Add("Content-Type", "application/json");
                                objreg.SetSuccessLog("AlZajelMobileIntegrationLog.log", "Load Request Posted sContent: " + sContent);
                                objreg.SetLog2("AlZajelMobileIntegrationLog", "Load Request Posted sContent: " + sContent);
                                objreg.SetLog2("AlZajelMobileIntegrationLog", " PostingURL :" + PostingURL);
                                var arrResponse = client.UploadString(PostingURL, sContent);
                                //var arrResponse = client.UploadString("http://" + serverip + "/iddemo/idm.api/integration/upsertDataList", sContent);
                                var lng = JsonConvert.DeserializeObject<Result>(arrResponse);

                                if (lng.ResponseStatus.IsSuccess == true)
                                {
                                    foreach (var item in clist.ItemList)
                                    {
                                        FConvert.LogFile("AlZajelMobileIntegrationLog.log", DateTime.Now.ToString() + " : " + "Data posted / updated to mobile app device for Voucher:- " + DocNo);
                                    }
                                    Message = "Posted Successfully";
                                }
                                else
                                {
                                    int ErrorListCount = lng.ErrorMessages.Count();
                                    if (ErrorListCount > 0)
                                    {
                                        var ErrorList = lng.ErrorMessages.ToList();
                                        foreach (var item in ErrorList)
                                        {
                                            objreg.SetErrorLog("AlZajelMobileIntegrationLog.log", "ErrorList Message for Voucher No '" + DocNo + "' data as :- " + item);
                                            objreg.SetLog2("AlZajelMobileIntegrationLog", "ErrorList Message for Voucher No '" + DocNo + "' data as :- " + item);
                                        }
                                    }

                                    int FailedListCount = lng.FailedList.Count();
                                    if (FailedListCount > 0)
                                    {
                                        var FailedList = lng.FailedList.ToList();
                                        foreach (var item in FailedList)
                                        {
                                            objreg.SetErrorLog("AlZajelMobileIntegrationLog.log", "FailedList Message for Voucher No '" + DocNo + "'");
                                            objreg.SetLog2("AlZajelMobileIntegrationLog", "FailedList Message for Voucher No '" + DocNo + "'");
                                        }
                                    }
                                }
                            }

                            #endregion
                        }
                        #endregion
                    }
                    else if (LinkType == "Mobile Others")
                    {
                        #region Mobile Others
                        AccessToken = GetAccessToken();
                        if (AccessToken == "")
                        {
                            Message = "Invalid Token";
                            objreg.SetLog("Invalid Token");
                            objreg.SetLog2("AlZajelMobileIntegrationLog", Message);
                            //FConvert.LogFile("AlZajelMenuCategoryFailed.log", DateTime.Now.ToString() + " : Invalid Token");
                            var sMessage = "Token Should not be Empty";
                            objreg.SetLog(sMessage);
                            objreg.SetLog2("AlZajelMobileIntegrationLog", sMessage);
                            return Json(new { success = "1" }, JsonRequestBehavior.AllowGet);
                        }

                        StockList clist = new StockList();
                        clist.AccessToken = AccessToken;
                        clist.ObjectType = "vanStock";

                        #region VoucherDetails
                        string query = String.Format(@"select sVoucherNo,iTransactionId,ouf.sCode [OutletFrom],outt.sCode [OutletTo],iProduct,u.sCode [Productunit] ,sBatchNo,
                                                        case when b.iExpiryDate=0 then '0'  else convert(varchar,dbo.IntToDate(b.iExpiryDate),23) end [ExpiryDate],
                                                        abs(fQuantity) Quantity,ISNULL(sNarration,'') Narration,GETDATE() [TransactionDate],
                                                        case when muo.OutletTypeID=0 then 'Warehouse' else 'VAN' end FromOutletType,
                                                        case when muto.OutletTypeID=0 then 'Warehouse' else 'VAN' end ToOutletType
                                                        from tCore_Header_0 h 
                                                        join tCore_Data_0 d on h.iHeaderId=d.iHeaderId
                                                        join tCore_Indta_0 i on i.iBodyId=d.iBodyId
                                                        join tCore_HeaderData5383_0 hd on hd.iHeaderId=h.iHeaderId
                                                        join tCore_Data_Tags_0 dt on dt.iBodyId=d.iBodyId
                                                        join mCore_OutletFrom ouf on ouf.iMasterId=iTag3028 and iStatus<>5
                                                        join mCore_OutletTo outt on outt.iMasterId=iTag3029  and outt.iStatus<>5
                                                        join mPos_Outlet ofr on ofr.sName=ouf.sName and ofr.iStatus<>5
                                                        join muPos_Outlet muo on muo.iMasterId=ofr.iMasterId
                                                        join mPos_Outlet oto on oto.sName=outt.sName and oto.iStatus<>5
                                                        join muPos_Outlet muto on muto.iMasterId=oto.iMasterId
                                                        join mCore_Units u on u.iMasterId=iUnit  and u.iStatus<>5
                                                        left join tCore_Batch_0 b on b.iBodyId=d.iBodyId
                                                        where iVoucherType=5383 and sVoucherNo= '{0}'", DocNo);
                        List<StockDetailsData> lc = new List<StockDetailsData>();
                        DataSet ds = objreg.GetData(query, CompanyId, ref errors1);

                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            var CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
                            StockDetailsData c = new StockDetailsData();
                            c.SourceStockCode = Convert.ToString(row["OutletFrom"]);
                            c.DestinationStockCode = Convert.ToString(row["OutletTo"]);
                            c.ProductId = Convert.ToInt32(row["iProduct"]);
                            c.ProductUnit = Convert.ToString(row["Productunit"]);
                            c.BatchId = Convert.ToString(row["sBatchNo"]);
                            if (Convert.ToString(row["ExpiryDate"]) == "0")
                            {
                                c.ExpiryDate = null;
                            }
                            else
                            {
                                c.ExpiryDate = Convert.ToString(row["ExpiryDate"]);
                            }
                            c.Qty = Convert.ToDecimal(row["Quantity"]);
                            c.TransactionDateTime = Convert.ToString(CurrentDate);
                            c.Comments = Convert.ToString(row["Narration"]);
                            if (Convert.ToString(row["FromOutletType"]) == "Warehouse" & Convert.ToString(row["ToOutletType"]) == "VAN")
                            {
                                c.Type = 2;
                            }
                            else if (Convert.ToString(row["FromOutletType"]) == "VAN" & Convert.ToString(row["ToOutletType"]) == "Warehouse")
                            {
                                c.Type = 3;
                            }
                            else if (Convert.ToString(row["FromOutletType"]) == "VAN" & Convert.ToString(row["ToOutletType"]) == "VAN")
                            {
                                c.Type = 6;
                            }
                            lc.Add(c);
                        }
                        #endregion

                        clist.ItemList = lc;

                        if (clist.ItemList.Count() > 0)
                        {
                            #region PostingSection

                            var sContent = new JavaScriptSerializer().Serialize(clist);
                            using (WebClient client = new WebClient())
                            {
                                client.Headers.Add("Content-Type", "application/json");
                                objreg.SetSuccessLog("AlZajelMobileIntegrationLog.log", "Mobile Request-Others Posted sContent: " + sContent);
                                objreg.SetLog2("AlZajelMobileIntegrationLog", "Mobile Request-Others Posted sContent: " + sContent);

                                objreg.SetLog2("AlZajelMobileIntegrationLog", " PostingURL :" + PostingURL);
                                var arrResponse = client.UploadString(PostingURL, sContent);
                                //var arrResponse = client.UploadString("http://" + serverip + "/iddemo/idm.api/integration/upsertDataList", sContent);
                                //var arrResponse = client.UploadString("http://185.151.4.210/iddemo/idm.api/integration/upsertDataList", sContent);
                                var lng = JsonConvert.DeserializeObject<Result>(arrResponse);

                                if (lng.ResponseStatus.IsSuccess == true)
                                {
                                    foreach (var item in clist.ItemList)
                                    {
                                        FConvert.LogFile("AlZajelMobileIntegrationLog.log", DateTime.Now.ToString() + " : " + "Data posted / updated to mobile app device for Voucher:- " + DocNo);
                                    }
                                    Message = "Posted Successfully";
                                }
                                else
                                {
                                    int ErrorListCount = lng.ErrorMessages.Count();
                                    if (ErrorListCount > 0)
                                    {
                                        var ErrorList = lng.ErrorMessages.ToList();
                                        foreach (var item in ErrorList)
                                        {
                                            objreg.SetErrorLog("AlZajelMobileIntegrationLog.log", "ErrorList Message for Voucher No '" + DocNo + "' data as :- " + item);
                                            objreg.SetLog2("AlZajelMobileIntegrationLog", "ErrorList Message for Voucher No '" + DocNo + "' data as :- " + item);
                                        }
                                    }

                                    int FailedListCount = lng.FailedList.Count();
                                    if (FailedListCount > 0)
                                    {
                                        var FailedList = lng.FailedList.ToList();
                                        foreach (var item in FailedList)
                                        {
                                            objreg.SetErrorLog("AlZajelMobileIntegrationLog.log", "FailedList Message for Voucher No '" + DocNo + "' as :" + item);
                                            objreg.SetLog2("AlZajelMobileIntegrationLog", "FailedList Message for Voucher No '" + DocNo + "' as :" + item);
                                        }
                                    }
                                }
                            }

                            #endregion
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                objreg.SetLog2("AlZajelMobileIntegrationLog", "Exception Message for Voucher No '" + DocNo + "' as :" + Message);
            }
            return Json(new { Message = Message }, JsonRequestBehavior.AllowGet);
        }

        public string GetAccessToken()
        {
            string AccessToken = "";
            Datum datanum = new Datum();
            datanum.CompanyCode = CompanyCode;
            datanum.Identifier = Identifier;
            datanum.Secret = Secret;
            datanum.Lng = Lng;
            string sContent = JsonConvert.SerializeObject(datanum);
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                var sMessage = " AccessTokenURL :" + AccessTokenURL;
                objreg.SetLog2("AlZajelMobileIntegrationLog", " AccessTokenURL :" + AccessTokenURL);
                var arrResponse = client.UploadString(AccessTokenURL, sContent);

                Resultlogin lng = JsonConvert.DeserializeObject<Resultlogin>(arrResponse);

                AccessToken = lng.AccessToken;
                if (lng.AccessToken == null || lng.AccessToken == "" || lng.AccessToken == "-1")
                {
                    return AccessToken;
                }
                else
                {
                }
            }

            return AccessToken;
        }
    }
}