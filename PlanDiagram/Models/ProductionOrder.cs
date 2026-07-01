using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanDiagram.Models
{
    public class ProductionOrder
    {
        public int OrderID { get; set; }
        public string DocNumber { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
