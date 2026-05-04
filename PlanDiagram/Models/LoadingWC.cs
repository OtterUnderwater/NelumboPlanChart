using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlanDiagram.Models
{
    // Модель для детализации загрузки по дням
    public class LoadingWC
    {
        public int ProdPlanRowID { get; set; } // Добавьте это поле
        public DateTime OnDate { get; set; }
        public double WorkTimeMin { get; set; }
        public int WorkTimeHours => (int)(WorkTimeMin / 60);
        public int WorkTimeMinutes => (int)(WorkTimeMin % 60);
        public string WorkTimeFormatted => $"{WorkTimeHours} ч {WorkTimeMinutes} мин";
    }
}
