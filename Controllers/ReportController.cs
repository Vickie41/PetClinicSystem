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





        // GET: Reports/PrescriptionReports
        public IActionResult PrescriptionReports(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddMonths(-3);
            endDate ??= DateTime.Today.AddDays(1);

            var prescriptions = _context.Prescriptions
                .Include(p => p.Consultation)
                .Where(p => p.PrescribedDate >= startDate && p.PrescribedDate <= endDate)
                .OrderByDescending(p => p.PrescribedDate)
                .ToList();

            // If you need patient information, you might need to load it separately
            var patientIds = prescriptions.Select(p => p.Consultation.PatientId).Distinct();
            var patients = _context.Patients.Where(p => patientIds.Contains(p.PatientId)).ToDictionary(p => p.PatientId);

            var model = new PrescriptionReportsViewModel
            {
                Prescriptions = prescriptions,
                Patients = patients,  // Add this to your ViewModel
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalPrescriptions = prescriptions.Count,
                MostPrescribedMedication = prescriptions
                    .GroupBy(p => p.MedicationName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };

            return View(model);
        }

        // GET: Reports/VaccineReports
        public IActionResult VaccineReports(DateTime? startDate, DateTime? endDate)
        {
            var viewModel = new VaccineReportsViewModel
            {
                StartDate = startDate ?? DateTime.Now.AddMonths(-1),
                EndDate = endDate ?? DateTime.Now
            };

            // Convert DateTime to DateOnly for comparison
            var startDateOnly = DateOnly.FromDateTime(viewModel.StartDate);
            var endDateOnly = DateOnly.FromDateTime(viewModel.EndDate);

            // Query VaccineRecords within date range
            var vaccineRecords = _context.VaccineRecords
                .Include(vr => vr.Vaccine)
                .Include(vr => vr.Patient)
                    .ThenInclude(p => p.Owner)
                .Include(vr => vr.AdministeredByNavigation)
                .Where(vr => vr.DateGiven >= startDateOnly && vr.DateGiven <= endDateOnly)
                .ToList();

            viewModel.VaccineRecords = vaccineRecords;
            viewModel.TotalVaccines = vaccineRecords.Count;

            // Get most common vaccine
            viewModel.MostCommonVaccine = vaccineRecords
                .GroupBy(vr => vr.Vaccine.Name)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "None";

            return View(viewModel);
        }

        // GET: Reports/TreatmentReports
        public IActionResult TreatmentReports(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddMonths(-6);
            endDate ??= DateTime.Today.AddDays(1);

            var consultationTreatments = _context.ConsultationTreatments
                .Include(ct => ct.Consultation)
                    .ThenInclude(c => c.Patient)
                        .ThenInclude(p => p.Owner)
                .Include(ct => ct.Consultation)
                    .ThenInclude(c => c.Vet)  // Use the Vet navigation property
                .Include(ct => ct.Treatment)
                .Where(ct => ct.Consultation.ConsultationDate >= startDate && ct.Consultation.ConsultationDate <= endDate)
                .OrderByDescending(ct => ct.Consultation.ConsultationDate)
                .ToList();

            var model = new TreatmentReportsViewModel
            {
                ConsultationTreatments = consultationTreatments,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalTreatments = consultationTreatments.Count,
                MostCommonTreatment = consultationTreatments
                    .GroupBy(ct => ct.Treatment.Name)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };

            return View(model);
        }

        // GET: Reports/DiagnosticReports
        public IActionResult DiagnosticReports(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddMonths(-6);
            endDate ??= DateTime.Today.AddDays(1);

            var diagnostics = _context.DiagnosticTests
                .Include(d => d.Consultation)
                    .ThenInclude(c => c.Patient)
                        .ThenInclude(p => p.Owner)
                .Include(d => d.Consultation)
                    .ThenInclude(c => c.Vet)
                .Where(d => d.TestDate >= startDate && d.TestDate <= endDate)
                .OrderByDescending(d => d.TestDate)
                .ToList();

            var model = new DiagnosticReportsViewModel
            {
                Diagnostics = diagnostics,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalTests = diagnostics.Count,
                MostCommonTest = diagnostics
                    .GroupBy(d => d.TestName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };

            return View(model);
        }

        // GET: Reports/ConsulantReports
        public IActionResult ConsulantReports(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddMonths(-3);
            endDate ??= DateTime.Today.AddDays(1);

            var consultations = _context.Consultations
                .Include(c => c.Vet)
                .Where(c => c.ConsultationDate >= startDate && c.ConsultationDate <= endDate)
                .OrderByDescending(c => c.ConsultationDate)
                .ToList();

            // Load patient information separately
            var patientIds = consultations.Select(c => c.PatientId).Distinct();
            var patients = _context.Patients
                .Where(p => patientIds.Contains(p.PatientId))
                .ToDictionary(p => p.PatientId);

            var model = new ConsultantReportsViewModel
            {
                Consultations = consultations,
                Patients = patients,  // Add this to your ViewModel
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalConsultations = consultations.Count,
                ConsultationsByVet = consultations
                    .GroupBy(c => c.Vet.FirstName + " " + c.Vet.LastName)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return View(model);
        }
    }

}
