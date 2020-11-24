namespace ProductionMonitor.Utils
{
    using System;
    using System.Globalization;

    /// <summary>
    /// This class shuffles the backgrounds based on
    /// the current week number and returns the one
    /// associated with the current day of the week.
    /// </summary>
    public static class BackgroundShuffler
    {
        /// <summary>
        /// Gets the background for the actual date after a shuffle.
        /// </summary>
        /// <returns>The filename of the background image</returns>
        public static string GetTodaysBackground()
        {
            var today = DateTime.Today;

            var backgrounds = new string[]
            {
                "DSC_3884.png",
                "IMG_1889.png",
                "IMG_8626.png",
                "IMG_9334.png",
                "IMG_9256.png",
                "LAN_2958.png",
                "LAN_3048.png"
            };

            Shuffle(backgrounds, GetIso8601WeekOfYear(today));
            var todaysBackground = backgrounds[today.DayOfWeek.GetHashCode()];
            return todaysBackground;
        }

        /// <summary>
        /// Shuffles an array of strings based on a key.
        /// </summary>
        /// <param name="toShuffle">The array to be shuffled</param>
        /// <param name="key">The key used to shuffle</param>
        private static void Shuffle(string[] toShuffle, int key)
        {
            int size = toShuffle.Length;
            var exchanges = GetShuffleExchanges(size, key);
            for (int i = size - 1; i > 0; i--)
            {
                int n = exchanges[size - 1 - i];
                var tmp = toShuffle[i];
                toShuffle[i] = toShuffle[n];
                toShuffle[n] = tmp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">The size of the output array</param>
        /// <param name="key">The key used to generate random numbers</param>
        /// <returns>An array of random integers</returns>
        private static int[] GetShuffleExchanges(int size, int key)
        {
            int[] exchanges = new int[size - 1];
            var rand = new Random(key);
            for (int i = size - 1; i > 0; i--)
            {
                int n = rand.Next(i + 1);
                exchanges[size - 1 - i] = n;
            }
            return exchanges;
        }

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        private static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}