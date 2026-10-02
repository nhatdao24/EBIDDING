using NHibernate;
using SharpSapRfc;
using SharpSapRfc.Plain;
using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using SMO.SAPINT;
using SMO.SAPINT.Function;
using System;
using System.Configuration;
using System.Data;
using SMO.Service.AD;
using System.Data.SqlClient;
using System.Linq;
using SMO.Repository.Implement.EB;

namespace SMO.Service.MD
{
    public class VendorService : GenericService<T_MD_VENDOR, VendorRepo>
    {
        public VendorService() : base()
        {
            
        }

        public void CheckCreateAccount(string id)
        {
            try
            {
                this.Get(id);
                var findContractor = UnitOfWork.Repository<ContractorRepo>().Queryable().FirstOrDefault(x => x.MA_SO_THUE == this.ObjDetail.MA_SO_THUE);
                if (findContractor != null)
                {
                    this.ObjDetail.STATUS = true;
                    this.ObjDetail.USER_NAME = findContractor.USER_NAME;
                    this.Update();
                    this.State = false;
                    return;
                }
                this.State = true;
            }
            catch (Exception ex)
            {
                this.State = false;
            }
        }

        public void Synchronize()
        {
            try
            {
                var systemConfig = new SystemConfigService();
                systemConfig.GetConfig();
                using (SapRfcConnection conn = new PlainSapRfcConnection(SAPDestitination.SapDestinationName,
                    systemConfig.ObjDetail.SAP_USER_NAME, systemConfig.ObjDetail.SAP_PASSWORD))
                {
                    var function = new Get_Vendor_Function();
                    function.Parameters = new
                    {
                        I_BUKRS = "F001"
                    };
                    var result = conn.ExecuteFunction(function).ToList();

                    SqlConnection objConn = new SqlConnection(ConfigurationManager.ConnectionStrings["SMO_MSSQL_Connection"].ConnectionString);
                    objConn.Open();

                    string tableName = "T_MD_VENDOR";
                    SqlDataAdapter daAdapter = new SqlDataAdapter("SELECT * FROM " + tableName, objConn);
                    daAdapter.UpdateBatchSize = 0;
                    DataSet dataSet = new DataSet(tableName);
                    daAdapter.FillSchema(dataSet, SchemaType.Source, tableName);
                    daAdapter.Fill(dataSet, tableName);
                    DataTable dataTable;
                    dataTable = dataSet.Tables[tableName];

                    var actionBy = ProfileUtilities.User.USER_NAME;
                    var actionDate = DateTime.Now;
                    foreach (var item in result)
                    {
                        var row = dataTable.Rows.Find(item.CODE);
                        if (row == null)
                        {
                            DataRow newRow = dataTable.NewRow();
                            newRow["CODE"] = item.CODE;
                            newRow["SHORT_NAME"] = item.SHORT_NAME;
                            newRow["LONG_NAME"] = item.LONG_NAME;
                            newRow["MA_SO_THUE"] = item.MA_SO_THUE;
                            newRow["DIA_CHI"] = item.DIA_CHI;
                            newRow["SO_DIEN_THOAI"] = item.SO_DIEN_THOAI;
                            newRow["SO_FAX"] = item.SO_FAX;
                            newRow["EMAIL"] = item.EMAIL;
                            newRow["WEBSITE"] = item.WEBSITE;
                            newRow["LIEN_HE"] = item.LIEN_HE;
                            newRow["LINH_VUC_CHINH"] = item.LINH_VUC_CHINH;
                            newRow["KHU_VUC"] = item.KHU_VUC;
                            newRow["TINH_THANH"] = item.TINH_THANH;
                            newRow["DON_VI_TUNG_GIAO_DICH"] = item.DON_VI_TUNG_GIAO_DICH;
                            newRow["CREATE_BY"] = actionBy;
                            newRow["CREATE_DATE"] = actionDate;
                            dataTable.Rows.Add(newRow);
                        }
                        else if ( row["SHORT_NAME"].ToString() != item.SHORT_NAME
                            || row["LONG_NAME"].ToString() != item.LONG_NAME
                            || row["MA_SO_THUE"].ToString() != item.MA_SO_THUE
                            || row["DIA_CHI"].ToString() != item.DIA_CHI
                            || row["SO_DIEN_THOAI"].ToString() != item.SO_DIEN_THOAI
                            || row["SO_FAX"].ToString() != item.SO_FAX
                            || row["EMAIL"].ToString() != item.EMAIL
                            || row["WEBSITE"].ToString() != item.WEBSITE
                            || row["LINH_VUC_CHINH"].ToString() != item.LINH_VUC_CHINH
                            || row["KHU_VUC"].ToString() != item.KHU_VUC
                            || row["TINH_THANH"].ToString() != item.TINH_THANH
                            || row["DON_VI_TUNG_GIAO_DICH"].ToString() != item.DON_VI_TUNG_GIAO_DICH
                            || row["LIEN_HE"].ToString() != item.LIEN_HE)
                        {
                            row.BeginEdit();
                            row["SHORT_NAME"] = item.SHORT_NAME;
                            row["LONG_NAME"] = item.LONG_NAME;
                            row["DIA_CHI"] = item.DIA_CHI;
                            row["SO_DIEN_THOAI"] = item.SO_DIEN_THOAI;
                            row["SO_FAX"] = item.SO_FAX;
                            row["EMAIL"] = item.EMAIL;
                            row["WEBSITE"] = item.WEBSITE;
                            row["LIEN_HE"] = item.LIEN_HE;
                            row["LINH_VUC_CHINH"] = item.LINH_VUC_CHINH;
                            row["KHU_VUC"] = item.KHU_VUC;
                            row["TINH_THANH"] = item.TINH_THANH;
                            row["DON_VI_TUNG_GIAO_DICH"] = item.DON_VI_TUNG_GIAO_DICH;
                            row["UPDATE_BY"] = actionBy;
                            row["UPDATE_DATE"] = actionDate;
                            row.EndEdit();
                        }
                    }

                    SqlCommandBuilder objCommandBuilder = new SqlCommandBuilder(daAdapter);
                    daAdapter.Update(dataSet, tableName);
                }
            }
            catch (Exception ex)
            {
                this.State = false;
                this.Exception = ex;
            }
        }
    }
}
