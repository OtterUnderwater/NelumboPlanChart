using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanDiagram.Models
{
    public class ProcessData
    {
        public int ProdPlanRowID { get; set; }
        public int StatusID { get; set; }
        public string WorkCenterName { get; set; }
        public string ProcessName { get; set; }
        public string OpName { get; set; }
        public double Qty { get; set; }
        public DateTime PlanStartTime { get; set; }
        public DateTime PlanEndTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double FullWorkTimeH { get; set; }
        public string HexCode { get; set; }
        public double OutQty { get; set; }
        public double FixQty { get; set; }
        public double NotOutQty { get; set; }
        public int Sort { get; set; }
        public int OrderID { get; set; }
        public string OrderName { get; set; }
        public int ClientOrderID { get; set; }
        public string ClientOrderName { get; set; }
        public int ItemID { get; set; }
        public string ItemFullName { get; set; }
        public List<LoadingWC> WorkTimeDay { get; set; } = new List<LoadingWC>();
        public double DurationDays => (PlanEndTime - PlanStartTime).TotalDays + 1;
        public string TooltipText => $"{ProcessName}\n{PlanStartTime:dd.MM.yyyy} - {PlanEndTime:dd.MM.yyyy}\n{FullWorkTimeH:F1} ч";
    }
}
