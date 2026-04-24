using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanDiagram.Models
{
    public class ProcessData
    {
        public string ProcessName { get; set; }
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanEndDate { get; set; }
        public double WorkTime { get; set; }
        public double DurationDays => (PlanEndDate - PlanStartDate).TotalDays + 1;
        public string TooltipText => $"{ProcessName}\n{PlanStartDate:dd.MM.yyyy} - {PlanEndDate:dd.MM.yyyy}\n{WorkTime:F1} ч";
    }

    public static class DataProvider
    {
        public static List<ProcessData> GetListProcesses()
        {
            return new List<ProcessData>
    {
        new ProcessData { ProcessName = "Перемычка - Сборка",
            PlanStartDate = new DateTime(2026, 5, 6),
            PlanEndDate = new DateTime(2026, 5, 6),
            WorkTime = 5.0 },

        new ProcessData { ProcessName = "Перемычка В - Сборка",
            PlanStartDate = new DateTime(2026, 4, 22),
            PlanEndDate = new DateTime(2026, 4, 22),
            WorkTime = 6.0 },

        new ProcessData { ProcessName = "Перемычка В - Контроль",
            PlanStartDate = new DateTime(2026, 4, 23),
            PlanEndDate = new DateTime(2026, 4, 23),
            WorkTime = 4.0 },

        new ProcessData { ProcessName = "Перемычка В - Комплектование",
            PlanStartDate = new DateTime(2026, 4, 24),
            PlanEndDate = new DateTime(2026, 4, 25),
            WorkTime = 12.0 },

        new ProcessData { ProcessName = "Перемычка Н - Вырубка",
            PlanStartDate = new DateTime(2026, 4, 22),
            PlanEndDate = new DateTime(2026, 4, 23),
            WorkTime = 10.0 },

        new ProcessData { ProcessName = "Перемычка Н (вторая копия) - Сборка",
            PlanStartDate = new DateTime(2026, 4, 22),
            PlanEndDate = new DateTime(2026, 4, 23),
            WorkTime = 7.5 },

        new ProcessData { ProcessName = "Моноблок Полный - Маркирование",
            PlanStartDate = new DateTime(2026, 5, 5),
            PlanEndDate = new DateTime(2026, 5, 5),
            WorkTime = 5.0 },

        new ProcessData { ProcessName = "Каркас - Контроль",
            PlanStartDate = new DateTime(2026, 4, 30),
            PlanEndDate = new DateTime(2026, 5, 1),
            WorkTime = 16.0 },

        new ProcessData { ProcessName = "Доп - Сборка",
            PlanStartDate = new DateTime(2026, 4, 22),
            PlanEndDate = new DateTime(2026, 4, 24),
            WorkTime = 17.6 },

        new ProcessData { ProcessName = "Доп - Консервация и упаковка",
            PlanStartDate = new DateTime(2026, 4, 25),
            PlanEndDate = new DateTime(2026, 4, 29),
            WorkTime = 35.0 },

        new ProcessData { ProcessName = "Каркас - Маркирование",
            PlanStartDate = new DateTime(2026, 5, 2),
            PlanEndDate = new DateTime(2026, 5, 4),
            WorkTime = 24.0 },

        new ProcessData { ProcessName = "Каркас камеры - Маркирование",
            PlanStartDate = new DateTime(2026, 4, 22),
            PlanEndDate = new DateTime(2026, 4, 22),
            WorkTime = 4.0 },

        new ProcessData { ProcessName = "Моноблок - Сборка",
            PlanStartDate = new DateTime(2026, 4, 22),
            PlanEndDate = new DateTime(2026, 4, 22),
            WorkTime = 4.8 }
    };
        }
    }

}
