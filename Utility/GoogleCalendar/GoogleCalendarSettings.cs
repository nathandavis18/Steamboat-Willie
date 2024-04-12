using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.GoogleCalendar
{
    public class GoogleCalendarSettings : IGoogleCalendarSettings
    {
        private string _ClientId {  get; set; }
        public string ClientId
        {
            get
            {
                if (String.IsNullOrEmpty(_ClientId))
                {
                    _ClientId = "";
                }
                return _ClientId;
            }
        }
        private string _ClientSecret { get; set; }
        public string ClientSecret
        {
            get
            {
                if (String.IsNullOrEmpty(_ClientSecret))
                {
                    _ClientSecret = "";
                }
                return _ClientSecret;
            }
        }
        private List<string> _Scope { get; set; }
        public List<string> Scope
        {
            get
            {
                if (_Scope == null)
                {
                    _Scope = new List<string>();
                }
                return _Scope;
            }
        }
        private string _ApplicationName { get; set; }
        public string ApplicationName
        {
            get
            {
                if (String.IsNullOrEmpty(_ApplicationName))
                {
                    _ApplicationName = "";
                }
                return _ApplicationName;
            }
        }
        private string _User { get; set; }
        public string User
        {
            get
            {
                if (String.IsNullOrEmpty(_User))
                {
                    _User = "";
                }
                return _User;
            }
        }
        private string _CalendarId { get; set; }
        public string CalendarId
        {
            get
            {
                if (String.IsNullOrEmpty(_CalendarId))
                {
                    _CalendarId = "";
                }
                return _CalendarId;
            }
        }

    }
}
