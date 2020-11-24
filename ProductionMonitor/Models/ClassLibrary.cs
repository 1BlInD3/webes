namespace ProductionMonitor.Models
{
    using ProductionMonitor.Utils;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;

    /// <summary>
    /// output class for presenting the daily data on screens
    /// </summary>
    public partial class Shift
    {
        public int Id { get; set; }
        public string UnitName { get; set; }
        public DateTime Date { get; set; }
        public short ShiftStart { get; set; }
        public short ShiftLength { get; set; }
        public int ProducedPieces { get; set; }
        public decimal ProducedValue { get; set; }
        public decimal WastedPieces { get; set; }
        public decimal WastedValue { get; set; }
        public int ShiftTarget { get; set; }

        public decimal GetProducedPercentage()
        {
            return ((decimal)ProducedPieces) / ((decimal)ShiftTarget / 100);
        }
        public decimal GetWastedPercentage()
        {
            return (decimal)WastedValue / ((decimal)ProducedValue / 100);
        }

        public bool IsTwelveHour()
            => this.ShiftLength == 12;
        public bool IsFirst()
            => this.ShiftStart == 0;
        public bool IsSecond()
            => this.ShiftStart == 8 | this.ShiftStart == 12;
        public bool IsThird()
            => this.ShiftStart + this.ShiftLength == 24;
        public string GetEfficiencyColor(char type)
        {
            if (type == 'p' || type == 'P')
            {
                if (this.GetProducedPercentage() <= 90) return ColorData.Red.ToFunctionString();
                else if (this.GetProducedPercentage() < 100) return ColorData.Yellow.ToFunctionString();
                else return ColorData.Green.ToFunctionString();
            }
            else
            {
                if (this.GetWastedPercentage() <= 0.65M) return ColorData.Green.ToFunctionString();
                else return ColorData.Red.ToFunctionString();
            }
        }
    }

    /// <summary>
    /// output class for representing daily data on screens for a unit
    /// </summary>
    public partial class ProductionDay
    {
        public string UnitName { get; }
        public DateTime Date { get; }
        public List<Shift> Shifts { get; set; }

        public ProductionDay()
        {
            this.UnitName = string.Empty;
            this.Date = DateTime.MinValue;
            this.Shifts = new List<Shift>();
        }
        public ProductionDay(string UnitName, DateTime Date)
        {
            this.UnitName = UnitName;
            this.Date = Date;
            this.Shifts = new List<Shift>();
            PopulateShifts();
        }

        private void PopulateShifts()
        {
            var sql = @"SELECT [Id],[UnitName],[Date],[ShiftStart],[ShiftLength],[ProducedPieces],
                               [ProducedValue],[WastedPieces],[WastedValue],[ShiftTarget]  
                        FROM [Fusetech].[dbo].[PmShift]
                        WHERE UnitName = '" + UnitName + "' and Date = '" + Date.ToString("yyyy-MM-dd") + "'";

            using (var conn = new SqlConnection(Utils.StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Shifts.Add(new Shift()
                            {
                                Id = int.Parse(rdr[0].ToString()),
                                UnitName = rdr[1].ToString(),
                                Date = DateTime.Parse(rdr[2].ToString()),
                                ShiftStart = short.Parse(rdr[3].ToString()),
                                ShiftLength = short.Parse(rdr[4].ToString()),
                                ProducedPieces = int.Parse(rdr[5].ToString()),
                                ProducedValue = decimal.Parse(rdr[6].ToString()),
                                WastedPieces = decimal.Parse(rdr[7].ToString()),
                                WastedValue = decimal.Parse(rdr[8].ToString()),
                                ShiftTarget = int.Parse(rdr[9].ToString())
                            });
                        }
                    }
                }
            }
        }
        public decimal GetProducedPercentage()
        {
            return (decimal)Shifts.Select(s => s.ProducedPieces).Sum() / ((decimal)Shifts.Select(s => s.ShiftTarget).Sum() / 100);
        }
        public decimal GetWastedPercentage()
        {
            return (decimal)Shifts.Select(s => s.WastedValue).Sum() / ((decimal)Shifts.Select(s => s.ProducedValue).Sum() / 100);
        }

        public string GetDateStr()
            => this.Date == DateTime.MinValue ? "N/A" : Date.ToString("MM-dd");
        public bool IsTwelveHourDay()
            => this.Shifts.Where(s => s.ShiftLength == 12).Count() > 0;
        public bool ShiftExists(short start)
            => this.Shifts.FirstOrDefault(s => s.ShiftStart == start) != null;
        public Shift GetShift(short start)
            => this.Shifts.FirstOrDefault(s => s.ShiftStart == start);
    }

    /// <summary>
    /// output class for representing weekly data on screens for a unit
    /// </summary>
    public partial class ProductionHistory : List<ProductionDay>
    {
        public const int NUM_OF_DAYS = 5;
        public string UnitName { get; }

        public ProductionHistory(string UnitName)
        {
            this.UnitName = UnitName;
            Populate();
        }

        public void Populate()
        {
            var sql = @"SELECT TOP(" + NUM_OF_DAYS + @") [Date]
                        FROM [Fusetech].[dbo].[PmShift]
                        where UnitName='" + UnitName + @"'
                        group by UnitName,Date
                        order by Date desc";

            using (var conn = new SqlConnection(Utils.StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            this.Add(new ProductionDay(UnitName, DateTime.Parse(rdr[0].ToString())));
                        }
                    }
                }
            }

            this.Reverse();
        }

        public string GetProdChartPath()
            => @"/Image/Graphs/" + this.UnitName + "p.png";
        public string GetWasteChartPath()
            => @"/Image/Graphs/" + this.UnitName + "w.png";
    }

    public class MonthlyFacts : List<MonthlyProduction>
    {
        public MonthlyFacts()
        {
            Populate();
        }

        public void Populate()
        {
            var sql = @"SELECT UnitName, Target, Fact FROM [Fusetech].[dbo].[PmMonthly]";

            using (var conn = new SqlConnection(Utils.StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var unit = rdr[0].ToString();
                            var target = int.Parse(rdr[1].ToString());
                            var fact = int.Parse(rdr[2].ToString());
                            this.Add(new MonthlyProduction() { UnitName = unit, Target = target, Produced = fact });
                        }
                    }
                }
            }

            sql = @"SELECT [Port],[Norma_muszak] FROM [Fusetech].[dbo].[Munkahelyek]";

            using (var conn = new SqlConnection(Utils.StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var unit = rdr[0].ToString();
                            var norm = int.Parse(rdr[1].ToString());
                            if (this.FirstOrDefault(c => c.UnitName == unit) != null) this.First(c => c.UnitName == unit).Target = norm;
                        }
                    }
                }
            }
        }
        public void Upload()
        {
            foreach (var mf in this)
            {
                var sql = @"update [Fusetech].[dbo].[PmMonthly] set Target=" + mf.Target + ", Fact=" + mf.Produced + " where UnitName='" + mf.UnitName + "'";

                using (var conn = new SqlConnection(Utils.StringData.ScalaWriteConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    public class MonthlyProduction
    {
        public string UnitName { get; set; }
        public int Target { get; set; }
        public int Produced { get; set; }

        public string GetEfficiencyColor()
        {
            return Produced < Target ? ColorData.Red.ToFunctionString() : ColorData.Green.ToFunctionString();
        }
    }
}