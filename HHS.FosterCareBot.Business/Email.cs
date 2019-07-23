using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using System;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace HHS.FosterCareBot.Business
{
    public static class Email
    {
        public static void SendAppointment(DateTime startDate, DateTime endDate, string toEmail, string fromEmail, string password)
        {
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.office365.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(fromEmail, password);

            MailMessage message = new MailMessage();
            message.To.Add(toEmail);
            message.From = new MailAddress(fromEmail);
            message.Subject = "Upcoming Court Date";
            message.Body = "Here's your upcoming court date";
            message.IsBodyHtml = true;
            var calendar = new Calendar();
            var iEvent = calendar.Create<CalendarEvent>();
            iEvent.Summary = "Court Hearing";
            iEvent.Start = new CalDateTime(startDate);
            iEvent.End = new CalDateTime(endDate);
            iEvent.Description = "Court Hearing Appointment";
            iEvent.Status = EventStatus.Confirmed;
            iEvent.Location = "County Court House";

            
            calendar.Events.Add(iEvent);

            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);
            var bytesCalendar = Encoding.UTF8.GetBytes(serializedCalendar);
            MemoryStream ms = new MemoryStream(bytesCalendar);
            System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(ms, "event.ics", "text/calendar");

            message.Attachments.Add(attachment);
            client.Send(message);
        }
    }
}
