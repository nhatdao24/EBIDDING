using SharpSapRfc;
using SharpSapRfc.Plain;
using SMO.Core.Entities;
using SMO.Repository.Implement.MD;
using SMO.SAPINT;
using SMO.SAPINT.Function;
using SMO.Service.AD;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SMO.Service.PR
{
    public class PRSAPService : GenericService<T_PR_SAP_HEADER, PRSAPRepo>
    {
        public PRSAPService() : base()
        {

        }

        public void Synchronize()
        {
            try
            {
                var systemConfig = new SystemConfigService();
                systemConfig.GetConfig();
                SAPDestitination.Init(systemConfig.ObjDetail.SAP_HOST, systemConfig.ObjDetail.SAP_CLIENT, systemConfig.ObjDetail.SAP_NUMBER);

                using (SapRfcConnection conn = new PlainSapRfcConnection(SAPDestitination.SapDestinationName,
                            systemConfig.ObjDetail.SAP_USER_NAME, systemConfig.ObjDetail.SAP_PASSWORD))
                {
                    var functionSAP = new Get_PR_Function();
                    if (systemConfig.ObjDetail.LAST_UPDATE_PR.HasValue)
                    {
                        functionSAP.Parameters = new
                        {
                            I_BUDAT = systemConfig.ObjDetail.LAST_UPDATE_PR.Value.AddDays(-1)
                        };
                    }

                    conn.ExecuteFunction(functionSAP);
                    var lstOutputHeader = functionSAP.Result.GetTable<T_PR_SAP_HEADER>("HEADER_PR");
                    var lstOutputDetail = functionSAP.Result.GetTable<T_PR_SAP_DETAIL>("ITEM_PR");

                    if (lstOutputHeader.Count() == 0)
                    {
                        return;
                    }

                    SqlConnection objConn = new SqlConnection(ConfigurationManager.ConnectionStrings["SMO_MSSQL_Connection"].ConnectionString);
                    objConn.Open();

                    string tableName = "T_PR_SAP_HEADER";
                    string tableNameDetail = "T_PR_SAP_DETAIL";

                    SqlDataAdapter daAdapter = new SqlDataAdapter("SELECT * FROM " + tableName, objConn);
                    SqlDataAdapter daAdapterComparment = new SqlDataAdapter("SELECT * FROM " + tableNameDetail, objConn);
                    daAdapter.UpdateBatchSize = 0;
                    daAdapterComparment.UpdateBatchSize = 0;
                    DataSet dataSet = new DataSet(tableName);
                    DataSet dataSetDetail = new DataSet(tableNameDetail);
                    daAdapter.FillSchema(dataSet, SchemaType.Source, tableName);
                    daAdapter.Fill(dataSet, tableName);
                    daAdapterComparment.FillSchema(dataSetDetail, SchemaType.Source, tableNameDetail);
                    daAdapterComparment.Fill(dataSetDetail, tableNameDetail);
                    DataTable dataTable;
                    DataTable dataTableDetail;
                    dataTable = dataSet.Tables[tableName];
                    dataTableDetail = dataSetDetail.Tables[tableNameDetail];

                    //var actionBy = ProfileUtilities.User.USER_NAME;
                    var actionDate = DateTime.Now;

                    foreach (var item in lstOutputHeader)
                    {
                        var row = dataTable.Rows.Find(item.CODE);
                        if (row == null)
                        {
                            DataRow newRow = dataTable.NewRow();
                            newRow["CODE"] = item.CODE;
                            newRow["COMPANY_NAME"] = item.COMPANY_NAME;
                            newRow["REQUIRE_NAME"] = item.REQUIRE_NAME;
                            newRow["REQUIRE_USER"] = item.REQUIRE_USER;
                            newRow["DEPARTMENT_NAME"] = item.DEPARTMENT_NAME;
                            if (item.REQUIRE_DATE.HasValue)
                            {
                                newRow["REQUIRE_DATE"] = item.REQUIRE_DATE.Value;
                            }
                            else
                            {
                                newRow["REQUIRE_DATE"] = DBNull.Value;
                            }

                            if (item.DESTINATION_DATE.HasValue)
                            {
                                newRow["DESTINATION_DATE"] = item.DESTINATION_DATE.Value;
                            }
                            else
                            {
                                newRow["DESTINATION_DATE"] = DBNull.Value;
                            }
                            newRow["CREATE_DATE"] = actionDate;
                            dataTable.Rows.Add(newRow);
                        }
                        else if (row["COMPANY_NAME"].ToString() != item.COMPANY_NAME ||
                            row["REQUIRE_NAME"].ToString() != item.REQUIRE_NAME ||
                            row["REQUIRE_USER"].ToString() != item.REQUIRE_USER ||
                            row["DEPARTMENT_NAME"].ToString() != item.DEPARTMENT_NAME
                            //row["NGAY_YEU_CAU"].ToString() != item.NGAY_YEU_CAU.ToString() ||
                            //row["THOI_HAN"].ToString() != item.THOI_HAN.ToString()
                            )
                        {
                            row.BeginEdit();
                            row["COMPANY_NAME"] = item.COMPANY_NAME;
                            row["REQUIRE_NAME"] = item.REQUIRE_NAME;
                            row["REQUIRE_USER"] = item.REQUIRE_USER;
                            row["DEPARTMENT_NAME"] = item.DEPARTMENT_NAME;
                            row["UPDATE_DATE"] = actionDate;
                            row.EndEdit();
                        }
                    }

                    string strDelete = $"DELETE T_PR_SAP_DETAIL WHERE HEADER_CODE IN ('{ string.Join("','", lstOutputHeader.Select(x => x.CODE))}')";

                    UnitOfWork.GetSession().CreateSQLQuery(strDelete).ExecuteUpdate();

                    SqlCommandBuilder objCommandBuilder = new SqlCommandBuilder(daAdapter);
                    daAdapter.Update(dataSet, tableName);

                    foreach (var item in lstOutputDetail)
                    {
                        DataRow newRow = dataTableDetail.NewRow();
                        newRow["PKID"] = Guid.NewGuid().ToString();
                        newRow["HEADER_CODE"] = item.HEADER_CODE;
                        newRow["ITEM_NUMBER"] = item.ITEM_NUMBER;
                        newRow["MATERIAL_CODE"] = item.MATERIAL_CODE;
                        newRow["MATERIAL_NAME"] = item.MATERIAL_NAME;
                        newRow["QUANTITY"] = item.QUANTITY;
                        newRow["UNIT"] = item.UNIT;
                        newRow["PRICE"] = item.PRICE;
                        newRow["SUM_VALUE"] = item.SUM_VALUE;
                        newRow["CURRENCY"] = item.CURRENCY;
                        newRow["CREATE_DATE"] = actionDate;
                        dataTableDetail.Rows.Add(newRow);
                    }

                    SqlCommandBuilder objCommandBuilderComparment = new SqlCommandBuilder(daAdapterComparment);
                    daAdapterComparment.Update(dataSetDetail, tableNameDetail);

                    systemConfig.ObjDetail.LAST_UPDATE_PR = DateTime.Now;
                    systemConfig.Update();
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
