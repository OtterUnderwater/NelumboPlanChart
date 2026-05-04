using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using System.Data.Common;

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
            using (var connection = new SqlConnection(_connection))
            {
                // 1. Получаем основные данные
                var processData = connection.Query<ProcessData>(
                    "mes.ext_ProdPlan",
                    new { ActionID = 1, OrderID = _orderID },
                    commandType: CommandType.StoredProcedure
                ).ToList();

                var lookup = processData.ToDictionary(p => p.ProdPlanRowID);

                // 2. Получаем детали загрузки
                var details = connection.Query<LoadingWC>(
                    "mes.ext_ProdPlan",
                    new { ActionID = 2, OrderID = _orderID },
                    commandType: CommandType.StoredProcedure
                );

                // Группируем детали по ProdPlanRowID
                foreach (var detail in details)
                {
                    if (lookup.TryGetValue(detail.ProdPlanRowID, out var process))
                        process.WorkTimeDay.Add(detail);             
                }
                return processData;
            }
        }

        /// <summary>
        /// Получение рабочих дней из таблицы Calendar
        /// </summary>
        public List<DateTime> GetWorkingDaysFromCalendar()
        {
            using (var connection = new SqlConnection(_connection))
            {
                return connection.Query<DateTime>("mes.ext_ProdPlan", new { ActionID = 20 },
                    commandType: CommandType.StoredProcedure).ToList();
            }
        }
    }
}