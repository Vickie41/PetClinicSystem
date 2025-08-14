using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;   
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetClinicSystem.Models;
using System.Linq;

namespace PetClinicSystem.Controllers
{
    [Authorize(Roles = "Admin,Staff,Veterinarian")]
    public class ReportController : Controller
    {
        private readonly PetClinicContext _context;

        public ReportController(PetClinicContext context)
        {
            _context = context;
        }

        // GET: Reports/Medical
        public IActionResult MedicalReports()
        {
            var consultations = _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Vet)
                .OrderByDescending(c => c.ConsultationDate)
                .ToList();

            var vaccineRecords = _context.VaccineRecords
                .Include(v => v.Vaccine)
                .Include(v => v.Patient)
                .ToList();

            var model = new MedicalReportsViewModel
            {
                Consultations = consultations,
                VaccineRecords = vaccineRecords
            };

            return View(model);
        }

        

        // GET: Reports/System
        public IActionResult SystemReports()
        {
            var users = _context.Users.ToList();
            var activityLogs = _context.ActivityLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(100)
                .ToList();

            var model = new SystemReportsViewModel
            {
                Users = users,
                ActivityLogs = activityLogs,
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.IsActive==true),
                Last24HoursActivity = activityLogs.Count(a => a.Timestamp > DateTime.Now.AddHours(-24))
            };

            return View(model);
        }

        // GET: Reports/Financial
        public IActionResult FinancialReports()
        {
            var bills = _context.Billings
                .Include(b => b.Consultation)
                .Include(b => b.Appointment)
                .Include(b => b.Patient)
                .Include(b => b.BillingDetails)
                .OrderByDescending(b => b.BillDate)
                .ToList();

            var model = new FinancialReportsViewModel
            {
                Bills = bills,
                TotalRevenue = bills.Sum(b => b.PaidAmount ?? 0),
                OutstandingBalance = bills.Sum(b => b.Balance ?? 0),
                AverageBillAmount = bills.Any() ? bills.Average(b => b.TotalAmount) : 0,
                PaidBillsCount = bills.Count(b => b.Status == "Paid"),
                PendingBillsCount = bills.Count(b => b.Status == "Pending")
            };

            return View(model);
        }


        // GET: Reports/Appointments
        public IActionResult AppointmentReports(DateTime? startDate, DateTime? endDate, string status = null)
        {
            // Set default date range if not provided
            startDate ??= DateTime.Today.AddMonths(-1);
            endDate ??= DateTime.Today.AddDays(1); // Include today

            var query = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.Owner)
                .Include(a => a.Vet)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate);

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(a => a.Status == status);
            }

            var appointments = query
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            var model = new AppointmentReportsViewModel
            {
                Appointments = appointments,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Status = status,
                TotalAppointments = appointments.Count,
                CompletedCount = appointments.Count(a => a.Status == "Completed"),
                CancelledCount = appointments.Count(a => a.Status == "Cancelled"),
                PendingCount = appointments.Count(a => a.Status == "Pending")
            };

            return View(model);
        }

        
    }

}
