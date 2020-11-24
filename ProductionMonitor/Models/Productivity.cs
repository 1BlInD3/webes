namespace ProductionMonitor.Models
{
    using ProductionMonitor.Utils;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;

    /// <summary>
    /// Stores data used to calculate productivity.
    /// </summary>
    public class ProductivityReport
    {
        /// <summary>
        /// The target growth of productivity (%)
        /// </summary>
        public static readonly decimal TARGET_PRODUCTIVITY_GROWTH = 4.0M;

        public string Munkahely { get; set; }
        public decimal? NormaIdo { get; set; }
        public decimal? DolgOra { get; set; }
        public int? DolgozokSzama { get; set; }
        public decimal? Produktivitas
        {
            get
            {
                if (DolgOra == 0) return null;
                else return NormaIdo / DolgOra * 100;
            }
        }

        /// <summary>
        /// Gets the calculated productivity or the amount of hours worked
        /// (when norm is 0 and is requested) as a presentable string
        /// with the unit of measurement.
        /// </summary>
        /// <param name="decimalPlaces">The accuracy of producivity returned</param>
        /// <param name="returnDolgOraWhenApplicable">Option to return hours worked when productivity is 0</param>
        /// <returns>The productivity or hours worked converted to string</returns>
        public string GetProduktivitas(int decimalPlaces = 0, bool returnDolgOraWhenApplicable = true)
        {
            var stringFormat = "0";

            if (decimalPlaces > 0)
            {
                stringFormat += ".";
                for (int i = 0; i < decimalPlaces; i++)
                    stringFormat += "#";
            }

            return Produktivitas == null && DolgOra > 0 && returnDolgOraWhenApplicable ? GetDolgOra() :
                   NormaIdo > 0 && (DolgOra == null | DolgOra == 0) ? "NaN" :
                   Produktivitas == null ? "—" :
                   (((double)Produktivitas).ToString(stringFormat) + "%");
        }
        /// <summary>
        /// Gets the target amount of hours to be worked as a presentable string
        /// with the unit of measurement.
        /// </summary>
        /// <returns>The target amount of hours converted to string</returns>
        public string GetNormaIdo()
            => NormaIdo == null ? "—" : ((Math.Round((decimal)NormaIdo)).ToString() + "ó");
        /// <summary>
        /// Gets the amount of hours worked as a presentable string
        /// with the unit of measurement.
        /// </summary>
        /// <returns>The amount of hours worked converted to string</returns>
        public string GetDolgOra()
            => DolgOra == null ? "—" : ((Math.Round((decimal)DolgOra)).ToString() + "ó");
        /// <summary>
        /// Gets the number of people worked as a presentable string
        /// with the unit of measurement.
        /// </summary>
        /// <returns>The number of people worked converted to string</returns>
        public string GetDolgozokSzama()
            => DolgozokSzama == null ? "—" : (((int)DolgozokSzama).ToString() + "fő");
        /// <summary>
        /// Gets a string describing a color function used in markup
        /// based on the target productivity and actual productivity.
        /// </summary>
        /// <param name="targetProductivity">The target productivity to which the colors are adjusted</param>
        /// <returns>A string describing a color function used in markup</returns>
        public string GetColor(decimal? targetProductivity = 100.0M)
        {
            targetProductivity = targetProductivity ?? 100.0M;
            // no hours worked
            if (DolgOra == null | DolgOra == 0) return ColorData.Green.ToFunctionString();
            // some hours worked
            if (NormaIdo == null | NormaIdo == 0) return ColorData.Red.ToFunctionString();
            // norma > 0 && dolgOra > 0 && prod != null
            if (Produktivitas >= targetProductivity + TARGET_PRODUCTIVITY_GROWTH) return ColorData.Green.ToFunctionString();
            else if (Produktivitas >= targetProductivity) return ColorData.Yellow.ToFunctionString();
            else return ColorData.Red.ToFunctionString();
        }

        /// <summary>
        /// Adds two ProductivityReports together to calculate sums of productivities.
        /// </summary>
        /// <param name="x">One of the reports</param>
        /// <param name="y">The other report</param>
        /// <returns>The sum of the two reports</returns>
        public static ProductivityReport operator +(ProductivityReport x, ProductivityReport y)
        {
            decimal normaIdo = 0, dolgOra = 0;
            int dolgozokSzama = 0;

            string munkahely = x.Munkahely;
            if (!string.IsNullOrEmpty(munkahely)) munkahely += " + ";
            munkahely += y.Munkahely;

            normaIdo += ((decimal)(x?.NormaIdo ?? 0) + (decimal)(y?.NormaIdo ?? 0));
            dolgOra += ((decimal)(x?.DolgOra ?? 0) + (decimal)(y?.DolgOra ?? 0));
            dolgozokSzama += ((int)(x?.DolgozokSzama ?? 0) + (int)(y?.DolgozokSzama ?? 0));

            return new ProductivityReport
            {
                Munkahely = munkahely,
                NormaIdo = normaIdo,
                DolgOra = dolgOra,
                DolgozokSzama = dolgozokSzama
            };
        }

        public override string ToString()
        {
            return $"{nameof(Munkahely)} = {Munkahely}\n" +
                   $"{nameof(NormaIdo)} = {GetNormaIdo()}\n" +
                   $"{nameof(DolgOra)} = {GetDolgOra()}\n" +
                   $"{nameof(DolgozokSzama)} = {GetDolgozokSzama()}\n" +
                   $"{nameof(Produktivitas)} = {GetProduktivitas(1)}\n";
        }
    }

    /// <summary>
    /// A design class to be inherited. Represents a collection of reports.
    /// </summary>
    public abstract class ProductivityReportCollection : List<ProductivityReport>
    {
        /// <summary>
        /// Gets the first report associated with the name of a workstation
        /// within the collection.
        /// </summary>
        /// <param name="munkahely">The name of the workstation</param>
        /// <returns>The first report associated with the name of the
        /// workstation or null</returns>
        public virtual ProductivityReport this[string munkahely]
        {
            get
            {
                return this.FirstOrDefault(x => x.Munkahely == munkahely);
            }
        }
    }

    /// <summary>
    /// A collection of productivity reports for a given date.
    /// </summary>
    public class DailyProductivityReportCollection : ProductivityReportCollection
    {
        //private DailyProductivityReportCollection lastProductiveDaysReport;
        //public DailyProductivityReportCollection LastProductiveDaysReport
        //{
        //    get
        //    {
        //        if (lastProductiveDaysReport == null)
        //        {
        //            var date = DateTime.Today;
        //            lastProductiveDaysReport = GetDailyReport(date);

        //            while (lastProductiveDaysReport.Sum(x => x.DolgOra) < 250)
        //            {
        //                date -= new TimeSpan(24, 0, 0);
        //                lastProductiveDaysReport = GetDailyReport(date);
        //            }
        //        }

        //        return lastProductiveDaysReport;
        //    }
        //}

        public static DailyProductivityReportCollection GetLastProductiveDaysReport()
        {
            var date = DateTime.Today;
            var lastProductiveDaysReport = GetDailyReport(date);

            while (lastProductiveDaysReport.Sum(x => x.DolgOra) < 250)
            {
                date -= new TimeSpan(24, 0, 0);
                lastProductiveDaysReport = GetDailyReport(date);
            }

            return lastProductiveDaysReport;
        }

        public DateTime Date { get; private set; }

        /// <summary>
        /// Gets a collection of productivity reports for a given date.
        /// </summary>
        /// <param name="date">The date associated with the reports</param>
        /// <returns>The collection of reports for the given date</returns>
        public static DailyProductivityReportCollection GetDailyReport(DateTime date)
        {
            var sql = $@"SELECT case when Munkahely = 'ÁRAMVÁLTÓ CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'ÁRAMVÁLTÓ TRANING' then 'ÁRAMVÁLTÓ EXTRA'
			                          when Munkahely = 'HH CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'HH TRANING' OR 
				                           Munkahely = 'MV CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'MV TRANING' then 'HH MV EXTRA'
			                          when Munkahely = 'NH-MERSEN CELLA VEZ.,GÉPBEÁLL., ELŐKÉSZ.' OR Munkahely = 'NH-MERSEN TRANING' then 'NH-MERSEN EXTRA'
			                          when Munkahely = 'NH-MSCH CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'NH-MSCH TRANING' then 'NH-MSCH EXTRA'
			                          when Munkahely = 'NH-WEBER CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'NH-WEBER TRANING' then 'NH-WEBER EXTRA'
			                          when Munkahely = 'SPEC TRANING' then 'SPEC EXTRA'
			                          when Munkahely = 'CÍMKE NYOMT.,SELEJTEZÉS' then 'WASTE'
			                          when Munkahely = '-TROGGER' then 'TROGGER'
			                          else Munkahely end as Munkahely
                              ,isnull(SUM(Ido),0) as NormaIdo
	                          ,SUM(DO.DolgOra) as DolgOra
	                          ,SUM(DO.DolgozokSzama) as DolgozokSzama

                        FROM BiReports.dbo.VBI_LejelentettNormaIdo 
                        left outer join BiReports.dbo.VBI_LedolgOrak as DO on Munkahely=DO.MunkaHely and LejelentesIdeje=DO.BelepesIdeje

                        where LejelentesIdeje='{((DateTime)date).Year.ToString()}-{((DateTime)date).Month.ToString()}-{((DateTime)date).Day.ToString()}' and Munkahely != 'BRAZING'

                        group by case when Munkahely = 'ÁRAMVÁLTÓ CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'ÁRAMVÁLTÓ TRANING' then 'ÁRAMVÁLTÓ EXTRA'
			                          when Munkahely = 'HH CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'HH TRANING' OR 
				                           Munkahely = 'MV CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'MV TRANING' then 'HH MV EXTRA'
			                          when Munkahely = 'NH-MERSEN CELLA VEZ.,GÉPBEÁLL., ELŐKÉSZ.' OR Munkahely = 'NH-MERSEN TRANING' then 'NH-MERSEN EXTRA'
			                          when Munkahely = 'NH-MSCH CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'NH-MSCH TRANING' then 'NH-MSCH EXTRA'
			                          when Munkahely = 'NH-WEBER CELLA VEZ.,GÉPBEÁLL.,ELŐKÉSZ.' OR Munkahely = 'NH-WEBER TRANING' then 'NH-WEBER EXTRA'
			                          when Munkahely = 'SPEC TRANING' then 'SPEC EXTRA'
			                          when Munkahely = 'CÍMKE NYOMT.,SELEJTEZÉS' then 'WASTE'
			                          when Munkahely = '-TROGGER' then 'TROGGER'
			                          else Munkahely end";

            var list = new DailyProductivityReportCollection() { Date = date };
            using (var conn = new SqlConnection(Utils.StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            bool normaIdoParsed = decimal.TryParse(rdr[1].ToString(), out decimal normaIdo);
                            bool dolgoraParsed = decimal.TryParse(rdr[2].ToString(), out decimal dolgOra);
                            bool dolgSzamaParsed = int.TryParse(rdr[3].ToString(), out int dolgSzama);

                            list.Add(new ProductivityReport
                            {
                                Munkahely = rdr[0].ToString().Trim(),
                                NormaIdo = normaIdoParsed ? (decimal?)normaIdo : (decimal?)null,
                                DolgOra = dolgoraParsed ? (decimal?)dolgOra : (decimal?)null,
                                DolgozokSzama = dolgSzamaParsed ? dolgSzama : (int?)null
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets a single productivity report from the collection associated
        /// with the name of the workstation.
        /// </summary>
        /// <param name="munkahely">The name of the workstation</param>
        /// <returns>The report from the collection associated with 
        /// the name of the workstation</returns>
        public ProductivityReport GetReport(string munkahely)
        {
            return this[munkahely];
        }
        /// <summary>
        /// Gets the sum of the reports within the collection which names
        /// are listed in the input argument.
        /// </summary>
        /// <param name="munkahelyek">The array of workstation names to be summed</param>
        /// <returns>The summed productivity report</returns>
        public ProductivityReport GetSumOfReports(string[] munkahelyek)
        {
            ProductivityReport total = new ProductivityReport { DolgOra = 0, DolgozokSzama = 0, NormaIdo = 0 };
            foreach (var munkahely in munkahelyek)
                total += this[munkahely];
            return total;
        }
    }

    /// <summary>
    /// A collection of productivity reports for a given year.
    /// </summary>
    public class YearlyProductivityReportCollection : ProductivityReportCollection
    {
        private static YearlyProductivityReportCollection lastYearsReport;
        public static YearlyProductivityReportCollection LastYearsReport
        {
            get => lastYearsReport ?? (lastYearsReport = GetLastYearsReport());
        }

        /// <summary>
        /// Gets a collection of productivity reports for a given year.
        /// </summary>
        /// <param name="date">The year associated with the reports</param>
        /// <returns>The collection of reports for the given year</returns>
        public static YearlyProductivityReportCollection GetLastYearsReport()
        {
            var sql = $@"SELECT Munkahely, [NormaIdo], [DolgOra]
                          FROM [BiReports].[dbo].[Productivity_Yearly]
                          where Year = {DateTime.Now.Year - 1}";

            var list = new YearlyProductivityReportCollection();// { Year = year };
            using (var conn = new SqlConnection(Utils.StringData.ScalaReadConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            bool normaIdoParsed = decimal.TryParse(rdr[1].ToString(), out decimal normaIdo);
                            bool dolgoraParsed = decimal.TryParse(rdr[2].ToString(), out decimal dolgOra);

                            list.Add(new ProductivityReport
                            {
                                Munkahely = rdr[0].ToString().Trim(),
                                NormaIdo = normaIdoParsed ? (decimal?)normaIdo : (decimal?)null,
                                DolgOra = dolgoraParsed ? (decimal?)dolgOra : (decimal?)null
                            });
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets the calculated productivity or the amount of hours worked
        /// (when norm is 0 and is requested) for last year as a presentable 
        /// string with the unit of measurement.
        /// </summary>
        /// <param name="munkahely">The name of the workstation</param>
        /// <param name="decimalPlaces">The accuracy of producivity returned</param>
        /// <returns>The productivity or hours worked converted to string</returns>
        public static string GetProduktivitas(string munkahely, int decimalPlaces = 1)
        {
            string produktivitas = LastYearsReport[munkahely]?.GetProduktivitas(decimalPlaces, returnDolgOraWhenApplicable:true);
            return produktivitas.Last() == 'ó' ? "—" : produktivitas;
        }
        /// <summary>
        /// Gets the calculated productivity or the amount of hours worked
        /// (when norm is 0 and is requested) for the workstations which names
        /// are listed in the input argument as a string with the unit of measurement.
        /// </summary>
        /// <param name="munkahelyek">The array of workstation names to be summed</param>
        /// <param name="decimalPlaces">The accuracy of producivity returned</param>
        /// <returns>The productivity or hours worked converted to string</returns>
        public static string GetSummedProduktivitas(string[] munkahelyek, int decimalPlaces = 0)
        {
            ProductivityReport totals = new ProductivityReport() { DolgOra = 0, NormaIdo = 0, DolgozokSzama = 0 };
            foreach (var munkahely in munkahelyek) totals += LastYearsReport[munkahely];
            return totals.GetProduktivitas(decimalPlaces);
        }
    }

    /// <summary>
    /// A collection of daily productivity report collection.
    /// </summary>
    public class WeeklyProductivityReportCollection : List<DailyProductivityReportCollection>
    {
        /// <summary>
        /// The number of days stored in the collection.
        /// </summary>
        public static readonly int NUMBER_OF_DAYS = 3;
        /// <summary>
        /// The minimum number of total hours needed to be worked a day in order for it not 
        /// to be ignored when pulling the weekly report.
        /// </summary>
        public static readonly int WORKHOUR_THRESHOLD = 250;

        //private WeeklyProductivityReportCollection weeklyReport;
        //public WeeklyProductivityReportCollection WeeklyReport
        //{
        //    get => weeklyReport ?? (weeklyReport = GetWeeklyReport());
        //}

        /// <summary>
        /// Gets a collection of daily reports for the amount of days stored in <see cref="NUMBER_OF_DAYS"/>
        /// field in chronological order. The last day will be the lastest report available.
        /// Days with the total hours worked less than the amount stored in <see cref="WORKHOUR_THRESHOLD"/>
        /// field are ignored.
        /// </summary>
        /// <returns>A collection of daily reports in chronological order</returns>
        public static WeeklyProductivityReportCollection GetWeeklyReport()
        {
            var weeklyReports = new WeeklyProductivityReportCollection();
            var date = DateTime.Today;

            do
            {
                var temp = DailyProductivityReportCollection.GetDailyReport(date);
                date -= new TimeSpan(24, 0, 0);
                if (temp.Sum(x => x.DolgOra) < WORKHOUR_THRESHOLD) continue;
                else weeklyReports.Insert(0, temp);
            }
            while (weeklyReports.Count < NUMBER_OF_DAYS);

            return weeklyReports;
        }

        /// <summary>
        /// Gets the calculated productivity or the amount of hours worked
        /// (when norm is 0 and is requested) for the given workstation and
        /// day index as a presentable string with the unit of measurement.
        /// </summary>
        /// <param name="munkahely">The name of the workstation</param>
        /// <param name="dayIndex">The index of the day within the collection</param>
        /// <param name="decimalPlaces">The accuracy of producivity returned</param>
        /// <returns>The productivity or hours worked converted to string</returns>
        public string GetProduktivitas(string munkahely, int dayIndex, int decimalPlaces = 0)
        {
            return this[dayIndex][munkahely]?.GetProduktivitas(decimalPlaces, returnDolgOraWhenApplicable:true);
        }
        /// <summary>
        /// Gets the calculated productivity or the amount of hours worked
        /// (when norm is 0 and is requested) for the given workstation names
        /// and day index as a presentable string with the unit of measurement.
        /// </summary>
        /// <param name="munkahelyek">The array of workstation names to be summed</param>
        /// <param name="dayIndex">The index of the day within the collection</param>
        /// <param name="decimalPlaces">The accuracy of producivity returned</param>
        /// <returns>The productivity or hours worked converted to string</returns>
        public string GetSummedProduktivitas(string[] munkahelyek, int dayIndex, int decimalPlaces = 0)
        {
            ProductivityReport totals = new ProductivityReport() { DolgOra = 0, NormaIdo = 0, DolgozokSzama = 0 };
            foreach (var munkahely in munkahelyek) totals += this[dayIndex][munkahely];
            return totals.GetProduktivitas(decimalPlaces);
        }
        /// <summary>
        /// Gets a string describing a color function used in markup
        /// based on the target productivity and actual productivity
        /// for the given workstation and day.
        /// </summary>
        /// <param name="munkahely">The name of the workstation</param>
        /// <param name="dayIndex">The index of the day within the collection</param>
        /// <returns>A string describing a color function used in markup</returns>
        public string GetColor(string munkahely, int dayIndex)
        {
            var dailyReportCollection = this[dayIndex];
            var stationsDailyReport = dailyReportCollection[munkahely];
            var stationsLastYearsReport = YearlyProductivityReportCollection.LastYearsReport[munkahely];

            if (stationsDailyReport == null || stationsLastYearsReport == null) return null;

            return stationsDailyReport.GetColor(stationsLastYearsReport.Produktivitas);
        }
        /// <summary>
        /// Gets a string describing a color function used in markup
        /// based on the target productivity and actual productivity for the sum
        /// of the workstations listed in the input argument and for the given day.
        /// </summary>
        /// <param name="munkahelyek">The array of workstation names to be summed</param>
        /// <param name="dayIndex">The index of the day within the collection</param>
        /// <returns></returns>
        public string GetSummedColor(string[] munkahelyek, int dayIndex)
        {
            ProductivityReport dailyTotals = new ProductivityReport() { DolgOra = 0, NormaIdo = 0, DolgozokSzama = 0 };
            foreach (var munkahely in munkahelyek) dailyTotals += this[dayIndex][munkahely];

            ProductivityReport lastYearsTotals = new ProductivityReport() { DolgOra = 0, NormaIdo = 0, DolgozokSzama = 0 };
            foreach (var munkahely in munkahelyek) lastYearsTotals += YearlyProductivityReportCollection.LastYearsReport[munkahely];

            if (dailyTotals == null || lastYearsTotals == null) return null;

            return dailyTotals.GetColor(lastYearsTotals.Produktivitas);
        }
    }
}