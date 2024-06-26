﻿using Hangfire;
using Infrastructure.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Utility;

namespace DataAccess
{
    public class EmailReminder
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly IRecurringJobManager _recurringJobManager;
        public EmailReminder(UnitOfWork unitOfWork, EmailSender emailSender, IRecurringJobManager recurringJobManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _recurringJobManager = recurringJobManager;
        }
        public void StartBackgroundTask()
        {
            _recurringJobManager.AddOrUpdate("emailReminder", () => SendEmail(), "*/15 6-20 * * *"); //Every 15 minutes from 6am - 8pm, every day, every month, every year
        }
        public void SendEmail()
        {
            var allUpcomingAppointments = _unitOfWork.Appointment.GetAll(includes: "ProviderAvailability") //Getting all appointments that are ~ 1 hour away. Adding 1 hours to adjust for smarterasp time being pacific
                .Where(a => a.ProviderAvailability.StartTime <= DateTime.Now.AddHours(1).AddMinutes(5).AddHours(1) && a.ProviderAvailability.StartTime > DateTime.Now.AddMinutes(55).AddHours(1));
            foreach (var appointment in allUpcomingAppointments)
            {
                var clientEmail = _unitOfWork.AppUser.GetById(appointment.ClientId).Email;
                var link = "\"https://nathandavis18.com\"";
                var message = EmailFormats.ReminderEmail.Replace("[Provider]", _unitOfWork.AppUser.GetById(appointment.ProviderAvailability.ProviderId).FullName).Replace("[Link]", link);
                _emailSender.SendEmail(clientEmail, "Appointment Reminder", message);
            }
        }
    }
}
