namespace SBMirror.Logic
{
    public class MoonPhase
    {

        public static int MoonPhaseCalculator(DateTime date)
        {
            // Base date for calculations: January 6, 2000 (a known new moon date)
            DateTime baseDate = new DateTime(2000, 1, 6);
            double synodicMonth = 29.53; // Average length of a lunar month in days

            // Calculate the difference in days from the base date
            TimeSpan difference = date - baseDate;
            double daysSinceBase = difference.TotalDays;

            // Calculate the current phase as a percentage (0% = new moon, 100% = full moon, etc.)
            double phase = daysSinceBase % synodicMonth / synodicMonth * 100;

            return (int)Math.Round(phase);
        }

        public static string GetMoonPhaseImage(int phase)
        {
            if (phase < 1 || phase >= 99)
                return "https://smd-cms.nasa.gov/wp-content/uploads/2023/08/new-moon.jpg?w=48&format=webp";
            else if (phase < 25)
                return "https://smd-cms.nasa.gov/wp-content/uploads/2023/08/waxing-crescent.jpg?w=48&format=webp";
            else if (phase < 50)
                return "https://smd-cms.nasa.gov/wp-content/uploads/2023/08/first-quarter.jpg?w=48&format=webp";
            else if (phase < 75)
                return "https://smd-cms.nasa.gov/wp-content/uploads/2023/08/waxing-gibbous.jpg?w=48&format=webp";
            else if (phase < 99)
                return "https://smd-cms.nasa.gov/wp-content/uploads/2023/08/full.jpg?w=48&format=webp";
            else if (phase < 125)
                return "https://smd-cms.nasa.gov/wp-content/uploads/2023/08/waning-gibbous.jpg?w=48&format=webp";
            else if (phase < 150)
                return "https://smd-cms.nasa.gov/wp-content/uploads/2023/08/third-quarter.jpg?w=48&format=webp";
            else
                return "https://smd-cms.nasa.gov/wp-content/uploads/2023/08/waning-crescent.jpg?w=48&format=webp";
        }
    }
}
