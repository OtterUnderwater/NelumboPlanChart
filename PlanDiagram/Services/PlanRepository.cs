using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Windows.Controls.Primitives;

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
                connection.Open();
                // 1. Получаем основные данные
                using (SqlCommand command = new SqlCommand("mes.ext_ProdPlan", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ActionID", 1);
                    command.Parameters.AddWithValue("@OrderID", _orderID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int prodPlanRowID = reader.GetOrdinal("ProdPlanRowID");
                        int workCenterName = reader.GetOrdinal("WorkCenterName");
                        int processName = reader.GetOrdinal("ProcessName");
                        int opName = reader.GetOrdinal("OpName");
                        int qty = reader.GetOrdinal("Qty");
                        int planStartDate = reader.GetOrdinal("PlanStartTime");
                        int planEndDate = reader.GetOrdinal("PlanEndTime");
                        int workTime = reader.GetOrdinal("FullWorkTimeH");
                        int hexCode = reader.GetOrdinal("HexCode");

                        while (reader.Read())
                        {
                            ProcessData data = new ProcessData
                            {
                                ProdPlanRowID = reader.GetInt32(prodPlanRowID),
                                WorkCenterName = reader.GetString(workCenterName),
                                ProcessName = reader.GetString(processName),
                                OpName = reader.GetString(opName),
                                Qty = (double)reader.GetDecimal(qty),
                                PlanStartDate = reader.GetDateTime(planStartDate),
                                PlanEndDate = reader.GetDateTime(planEndDate),
                                WorkTime = (double)reader.GetDecimal(workTime),
                                HexCode = reader.GetString(hexCode),
                                WorkTimeDay = new List<LoadingWC>()
                            };
                            processData.Add(data);
                        }
                    }
                }

                // 2. Получаем детали загрузки по дням (ActionID=2)
                using (SqlCommand command = new SqlCommand("mes.ext_ProdPlan", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ActionID", 2);
                    command.Parameters.AddWithValue("@OrderID", _orderID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int rowID = reader.GetOrdinal("ProdPlanRowID");
                        int onDate = reader.GetOrdinal("OnDate");
                        int workTimeMin = reader.GetOrdinal("WorkTimeMin");

                        // Создадим словарь для быстрого доступа к спискам деталей
                        var detailsDict = processData.ToDictionary(p => p.ProdPlanRowID);

                        while (reader.Read())
                        {
                            int prodPlanRowID = reader.GetInt32(rowID);
                            if (detailsDict.TryGetValue(prodPlanRowID, out ProcessData process))
                            {
                                process.WorkTimeDay.Add(new LoadingWC
                                {
                                    OnDate = reader.GetDateTime(onDate),
                                    WorkTimeMin = (double)reader.GetDecimal(workTimeMin)
                                });
                            }
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

        /// <summary>
        /// Получение рабочих дней из таблицы Calendar
        /// </summary>
        public List<DateTime> GetWorkingDaysFromCalendar()
        {
            List<DateTime> workingDays = new List<DateTime>();

            using (SqlConnection connection = new SqlConnection(_connection))
            {
                using (SqlCommand command = new SqlCommand("mes.ext_ProdPlan", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ActionID", 20);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            workingDays.Add(reader.GetDateTime(0));
                        }
                    }
                }
            }
            return workingDays;
        }
    }
}