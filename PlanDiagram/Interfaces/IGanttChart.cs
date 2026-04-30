using PlanDiagram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PlanDiagram.Interfaces
{
    public interface IGanttChart
    {
        void Build(DateTime minDate, DateTime maxDate, List<ProcessData> processes, Canvas ganttCanvas);
        void DrawDateHeader(Canvas dateHeaderCanvas);
        double GetTotalWidth();
    }
}
