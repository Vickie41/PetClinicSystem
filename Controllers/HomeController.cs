using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PetClinicSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly PetClinicContext _context;

        public HomeController(PetClinicContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var todayDateOnly = DateOnly.FromDateTime(DateTime.Today);
            var todayDateTime = DateTime.Today;
            var firstDayOfMonth = new DateTime(todayDateTime.Year, todayDateTime.Month, 1);
            var firstDayOfYear = new DateTime(todayDateTime.Year, 1, 1);

            var model = new AdminDashboardViewModel
            {
                // Change from: .Count(p => p.IsActive.GetValueOrDefault())
                TodayAppointmentsCount = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == todayDateTime),
                CompletedAppointmentsCount = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == todayDateTime && a.Status == "Completed"),
                // Fix for nullable boolean
                ActivePatientsCount = await _context.Patients
                    .CountAsync(p => p.IsActive.HasValue && p.IsActive.Value),
                // Fix for nullable boolean
                DogsCount = await _context.Patients
                    .CountAsync(p => p.Species == "Dog" && p.IsActive.HasValue && p.IsActive.Value),
                // Fix for nullable boolean
                CatsCount = await _context.Patients
                    .CountAsync(p => p.Species == "Cat" && p.IsActive.HasValue && p.IsActive.Value),
                MonthlyRevenue = await _context.Billings
                    .Where(b => b.BillDate >= firstDayOfMonth && b.Status == "Paid")
                    .SumAsync(b => b.TotalAmount),
                YearlyRevenue = await _context.Billings
                    .Where(b => b.BillDate >= firstDayOfYear && b.Status == "Paid")
                    .SumAsync(b => b.TotalAmount),
                VaccinationsDueCount = await _context.VaccineRecords
            .CountAsync(v => v.NextDueDate.HasValue &&
                   v.NextDueDate <= todayDateOnly.AddDays(30)),
                OverdueVaccinationsCount = await _context.VaccineRecords
            .CountAsync(v => v.NextDueDate.HasValue &&
                   v.NextDueDate < todayDateOnly),
                TotalUsers = await _context.Users.CountAsync(),
                NewUsersThisMonth = await _context.Users
                    .CountAsync(u => u.CreatedDate >= firstDayOfMonth),
                // Fix for nullable boolean
                ActiveVets = await _context.Users
                    .CountAsync(u => u.Role == "Veterinarian" && u.IsActive.HasValue && u.IsActive.Value),
                // Fix for nullable boolean
                ActiveStaff = await _context.Users
                    .CountAsync(u => u.Role == "Staff" && u.IsActive.HasValue && u.IsActive.Value),
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.Owner)
                    .Include(a => a.Vet)
                    .Where(a => a.AppointmentDate >= todayDateTime && a.AppointmentDate <= todayDateTime.AddDays(7))
                    .OrderBy(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync(),
                RecentActivities = await _context.ActivityLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5)
                    .ToListAsync(),
                RecentPayments = await _context.Billings
                    .Where(b => b.Status == "Paid")
                    .OrderByDescending(b => b.BillDate)
                    .Take(5)
                    .ToListAsync(),
                // Fix for nullable boolean
                SpeciesDistribution = await _context.Patients
                    .Where(p => p.IsActive.HasValue && p.IsActive.Value)
                    .GroupBy(p => p.Species)
                    .Select(g => new { Species = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Species, g => g.Count),
                MonthlyAppointments = await _context.Appointments
                    .Where(a => a.AppointmentDate >= firstDayOfMonth.AddMonths(-6))
                    .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new {
                        Month = g.Key.Month.ToString("MMM", CultureInfo.InvariantCulture),
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(g => g.Month, g => g.Count),
                RevenueByService = await _context.BillingDetails
                    .Include(bd => bd.Bill)
                    .Where(bd => bd.Bill.BillDate >= firstDayOfMonth && bd.Bill.Status == "Paid")
                    .GroupBy(bd => bd.ItemType)
                    .Select(g => new { Service = g.Key, Amount = g.Sum(bd => bd.TotalPrice) })
                    .ToDictionaryAsync(g => g.Service, g => g.Amount)
            };

            // Calculate revenue change percentage
            var lastMonthRevenue = await _context.Billings
                .Where(b => b.BillDate >= firstDayOfMonth.AddMonths(-1) &&
                       b.BillDate < firstDayOfMonth && b.Status == "Paid")
                .SumAsync(b => b.TotalAmount);

            model.RevenueChangePercentage = lastMonthRevenue > 0 ?
                ((model.MonthlyRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

            return View(model);
        }

        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> VetDashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var model = new VetDashboardViewModel
            {
                MyAppointmentsToday = await _context.Appointments
                    .CountAsync(a => a.VetId == userId && a.AppointmentDate.Date == today),
                MyCompletedAppointments = await _context.Appointments
                    .CountAsync(a => a.VetId == userId && a.AppointmentDate.Date == today && a.Status == "Completed"),
                PatientsToFollowUp = await _context.Consultations
                    .CountAsync(c => c.VetId == userId && c.FollowUpDate.HasValue && c.FollowUpDate >= today),
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.Owner)
                    .Where(a => a.VetId == userId && a.AppointmentDate >= today && a.AppointmentDate <= today.AddDays(7))
                    .OrderBy(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync(),
                RecentPatients = await _context.Patients
                    .Include(p => p.Owner)
                    .Where(p => p.Consultations.Any(c => c.VetId == userId))
                    .OrderByDescending(p => p.Consultations.Max(c => c.ConsultationDate))
                    .Take(5)
                    .ToListAsync(),
                RecentPrescriptions = await _context.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Patient)
                    .Where(p => p.Consultation.VetId == userId)
                    .OrderByDescending(p => p.PrescribedDate)
                    .Take(5)
                    .ToListAsync(),
                RecentActivities = await _context.ActivityLogs
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5)
                    .ToListAsync(),
                SpeciesDistribution = await _context.Consultations
                    .Include(c => c.Patient)
                    .Where(c => c.VetId == userId)
                    .GroupBy(c => c.Patient.Species)
                    .Select(g => new { Species = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Species, g => g.Count),
                MonthlyAppointments = await _context.Appointments
                    .Where(a => a.VetId == userId && a.AppointmentDate >= firstDayOfMonth.AddMonths(-6))
                    .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new {
                        Month = g.Key.Month.ToString("MMM", CultureInfo.InvariantCulture),
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(g => g.Month, g => g.Count)
            };

            return View(model);
        }

        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> StaffDashboard()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var model = new StaffDashboardViewModel
            {
                TodayAppointmentsCount = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today),
                CompletedAppointmentsCount = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today && a.Status == "Completed"),
                NewPatientsThisMonth = await _context.Patients
                    .CountAsync(p => p.CreatedDate >= firstDayOfMonth),
                PendingPayments = await _context.Billings
                    .CountAsync(b => b.Status == "Pending"),
                PendingPaymentsAmount = await _context.Billings
                    .Where(b => b.Status == "Pending")
                    .SumAsync(b => b.Balance.GetValueOrDefault()),
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.Owner)
                    .Include(a => a.Vet)
                    .Where(a => a.AppointmentDate >= today && a.AppointmentDate <= today.AddDays(3))
                    .OrderBy(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync(),
                NewOwners = await _context.Owners
                    .OrderByDescending(o => o.CreatedDate)
                    .Take(5)
                    .ToListAsync(),
                RecentActivities = await _context.ActivityLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5)
                    .ToListAsync(),
                MonthlyAppointments = await _context.Appointments
                    .Where(a => a.AppointmentDate >= firstDayOfMonth.AddMonths(-6))
                    .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new {
                        Month = g.Key.Month.ToString("MMM", CultureInfo.InvariantCulture),
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(g => g.Month, g => g.Count)
            };

            return View(model);
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> ClientDashboard()
        {
            // Initialize with default values
            var model = new ClientDashboardViewModel
            {
                MyPets = new List<Patient>(),
                UpcomingAppointments = new List<Appointment>(),
                UpcomingVaccinations = new List<VaccineRecord>(),
                ActivePrescriptions = new List<Prescription>(),
                OutstandingBalance = 0
            };

           
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var owner = await _context.Owners.FirstOrDefaultAsync(o => o.UserId == userId);

                if (owner != null)
                {
                    model.MyPets = await _context.Patients
                        .Where(p => p.OwnerId == owner.OwnerId && p.IsActive.HasValue && p.IsActive.Value)
                        .ToListAsync() ?? new List<Patient>();

                    model.UpcomingAppointments = await _context.Appointments
                        .Include(a => a.Patient)
                        .Include(a => a.Vet)
                        .Where(a => a.Patient.OwnerId == owner.OwnerId && a.AppointmentDate >= DateTime.Today)
                        .OrderBy(a => a.AppointmentDate)
                        .Take(5)
                        .ToListAsync() ?? new List<Appointment>();

                    model.UpcomingVaccinations = await _context.VaccineRecords
                        .Include(v => v.Patient)
                        .Include(v => v.Vaccine)
                        .Where(v => v.Patient.OwnerId == owner.OwnerId &&
                               v.NextDueDate.HasValue &&
                               v.NextDueDate >= DateOnly.FromDateTime(DateTime.Today))
                        .OrderBy(v => v.NextDueDate)
                        .Take(5)
                        .ToListAsync() ?? new List<VaccineRecord>();

                    model.ActivePrescriptions = await _context.Prescriptions
                        .Include(p => p.Consultation)
                            .ThenInclude(c => c.Patient)
                        .Where(p => p.Consultation.Patient.OwnerId == owner.OwnerId &&
                                   p.PrescribedDate.HasValue &&
                                   p.PrescribedDate.Value.AddDays(30) >= DateTime.Today)
                        .OrderByDescending(p => p.PrescribedDate)
                        .Take(5)
                        .ToListAsync() ?? new List<Prescription>();

                    model.OutstandingBalance = await _context.Billings
                        .Include(b => b.Consultation)
                            .ThenInclude(c => c.Patient)
                        .Where(b => b.Consultation.Patient.OwnerId == owner.OwnerId &&
                               b.Status == "Pending")
                        .SumAsync(b => b.Balance.GetValueOrDefault());
                }
            
           

            return View(model);
        }
    }
}