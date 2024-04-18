namespace Utility
{
    public static class EmailFormats
    {
        public const string AppointmentCanceled = "<html><p>Your appointment with [ProviderName] has been canceled. <br /> <br />Reason: [ProviderReason]. <br /> <br />If you would like to schedule a new appointment, <a href=[Link]>click here</a>.\r\n   </p>\r\n  </html>";
        public const string ConfirmEmail = $"Please confirm your account by <a href='[ConfirmEmailLink]'>clicking here</a>.";
        public const string ReminderEmail = $"Testing that your appointment is 1 hour away";
        public const string ProviderAccountCreated = $"Your account has been created! <br /><br /> Your username is [Email] <br /> Your password is [Password] <br />You can <a href=[Link]>click here</a> to login. It is recommended you change your password after logging in."
             + " You can change your password anytime from the profile page!";
    }
}
