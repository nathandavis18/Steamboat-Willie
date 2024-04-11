﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Utility
{
    public static class EmailFormats
    {
        public const string AppointmentCanceled = "<html><p>Your appointment with [ProviderName] has been canceled. <br /> <br />Reason: [ProviderReason]. <br /> <br />If you would like to schedule a new appointment, <a href=\"https://steamboatwillie.nathandavis18.com/Appointments\">click here</a>.\r\n   </p>\r\n  </html>";
        public const string ConfirmEmail = $"Please confirm your account by <a href='[ConfirmEmailLink]'>clicking here</a>.";
    }
}