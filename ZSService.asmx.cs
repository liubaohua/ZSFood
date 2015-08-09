using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Data;
using UFIDA.U8.U8APIFramework;
using UFIDA.U8.U8APIFramework.Parameter;
using UFIDA.U8.U8MOMAPIFramework;
using System.Text;
using UFIDA.U8.U8APIFramework.Meta;
using System.Xml;
using System.IO;
using MyApp.Service1;
using System.Net;
using MSXML2;


namespace MyWebApp
{
    /// <summary>
    /// MyServer 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://zsservice.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class MyServer : System.Web.Services.WebService
    {
        U8Login.clsLogin ulogin = null;

        private SqlConnection sqlConnection1 = null;
        private static String ConnString = null;

        public DataTable getSqlData(string sql)
        {
            initConn();
            SqlCommand cmdSelect = new SqlCommand(sql, this.sqlConnection1);
            this.sqlConnection1.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmdSelect);
            System.Data.DataTable dt = new System.Data.DataTable();
            da.Fill(dt);
            this.sqlConnection1.Close();
            return dt;
        }

        private void initConn()
        {
            if(sqlConnection1 == null)
                sqlConnection1 = new SqlConnection();
        }

        private void setConnStr(string s)
        {
            initConn();//u8Login.UfDbName
            s = s.Replace("PROVIDER=SQLOLEDB;", "");
            ConnString = s;
            sqlConnection1.ConnectionString = s;
        }


