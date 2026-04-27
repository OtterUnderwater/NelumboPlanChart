using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PlanDiagram.Services
{
    public class PlanRepository
    {
        private string _connection;
        private int? _orderID;
        public PlanRepository(string connection, int? orderID)
        {
            _connection = connection;
            _orderID = orderID;
        }

        /// <summary>
        /// Получение плана
        /// </summary>
        /// <returns></returns>
        public List<ProcessData> GetPlanList()
        {
            List<ProcessData> processData = new List<ProcessData>();
            using (SqlConnection connection = new SqlConnection(_connection))
            {
                using (SqlCommand command = new SqlCommand("mes.ext_ProdPlan", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ActionID", 1);
                    command.Parameters.AddWithValue("@OrderID", _orderID);                    
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int workCenterName = reader.GetOrdinal("WorkCenterName");
                        int processName = reader.GetOrdinal("ProcessName");
                        int opName = reader.GetOrdinal("OpName");
                        int qty = reader.GetOrdinal("Qty");
                        int planStartDate = reader.GetOrdinal("PlanStartTime");
                        int planEndDate = reader.GetOrdinal("PlanEndTime");
                        int workTime = reader.GetOrdinal("FullWorkTimeH");
                        while (reader.Read())
                        {           
                            ProcessData data = new ProcessData
                            {
                                WorkCenterName = reader.GetString(workCenterName),
                                ProcessName = reader.GetString(processName),
                                OpName = reader.GetString(opName),
                                Qty = reader.GetDouble(qty),
                                PlanStartDate = reader.GetDateTime(planStartDate),
                                PlanEndDate = reader.GetDateTime(planEndDate),
                                WorkTime = reader.GetDouble(workTime)
                            };
                            processData.Add(data);
                        }
                    }
                }
            }
            return processData;
        }

        /// <summary>
        /// Получение плана по диапазону дат
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<ProcessData> GetPlanDate(DateTime startDate, DateTime endDate)
        {
            var allRowsPlan = GetPlanList();
            var result = new List<ProcessData>();

            result = allRowsPlan
                .Where(t => t.PlanEndDate >= startDate && t.PlanStartDate <= endDate)
                .OrderBy(t => t.PlanStartDate).OrderBy(t => t.PlanEndDate)
                .ToList();

            return result;
        }
    }
}
