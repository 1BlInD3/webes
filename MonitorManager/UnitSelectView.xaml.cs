using ProductionMonitor.Models;
using ProductionMonitor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for UnitSelectView.xaml
    /// </summary>
    public partial class UnitSelectView : UserControl
    {
        private static string GetNames(string[] group)
        {
            var names = string.Empty;
            for (int i = 0; i < group.Length; i++)
            {
                names += group[i];
                if (i != group.Length - 1)
                    names += ", ";
            }
            return names;
        }
        private Window parent;

        public UnitSelectView(Window parent)
        {
            this.parent = parent;
            InitializeComponent();
            //cbGroup1.Content = GetNames(StringData.Group1);
            cbGroup2.Content = GetNames(StringData.Group2);
            cbGroup3.Content = GetNames(StringData.Group3);
            cbGroup4.Content = GetNames(StringData.Group4);
        }

        private void cbGroup_Checked(object sender, RoutedEventArgs e)
        {
            bool anyChecked = /*cbGroup1.IsChecked == true |*/
                cbGroup2.IsChecked == true |
                cbGroup3.IsChecked == true |
                cbGroup4.IsChecked == true;

            if (anyChecked) ((Button)parent.FindName("btnForth")).IsEnabled = true;
            else ((Button)parent.FindName("btnForth")).IsEnabled = false;
        }

        public List<string> GetSelectedUnits()
        {
            var list = new List<string>();
            //if (cbGroup1.IsChecked == true)
            //    foreach (var n in StringData.Group1) list.Add(n);
            if (cbGroup2.IsChecked == true)
                foreach (var n in StringData.Group2) list.Add(n);
            if (cbGroup3.IsChecked == true)
                foreach (var n in StringData.Group3) list.Add(n);
            if (cbGroup4.IsChecked == true)
                foreach (var n in StringData.Group4) list.Add(n);
            
            return list;
        }
    }
}