        String ReadXmlData(String ElementName, String ElementName2)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("c:\\Config.xml");
            XmlNode root = doc.DocumentElement[ElementName];
            if (root != null && root.SelectSingleNode(ElementName2) != null)
                return root.SelectSingleNode(ElementName2).InnerText;
            return "";
        }

        private string PostData(string url,string content)
        {
            try
            {
                if (url == null || url.Length == 0)
                    url = "http://172.16.11.1/freshBeef/ws/zhengShanWs";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                request.ContentLength = bytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                requestStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(requestStream);
                string resultStr = reader.ReadToEnd();
                reader.Close();
                requestStream.Close();
                response.Close();
                return resultStr;
            }
            catch (Exception e)
            {
                return e.Message;
                //throw;
            }
        }

        private void ConcatXml(StringBuilder sb,DataRow dr,string tagname,string colname)
        {
            sb.Append("<" + tagname + ">");
            sb.Append(dr[colname].ToString());
            sb.Append("</" + tagname + ">");
        }

        [WebMethod]
        public string InvokeU8Api(string method,string content)
        {
            string retstr = "";
            try
            {
                string AccCode = ReadXmlData("Detail", "AccCode");
                string User = ReadXmlData("Detail", "User");
                string Password = ReadXmlData("Detail", "Password");
                string Server = ReadXmlData("Detail", "Server");


                ulogin = new U8Login.clsLogin();
                U8Login.clsLogin u8Login = new U8Login.clsLogin();
                // string taskid = clslogin.GetTaskID("DP");
                String sSubId = "DP";//AS DP
                String sAccId = AccCode;
                String sYear = DateTime.Now.Year.ToString();
                String sUserID = User;
                String sPassword = Password;
                String sDate = DateTime.Now.ToShortDateString();
                String sServer = Server;//USER-20150630LA
                String sSerial = "";
                if (u8Login.Login(ref sSubId, ref sAccId, ref sYear, ref sUserID, ref sPassword, ref sDate, ref sServer, ref sSerial))
                {
                    setConnStr(u8Login.UfDbName);
                    //retstr = "login ok!";
                }
                else
                {
                    retstr = u8Login.ShareString;
                    Marshal.FinalReleaseComObject(u8Login);
                    return retstr;
                }

                string ServiceURL = ReadXmlData("Detail", "ServiceURL");
                string ServiceName = ReadXmlData("Detail", "ServiceName");

                if (method.Equals("InventoryQTY"))//对方来调 
                {
                    DataTable dt = getSqlData("select cinvcode,cwhcode,iquantity,fAvaQuantity from currentstock where cinvcode='" + content + "'");
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<?xml version='1.0' encoding='UTF-8'?>   <DATA>  ");
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            sb.Append("<RECORD>");

                            sb.Append("<CINVCODE>");
                            sb.Append(dt.Rows[i][0].ToString());
                            sb.Append("</CINVCODE>");

                            sb.Append("<CWHCODE>");
                            sb.Append(dt.Rows[i][1].ToString());
                            sb.Append("</CWHCODE>");

                            sb.Append("<IQUANTITY>");
                            sb.Append(dt.Rows[i][2].ToString());
                            sb.Append("</IQUANTITY>");

                            sb.Append("<FAVAQUANTITY>");
                            sb.Append(dt.Rows[i][3].ToString());
                            sb.Append("</FAVAQUANTITY>");

                            sb.Append("</RECORD>");
                        }
                        sb.Append("</DATA>");
                        return sb.ToString();
                    }
                    else
                        return "查不到现存量信息";
                }
                if (method.Equals("Inventory"))//U8触发器调用，再调用对方ws
                {
                    DataTable dt = getSqlData("select cinvcode,cinvname,cinvstd,ccomunitcode,cinvccode,cbarcode,'0' as cisgift,'0' as gd_is_gift,iinvsalecost from inventory with(nolock) where cinvcode='" + content + "'");
                    DataRow dr = dt.Rows[0];

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<DATA><RECORD>");
                    ConcatXml(sb, dr, "CINVCODE", "cinvcode");
                    ConcatXml(sb, dr, "CINVNAME", "cinvname");
                    ConcatXml(sb, dr, "CINVSTD", "cinvstd");
                    ConcatXml(sb, dr, "CINVCCODE", "cinvccode");
                    ConcatXml(sb, dr, "CCOMUNITCODE", "ccomunitcode");
                    ConcatXml(sb, dr, "CBARCODE", "cbarcode");
                    ConcatXml(sb, dr, "CISGIFT", "cisgift");
                    ConcatXml(sb, dr, "iInvSaleCost", "iInvSaleCost");
                    ConcatXml(sb, dr, "gd_is_gift", "gd_is_gift");
                    sb.Append("</RECORD>");
                    sb.Append("</DATA>");
                    ZSNS.ZhengShanWsIClient client = new ZSNS.ZhengShanWsIClient(ServiceName,ServiceURL);
                    return client.inventory(sb.ToString());
                    //return sb.ToString();
                    //return "SUCC";
                }
                if (method.Equals("InventoryClass"))//暂时 不使用 
                {
                    DataTable dt = getSqlData("select CINVCCODE,CINVCNAME,(select cInvCCode from InventoryClass b with(nolock) where b.iInvCGrade+1=h.iInvCGrade and LEFT(h.cinvccode,LEN(b.cinvccode))=b.cinvccode) as CINVCCODE_F from inventoryclass h with(nolock)");
                    string str = "";
                    str += "<DATA>";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        str += "<RECORD>";
                        str += GetXmlTag("CINVCCODE", dt.Rows[i]["CINVCCODE"].ToString());
                        str += GetXmlTag("CINVCNAME", dt.Rows[i]["CINVCCODE"].ToString());
                        str += GetXmlTag("CINVCCODE_F", dt.Rows[i]["CINVCCODE_F"].ToString());
                        str += "</RECORD>";
                    }
                    str += "</DATA>";
                    ZSNS.ZhengShanWsIClient client = new ZSNS.ZhengShanWsIClient(ServiceName, ServiceURL);
                    return client.InventoryClass(str);
                }
                if (method.Equals("ST_AppTransVouchAdd"))//本地U8调拨申请单审核时触发
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine("select h.cTVCode as vouchid,h.dtvdate as vouchdate,ow.cwhname as owhname,");
                    sql.AppendLine("iw.cwhname as iwhname,p.cPersonname as Person,");
                    sql.AppendLine("b.cinvcode,i.cinvname,b.fSaleCost as unitprice,b.itvquantity as qty,b.cbmemo as memo ");
                    sql.AppendLine("from ST_AppTransVouch h with(nolock) inner join ST_AppTransVouchs b with(nolock) on h.ID = b.ID");
                    sql.AppendLine("inner join Inventory i with(nolock) on i.cInvCode = b.cInvcode ");
                    sql.AppendLine("inner join warehouse ow with(nolock) on ow.cwhcode = h.cOWhCode");
                    sql.AppendLine("inner join warehouse iw with(nolock) on iw.cwhcode = h.cIWhCode");
                    sql.AppendLine("left join person p with(nolock) on p.cPersoncode = h.cPersonCode ");
                    sql.AppendLine("where h.cTVCode='"+content+"'");

                    DataTable dt = getSqlData(sql.ToString());
                    if (dt.Rows.Count == 0)
                        return "无此调拨申请单号"+content;
                    DataRow dr = dt.Rows[0];
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<RECORD>");
                    sb.Append("<ORDER>");
                    sb.Append("<ORDERID>");
                    sb.Append("<HEADER>");
                    ConcatXml(sb, dr, "VOUCHID", "vouchid");
                    ConcatXml(sb, dr, "VOUCHDATE", "vouchdate");
                    ConcatXml(sb, dr, "OWHNAME", "owhname");
                    ConcatXml(sb, dr, "IWHNAME", "iwhname");
                    ConcatXml(sb, dr, "PERSON", "person");

                    sb.Append("</HEADER>");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        sb.Append("<DETAIL>");
                        ConcatXml(sb, dt.Rows[i], "CINVCODE", "cinvcode");
                        ConcatXml(sb, dt.Rows[i], "CINVNAME", "cinvname");
                        ConcatXml(sb, dt.Rows[i], "UNITPRICE", "unitprice");
                        ConcatXml(sb, dt.Rows[i], "QTY", "qty");
                        ConcatXml(sb, dt.Rows[i], "MEMO", "memo");
                        sb.Append("</DETAIL>");
                    }
                    sb.Append("</ORDERID>");
                    sb.Append("</ORDER>");
                    sb.Append("</RECORD>");
                    sb.Append("</DATA>");
                    ZSNS.ZhengShanWsIClient c = new ZSNS.ZhengShanWsIClient(ServiceName, ServiceURL);
                    return c.ST_AppTransVouchAdd(sb.ToString());
                }

                string api = "";
                if (method.Equals("TransVouchAdd"))//调拨单生成
                {
                    api = "U8API/TransVouch/Add";
                }
                else if (method.Equals("audit"))//调拨单审核
                {
                    api = "U8API/TransVouch/Delete";
                }
                else if (method.Equals("load"))//调拨申请单查询，废弃
                {
                    api = "U8API/TransRequestVouch/Load";
                }else
                if (method.Equals("OutboundOrderAdd"))
                    api = "U8API/saleout/Add";
                if (method.Equals("OutboundOrderDone"))
                    api = "U8API/saleout/CancelAudit";//Delete
                


                U8EnvContext envContext = new U8EnvContext();
                envContext.U8Login = u8Login;

                //string method = "audit";
                
                U8ApiAddress myApiAddress = new U8ApiAddress(api);
                U8ApiBroker broker = new U8ApiBroker(myApiAddress, envContext);
                MSXML2.DOMDocument domMsg = new MSXML2.DOMDocument();
                if (method.Equals("TransVouchAdd"))//基本完成
                {
                    broker.AssignNormalValue("sVouchType", "12");

                    BusinessObject DomHead = broker.GetBoParam("DomHead");
                    DomHead.RowCount = 1;

                    XmlDocument xml = new XmlDocument();
                    //xml.LoadXml(content.Trim());
                    xml.Load("c:\\DATA.xml");

                    XmlNodeList xnList = xml.SelectNodes("/DATA/ORDER/ORDERID/DETAIL");
                    foreach (XmlNode xn in xnList)
                    {
                        retstr += "开始读取XML";
                        string VOUCHID = xn["VOUCHID"].InnerText;
                        string VOUCHDATE = xn["VOUCHDATE"].InnerText;
                        
                        //DomHead[0]["id"] = "2"; //主关键字段，int类型
                        DomHead[0]["ctvcode"] = VOUCHID; //单据号，string类型
                        DomHead[0]["dtvdate"] = VOUCHDATE;  //日期，DateTime类型
                        DataTable dt = getSqlData("select cwhcode from warehouse where cwhname='" + xn["OWHNAME"].InnerText+"'");
                        if (dt.Rows.Count > 0)
                            DomHead[0]["cowhcode"] = dt.Rows[0][0].ToString();
                        dt = getSqlData("select cwhcode from warehouse where cwhname='" + xn["IWHNAME"].InnerText + "'");
                        if (dt.Rows.Count > 0)
                            DomHead[0]["ciwhcode"] = dt.Rows[0][0].ToString();

                        DomHead[0]["cwhname"] = xn["OWHNAME"].InnerText; //转出仓库，string类型
                        DomHead[0]["cwhname_1"] = xn["IWHNAME"].InnerText; //转入仓库，string类型

                        

                        //DomHead[0]["ciwhcode"] = "2";//转入仓库编码，string类型
                        //DomHead[0]["cowhcode"] = "1"; //转出仓库编码，string类型


                        //DomHead[0]["cordcode"] = "2"; //出库类别编码，string类型
                        //DomHead[0]["cirdcode"] = "1"; //入库类别编码，string类型

                        //DomHead[0]["crdname_1"] = "销售出库";//出库类别
                        //DomHead[0]["crdname"] = "采购入库";//入库类别

                        //DomHead[0]["cdepname_1"] = "采购部";
                        //DomHead[0]["cdepname"] = "销售部";
                        //DomHead[0]["codepcode"] = "1";
                        //DomHead[0]["cidepcode"] = "2";

                        //DomHead[0]["cpersoncode"] = "1";
                        DomHead[0]["cpersonname"] = xn["PERSON"].InnerText;//经手人
                        dt = getSqlData("select cPersonCode from Person where cPersonName='" + xn["PERSON"].InnerText + "'");
                        if (dt.Rows.Count > 0)
                            DomHead[0]["cpersoncode"] = dt.Rows[0][0].ToString();

                        //DomHead[0]["iamount"] = "";//现存量
                        DomHead[0]["dnmaketime"] = DateTime.Now;
                        DomHead[0]["ctvmemo"] = xn["MEMO"].InnerText;
                        DomHead[0]["cinvname"] = xn["CINVNAME"].InnerText;
                        //DomHead[0]["iavaquantity"] = "80"; 可用量

                        DomHead[0]["csource"] = "1"; //1 -- 库存 2 -- 零售 3 -- 预留
                        DomHead[0]["cmaker"] = User;
                        DomHead[0]["csource"] = "1";
                        DomHead[0]["itransflag"] = "正向";
                        DomHead[0]["vt_id"] = 89;
                        DomHead[0]["dnmaketime"] = DateTime.Now.ToLongDateString();
                        //DomHead[0]["ufts"] = "                      275.5169";
                        DomHead[0]["btransflag"] = false;


                        BusinessObject domBody = broker.GetBoParam("domBody");
                        domBody.RowCount = 1;

                        //domBody[0]["autoid"] = "2"; //主关键字段，int类型
                        domBody[0]["cinvcode"] = xn["CINVCODE"].InnerText;//存货编码，string类型
                        domBody[0]["cinvname"] = xn["CINVNAME"].InnerText;
                        //domBody[0]["cinvstd"] = "";


                        domBody[0]["itvquantity"] =getDouble(xn["QTY"].InnerText); //数量，double类型 

                        DataTable dt3 = getSqlData("select cComUnitCode from inventory where cinvcode='" + xn["CINVCODE"].InnerText + "'");

                        //domBody[0]["itvnum"] = 0.1;


                        domBody[0]["ctvbatch"] = "";
                        if (dt3.Rows.Count > 0)
                        {
                            domBody[0]["cinvm_unit"] = dt3.Rows[0][0].ToString();
                            //domBody[0]["cinva_unit"] = "4";
                            //domBody[0]["cassunit"] = "4";
                        }
                        
                        domBody[0]["iexpiratdatecalcu"] = 0;
                        domBody[0]["issotype"] = 0;
                        domBody[0]["idsotype"] = 0;
                        domBody[0]["isoseq"] = "";
                        domBody[0]["idsoseq"] = "";
                        domBody[0]["issodid"] = "";
                        domBody[0]["idsodid"] = "";
                        domBody[0]["cinvaddcode"] = "";
                        domBody[0]["corufts"] = "                              ";
                        domBody[0]["cdsocode"] = "";
                        domBody[0]["csocode"] = "";


                        domBody[0]["bcosting"] = "1";
                        domBody[0]["cposition"] = "";

                        //domBody[0]["iinvexchrate"] = 100;
                        domBody[0]["ctvcode"] = VOUCHID;
                        domBody[0]["fsalecost"] = getDouble(xn["UNITPRICE"].InnerText) * getDouble(xn["QTY"].InnerText);
                        domBody[0]["fsaleprice"] = getDouble(xn["UNITPRICE"].InnerText);
                        //domBody[0]["itvpcost"] = getDouble(xn["itvpcost"].InnerText);
;
                        domBody[0]["itvaprice"] = 0;
                        domBody[0]["itvpprice"] = 0;
                        domBody[0]["itvacost"] = 0;

                        //domBody[0]["igrossweight"] = "3";
                        //domBody[0]["inetweight"] = "1";

                        domBody[0]["editprop"] = "A";
                        retstr += "读取XML完成";
                    }

                    broker.AssignNormalValue("domPosition", null);
                    broker.AssignNormalValue("errMsg", "");
                    broker.AssignNormalValue("cnnFrom", null);
                    broker.AssignNormalValue("VouchId", "");
                    broker.AssignNormalValue("domMsg", domMsg);

                    broker.AssignNormalValue("bCheck", false);
                    broker.AssignNormalValue("bBeforCheckStock", false);
                    broker.AssignNormalValue("bIsRedVouch", false);
                    broker.AssignNormalValue("sAddedState", "");
                    broker.AssignNormalValue("bReMote", false);

                }
                if (method.Equals("OutboundOrderAdd"))
                {
                    broker.AssignNormalValue("sVouchType", "32");//新增0 修改1
                    broker.AssignNormalValue("vNewID", "");
                }
                if (method.Equals("OutboundOrderDone"))//出库单审核
                {
                    
                    DataTable dt = getSqlData("select CONVERT(money,ufts) as ufts from rdrecord32 where cCode='" + content + "'");
                    double ufts = 0;
                    if (dt.Rows.Count > 0)
                        ufts = double.Parse(dt.Rows[0][0].ToString());
                    else
                        return "找不到此销售出库单号:" + content;
                    broker.AssignNormalValue("sVouchType", "32");
                    broker.AssignNormalValue("VouchId", content);//单据号
                    broker.AssignNormalValue("errMsg", "");
                    broker.AssignNormalValue("cnnFrom", null);
                    broker.AssignNormalValue("TimeStamp", String.Format("{0,30}", String.Format("{0:0.0000}", ufts)));// "                      275.5210"     
                    broker.AssignNormalValue("domMsg", domMsg);//new MSXML2.DOMDocument()
                    broker.AssignNormalValue("bCheck", false);
                    broker.AssignNormalValue("bBeforCheckStock", false);
                    broker.AssignNormalValue("bList", false);
                    //删除单据不需要下面三个参数
                    //broker.AssignNormalValue("MakeWheres", null);
                    //broker.AssignNormalValue("sWebXml", "");
                    //broker.AssignNormalValue("oGenVouchIds", null);
                }
                if (method.Equals("audit"))//调拨单审核，未解决
                {
                    DataTable dt = getSqlData("select CONVERT(money,ufts) as ufts from transvouch where cTVCode='" + content + "'");
                    double ufts = 0;
                    //string ts = "";
                    if (dt.Rows.Count > 0)
                    {
                        //DataTable dt2 = getSqlData("select convert(timestamp,CONVERT(money," + String.Format("{0,30}", String.Format("{0:0.0000}", ufts)) + ")) as ufts ");
                        ufts = double.Parse(dt.Rows[0][0].ToString());
                        //ts = dt2.Rows[0][0].ToString();
                    }
                    else
                        return "找不到此调拨单号:"+content;
                    broker.AssignNormalValue("sVouchType", "12");
                    broker.AssignNormalValue("VouchId", content);//单据号
                    broker.AssignNormalValue("errMsg", "");
                    broker.AssignNormalValue("cnnFrom", null);
                    broker.AssignNormalValue("TimeStamp", String.Format("{0,30}", String.Format("{0:0.0000}", ufts)));// "                      275.5210"  
                    broker.AssignNormalValue("domMsg", domMsg);//new MSXML2.DOMDocument()
                    broker.AssignNormalValue("bCheck", false);
                    broker.AssignNormalValue("bBeforCheckStock", false);
                    broker.AssignNormalValue("bList", false);

                    //broker.AssignNormalValue("MakeWheres", null);
                    //broker.AssignNormalValue("sWebXml", "");
                    //broker.AssignNormalValue("oGenVouchIds", null);
                }
                if (method.Equals("load"))//deprecated. not in use
                {
                    broker.AssignNormalValue("sVouchType", "62");
                    broker.AssignNormalValue("sWhere", " VouchId='0000000001'");
                    broker.AssignNormalValue("bGetBlank", false);
                    broker.AssignNormalValue("sBodyWhere_Order", "cInvcode");
                    broker.AssignNormalValue("errMsg", "");
                    broker.AssignNormalValue("domPos", domMsg);
                    BusinessObject obj = broker.GetBoParam("domHead");
                    BusinessObject obj1 = broker.GetBoParam("domBody");
                }
                retstr += "开始调用";
                if (!broker.Invoke())
                {
                    retstr += "调用失败:";
                    //错误处理
                    Exception apiEx = broker.GetException();
                    if (apiEx != null)
                    {
                        if (apiEx is MomSysException)
                        {
                            MomSysException sysEx = apiEx as MomSysException;
                            //Console.WriteLine("系统异常：" + sysEx.Message);
                            retstr = sysEx.Message + "[MomSysException]";
                            //todo:异常处理
                        }
                        else if (apiEx is MomBizException)
                        {
                            MomBizException bizEx = apiEx as MomBizException;
                            //Console.WriteLine("API异常：" + bizEx.Message);
                            retstr = bizEx.Message + "[MomBizException]";
                            //todo:异常处理
                        }
                    }
                    //结束本次调用，释放API资源
                    broker.Release();
                    //return;
                }
                else
                {
                    retstr += "调用成功：";
                    if (method.Equals("load"))
                    {
                        System.String result = broker.GetReturnValue() as System.String;
                        if (string.IsNullOrEmpty(result))
                        {
                            retstr += "加载调拨申请单成功！";
                            //Console.WriteLine("加载销售订单成功！");

                            //获取out/inout参数值
                            //MSXML2.XMLDocument xmlResult = broker.GetResult("domHead") as MSXML2.XMLDocument;
                            MSXML2.DOMDocumentClass xmlHead = broker.GetResult("DomHead") as MSXML2.DOMDocumentClass;
                            xmlHead.save("TransRequestVouchHead.xml");


                            //out参数domBody为BO对象(表体)，此BO对象的业务类型为销售订单。BO参数均按引用传递，具体请参考服务接口定义
                            //如果要取原始的XMLDOM对象结果，请使用GetResult()
                            MSXML2.DOMDocumentClass xmlBody = broker.GetResult("domBody") as MSXML2.DOMDocumentClass;
                            xmlBody.save("TransRequestVouchBody.xml");
                        }
                        else
                        {
                            retstr += "加载调拨申请单失败！";
                            //Console.WriteLine("加载销售订单失败！");
                        }
                    }
                    else
                    {
                        DOMDocument doc = (DOMDocument)broker.GetResult("domMsg");
                        if (doc != null)
                            retstr += doc.text + "|";
                        retstr += broker.GetResult("errMsg") + "|" + broker.GetResult("retval").ToString();
                    }
                }
            }
            catch (Exception e)
            {
                retstr += "异常:" + e.Message+"\n"+e.StackTrace;
                return retstr;
                //throw;
            }
            return retstr;
        }

        private string GetXmlTag(string tagname,string value)
        {
            return "<"+tagname+">"+value+"</"+tagname+">";
        }

        private string IsNull(object obj)
        {
            if (obj == null) return "";
            return obj.ToString();
        }

        private double getDouble(string str)
        {
            if (str == null || str.Trim().Equals(""))
                return 0;
            return double.Parse(str.Trim());
        }

        [WebMethod]
        public int Add(int i, int j)
        {
            return i + j;
        }

        [WebMethod]
        public string ToUpper(string src)
        {
            return src.ToUpper();
        }
        
        [WebMethod]
        public string Test()
        {
            return "HellWorldWS";
        }


    }
}
