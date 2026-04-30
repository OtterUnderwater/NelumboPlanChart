using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanDiagram.Models
{
    public class ProcessData
    {
        public string WorkCenterName { get; set; }
        public string ProcessName { get; set; }
        public string OpName { get; set; }
        public double Qty { get; set; }
        public DateTime PlanStartDate { get; set; }  //PlanStartTime
        public DateTime PlanEndDate { get; set; }  //PlanEndTime
        public double WorkTime { get; set; } //FullWorkTimeH
        public string HexCode { get; set; }
        public double DurationDays => (PlanEndDate - PlanStartDate).TotalDays + 1;
        public string TooltipText => $"{ProcessName}\n{PlanStartDate:dd.MM.yyyy} - {PlanEndDate:dd.MM.yyyy}\n{WorkTime:F1} ч";
    }
}
