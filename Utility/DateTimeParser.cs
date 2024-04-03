namespace Utility
{
    public static class DateTimeParser
    {
        public static string ParseDateTime(DateTime dt)
        {
            string returnString = ""; //The final string
            var dts = dt.ToString(); //Date Time String initial
            var split = dts.Split(' ', ' '); //Splitting the segments

            //All of the date parsing stuff
            var dateStrings = split[0].Split("/");
            string monthString = dateStrings[0];
            string dayString = dateStrings[1];
            string yearString = dateStrings[2];
            int month = int.Parse(monthString);
            int day = int.Parse(dayString);
            if(month < 10)
            {
                monthString = "0" + month.ToString();
            }
            if(day < 10)
            {
                dayString = "0" + day.ToString();
            }

            // All of the time parsing stuff
            var timeStrings = split[1].Split(":");
            int hour = int.Parse(timeStrings[0]);
            string hourString = "";
            string minuteString = timeStrings[1];
            string secondString = timeStrings[2];
            if (split[2].Equals("AM"))
            {
                if(hour < 10)
                {
                    hourString = "0" + hour.ToString();
                }
                else if(hour == 12)
                {
                    hourString = "00";
                }
                else
                {
                    hourString = hour.ToString();
                }
            }
            else if(split[2].Equals("PM"))
            {
                if (hour == 12)
                {
                    hourString = "12";
                }
                else
                {
                    hour += 12;
                    hourString = hour.ToString();
                }
            }

            //Creating the final return string
            returnString = yearString + "-" + monthString + "-" + dayString + "T" + hourString + ":" + minuteString + ":" + secondString;
            return returnString;
        }
    }
}
