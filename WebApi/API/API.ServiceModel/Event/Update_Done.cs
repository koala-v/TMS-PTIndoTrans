using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack;
using ServiceStack.ServiceHost;
using ServiceStack.OrmLite;
using WebApi.ServiceModel.Tables;

namespace WebApi.ServiceModel.Event
{
    [Route("/event/action/update/done", "Get")]
    public class Update_Done : IReturn<CommonResponse>
    {
        public string JobNo { get; set; }
        public int JobLineItemNo { get; set; }
        public int LineItemNo { get; set; }
        public string DoneFlag { get; set; }
        public DateTime DoneDateTime { get; set; }
        public string Remark { get; set; }
        public string ContainerNo { get; set; }
        public string PhoneNumber { get; set; }
    }
    public class Update_Done_Logic
    {
        public IDbConnectionFactory DbConnectionFactory { get; set; }
        public int UpdateDone(Update_Done request) 
        {
            int Result = -1;
            List<Jmjm4> ResultJmjm4 = null;
            List<Jmje2> ResultJmje2 = null;
            string strEventCode = "";
            string strSqlJmje2 = "";
            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection("TMS"))
                {
                    if (request.DoneDateTime != DateTime.MinValue)
                    {
                        Result = db.Update<Jmjm4>(new { DoneDateTime = request.DoneDateTime, DoneFlag = request.DoneFlag, Remark = request.Remark, ContainerNo = request.ContainerNo }, p => p.JobNo == request.JobNo && p.JobLineItemNo == request.JobLineItemNo && p.LineItemNo == request.LineItemNo);
                    }
                    else
                    {
                        Result = db.Update<Jmjm4>(new { DoneFlag = request.DoneFlag, Remark = request.Remark, ContainerNo = request.ContainerNo }, p => p.JobNo == request.JobNo && p.JobLineItemNo == request.JobLineItemNo && p.LineItemNo == request.LineItemNo);  
                    }
																				if (Result > 0)
																				{
																								InsertContainerNo(request);
																				}




                    string strSql = "select (Select  EventCode From jmjm3 where jmjm3.jobno=jmjm4.jobno and jmjm3.LineItemNo =jmjm4.JobLineItemNo ) as EventCode,* from jmjm4 where jobno ='" + request.JobNo + "'And JobLineItemNo='"+request.JobLineItemNo +"' And LineItemNo='"+request.LineItemNo+"' and PhoneNumber='" + request.PhoneNumber + "' ";
                    ResultJmjm4 = db.Select<Jmjm4>(strSql);
                    if (ResultJmjm4.Count > 0)
                    {
                        strEventCode = Modfunction.CheckNull(ResultJmjm4[0].EventCode);
                          strSqlJmje2 = "Select  InsertNextEventCode ,InsertNextEventGroup From jmje2 Where EventCode='" + strEventCode + "' And LineItemNo in (Select EventLineItemNo From Jmjm4 Where  jobno ='" + request.JobNo + "'And JobLineItemNo='" + request.JobLineItemNo + "' And LineItemNo='" + request.LineItemNo + "' )";
                              ResultJmje2  = db.Select<Jmje2>(strSqlJmje2);
                        if (ResultJmje2.Count > 0)
                        {
                            if (Modfunction.CheckNull(ResultJmje2[0].InsertNextEventCode) == "" && Modfunction.CheckNull(ResultJmje2[0].InsertNextEventGroup) == "")
                           {

                           }
                            else
                           {
                                   string strUpdateStautsCode;
                                   strUpdateStautsCode = "Update  Jmjm3 Set StatusCode ='CMP' Where JobNo ='" + request.JobNo + "' And LineItemNo ='"+request.JobLineItemNo+"'";
                                   db.ExecuteSql(strUpdateStautsCode);
                           }
                        }
                    }


                    //for (int intI = 0; intI < ResultJmjm4.Count; intI++)
                    //{
                    //    if (ResultJmjm4[intI].DoneFlag != "Y")
                    //    {
                    //        blnDoneFlag = false;
                    //        break;
                    //    }
                    //    else
                    //    {
                    //    }
                    //}

                    //if (blnDoneFlag == true)
                    //{
                    //    string strItemName="";
                    //    string strEventCode = "";
                    //    string strSqlJmje2 = "";
                    //    for (int intI = 0; intI < ResultJmjm4.Count; intI++)
                    //    {
                    //        strItemName = Modfunction.CheckNull(ResultJmjm4[intI].ItemName);
                    //        strEventCode = Modfunction.CheckNull(ResultJmjm4[intI].EventCode);
                    //        strSqlJmje2 = "Select  InsertNextEventCode ,InsertNextEventGroup From jmje2 Where EventCode='" + strEventCode + "' And ItemName='" + strItemName + "'";
                    //        ResultJmje2  = db.Select<Jmje2>(strSqlJmje2);
                    //        if (Modfunction.CheckNull(ResultJmje2[0].InsertNextEventCode)=="" && Modfunction.CheckNull(ResultJmje2[0].InsertNextEventGroup) == "")
                    //        {

                    //        }
                    //        else
                    //        {
                    //            string strUpdateStautsCode;
                    //            strUpdateStautsCode = "Update  Jmjm3 Set StatusCode ='Complete' Where JobNo ='" + request.JobNo + "' And LineItemNo ='"+request.JobLineItemNo+"'";
                    //            db.ExecuteSql(strUpdateStautsCode);
                    //        }
                    //    }

                    //}


                }

            }
            catch { throw; } 
            return Result;
        }
        public long InsertContainerNo(Update_Done request)
        {
            long Result = -1;
            try
            {
                if (string.IsNullOrEmpty(request.ContainerNo) || request.ContainerNo.Length < 1)
                {
                    return Result;
                }
                using (var db = DbConnectionFactory.OpenDbConnection("TMS"))
                {                    
                    Result = db.Scalar<int>(
                        "Select count(*) From Jmjm6 Where Jmjm6.JobNo={0} And jmjm6.ContainerNo={1}",request.JobNo,request.ContainerNo
                    );
                    if (Result < 1)
                    {
                        int count = db.Scalar<int>(
                            "Select count(*) From Jmjm6 Where Jmjm6.JobNo={0}",request.JobNo
                        );
                        db.InsertParam<Jmjm6>(new Jmjm6 { JobNo = request.JobNo, LineItemNo = count + 1, ContainerNo = request.ContainerNo });
                        Result = 0;
                    }
                    else { Result = -1; }
                }
            }
            catch { throw; }
            return Result;
        }
    }
}
