using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanDiagram.Models
{
    public class GanttRowModel
    {
        public string ProcessName { get; set; }
        public ProcessData TaskData { get; set; }
        public GanttItemModel GanttItem { get; set; }
        public double TotalWidth { get; set; }
    }
}
