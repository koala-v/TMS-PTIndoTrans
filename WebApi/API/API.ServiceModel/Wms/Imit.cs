﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ServiceStack;
using ServiceStack.ServiceHost;
using ServiceStack.OrmLite;
using WebApi.ServiceModel.Tables;

namespace WebApi.ServiceModel.Wms
{
    [Route("/wms/imit1/create", "Get")]					//create?UserID=
    [Route("/wms/imit1/confirm", "Get")]				//confirm?TrxNo= &UpdateBy=
    [Route("/wms/imit2/create", "Get")]					//create?TrxNo= &LineItemNo= &Impm1TrxNo= &NewStoreNo= &Qty= &UpdateBy= 
    public class Imit : IReturn<CommonResponse>
    {
        public string UserID { get; set; }
        public string TrxNo { get; set; }
        public string UpdateBy { get; set; }
        public string Impm1TrxNo { get; set; }
        public string LineItemNo { get; set; }
        public string NewStoreNo { get; set; }
        public string Qty { get; set; }
    }
    public class Imit_Logic
    {
        public IDbConnectionFactory DbConnectionFactory { get; set; }
        public List<Imit1> Insert_Imit1(Imit request)
        {
            List<Imit1> Result = null;
            int intResult = -1;
            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection("WMS"))
                {
                    string strSql = "EXEC spi_Imit1 @CustomerCode,@Description1,@Description2,@GoodsTransferNoteNo,@RefNo,@TransferBy,@TransferDateTime,@TrxNo,@WorkStation,@CreateBy,@UpdateBy";
                    intResult = db.SqlScalar<int>(strSql,
                                    new
                                    {
                                        CustomerCode = "",
                                        Description1 = "",
                                        Description2 = "",
                                        GoodsTransferNoteNo = "",
                                        RefNo = "",
                                        TransferBy = request.UserID,
                                        TransferDateTime = DateTime.Now,
                                        TrxNo = "",
                                        WorkStation = "APP",
                                        CreateBy = request.UserID,
                                        UpdateBy = request.UserID
                                    });
                    if (intResult > -1)
                    {
                        strSql = "Select top 1 * From Imit1 Order By CreateDateTime Desc";
                        Result = db.Select<Imit1>(strSql);
                    }
                }
            }
            catch { throw; }
            return Result;
        }
        public int Confirm_Imit1(Imit request)
        {
            int Result = -1;
            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection("WMS"))
                {
                    string strSql = "EXEC spi_Imit_Confirm " + int.Parse(request.TrxNo) + ",'" + request.UpdateBy + "'";
                    Result = db.SqlScalar<int>(strSql);
                }
            }
            catch { throw; }
            return Result;
        }
        public int Insert_Imit2(Imit request)
        {
            int Result = -1;
            try
            {
                using (var db = DbConnectionFactory.OpenDbConnection())
                {
                    string strSql = "Select Impm1.*,Impr1.PackingPackageSize,Impr1.WholePackageSize " +
                                "From Impm1 Join Impr1 On Impm1.ProductTrxNo = Impr1.TrxNo " +
                                "Where Impm1.TrxNo=" + int.Parse(request.Impm1TrxNo);
                    List<Impm1> impm1s = db.Select<Impm1>(strSql);
                    if (impm1s.Count > 0)
                    {
                        switch (impm1s[0].DimensionFlag)
                        {
                            case "1":
                                db.Insert(
                                                new Imit2
                                                {
                                                    TrxNo = int.Parse(request.TrxNo),
                                                    LineItemNo = int.Parse(request.LineItemNo),
                                                    MovementTrxNo = int.Parse(request.Impm1TrxNo),
                                                    NewStoreNo = request.NewStoreNo,
                                                    NewWarehouseCode = impm1s[0].WarehouseCode,
                                                    StoreNo = impm1s[0].StoreNo,
                                                    WarehouseCode = impm1s[0].WarehouseCode,
                                                    ProductTrxNo = impm1s[0].ProductTrxNo,
                                                    PackingQty = int.Parse(request.Qty),
                                                    WholeQty = int.Parse(request.Qty) * impm1s[0].PackingPackageSize,
                                                    LooseQty = int.Parse(request.Qty) * impm1s[0].PackingPackageSize * impm1s[0].WholePackageSize,
                                                    Volume = int.Parse(request.Qty) * impm1s[0].UnitVol,
                                                    Weight = int.Parse(request.Qty) * impm1s[0].UnitWt,
                                                    SpaceArea = impm1s[0].SpaceArea * int.Parse(request.Qty) / impm1s[0].BalancePackingQty,
                                                    UpdateBy = request.UpdateBy
                                                }
                                );
                                db.SqlScalar<int>("Update Impm1 set BalancePackingQty= BalancePackingQty - " + request.Qty.ToString() + ", BalanceWholeQty=BalanceWholeQty-" + (int.Parse(request.Qty) * impm1s[0].PackingPackageSize) + ", BalanceLooseQty=BalanceLooseQty-" + (int.Parse(request.Qty) * impm1s[0].PackingPackageSize * impm1s[0].WholePackageSize) + ", BalanceVolume = BalanceVolume - " + (int.Parse(request.Qty) * impm1s[0].UnitVol) + ", BalanceWeight = BalanceWeight - " + (int.Parse(request.Qty) * impm1s[0].UnitWt) + ", BalanceSpaceArea = BalanceSpaceArea - " + (impm1s[0].SpaceArea * int.Parse(request.Qty) / impm1s[0].BalancePackingQty) + ",UpdateDateTime=getdate(),UpdateBy='" + request.UpdateBy + "' Where TrxNo = " + int.Parse(request.Impm1TrxNo));
                                db.SqlScalar<int>("EXEC spi_Imit2_Impm " + int.Parse(request.TrxNo) + "," + int.Parse(request.LineItemNo) + ",'" + request.UpdateBy + "'");
                                break;
                            case "2":
                                db.Insert(
                                                new Imit2
                                                {
                                                    TrxNo = int.Parse(request.TrxNo),
                                                    LineItemNo = int.Parse(request.LineItemNo),
                                                    NewStoreNo = request.NewStoreNo,
                                                    UpdateBy = request.UpdateBy,
                                                    MovementTrxNo = int.Parse(request.Impm1TrxNo),
                                                    NewWarehouseCode = impm1s[0].WarehouseCode,
                                                    StoreNo = impm1s[0].StoreNo,
                                                    WarehouseCode = impm1s[0].WarehouseCode,
                                                    ProductTrxNo = impm1s[0].ProductTrxNo,
                                                    WholeQty = int.Parse(request.Qty),
                                                    LooseQty = int.Parse(request.Qty) * impm1s[0].WholePackageSize,
                                                    Volume = int.Parse(request.Qty) * impm1s[0].UnitVol,
                                                    Weight = int.Parse(request.Qty) * impm1s[0].UnitWt,
                                                    SpaceArea = impm1s[0].SpaceArea * int.Parse(request.Qty) / impm1s[0].BalanceWholeQty,

                                                }
                                );
                                db.SqlScalar<int>("Update Impm1 set BalanceWholeQty=BalanceWholeQty-" + int.Parse(request.Qty) + ", BalanceLooseQty=BalanceLooseQty-" + (int.Parse(request.Qty) * impm1s[0].WholePackageSize) + ", BalanceVolume = BalanceVolume - " + (int.Parse(request.Qty) * impm1s[0].UnitVol) + ", BalanceWeight = BalanceWeight - " + (int.Parse(request.Qty) * impm1s[0].UnitWt) + ", BalanceSpaceArea = BalanceSpaceArea - " + (impm1s[0].SpaceArea * int.Parse(request.Qty) / impm1s[0].BalanceWholeQty) + ",UpdateDateTime=getdate(),UpdateBy='" + request.UpdateBy + "' Where TrxNo = " + int.Parse(request.Impm1TrxNo));
                                db.SqlScalar<int>("EXEC spi_Imit2_Impm " + int.Parse(request.TrxNo) + "," + int.Parse(request.LineItemNo) + ",'" + request.UpdateBy + "'");
                                break;
                            default:
                                db.Insert(
                                                new Imit2
                                                {
                                                    TrxNo = int.Parse(request.TrxNo),
                                                    LineItemNo = int.Parse(request.LineItemNo),
                                                    NewStoreNo = request.NewStoreNo,
                                                    UpdateBy = request.UpdateBy,
                                                    MovementTrxNo = int.Parse(request.Impm1TrxNo),
                                                    NewWarehouseCode = impm1s[0].WarehouseCode,
                                                    StoreNo = impm1s[0].StoreNo,
                                                    WarehouseCode = impm1s[0].WarehouseCode,
                                                    ProductTrxNo = impm1s[0].ProductTrxNo,
                                                    LooseQty = int.Parse(request.Qty),
                                                    Volume = int.Parse(request.Qty) * impm1s[0].UnitVol,
                                                    Weight = int.Parse(request.Qty) * impm1s[0].UnitWt,
                                                    SpaceArea = impm1s[0].SpaceArea * int.Parse(request.Qty) / impm1s[0].BalanceLooseQty,
                                                }
                                );
                                db.SqlScalar<int>("Update Impm1 set BalanceLooseQty=BalanceLooseQty-" + int.Parse(request.Qty) + ", BalanceVolume = BalanceVolume - " + (int.Parse(request.Qty) * impm1s[0].UnitVol) + ", BalanceWeight = BalanceWeight - " + (int.Parse(request.Qty) * impm1s[0].UnitWt) + ", BalanceSpaceArea = BalanceSpaceArea - " + (impm1s[0].SpaceArea * int.Parse(request.Qty) / impm1s[0].BalanceLooseQty) + ",UpdateDateTime=getdate(),UpdateBy='" + request.UpdateBy + "' Where TrxNo = " + int.Parse(request.Impm1TrxNo));
                                db.SqlScalar<int>("EXEC spi_Imit2_Impm " + int.Parse(request.TrxNo) + "," + int.Parse(request.LineItemNo) + ",'" + request.UpdateBy + "'");
                                break;
                        }
                        Result = 1;
                    }
                }
            }
            catch { throw; }
            return Result;
        }
    }
}
