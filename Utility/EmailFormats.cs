using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Utility
{
    public static class EmailFormats
    {
        public const string AppointmentCanceled = "<html>\r\n    <h1>Appointment Canceled</h1>\r\n    <br />\r\n    <p>Your appointment with [ProviderName] has been canceled. <br /> <br />\r\n       Reason: [ProviderReason]. <br /> <br />\r\n       If you would like to schedule a new appointment, <a href=\"https://nathandavis18-001-site2.ftempurl.com/Appointments\">click here</a>.\r\n   </p>\r\n  </html>";
        public const string ConfirmEmail = $"Please confirm your account by <a href='[ConfirmEmailLink]'>clicking here</a>.";
    }
}
