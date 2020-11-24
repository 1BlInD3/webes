using ProductionMonitor.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public DailyReport Report { get; set; }

        public ReportView(DailyReport report)
        {
            this.Report = report;
            InitializeComponent();
            if (Report.Shifts.Count == 0) Report.SetEightHourShifts();
            else if (Report.Shifts.Count == 2) cbTwelveHourDay.IsChecked = true;
            SetTextBlockValues();
            if (Report.IsHH() | Report.IsMV()) cbTwelveHourDay.IsChecked = true;
        }

        private void SetTextBlockValues()
        {
            tbName.Text = Report.UnitName;
            tbDate.Text = Report.Date.ToLongDateString();
            if (Report.Shifts.Count == 3)
            {
                tbShift1.Text = "Délelőtti műszak:";
                tbShift2.Text = "Délutáni műszak:";
                tbShift3.Text = "Éjszakai műszak:";
            }
            else if (Report.Shifts.Count == 2)
            {
                tbShift1.Text = "Nappali műszak:";
                tbShift2.Text = "Éjjeli műszak:";
                tbShift3.Text = "";
            }
            else
            {
                tbShift1.Text = "ERROR";
                tbShift2.Text = "ERROR";
                tbShift3.Text = "ERROR";
            }
            if (this.Report.Shifts[0] != null)
            {
                tbShift1Target.Text = this.Report.Shifts[0].ShiftTarget.ToString();
                tbShift1AmountP.Text = this.Report.Shifts[0].ProducedPieces.ToString();
                tbShift1PercentP.Text = this.Report.Shifts[0].GetProducedPercentage().ToString("N2");
                tbShift1AmountW.Text = this.Report.Shifts[0].WastedPieces.ToString("N2");
                tbShift1PercentW.Text = this.Report.Shifts[0].GetWastedPercentage().ToString("N2");
            }
            else
            {
                tbShift1Target.Text = "-";
                tbShift1AmountP.Text = "-";
                tbShift1PercentP.Text = "-";
                tbShift1AmountW.Text = "-";
                tbShift1PercentW.Text = "-";
            }
            if (this.Report.Shifts[1] != null)
            {
                tbShift2Target.Text = this.Report.Shifts[1].ShiftTarget.ToString();
                tbShift2AmountP.Text = this.Report.Shifts[1].ProducedPieces.ToString();
                tbShift2PercentP.Text = this.Report.Shifts[1].GetProducedPercentage().ToString("N2");
                tbShift2AmountW.Text = this.Report.Shifts[1].WastedPieces.ToString("N2");
                tbShift2PercentW.Text = this.Report.Shifts[1].GetWastedPercentage().ToString("N2");
            }
            else
            {
                tbShift2Target.Text = "-";
                tbShift2AmountP.Text = "-";
                tbShift2PercentP.Text = "-";
                tbShift2AmountW.Text = "-";
                tbShift2PercentW.Text = "-";
            }
            if (this.Report.Shifts.Count > 2)
            {
                if (this.Report.Shifts[2] != null)
                {
                    tbShift3Target.Text = this.Report.Shifts[2].ShiftTarget.ToString();
                    tbShift3AmountP.Text = this.Report.Shifts[2].ProducedPieces.ToString();
                    tbShift3PercentP.Text = this.Report.Shifts[2].GetProducedPercentage().ToString("N2");
                    tbShift3AmountW.Text = this.Report.Shifts[2].WastedPieces.ToString("N2");
                    tbShift3PercentW.Text = this.Report.Shifts[2].GetWastedPercentage().ToString("N2");
                }
                else
                {
                    tbShift3Target.Text = "-";
                    tbShift3AmountP.Text = "-";
                    tbShift3PercentP.Text = "-";
                    tbShift3AmountW.Text = "-";
                    tbShift3PercentW.Text = "-";
                }
            }
            else
            {
                tbShift3Target.Text = "";
                tbShift3AmountP.Text = "";
                tbShift3PercentP.Text = "";
                tbShift3AmountW.Text = "";
                tbShift3PercentW.Text = "";
            }
        }

        private void cbTwelveHourDay_Checked(object sender, RoutedEventArgs e)
        {
            Report.SetTwelveHourShifts();
            SetTextBlockValues();
        }

        private void cbTwelveHourDay_Unchecked(object sender, RoutedEventArgs e)
        {
            Report.SetEightHourShifts();
            SetTextBlockValues();
        }
    }
}
