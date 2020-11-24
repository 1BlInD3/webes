using ProductionMonitor.Models;
using ProductionMonitor.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorManager
{
    /// <summary>
    /// represents a single station
    /// </summary>
    class Unit
    {
        public string Name { get; }
        public decimal Norm { get; }
        public List<DailyReport> DailyReports { get; set; }

        public Unit(string Name, decimal Norm)
        {
            this.Name = Name;
            this.Norm = Norm;
            this.DailyReports = new List<DailyReport>();
        }

        public void PopulateDailyReports(DateTime from, DateTime thru)
        {
            foreach (var d in EachDay(from, thru))
                DailyReports.Add(new DailyReport(this.Name, this.Norm, d));
        }
        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
        public DateTime? GetLastUploadedDataDate()
        {
            DateTime? date = null;

            var sql = @"SELECT TOP (1) [Date]
                        FROM[Fusetech].[dbo].[PmShift]
                        where UnitName = '" + this.Name + @"'
                        order by Date desc";

            using (var conn = new SqlConnection(StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read()) date = DateTime.Parse(rdr[0].ToString());
                    }
                }
            }

            return date;
        }
    }

    /// <summary>
    /// collection of transactions for a given day (from 6:00 to 6:00) and unit
    /// </summary>
    public class DailyReport : List<Report>
    {
        public string UnitName { get; }
        public decimal UnitNorm { get; }
        public DateTime Date { get; set; }
        public List<Shift> Shifts { get; set; }

        public DailyReport(string UnitName, decimal UnitNorm, DateTime Date)
        {
            this.UnitName = UnitName;
            this.UnitNorm = UnitNorm;
            this.Date = Date;
            this.Shifts = new List<Shift>();
            Populate();
            SetEightHourShifts();
        }

        private void Populate()
        {
            var sqlMain = @"SELECT case when Munkahely_R = 'ÁRAMVÁLTÓ2' then 'ÁRAMVÁLTÓ'
	                           else Munkahely_R end as Port,
	                           DATEADD(MINUTE, -(5*60+55), JelentIdo) as Ido, 
                                case when VF_SC010300_Stockfile.ProductGroup = '0180' and 
	                                (SUBSTRING(VF_SC010300_Stockfile.StockCode, 10, 1) = '3' or 
	                                SUBSTRING(VF_SC010300_Stockfile.StockCode, 11, 1) = '3') then Mennyiseg * 3
	                                else Mennyiseg end as Darab,
	                                    case when VF_SC010300_Stockfile.ProductGroup = '0180' and 
	                                (SUBSTRING(VF_SC010300_Stockfile.StockCode, 10, 1) = '3' or 
	                                SUBSTRING(VF_SC010300_Stockfile.StockCode, 11, 1) = '3') then StdCostPric1 / 3
	                                else StdCostPric1 end as Ertek
                        FROM Fusetech.dbo.MrendiJelentett
                        inner join [Fusetech].dbo.Munkarendeles on MRendeles_R = MRendeles
                        inner join [ScaCompDB].dbo.VF_SC010300_Stockfile on Cikkszam = StockCode ";

            var sqlWhere = @"where Munkahely_R = '" + UnitName + "' and CONVERT(date,DATEADD(MINUTE, -(5*60+55), JelentIdo)) = '" + Date.ToString("yyyy-MM-dd") + "'";
            if (this.IsAramvalto()) sqlWhere = @"where (Munkahely_R = 'ÁRAMVÁLTÓ' or Munkahely_R = 'ÁRAMVÁLTÓ2') and CONVERT(date,DATEADD(MINUTE, -(5*60+55), JelentIdo)) = '" + Date.ToString("yyyy-MM-dd") + "'";

            var sql = sqlMain + " " + sqlWhere;

            using (var conn = new SqlConnection(StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            this.Add(new Report(DateTime.Parse(rdr[1].ToString()),
                                                decimal.Parse(rdr[2].ToString()),
                                                decimal.Parse(rdr[3].ToString()),
                                                false));
                        }
                    }
                }
            }

            sqlMain = @"SELECT Port, DATEADD(MINUTE, -(5*60+55), Sel_datum) as Ido, Menny-VisszMenny as Darab, StdCostPric1 as Ertek
                    FROM Fusetech.dbo.Selejt
                    inner join [ScaCompDB].dbo.VF_SC010300_Stockfile on Cikkszam = StockCode ";

            sqlWhere = @"where Port = '" + UnitName + "' and CONVERT(date, DATEADD(MINUTE, -(5 * 60 + 55), Sel_datum)) = '" +
                       Date.ToString("yyyy-MM-dd") + "' and Status=3 and SelejtFajta>3";
            if (this.IsAramvalto()) sqlWhere = @"where (Port = 'ÁRAMVÁLTÓ' or Port = 'ÁRAMVÁLTÓ2') and CONVERT(date, DATEADD(MINUTE, -(5 * 60 + 55), Sel_datum)) = '" +
                       Date.ToString("yyyy-MM-dd") + "' and Status=3 and SelejtFajta>3";

            sql = sqlMain + " " + sqlWhere;

            using (var conn = new SqlConnection(StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            this.Add(new Report(DateTime.Parse(rdr[1].ToString()),
                                                decimal.Parse(rdr[2].ToString()),
                                                decimal.Parse(rdr[3].ToString()),
                                                true));
                        }
                    }
                }
            }
        }
        public Shift GetShift(short startHour, short length)
        {
            var prodReports = this.Where(w => !w.IsWaste && w.Time.Hour >= startHour && w.Time.Hour < startHour + length);
            var wasteReports = this.Where(w => w.IsWaste && w.Time.Hour >= startHour && w.Time.Hour < startHour + length);
            return new Shift()
            {
                UnitName = this.UnitName,
                Date = this.Date,
                ShiftStart = startHour,
                ShiftLength = length,
                ProducedPieces = (int)prodReports.Select(r => r.Amount).Sum(),
                ProducedValue = (decimal)prodReports.Select(r => r.GetValue()).Sum(),
                WastedPieces = (decimal)wasteReports.Select(r => r.Amount).Sum(),
                WastedValue = (decimal)wasteReports.Select(r => r.GetValue()).Sum(),
                ShiftTarget = (int)(length * this.UnitNorm) //does not apply for ARAMVALTO
            };
        }
        public void SetEightHourShifts()
        {
            this.Shifts = new List<Shift>();
            for (int i = 0; i < 3; i++)
            {
                var s = GetShift((short)(i * 8), 8);
                if (s.ProducedPieces > 0) Shifts.Add(s);
                else Shifts.Add(null);
            }
            if (this.IsAramvalto())
            {
                int numOfShifts = 0;
                for (int i = 0; i < 3; i++) if (this.Shifts[i] != null) numOfShifts++;
                for (int i = 0; i < 3; i++) if (this.Shifts[i] != null) this.Shifts[i].ShiftTarget = (int)(this.UnitNorm * 8 / numOfShifts);
            }
        }
        public void SetTwelveHourShifts()
        {
            this.Shifts = new List<Shift>();
            for (int i = 0; i < 2; i++)
            {
                var s = GetShift((short)(i * 12), 12);
                if (s.ProducedPieces > 0) Shifts.Add(s);
                else Shifts.Add(null);
            }
            // áramváltón a norma*8 a napi target műszakok számától függetlenül
            if (this.IsAramvalto())
            {
                int numOfShifts = 0;
                for (int i = 0; i < 2; i++) if (this.Shifts[i] != null) numOfShifts++;
                for (int i = 0; i < 2; i++) if (this.Shifts[i] != null) this.Shifts[i].ShiftTarget = (int)(this.UnitNorm * 8 / numOfShifts);
            }
            // hh-n és mv-n kismama műszak van 8-16, norma*8 a műszak target
            if (this.IsHH() | this.IsMV())
            {
                for (int i = 0; i < 2; i++) if (this.Shifts[i] != null) this.Shifts[i].ShiftTarget = (int)(this.UnitNorm * 8);
            }
        }
        public bool IsActiveProductionDay()
        {
            if (GetShift(0, 24).ProducedPieces > 0) return true;
            return false;
        }
        public bool IsAramvalto()
            => this.UnitName == "ÁRAMVÁLTÓ";
        public bool IsHH()
            => this.UnitName == "HH";
        public bool IsMV()
            => this.UnitName == "MV";
    }

    /// <summary>
    /// collection of unconfirmed daily reports since last confirmation or given date
    /// </summary>
    public class UnconfirmedDailyReports : List<DailyReport> //date
    {
        public UnconfirmedDailyReports(List<string> unitNames)
        {
            Populate(unitNames);
        }

        public void Populate(List<string> unitNames)
        {
            var units = new List<Unit>();

            var sql = @"SELECT RTRIM(Port) as Port, CONVERT(decimal(18,6),[Norma_muszak])/8 as Norma FROM[Fusetech].[dbo].[Munkahelyek] 
                        where NULLIF([Munkarendeles], '') IS NOT NULL group by RTRIM(Port), Norma_muszak";

            using (var conn = new SqlConnection(StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            units.Add(new Unit((string)rdr[0], (decimal)rdr[1]));
                        }
                    }
                }
            }

            foreach (var u in units)
            {
                if (unitNames.Contains(u.Name))
                {
                    DateTime? dt = u.GetLastUploadedDataDate();
                    DateTime from = dt == null ? DateTime.Today - new TimeSpan(24 * ProductionHistory.NUM_OF_DAYS, 0, 0) : (DateTime)dt + new TimeSpan(24, 0, 0);
                    DateTime thru = DateTime.Today - new TimeSpan(24, 0, 0);
                    foreach (var date in Unit.EachDay(from, thru))
                    {
                        var dr = new DailyReport(u.Name, u.Norm, date);
                        if (dr.IsActiveProductionDay()) this.Add(dr);
                    }
                }
            }
        }
        public void Upload()
        {
            var sql = @"INSERT INTO [Fusetech].[dbo].[PmShift] ([UnitName],[Date],[ShiftStart],[ShiftLength],[ProducedPieces],[ProducedValue],[WastedPieces],[WastedValue],[ShiftTarget])
                        VALUES (@UnitName,@Date,@ShiftStart,@ShiftLength,@ProducedPieces,@ProducedValue,@WastedPieces,@WastedValue,@ShiftTarget)";

            using (var conn = new SqlConnection(StringData.ScalaWriteConnStr))
            {
                conn.Open();
                foreach (var r in this)
                {
                    foreach (var s in r.Shifts)
                    {
                        if (s != null)
                            using (SqlCommand command = new SqlCommand(sql, conn))
                            {
                                command.Parameters.AddWithValue("@UnitName", s.UnitName);
                                command.Parameters.AddWithValue("@Date", s.Date);
                                command.Parameters.AddWithValue("@ShiftStart", s.ShiftStart);
                                command.Parameters.AddWithValue("@ShiftLength", s.ShiftLength);
                                command.Parameters.AddWithValue("@ProducedPieces", s.ProducedPieces);
                                command.Parameters.AddWithValue("@ProducedValue", s.ProducedValue);
                                command.Parameters.AddWithValue("@WastedPieces", s.WastedPieces);
                                command.Parameters.AddWithValue("@WastedValue", s.WastedValue);
                                command.Parameters.AddWithValue("@ShiftTarget", s.ShiftTarget);

                                int result = command.ExecuteNonQuery();
                            }
                    }
                }
            }
        }
    }

    /// <summary>
    /// represents one production/waste transaction
    /// </summary>
    public class Report
    {
        public DateTime Time { get; }
        public decimal Amount { get; }
        public decimal UnitPrice { get; }
        public bool IsWaste { get; set; }

        public Report(DateTime Time, decimal Amount, decimal UnitPrice, bool IsWaste)
        {
            this.Time = Time;
            this.Amount = Amount;
            this.UnitPrice = UnitPrice;
            this.IsWaste = IsWaste;
        }

        public decimal GetValue()
        {
            return Amount * UnitPrice;
        }
    }
}
