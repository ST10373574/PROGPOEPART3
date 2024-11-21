using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prog2bPOEPart2.Data;
using Prog2bPOEPart2.Models;

namespace Prog2bPOEPart2.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Document.Include(d => d.Claim);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .Include(d => d.Claim)
                .FirstOrDefaultAsync(m => m.DocumentId == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        [Authorize(Roles = "Lecturer")]
        // GET: Documents/Create
        public IActionResult Create()
        {
            // Modify the SelectList to display Claim.Name instead of ClaimId
            ViewData["ClaimId"] = new SelectList(_context.Claim, "ClaimId", "Name");
            return View(new Document()); // Empty model to pass
        }

        // POST: Documents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, [Bind("ClaimId")] Document document)
        {
            const long maxFileSize = 15 * 1024 * 1024;
            string[] permittedExtensions = { ".pdf", ".doc", ".docx" };

            if (file != null && file.Length > 0)
            {
                if (file.Length > maxFileSize)
                {
                    ModelState.AddModelError("File", "The file size cannot exceed 15 MB. Please upload a smaller file.");
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!permittedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("File", "Only PDF and Word document files (.pdf, .doc, .docx) are allowed.");
                }

                if (ModelState.IsValid)
                {
                    document.FileName = Path.GetFileName(file.FileName);
                    document.FileSize = file.Length;
                    document.FileType = file.ContentType;
                    document.UploadedDate = DateTime.Now;

                    var documentsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");
                    if (!Directory.Exists(documentsPath))
                    {
                        Directory.CreateDirectory(documentsPath);
                    }

                    var filePath = Path.Combine(documentsPath, document.FileName);
                    document.FilePath = filePath;

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.Add(document);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                ModelState.AddModelError("File", "Please upload a file.");
            }

            // Modify the SelectList to display Claim.Name instead of ClaimId
            ViewData["ClaimId"] = new SelectList(_context.Claim, "ClaimId", "Name", document.ClaimId);
            return View(document);
        }

        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            // Modify the SelectList to display Claim.Name instead of ClaimId
            ViewData["ClaimId"] = new SelectList(_context.Claim, "ClaimId", "Name", document.ClaimId);
            return View(document);
        }


        // POST: Documents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DocumentId,FileName,FilePath,FileSize,FileType,UploadedDate,ClaimId")] Document document)
        {
            if (id != document.DocumentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.DocumentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClaimId"] = new SelectList(_context.Claim, "ClaimId", "ClaimId", document.ClaimId);
            return View(document);
        }

        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .Include(d => d.Claim)
                .FirstOrDefaultAsync(m => m.DocumentId == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Document.FindAsync(id);
            if (document != null)
            {
                _context.Document.Remove(document);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentExists(int id)
        {
            return _context.Document.Any(e => e.DocumentId == id);
        }

        // GET: Documents/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Document.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = document.FilePath;
            var fileName = document.FileName;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}

//Code attribution
//Used the Entity Framework Scaffolding
//CRUD