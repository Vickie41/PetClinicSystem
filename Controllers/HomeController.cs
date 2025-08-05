using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetClinicSystem.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace VetClinicPro.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(PetClinicContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Admin"))
            {
                var model = await GetAdminDashboardData();
                return View("AdminDashboard", model);
            }
            else if (User.IsInRole("Veterinarian"))
            {
                var model = await GetVetDashboardData(user.UserId);
                return View("VetDashboard", model);
            }
            else if (User.IsInRole("Staff"))
            {
                var model = await GetStaffDashboardData();
                return View("StaffDashboard", model);
            }
            else if (User.IsInRole("Client"))
            {
                var model = await GetClientDashboardData(user.UserId);
                return View("ClientDashboard", model);
            }

            return View("Dashboard");
        }

        private async Task<AdminDashboardViewModel> GetAdminDashboardData()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayOfYear = new DateTime(today.Year, 1, 1);

            var model = new AdminDashboardViewModel
            {
                TodayAppointmentsCount = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today),
                CompletedAppointmentsCount = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today && a.Status == "Completed"),
                ActivePatientsCount = await _context.Patients
                    .CountAsync(p => p.IsActive.GetValueOrDefault()),
                DogsCount = await _context.Patients
                    .CountAsync(p => p.Species == "Dog" && p.IsActive.GetValueOrDefault()),
                CatsCount = await _context.Patients
                    .CountAsync(p => p.Species == "Cat" && p.IsActive.GetValueOrDefault()),
                MonthlyRevenue = await _context.Billings
                    .Where(b => b.BillDate >= firstDayOfMonth && b.Status == "Paid")
                    .SumAsync(b => b.TotalAmount),
                YearlyRevenue = await _context.Billings
                    .Where(b => b.BillDate >= firstDayOfYear && b.Status == "Paid")
                    .SumAsync(b => b.TotalAmount),
                VaccinationsDueCount = await _context.VaccineRecords
                    .CountAsync(v => v.NextDueDate.HasValue &&
                           v.NextDueDate.Value.ToDateTime(TimeOnly.MinValue) <= today.AddDays(30)),
                OverdueVaccinationsCount = await _context.VaccineRecords
                    .CountAsync(v => v.NextDueDate.HasValue &&
                           v.NextDueDate.Value.ToDateTime(TimeOnly.MinValue) < today),
                TotalUsers = await _context.Users.CountAsync(),
                NewUsersThisMonth = await _context.Users
                    .CountAsync(u => u.CreatedDate >= firstDayOfMonth),
                ActiveVets = await _context.Users
                    .CountAsync(u => u.Role == "Veterinarian" && u.IsActive.GetValueOrDefault()),
                ActiveStaff = await _context.Users
                    .CountAsync(u => u.Role == "Staff" && u.IsActive.GetValueOrDefault()),
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.Owner)
                    .Include(a => a.Vet)
                    .Where(a => a.AppointmentDate >= today && a.AppointmentDate <= today.AddDays(7))
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
                SpeciesDistribution = await _context.Patients
                    .Where(p => p.IsActive.GetValueOrDefault())
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

            return model;
        }

        private async Task<VetDashboardViewModel> GetVetDashboardData(int userId)
        {
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

            return model;
        }

        private async Task<StaffDashboardViewModel> GetStaffDashboardData()
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

            return model;
        }

        private async Task<ClientDashboardViewModel> GetClientDashboardData(int userId)
        {
            var today = DateTime.Today;
            var owner = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.Owner)
                .FirstOrDefaultAsync();

            if (owner == null)
            {
                return new ClientDashboardViewModel();
            }

            var model = new ClientDashboardViewModel
            {
                MyPets = await _context.Patients
                    .Where(p => p.OwnerId == owner.OwnerId && p.IsActive.GetValueOrDefault())
                    .ToListAsync(),
                UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Vet)
                    .Where(a => a.Patient.OwnerId == owner.OwnerId && a.AppointmentDate >= today)
                    .OrderBy(a => a.AppointmentDate)
                    .Take(5)
                    .ToListAsync(),
                UpcomingVaccinations = await _context.VaccineRecords
                    .Include(v => v.Patient)
                    .Include(v => v.Vaccine)
                    .Where(v => v.Patient.OwnerId == owner.OwnerId &&
                           v.NextDueDate.HasValue &&
                           v.NextDueDate.Value.ToDateTime(TimeOnly.MinValue) >= today)
                    .OrderBy(v => v.NextDueDate)
                    .Take(5)
                    .ToListAsync(),
                ActivePrescriptions = await _context.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c.Patient)
                    .Where(p => p.Consultation.Patient.OwnerId == owner.OwnerId &&
                               p.PrescribedDate.HasValue &&
                               p.PrescribedDate.Value.AddDays(30) >= today)
                    .OrderByDescending(p => p.PrescribedDate)
                    .Take(5)
                    .ToListAsync(),
                OutstandingBalance = await _context.Billings
                    .Include(b => b.Consultation)
                        .ThenInclude(c => c.Patient)
                    .Where(b => b.Consultation.Patient.OwnerId == owner.OwnerId &&
                           b.Status == "Pending")
                    .SumAsync(b => b.Balance.GetValueOrDefault())
            };

            return model;
        }
    }
}