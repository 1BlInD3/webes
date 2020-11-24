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
    /// Interaction logic for MonthliesView.xaml
    /// </summary>
    public partial class MonthliesView : UserControl
    {
        private Window Parent { get; set; }
        public MonthlyFacts Monthlies { get; set; }
        private List<TextBox> Boxes;

        public MonthliesView(Window parent)
        {
            InitializeComponent();

            this.Parent = parent;
            Monthlies = new MonthlyFacts();
            Boxes = new List<TextBox>();

            for (int i=0; i< Monthlies.Count; i++)
            {
                var tb = new TextBlock();
                tb.Text = Monthlies[i].UnitName;
                tb.FontWeight = FontWeights.Bold;
                grid.Children.Add(tb);
                Grid.SetColumn(tb, (int)(i / 9) * 3);
                Grid.SetRow(tb, i % 9 + 1);

                var tb2 = new TextBlock();
                tb2.Text = Monthlies[i].Target.ToString();
                grid.Children.Add(tb2);
                Grid.SetColumn(tb2, (int)(i / 9) * 3 + 1);
                Grid.SetRow(tb2, i % 9 + 1);

                var tb3 = new TextBox();
                tb3.Text = Monthlies[i].Produced.ToString();
                Boxes.Add(tb3);
                tb3.VerticalContentAlignment = VerticalAlignment.Center;
                tb3.HorizontalContentAlignment = HorizontalAlignment.Center;
                grid.Children.Add(tb3);
                Grid.SetColumn(tb3, (int)(i / 9) * 3 + 2);
                Grid.SetRow(tb3, i % 9 + 1);
            }
        }

        public void FinalizeChanges()
        {
            for (int i = 0; i < Monthlies.Count; i++)
            {
                Monthlies[i].Produced = int.Parse(Boxes[i].Text);
            }
            Monthlies.Upload();
        }
    }
}
