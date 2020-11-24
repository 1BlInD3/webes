using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionMonitor.Models
{
    public class Unit
    {
        public string Description { get; set; }
        public int FirstShiftProd { get; set; }
        public int SecondShiftProd { get; set; }
        public int ThirdShiftProd { get; set; }
        public double FirstShiftWaste { get; set; }
        public double SecondShiftWaste { get; set; }
        public double ThirdShiftWaste { get; set; }
        public int[] ProductionHistory { get; set; }
        public double[] WasteHistory { get; set; }
    }
}