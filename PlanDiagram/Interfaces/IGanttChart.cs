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
        void Build(List<ProcessData> proc, List<DateTime> workDays, Canvas ganttCanvas);
    }
}