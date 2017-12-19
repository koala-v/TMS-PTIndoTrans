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
            List<Jmjm4> ResultJmjm4DoneFlag = null;
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
                                bool BlnDoneFlag = true; 
                                string strJmjm4DoneFlag = "select DoneFlag  from jmjm4 where jobno ='" + request.JobNo + "'And JobLineItemNo='" + request.JobLineItemNo + "' and PhoneNumber='" + request.PhoneNumber + "' ";
                                ResultJmjm4DoneFlag = db.Select<Jmjm4>(strJmjm4DoneFlag);
                                if (ResultJmjm4DoneFlag.Count > 0)
                                {
                                    for (int i = 0; i < ResultJmjm4DoneFlag.Count; i++)
                                    {
                                        if (ResultJmjm4DoneFlag[i].DoneFlag == "N")
                                        {
                                            BlnDoneFlag = false;
                                            break;
                                        }
                                       
                                    }

                                    if (BlnDoneFlag == true)
                                    {
                                        string strUpdateStautsCode;
                                        strUpdateStautsCode = "Update  Jmjm3 Set StatusCode ='CMP' Where JobNo ='" + request.JobNo + "' And LineItemNo ='" + request.JobLineItemNo + "'";
                                        db.ExecuteSql(strUpdateStautsCode);
                                    }
                                }
                            }
                            else
                           {
                                if (Modfunction.CheckNull(ResultJmje2[0].InsertNextEventCode).Length > 0)
                                {
                                    SaveInsertNextEventCode(Modfunction.CheckNull(ResultJmje2[0].InsertNextEventCode),request.JobLineItemNo,request.JobNo ,request.PhoneNumber);
                                }


                                if (Modfunction.CheckNull(ResultJmje2[0].InsertNextEventGroup).Length > 0)
                                {
                                    SaveInsertGroupNameEvent(Modfunction.CheckNull(ResultJmje2[0].InsertNextEventGroup), request.JobLineItemNo, request.JobNo, request.PhoneNumber);
                                }

                                   string strUpdateStautsCode;
                                   strUpdateStautsCode = "Update  Jmjm3 Set StatusCode ='CMP' Where JobNo ='" + request.JobNo + "' And LineItemNo ='"+request.JobLineItemNo+"'";
                                   db.ExecuteSql(strUpdateStautsCode);
                           }
                        }
                    }



                }

            }
            catch { throw; } 
            return Result;
        }
        public void SaveInsertNextEventCode(string strInsertNextEventCode, int intMaxLIneItemNo,string strJobNo,string strUserId)
        {

            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection("TMS"))
                {

                    int intMaxLineItemNo = intMaxLIneItemNo + 1;
                    string StrUpdateJmjm3 = "Update Jmjm3 Set LineItemNo = LineItemNo + 1  Where JobNo = '" + strJobNo + "' AND LineItemNo > " + intMaxLineItemNo;
                    db.ExecuteSql(StrUpdateJmjm3);
                    string StrUpdateJmjm4 = "Update Jmjm4 Set JobLineItemNo = JobLineItemNo + 1 Where JobNo ='" + strJobNo + "' AND JobLineItemNo > " + intMaxLineItemNo;
                    db.ExecuteSql(StrUpdateJmjm4);
                    string StrInsertJmjm3 = "INSERT INTO Jmjm3 (JobNo, LineItemNo,EventCode, Description, ShowETrackFlag,Remark,UpdateBy, UpdateDateTime) " + "Select '" + strJobNo + "'," + intMaxLineItemNo + ",EventCode,Description,EtrackFlag,Remark,'" + strUserId + "', getdate() from Jmje1 Where  EventCode= '" + strInsertNextEventCode + "'";
                    db.ExecuteSql(StrInsertJmjm3);
                    UpdateEventListToJmjm4(strInsertNextEventCode, intMaxLineItemNo, strJobNo);
                }

            }
            catch { throw; }
          


      }

        public void SaveInsertGroupNameEvent(string strInsertNextGroupCode, int intLineItem, string strJobNo, string strUserId)
        {
            List < Jmjm3 >  ResultJmjm3= null;
            List<Jmeg2> ResultJmeg2 = null;


            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection("TMS"))
                {
                    int intMaxLineItemNo = 0;
                    string strSelectJmjm3 = "Select Max(LineItemNo)  as  LineItemNo from Jmjm3 Where JobNo = '" + strJobNo+"'";
                    ResultJmjm3 = db.Select<Jmjm3>(strSelectJmjm3);
                    if (ResultJmjm3.Count > 0)
                    {
                        intMaxLineItemNo = ResultJmjm3[0].LineItemNo + 1;
                    }

                    string strSelectJmeg2 = "Select EventCode from Jmeg2 Where GroupCode = '" + strInsertNextGroupCode + "'";
                    ResultJmeg2 = db.Select<Jmeg2>(strSelectJmeg2);
                    if (ResultJmeg2.Count > 0)
                    {
                        if (intLineItem > 0)
                        {
                            string StrUpdateJmjm7 = "Update Jmjm7 Set JobLineItemNo = JobLineItemNo + " + ResultJmeg2.Count + " Where JobNo = '" +strJobNo+ "' AND JobLineItemNo > " +intLineItem;
                            db.ExecuteSql(StrUpdateJmjm7);

                            string StrUpdateJmjm4 = "Update Jmjm4 Set JobLineItemNo = JobLineItemNo + " + ResultJmeg2.Count + " Where JobNo = '" + strJobNo +  "' AND JobLineItemNo > " + intLineItem;
                            db.ExecuteSql(StrUpdateJmjm4);

                            string StrUpdateJmjm3 = "update Jmjm3 Set LineItemNo = LineItemNo + " + ResultJmeg2.Count + " Where JobNo = '" + strJobNo + "' AND LineItemNo > " + intLineItem;
                            db.ExecuteSql(StrUpdateJmjm3);

                            intMaxLineItemNo = intLineItem + 1;

                         
                        }
                        for (int i = 0; i < ResultJmeg2.Count; i++)
                        {
                            string StrInsertJmjm3 = "INSERT INTO Jmjm3 (JobNo, LineItemNo,EventCode, Description, ShowETrackFlag,Remark,UpdateBy, UpdateDateTime) " +"Select '" +strJobNo+"'," +intMaxLineItemNo +",EventCode,Description,EtrackFlag,Remark,'" + strUserId+ "', getdate() from Jmje1 Where  EventCode= '"+ ResultJmeg2[i].EventCode+ "'";
                            db.ExecuteSql(StrInsertJmjm3);
                            UpdateEventListToJmjm4(Modfunction.CheckNull(ResultJmeg2[i].EventCode), intMaxLineItemNo, strJobNo);
                            intMaxLineItemNo = intMaxLineItemNo + 1;
                        }
                    }

             
                 
                }

            }
            catch { throw; }

        }

        public void UpdateEventListToJmjm4( string strEventCode,int intJobLineItemNo,string strJobNo)
        {
            List<Jmje2> ResultJmje2 = null;
            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection("TMS"))
                {
                    if (strEventCode != null && strEventCode != "")
                    {
                        string strSelectJmje2 = "Select * from Jmje2 Where EventCode = '" + strEventCode + "'";
                        ResultJmje2 = db.Select<Jmje2>(strSelectJmje2);
                        if (ResultJmje2.Count > 0)
                        {
                            int intLineItemNo = 1;
                            for (int i = 0; i < ResultJmje2.Count; i++)
                            {
                                string insertJmjm4 = "Insert Into Jmjm4(JobNo,JobLineItemNo,EventLineItemNo,LineItemNo,DoneFlag,ItemName,MobileUser,Remark) values (" +
                                                                       "'"+ strJobNo + "',"+ intJobLineItemNo + ","+ ResultJmje2[i].LineItemNo+ ","+ intLineItemNo + ",NULL,'" + Modfunction.CheckNull(ResultJmje2[i].ItemName) + "','"+ Modfunction.CheckNull(ResultJmje2[i].MobileUser) + "', '' )";
                                intLineItemNo = intLineItemNo + 1;
                                db.ExecuteSql(insertJmjm4);
                            }
                        }
                    }
                }

            }
            catch { throw; }
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
