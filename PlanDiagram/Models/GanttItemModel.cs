using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PlanDiagram.Models
{
    public class GanttItemModel
    {
        public double Width { get; set; }
        public double Left { get; set; }
        public string DisplayText { get; set; }
        public Brush Color { get; set; }
        public Brush HoverColor { get; set; }
        public ProcessData TaskData { get; set; }
    }
}
