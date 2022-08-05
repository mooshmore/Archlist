using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace Helpers
{
    /// <summary>
    /// A class that contains methods referring to time and date, like methods to check a hour format or time 
    /// converters and others.
    /// </summary>
    public static class TimeAndDate
    {
        #region Time format checking

        /// <summary>
        /// Checks if the given time string matches the regex format, that is - [0-23]:[00-59]. 
        /// Examples: 2:35 - true, 06:52 - true 15:9 - false, 20:60 - false
        /// </summary>
        /// <param name="time">The time string in a HH:mm format.</param>
        /// <param name="noHourLimit">Allows for any number of hours to be given.</param>
        /// <returns>A true if the string matches the format, false if it doesn't.</returns>
        public static bool CheckTimeFormat(string time, bool noHourLimit = false)
        {
            return noHourLimit
                ? new Regex("^\\d*[:][0-5][0-9]$").IsMatch(time)
                : new Regex("^([0-9]|0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$").IsMatch(time);
        }

        /// <summary>
        /// It also fixes a given time format in case when only hour is given.
        /// Checks if the given time string matches the regex format and also removes a zero from the hour up front,
        /// example 06:52 -> 6:52.
        /// Pass the time parameter by a reference to have it changed. The method does not change the string if it 
        /// doesn't match the format (besides adding :00).
        /// </summary>
        /// <param name="time">The time string in a HH:mm format.</param>
        /// <param name="addHourLeadingZero">Whether to add the leading zero to the hour.</param>
        /// <param name="noHourLimit">If true allows for times with hour higher than 24, like 135146:43.</param>
        /// <param name="emptyIsCorrect">Whether to count an empty time as a correct or not.</param>
        /// <returns>True if the format was correct; false if it was not.</returns>
        public static bool CheckAndFixTimeFormat(ref string time, bool addHourLeadingZero = false, bool noHourLimit = false, bool emptyIsCorrect = false)
        {
            // If only hours are given ( 8 or 22) add :00 to it ( 8 -> 8:00, 22 -> 22:00)
            if (time == "") return emptyIsCorrect;

            if (!time.Contains(":"))
                time += ":00";

            if (noHourLimit)
                return CheckTimeFormat(time, true);
            else if (CheckTimeFormat(time))
            {
                if (!addHourLeadingZero && (time.Length == 5) && (time.Substring(0, 1) == "0"))
                    time = time[1..];
                return true;
            }
            else
                return false;
        }

        #endregion

        #region Time formatters

        /// <summary>
        /// Creates a formatted time (HH:mm) from the given hours and minutes.
        /// </summary>
        /// <param name="addHourLeadingZero">Whether to add the zero at the front if the hour < 10. False by default. Example - true - 05:30, false - 5:30.</param>
        /// <returns>Formatted time. Returns empty string if both hour and minute were empty.</returns>
        public static string ToTime(string hour, string minute, bool addHourLeadingZero = false)
        {
            if (hour.Length == 0 && minute.Length == 0) return "";

            if (hour.Length == 0) hour = "0";
            if (addHourLeadingZero && hour.Length == 1) hour = $"0{hour}";

            if (minute.Length == 0) minute = "00";
            if (minute.Length == 1) minute = $"0{minute}";
            return hour + ":" + minute;
        }

        /// <summary>
        /// Creates a formatted time (HH:mm) from the given hours and minutes.
        /// </summary>
        /// <param name="addHourLeadingZero">Whether to add the zero at the front if the hour < 10. False by default. Example - true - 05:30, false - 5:30.</param>
        /// <returns>Formatted time. Returns empty string if both hour and minute were empty.</returns>
        public static string ToTime(int hour, int minute, bool addHourLeadingZero = false) => ToTime(hour.ToString(), minute.ToString(), addHourLeadingZero);

        /// <summary>
        /// Creates a formatted time (HH:mm) from the given hours and minutes.
        /// </summary>
        /// <param name="addHourLeadingZero">Whether to add the zero at the front if the hour < 10. False by default. Example - true - 05:30, false - 5:30.</param>
        /// <returns>Formatted time. Returns empty string if both hour and minute were empty.</returns>
        public static string ToTime(int? hour, int? minute, bool addHourLeadingZero = false, bool emptyStringOnBothNull = true)
        {
            if (hour == null && minute == null && emptyStringOnBothNull) return "";
            if (hour == null) hour = 0;
            if (minute == null) minute = 0;

            return ToTime(hour.ToString(), minute.ToString(), addHourLeadingZero);
        }

        /// <summary>
        /// Creates a time string HH:MM from the given hour and minute.
        /// </summary>
        /// <param name="minutes">The time in int(minutes).</param>
        /// <param name="addHourLeadingZero">Whether to add the zero at the front if the hour < 10. False by default. Example - true - 05:30, false - 5:30.</param>
        /// <param name="returnEmptyString">If true it will return "" if the minutes == 0. False by default.</param>
        /// <returns>A time string in a defaul time format - HH:MM</returns>
        public static string ToTime(this int minutes, bool addHourLeadingZero = false, bool returnEmptyString = false) => minutes == 0 && returnEmptyString ? "" : ToTime(minutes.Hours(), minutes.Minutes(), addHourLeadingZero);

        /// <summary>
        /// Creates a time string HH:MM from the given hour and minute. Will return empty string if nullMinutes = null.
        /// </summary>
        /// <param name="nullMinutes">The time in int?(minutes).</param>
        /// <param name="addHourLeadingZero">Whether to add the zero at the front if the hour < 10. False by default. Example - true - 05:30, false - 5:30.</param>
        /// <param name="returnEmptyString_zero">If true it will return "" if the minutes == 0. False by default.</param>
        /// <returns>If the nullMinutes are null returns an empty string; Otherwise a time string in a defaul time format - HH:MM </returns>
        public static string ToTime(this int? nullMinutes, bool addHourLeadingZero = false, bool returnEmptyString_zero = false)
        {
            if (nullMinutes == null) return "";
            if (nullMinutes == 0 && returnEmptyString_zero) return "";
            int minutes = nullMinutes.ToInt();
            return ToTime(minutes.Hours(), minutes.Minutes(), addHourLeadingZero);
        }

        /// <summary>
        /// Fills time value for ex. 5: => 5:00, 24:5 => 24:50, 2 => 2:00
        /// </summary>
        /// <param name="time">Reference of time string</param>
        /// <param name="noHourLimit">If true allows to hour be higher than 24</param>
        public static void AutoFillTime(ref string time, bool noHourLimit = false)
        {
            // Check length of the passed string time
            if ((time.Length > 0 && time.Length < 5) || noHourLimit)
            {
                // Check whether string contains colon sign
                if (time.Contains(":"))
                {
                    // Split by colon
                    string[] arr_time = time.Split(':');

                    // Check whether first letters are numbers
                    if (!int.TryParse(arr_time[0], out _)) return;

                    if (((arr_time[0].Length == 1 || arr_time[0].Length == 2) && !noHourLimit) || (arr_time[0].Length != 0 && noHourLimit))
                    {
                        for (int i = 0; i < 2 - arr_time[1].Length; i++)
                            time += "0";
                    }
                }
                else
                {
                    if (((time.Length == 1 || time.Length == 2) && !noHourLimit) || (time.Length != 0 && noHourLimit))
                    {
                        // Check whether letters are numbers
                        if (!int.TryParse(time, out _)) return;

                        time += ":00";
                    }
                }
            }
        }

        #endregion

        #region Add leading zero

        /// <summary>
        /// Adds a zero at the front to the string if the value is less than 10. Example 5 -> 05
        /// </summary>
        /// <returns>The changed string.</returns>
        public static string AddLeadingZero(this int value) => value < 10 ? $"0{value}" : value.ToString();

        /// <summary>
        /// Adds a zero at the front to the string if the value is less than 10. Example 5 -> 05
        /// </summary>
        /// <returns>The changed string.</returns>
        public static string AddLeadingZero(this int? value) => value == null ? "" : ((int)value).AddLeadingZero();

        /// <summary>
        /// Adds a zero at the front to the string if the value is less than 100, and two zeros if the value is less than < 10. Example 5 -> 05
        /// </summary>
        /// <returns>The changed string.</returns>
        public static string AddDoubleLeadingZero(this int value) => value < 10 ? $"00{value}" : value < 100 ? $"0{value}" : value.ToString();

        #endregion

        #region Day & month names and indexes

        /// <summary>
        /// Returns the name of the day based on the given day number. The week starts from 0 - sunday.
        /// </summary>
        /// <returns>The name of the day..</returns>
        public static string DayName(this DateTime date)
        {
            return (int)date.DayOfWeek switch
            {
                1 => "Poniedziałek",
                2 => "Wtorek",
                3 => "Środa",
                4 => "Czwartek",
                5 => "Piątek",
                6 => "Sobota",
                0 => "Niedziela",
                _ => throw new Exception("Incorrect day index code"),
            };
        }

        /// <summary>
        /// Returns a short version of a day name based on the given day number. The week starts from 0 - sunday.
        /// </summary>
        /// <returns>Short version of a day name.</returns>
        public static string DayName_short(this DateTime date)
        {
            return (int)date.DayOfWeek switch
            {
                1 => "Pn",
                2 => "Wt",
                3 => "Śr",
                4 => "Czw",
                5 => "Pt",
                6 => "Sb",
                0 => "Nd",
                _ => throw new Exception("Incorrect day index code"),
            };
        }

        /// <summary>
        /// Translates a string month to month index, ex: "Styczeń" -> "01"
        /// </summary>
        /// <param name="month">The name of the month.</param>
        /// <returns>SQL month index.</returns>
        public static int MonthIndex(this string month)
        {
            return month switch
            {
                "Styczeń" => 1,
                "Luty" => 2,
                "Marzec" => 3,
                "Kwiecień" => 4,
                "Maj" => 5,
                "Czerwiec" => 6,
                "Lipiec" => 7,
                "Sierpień" => 8,
                "Wrzesień" => 9,
                "Październik" => 10,
                "Listopad" => 11,
                "Grudzień" => 12,
                _ => throw new Exception("Incorrect month name"),
            };
        }

        /// <summary>
        /// Translates a string month to month index, ex: "Styczeń" -> "01"
        /// </summary>
        /// <param name="index">The index of the month.</param>
        /// <returns>SQL month index.</returns>
        public static string MonthName(this int index)
        {
            return index switch
            {
                1 => "Styczeń",
                2 => "Luty",
                3 => "Marzec",
                4 => "Kwiecień",
                5 => "Maj",
                6 => "Czerwiec",
                7 => "Lipiec",
                8 => "Sierpień",
                9 => "Wrzesień",
                10 => "Październik",
                11 => "Listopad",
                12 => "Grudzień",
                _ => throw new Exception("Incorrect day index code"),
            };
        }

        /// <summary>
        /// Converts US day of the week index to EU day index (the week starts from monday in EU vs from saturday in US)
        /// </summary>
        public static int ToEUDayIndex(this DayOfWeek dayOfTheWeek)
        {
            return (int)dayOfTheWeek switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                4 => 3,
                5 => 4,
                6 => 5,
                0 => 6,
                _ => throw new Exception("Incorrect day of the week"),
            };
        }

        #endregion

        #region Hours & minutes from strings and ints

        /// <summary>
        /// Returns the hours component from the given time.
        /// </summary>
        /// <param name="time">The time string in a HH:mm format.</param>
        /// <returns>Hours converted from the time.</returns>
        public static int Hours(this string time) => int.Parse(time.Substring(0, time.IndexOf(":")));

        /// <summary>
        /// Returns the hours component from the given time.
        /// If the time is null or empty it returns null.
        /// </summary>
        /// <param name="time">The time string in a HH:mm format.</param>
        /// <returns>Null if the given time is null or empty; The hours component</returns>
        public static int? Hours_null(this string time) => time.IsNullOrEmpty() ? null : (int?)int.Parse(time.Substring(0, time.IndexOf(":")));

        /// <summary>
        /// Returns the minutes component from the given time.
        /// </summary>
        /// <param name="time">The time string in a HH:mm format.</param>
        /// <returns>Minutes converted from the given time.</returns>
        public static int Minutes(this string time) => int.Parse(time[(time.IndexOf(":") + 1)..]);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">The time string in a HH:mm format.</param>
        /// <returns></returns>
        public static int? Minutes_null(this string time) => time == null || time == "" ? null : int.Parse(time[(time.IndexOf(":") + 1)..]);

        /// <summary>
        /// Returns the integer of hours in the given time. Example: 200 (minutes) -> 3 (hours) ( because 180 minutes )
        /// </summary>
        /// <returns>The integer of hours in the given minutes.</returns>
        public static int Hours(this int minutes) => minutes / 60;

        /// <summary>
        /// Returns the integer of hours in the given time. If the value is null it returns null. Example: 200 (minutes) -> 3 (hours) ( because 180 minutes )
        /// </summary>
        /// <returns>The integer of hours in the given minutes. Returns null if the value is also null.</returns>
        public static int? Hours(this int? minutes) => minutes == null ? null : minutes / 60;

        /// <summary>
        /// Returns the remaining minutes when counting hours. Example 200 (minutes) -> 20 (minutes) ( 3 hours = 180 minutes -> 20 minutes of rest)
        /// </summary>
        /// <returns>The remaining minutes.</returns>
        public static int Minutes(this int minutes) => minutes % 60;

        /// <summary>
        /// Returns the remaining minutes when counting hours. If the value is null it returns null. Example 200 (minutes) -> 20 (minutes) ( 3 hours = 180 minutes -> 20 minutes of rest)
        /// </summary>
        /// <returns>The remaining minutes. Returns null if the value is also null.</returns>
        public static int? Minutes(this int? minutes) => minutes == null ? null : minutes % 60;

        #endregion

        #region DateTime methods

        /// <summary>
        /// Returns a EU day index from the given date.
        /// </summary>
        /// <returns>A EU day index from the given date, starting from 0 - monday.</returns>
        public static int DayOfWeek(this DateTime date) => date.DayOfWeek.ToEUDayIndex();

        /// <summary>
        /// Returns the number of days in the month. Date overload.
        /// </summary>
        /// <returns>The number of days in the month of the given date.</returns>
        public static int DaysInMonth(this DateTime date) => DateTime.DaysInMonth(date.Year, date.Month);

        /// <summary>
        /// Returns a IEnumerable<DateTime> list that has each day inside it from the start to the end of the declared dates.
        /// Usage (ForEach loop):
        /// foreach (DateTime day_date in MethodsCluster.EachDay(date_start, date_end))
        /// </summary>
        /// <param name="dateStart">The start of the period.</param>
        /// <param name="dateEnd">The end of the period.</param>
        /// <returns>IEnumerable<DateTime> list that has each day inside it from the start to the end of the declared dates.</returns>
        public static IEnumerable<DateTime> EachDay(DateTime dateStart, DateTime dateEnd)
        {
            for (DateTime day = dateStart.Date; day.Date <= dateEnd.Date; day = day.AddDays(1))
                yield return day;
        }

        /// <summary>
        /// Removes all values from the given dateTime except for year, month and the day.
        /// </summary>
        /// <returns>The newly created, "flattened" dateTime.</returns>
        public static DateTime Flatten(this DateTime date) => new(date.Year, date.Month, date.Day);

        #endregion

        #region TimeSpan methods

        #region Time components

        /// <summary>
        /// Gets the hours component of the given timeSpan.
        /// </summary>
        /// <returns>The hour component of the given timeSpan. The return value ranges from -23 through 23.</returns>
        public static int? Hours(this TimeSpan? timeSpan) => timeSpan == null ? null : (int?)((TimeSpan)timeSpan).Hours;

        /// <summary>
        /// Gets the minutes component of the given timeSpan.
        /// </summary>
        /// <returns>The minute component of the given timeSpan. The return value ranges from -59 through 59.</returns>
        public static int? Minutes(this TimeSpan? timeSpan) => timeSpan == null ? null : (int?)((TimeSpan)timeSpan).Minutes;

        /// <summary>
        /// Gets the days component of the given timeSpan.
        /// </summary>
        /// <returns>The day component of the given timeSpan. The return value can be positive or negative.</returns>
        public static int? Days(this TimeSpan? timeSpan) => timeSpan == null ? null : (int?)((TimeSpan)timeSpan).Days;

        #endregion

        /// <summary>
        /// Converts the given timespan to minutes =>  (days * 24 * 60) + (hours * 60) + minutes. If the timeSpan is null it returns null.
        /// </summary>
        /// <returns>Null if the given timeSpan was null; Otherwise the minutes converted from the time in the timeSpan.</returns>
        public static int? ToMinutes(this TimeSpan? timeSpan) => timeSpan == null ? null : (timeSpan.Days() * 24 * 60) + (timeSpan.Hours() * 60) + timeSpan.Minutes();

        /// <summary>
        /// Converts the timeSpan value to a HH:mm format without a leading zero. 
        /// This does not display days, miliseconds and the negative sign (if the value is negative).
        /// </summary>
        /// <returns>A string created from the given timeSpan.</returns>
        public static string ToTimeFormat(this TimeSpan timeSpan) => $"{timeSpan.Hours}:{timeSpan.Minutes.AddLeadingZero()}";

        /// <summary>
        /// Converts the timeSpan value to a HH:mm format without a leading zero. If the the value is null it returns an empty string.
        /// This does not display days, miliseconds and the negative sign (if the value is negative).
        /// </summary>
        /// <returns>A string created from the given timeSpan.</returns>
        public static string ToTimeFormat(this TimeSpan? timeSpan) => timeSpan == null ? "" : $"{timeSpan.Hours()}:{timeSpan.Minutes().AddLeadingZero()}";

        /// <summary>
        /// Returns the timeSpan of the two difference between timeSpanTo - timeSpanFrom. 
        /// If the timeSpanTo is smaller than timeSpanFrom then the operation is reversed (timeSpanFrom - timeSpanTo);
        /// </summary>
        /// <returns>The timeSpan between the two given values.</returns>
        public static TimeSpan TimeSpan(TimeSpan timeSpanFrom, TimeSpan timeSpanTo) => timeSpanFrom <= timeSpanTo ? timeSpanTo - timeSpanFrom : timeSpanFrom - timeSpanTo;

        /// <summary>
        /// Returns the timeSpan of the two difference between timeSpanTo - timeSpanFrom. 
        /// If any of the two values are null then null is returned.
        /// If the timeSpanTo is smaller than timeSpanFrom then the operation is reversed (timeSpanFrom - timeSpanTo);
        /// </summary>
        /// <returns>Null if any of the two values are null; The timeSpan between the two given values.</returns>
        public static TimeSpan? TimeSpan(TimeSpan? timeSpanFrom, TimeSpan? timeSpanTo)
        {
            return timeSpanFrom == null || timeSpanTo == null
                ? null
                : timeSpanFrom <= timeSpanTo ? timeSpanTo - timeSpanFrom : new TimeSpan(24, 0, 0) - (timeSpanFrom - timeSpanTo);
        }

        /// <summary>
        /// Checks if the given timeSpan equals 0; that is 0 day, hours and minutes.
        /// </summary>
        /// <returns>True if the timeSpan equals 0; False if not.</returns>
        public static bool IsEmpty(this TimeSpan timeSpan) => timeSpan.Days == 0 && timeSpan.Hours == 0 && timeSpan.Minutes == 0;

        /// <summary>
        /// Checks if the given timeSpan equals null or 0; that is 0 day, hours and minutes.
        /// </summary>
        /// <returns>True if the timeSpan equals 0 or null; False if not.</returns>
        public static bool IsEmpty(this TimeSpan? timeSpan) => timeSpan == null || ((TimeSpan)timeSpan).IsEmpty();

        /// <summary>
        /// Checks if day, hour, minutes, second or miliseconds component are negative (smaller than 0).
        /// </summary>
        /// <returns>True if any of the timeSpan components are negative; False if not.</returns>
        public static bool IsNegative(this TimeSpan timeSpan) => timeSpan.Days < 0 || timeSpan.Hours < 0 || timeSpan.Minutes < 0 || timeSpan.Seconds < 0 || timeSpan.Milliseconds < 0;

        /// <summary>
        /// Checks if day, hour, minutes, second or miliseconds component are negative (smaller than 0).
        /// If the given timeSpan equals null then a false is returned.
        /// </summary>
        /// <returns>True if any of the timeSpan components are negative; False if not or if the timeSpan is null.</returns>
        public static bool IsNegative(this TimeSpan? timeSpan) => timeSpan != null && ((TimeSpan)timeSpan).IsNegative();

        /// <summary>
        /// Removes all of the data from the given TimeSpan? but the hours and minutes.
        /// </summary>
        public static TimeSpan? Flatten(this TimeSpan? timeSpan) => timeSpan == null ? null : (TimeSpan?)$"{timeSpan.ToTimeFormat()}".ToTimeSpan();

        #endregion
    }
}
