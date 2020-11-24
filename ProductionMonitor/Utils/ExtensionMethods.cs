namespace ProductionMonitor.Utils
{
    /// <summary>
    /// A class used to store extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts the color to a 'System.Windows.Media.Color'.
        /// </summary>
        /// <param name="color">A 'System.Drawing.Color' to be converted</param>
        /// <returns>The converted System.Windows.Media.Color</returns>
        public static System.Windows.Media.Color ToWinMediaColor(this System.Drawing.Color color)
        {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts a System.Drawing.Color to a string describing
        /// a function used in markup.
        /// </summary>
        /// <param name="color">A 'System.Drawing.Color' to be converted</param>
        /// <returns>A string describing the function</returns>
        public static string ToFunctionString(this System.Drawing.Color color)
        {
            return "rgba(" + color.R + "," + color.G + "," + color.B + "," + ((float)color.A / 256).ToString().Replace(',', '.') + ")";
        }
    }
}