using ProductionMonitor.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonitorManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UnconfirmedDailyReports Reports { get; set; }
        private int currentPage;
        private bool started;

        public MainWindow()
        {
            InitializeComponent();
            if (Environment.UserName.ToLower() == "kovacsistvan" |
                Environment.UserName.ToLower() == "puskaattila")

            {
                //btnMonthlies.IsEnabled = true;
                btnGraph.IsEnabled = true;
            }
            ccReport.Content = new UnitSelectView(this);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            currentPage--;
            btnForth.IsEnabled = true;
            if (currentPage - 1 < 0) btnBack.IsEnabled = false;
            SetPage();
        }

        private void btnForth_Click(object sender, RoutedEventArgs e)
        {
            if (!started)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Reports = new UnconfirmedDailyReports((ccReport.Content as UnitSelectView).GetSelectedUnits());
                Mouse.OverrideCursor = Cursors.Arrow;
                if (Reports.Count == 1)
                {
                    btnForth.IsEnabled = false;
                    btnBack.IsEnabled = false;
                    btnDone.IsEnabled = true;
                    SetPage();
                }
                else if (Reports.Count == 0)
                {
                    btnDone.IsEnabled = false;
                    btnBack.IsEnabled = false;
                    btnForth.IsEnabled = false;
                    ccReport.Content = "Nincsenek jóváhagyásra váró adatok a választott cellákhoz!";
                }
                else
                {
                    btnDone.IsEnabled = false;
                    btnBack.IsEnabled = false;
                    btnForth.IsEnabled = true;
                    SetPage();
                }
                btnForth.Content = "Tovább";
                started = true;
                //btnMonthlies.IsEnabled = false;
            }
            else
            {
                currentPage++;
                btnBack.IsEnabled = true;
                if (currentPage + 1 >= Reports.Count)
                {
                    btnForth.IsEnabled = false;
                    btnDone.IsEnabled = true;
                }
                SetPage();
            }
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            if (ccReport.Content is MonthliesView)
            {
                try
                {
                    (ccReport.Content as MonthliesView).FinalizeChanges();
                    MessageBox.Show("Az adatok jóváhagyása befejeződött!", "Jóváhagyás", MessageBoxButton.OK, MessageBoxImage.Information);
                    ccReport.Content = "Nincsenek jóváhagyásra váró adatok!";
                    btnDone.IsEnabled = false;
                }
                catch
                {
                    MessageBox.Show("Az adatok jóváhagyása sikertelen! Számok azok, Puska?", "Jóváhagyás", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
            btnDone.IsEnabled = false;
            btnBack.IsEnabled = false;
            btnForth.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            Reports.Upload();
            foreach (var n in GetUnitNames()) ChartGenerator.GenerateCharts(new ProductionHistory(n));
            Mouse.OverrideCursor = Cursors.Arrow;
            MessageBox.Show("Az adatok jóváhagyása befejeződött!", "Jóváhagyás", MessageBoxButton.OK, MessageBoxImage.Information);
            ccReport.Content = "Nincsenek jóváhagyásra váró adatok!";
            tbPages.Text = string.Empty;
        }

        private void SetPage()
        {
            ccReport.Content = new ReportView(Reports[currentPage]);
            tbPages.Text = (currentPage + 1) + "/" + Reports.Count;
        }

        public static IEnumerable<string> GetUnitNames()
        {
            var connStr = @"data source=Scala1;
                            initial catalog = Fusetech; 
                            user id = scala_read; 
                            password=scala_read;";

            var sql = @"SELECT DISTINCT([UnitName])
                        FROM [Fusetech].[dbo].[PmShift]";

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            yield return rdr[0].ToString();
                        }
                    }
                }
            }
        }

        private void btnMonthlies_Click(object sender, RoutedEventArgs e)
        {
            //btnMonthlies.IsEnabled = false;
            btnBack.IsEnabled = false;
            btnForth.IsEnabled = false;
            btnDone.IsEnabled = true;
            ccReport.Content = new MonthliesView(this);
        }

        private void btnGraph_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            foreach (var n in GetUnitNames()) ChartGenerator.GenerateCharts(new ProductionHistory(n));
            Mouse.OverrideCursor = Cursors.Arrow;
        }
    }
}
