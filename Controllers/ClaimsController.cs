using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog2bPOEPart2.Data;
using Prog2bPOEPart2.Models;
using Prog2bPOEPart2.Services;

namespace Prog2bPOEPart2.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly PdfReportService _pdfReportService;

        public ClaimsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, PdfReportService pdfReportService)
        {
            _userManager = userManager;
            _context = context;
            _pdfReportService = pdfReportService;
        }
    // GET: Claims
    public async Task<IActionResult> Index()
        {
            var claims = await _context.Claim.ToListAsync();
            return View(claims);
        }

        // GET: Claims/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var claim = await _context.Claim.FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            return View(claim);
        }

        // GET: Claims/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClaimId,Name,DateSubmitted,HoursWorked,HourlyRate,TotalEarning,ExtraNotes,Status,IsProgrammeCoordinatorApproved,IsAcademicManagerApproved")] Claim claim)
        {
            if (ModelState.IsValid)
            {
                claim.UserID = _userManager.GetUserId(User); // Add UserID
                claim.Status = "Pending"; // Default status
                _context.Add(claim);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }

        // GET: Claims/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var claim = await _context.Claim.FindAsync(id);
            if (claim == null) return NotFound();

            return View(claim);
        }

        // POST: Claims/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClaimId,Name,DateSubmitted,HoursWorked,HourlyRate,TotalEarning,ExtraNotes,Status,IsProgrammeCoordinatorApproved,IsAcademicManagerApproved")] Claim claim)
        {
            if (id != claim.ClaimId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(claim);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClaimExists(claim.ClaimId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }

        // GET: Claims/ApproveReject/5
        [Authorize(Roles = "Programme Coordinator, Academic Manager")]
        public async Task<IActionResult> ApproveReject(int id)
        {
            var claim = await _context.Claim.FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            // Update approval status based on role
            if (User.IsInRole("Programme Coordinator"))
                claim.IsProgrammeCoordinatorApproved = true;
            if (User.IsInRole("Academic Manager"))
                claim.IsAcademicManagerApproved = true;

            // Update overall status
            claim.Status = (claim.IsProgrammeCoordinatorApproved && claim.IsAcademicManagerApproved)
                ? "Approved"
                : "Pending";

            _context.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = claim.ClaimId });
        }
        // POST: Claims/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Programme Coordinator, Academic Manager")]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claim.FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            // Update individual approval status
            if (User.IsInRole("Programme Coordinator"))
                claim.IsProgrammeCoordinatorApproved = true;

            if (User.IsInRole("Academic Manager"))
                claim.IsAcademicManagerApproved = true;

            // Dynamically calculate overall status
            if (claim.IsProgrammeCoordinatorApproved && claim.IsAcademicManagerApproved)
                claim.Status = "Approved";
            else if (claim.IsProgrammeCoordinatorApproved || claim.IsAcademicManagerApproved)
                claim.Status = "Pending"; // One has approved; waiting for the other
            else
                claim.Status = "Rejected"; // Both have acted, but both rejected (fallback)

            _context.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // POST: Claims/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Programme Coordinator, Academic Manager")]
        public async Task<IActionResult> Reject(int id)
        {
            var claim = await _context.Claim.FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            // Update individual rejection status
            if (User.IsInRole("Programme Coordinator"))
                claim.IsProgrammeCoordinatorApproved = false; // Explicitly mark as not approved

            if (User.IsInRole("Academic Manager"))
                claim.IsAcademicManagerApproved = false; // Explicitly mark as not approved

            // Dynamically calculate overall status
            if (claim.IsProgrammeCoordinatorApproved == false || claim.IsAcademicManagerApproved == false)
                claim.Status = "Rejected"; // Any rejection causes overall rejection
            else if (claim.IsProgrammeCoordinatorApproved && claim.IsAcademicManagerApproved)
                claim.Status = "Approved"; // If both approve
            else
                claim.Status = "Pending"; // Otherwise, it's still pending

            _context.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> DownloadReport()
        {
            // Fetch all claims data
            var claims = await _context.Claim.ToListAsync();

            // Generate PDF report using the PdfReportService
            var pdfBytes = _pdfReportService.GeneratePdfReport(claims);

            // Return the PDF as a file download
            return File(pdfBytes, "application/pdf", "ClaimsReport.pdf");
        }
        // POST: Claims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var claim = await _context.Claim.FindAsync(id);
            if (claim != null)
            {
                _context.Claim.Remove(claim);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ClaimExists(int id)
        {
            return _context.Claim.Any(e => e.ClaimId == id);
        }
    }
}


//Code attribution
//Used the Entity Framework Scaffolding
//CRUD

//Identity Scaffolding was used
//https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0&tabs=visual-studio