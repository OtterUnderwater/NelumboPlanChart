using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlanDiagram.Helpers
{
    public static class DateHelper
    {
        public static bool IsDateRange(DateTime taskStart, DateTime taskEnd, DateTime filterStart, DateTime filterEnd)
        {
            return Max(taskStart, filterStart) <= Min(taskEnd, filterEnd);
        }
        private static DateTime Max(DateTime date1, DateTime date2) => date1 > date2 ? date1 : date2;
        private static DateTime Min(DateTime date1, DateTime date2) => date1 < date2 ? date1 : date2;
    }
}
