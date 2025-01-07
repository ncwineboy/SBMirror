using SBMirror.Models.Weather;

namespace SBMirror.Logic
{
    internal static class SunriseSunsetCalculator
    {
        public static SunriseSunset SunriseSunSetCalc(double latitude, double longitude)
        {
            SunriseSunset returnval = new SunriseSunset();
            DateTime date = DateTime.UtcNow;
            returnval.SunriseUTC = CalculateSunrise(latitude, longitude, date);
            returnval.SunsetUTC = CalculateSunset(latitude, longitude, date);
            returnval.isDaytime = returnval.Sunrise <= date && returnval.Sunset >= date;
            returnval.Calculated = true;
            return returnval;
        }

        private static DateTime CalculateSunrise(double latitude, double longitude, DateTime date)
        {
            return CalculateSunEvent(latitude, longitude, date, true);
        }

        private static DateTime CalculateSunset(double latitude, double longitude, DateTime date)
        {
            return CalculateSunEvent(latitude, longitude, date, false);
        }

        private static DateTime CalculateSunEvent(double latitude, double longitude, DateTime date, bool isSunrise)
        {
            int dayOfYear = date.DayOfYear;
            double zenith = 90.833; // Official zenith for sunrise/sunset

            // Convert longitude to hour value and calculate approximate time
            double lngHour = longitude / 15.0;
            double t = isSunrise ? dayOfYear + (6 - lngHour) / 24 : dayOfYear + (18 - lngHour) / 24;

            // Calculate the Sun's mean anomaly
            double M = 0.9856 * t - 3.289;

            // Calculate the Sun's true longitude
            double L = M + 1.916 * Math.Sin(DegToRad(M)) + 0.020 * Math.Sin(DegToRad(2 * M)) + 282.634;
            L = NormalizeAngle(L);

            // Calculate the Sun's right ascension
            double RA = RadToDeg(Math.Atan(0.91764 * Math.Tan(DegToRad(L))));
            RA = NormalizeAngle(RA);

            // Right ascension value needs to be in the same quadrant as L
            double Lquadrant = Math.Floor(L / 90) * 90;
            double RAquadrant = Math.Floor(RA / 90) * 90;
            RA += Lquadrant - RAquadrant;

            // Convert RA to hours
            RA /= 15;

            // Calculate the Sun's declination
            double sinDec = 0.39782 * Math.Sin(DegToRad(L));
            double cosDec = Math.Cos(Math.Asin(sinDec));

            // Calculate the Sun's local hour angle
            double cosH = (Math.Cos(DegToRad(zenith)) - sinDec * Math.Sin(DegToRad(latitude))) / (cosDec * Math.Cos(DegToRad(latitude)));

            if (cosH > 1)
            {
                throw new Exception("The sun never rises on this date at this location.");
            }
            else if (cosH < -1)
            {
                throw new Exception("The sun never sets on this date at this location.");
            }

            // Calculate H and convert to hours
            double H = isSunrise ? 360 - RadToDeg(Math.Acos(cosH)) : RadToDeg(Math.Acos(cosH));
            H /= 15;

            // Calculate local mean time
            double T = H + RA - 0.06571 * t - 6.622;

            // Adjust to UTC
            double UT = T - lngHour;
            UT = NormalizeTime(UT);


            var returnval = DateTime.SpecifyKind(date.Date.AddHours(UT), DateTimeKind.Utc);
            // Convert to DateTime
            return returnval;
        }

        private static double DegToRad(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double RadToDeg(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        private static double NormalizeAngle(double angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }

        private static double NormalizeTime(double time)
        {
            time %= 24;
            if (time < 0) time += 24;
            return time;
        }
    }
}
