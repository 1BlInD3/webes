namespace MonitorManager
{
    using LiveCharts;
    using LiveCharts.Configurations;
    using LiveCharts.Defaults;
    using LiveCharts.Wpf;
    using ProductionMonitor.Utils;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Generates a png image to path based on input data
    /// presenting production or waste data provided as
    /// percentage values relative to target.
    /// </summary>
    /// <remarks>
    /// Charts on a 4K screen appear 519px / 156px
    /// </remarks>
    public static class ChartGenerator
    {
        public const string TARGET_FOLDER_PATH = @"\\scala1\TesztWeb\Image\Graphs\";
        public const double PRODUCTION_TARGET = 100D;
        public const double WASTE_TARGET = 0.65D;
        public const double RENDER_WIDTH = 519D;
        public const double RENDER_HEIGHT = 156D;
        public const double LABEL_SIZE = RENDER_HEIGHT / 7;
        public const int IMAGE_DPI = 96;

        /// <summary>
        /// Generates chart images for a given unit's history.
        /// </summary>
        /// <param name="history">The ProductionHistory from which the images are generated</param>
        public static void GenerateCharts(ProductionMonitor.Models.ProductionHistory history)
        {
            var prodData = history.Select(pd => (double)pd.GetProducedPercentage());
            GenerateChart(history.UnitName, prodData.ToArray(), false);
            var wasteData = history.Select(pd => (double)pd.GetWastedPercentage());
            GenerateChart(history.UnitName, wasteData.ToArray(), true);
        }

        /// <summary>
        /// Generates a png image to present production history.
        /// </summary>
        /// <param name="unitName">The unit for which the image is genrated</param>
        /// <param name="data">The data used to generate the chart</param>
        /// <param name="isWasteChart">The type of chart to be generated</param>
        private static void GenerateChart(string unitName, double[] data, bool isWasteChart)
        {
            var worker = new Thread(() =>
            {
                var fileName = GetTargetImageFullName(unitName, isWasteChart);
                var chart = BuildChart(data, isWasteChart);
                var visual = CreateVisual(chart) as FrameworkElement;
                RenderVisualAsPngImage(visual, fileName);
                //RenderVisualAsPngImage(CreateVisual(BuildChart(data, isWasteChart)) as FrameworkElement, GetTargetImageFullName(unitName, isWasteChart));
            });
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
            worker.Join();
        }
        /// <summary>
        /// Gets the name full name of the png image associated with the production unit
        /// </summary>
        /// <param name="unitName">The unit for which the image is genrated</param>
        /// <param name="isWasteChart">The type of chart to be generated</param>
        /// <returns>The full name of the image</returns>
        private static string GetTargetImageFullName(string unitName, bool isWasteChart)
        {
            return TARGET_FOLDER_PATH + unitName + (isWasteChart ? "w" : "p") + ".png";
        }
        /// <summary>
        /// Builds a chart based on the data provided with the appropriate design choices.
        /// </summary>
        /// <param name="data">The data used in the chart</param>
        /// <param name="isWasteChart">The type of chart to be generated</param>
        /// <returns>A cartesian chart of the data</returns>
        private static CartesianChart BuildChart(double[] data, bool isWasteChart)
        {
            // chart
            var chart = new CartesianChart()
            {
                DisableAnimations = true,
                Width = RENDER_WIDTH,
                Height = RENDER_HEIGHT
            };

            // setting up axes
            var axX = new Axis()
            {
                Separator = new LiveCharts.Wpf.Separator() { Step = 1 },
                ShowLabels = false
            };
            var axY = new Axis()
            {
                MinValue = 0,
                MaxValue = isWasteChart ? data.Max() + 0.1 > WASTE_TARGET * 2 ? data.Max() + 0.1 : WASTE_TARGET * 2 :
                                          data.Max() + 5 > PRODUCTION_TARGET * 2 ? data.Max() + 5 : PRODUCTION_TARGET * 2,
                FontSize = LABEL_SIZE,
                ShowLabels = false
            };

            // target as constant series over time
            double[] targets = new double[data.Length];
            for (int i = 0; i < targets.Length; i++) targets[i] = isWasteChart ? WASTE_TARGET : PRODUCTION_TARGET;
            var seriesTarget = new LineSeries()
            {
                Values = new ChartValues<double>(targets),
                PointGeometry = null,
                Stroke = new System.Windows.Media.SolidColorBrush(ColorData.Blue.ToWinMediaColor()),
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent),
                StrokeThickness = RENDER_HEIGHT / 50,
                StrokeDashArray = new System.Windows.Media.DoubleCollection { 2, 1 },
            };

            // data series
            var seriesData = new ColumnSeries()
            {
                Values = new ChartValues<ObservableValue>(),
                PointGeometry = null,
                //Fill = new System.Windows.Media.SolidColorBrush(isWasteChart ? wasteColor : producedColor),
                MaxColumnWidth = RENDER_WIDTH / 12,
                FontSize = LABEL_SIZE,
                DataLabels = true,
                LabelPoint = point => point.Y.ToString("N2") + "%",
                LabelsPosition = BarLabelPosition.Top,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
            };
            foreach (var d in data) seriesData.Values.Add(new ObservableValue(d));

            // add items to chart
            chart.AxisX.Add(axX);
            chart.AxisY.Add(axY);
            chart.Series.Add(seriesData);
            chart.Series.Add(seriesTarget);

            if (!isWasteChart)
            {
                chart.Series.Configuration = Mappers.Xy<ObservableValue>()
                    .X((item, index) => index)
                    .Y(item => item.Value)
                    .Fill(item => item.Value >= 100 ? new System.Windows.Media.SolidColorBrush(ColorData.Green.ToWinMediaColor()) :
                                  item.Value >= 90 ? new System.Windows.Media.SolidColorBrush(ColorData.Yellow.ToWinMediaColor()) :
                                  new System.Windows.Media.SolidColorBrush(ColorData.Red.ToWinMediaColor()));
            }
            else
            {
                chart.Series.Configuration = Mappers.Xy<ObservableValue>()
                    .X((item, index) => index)
                    .Y(item => item.Value)
                    .Fill(item => item.Value > WASTE_TARGET ? new System.Windows.Media.SolidColorBrush(ColorData.Red.ToWinMediaColor()) :
                                  new System.Windows.Media.SolidColorBrush(ColorData.Green.ToWinMediaColor()));
            }

            return chart;
        }
        /// <summary>
        /// Creates a visual from a cartesian chart.
        /// </summary>
        /// <param name="chart">The chart to be transformed</param>
        /// <returns>The transformed chart</returns>
        private static System.Windows.Media.Visual CreateVisual(CartesianChart chart)
        {
            chart.Background = null;
            var viewbox = new Viewbox { Child = chart };
            viewbox.Measure(chart.RenderSize);
            viewbox.Arrange(new Rect(new Point(0, 0), chart.RenderSize));
            chart.Update(true, true); //force chart redraw
            viewbox.UpdateLayout();

            return chart as System.Windows.Media.Visual;
        }
        /// <summary>
        /// Renders a visual as a png image.
        /// </summary>
        /// <param name="visual">The visual to be rendered</param>
        /// <param name="fileName">The name of the png file</param>
        private static void RenderVisualAsPngImage(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 
                                                IMAGE_DPI, IMAGE_DPI, System.Windows.Media.PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using (var stream = File.Create(fileName)) encoder.Save(stream);
        }
    }
}