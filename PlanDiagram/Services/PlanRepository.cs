using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

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
        public (List<ProcessData> process, List<ProductionOrder> prodOrders, List<ClientOrder> clientOrders, List<Item> items) GetPlanList()
        {
            using (var connection = new SqlConnection(_connection))
            {
                var productionOrders = new List<ProductionOrder>();
                var clientOrders = new List<ClientOrder>();
                var items = new List<Item>();

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
                    new { ActionID = 2 },
                    commandType: CommandType.StoredProcedure
                );

                // 3. Получаем данные для фильтрации
                using (var multi = connection.QueryMultiple(
                    "mes.ext_ProdPlan",
                    new { ActionID = 3 },
                    commandType: CommandType.StoredProcedure))
                {
                    productionOrders = multi.Read<ProductionOrder>().ToList();
                    clientOrders = multi.Read<ClientOrder>().ToList();
                    items = multi.Read<Item>().ToList();
                }

                // Группируем детали по ProdPlanRowID
                foreach (var detail in details)
                {
                    if (lookup.TryGetValue(detail.ProdPlanRowID, out var process))
                        process.WorkTimeDay.Add(detail);             
                }
                return (processData, productionOrders, clientOrders, items);
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