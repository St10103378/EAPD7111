using Logistics.Data;
using Logistics.Enums;
using Logistics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Controllers
{
   
    public class ContractsController : Controller
    {
        private readonly GLMSDbContext _context;

        public ContractsController(GLMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
    DateTime? startDate,
    DateTime? endDate,
    string status,
    int? clientId)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            
            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

           
            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

          
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse(status, out ContractStatus parsedStatus))
                {
                    query = query.Where(c => c.Status == parsedStatus);
                }
            }

           
            if (clientId.HasValue)
            {
                query = query.Where(c => c.ClientId == clientId.Value);
            }

            var contracts = await query.ToListAsync();

            return View(contracts);
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }
        public IActionResult Create()
        {
            ViewData["ClientId"] =
                new SelectList(_context.Clients,
                    "Id",
                    "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Contract contract)
        {
            if (contract.EndDate < contract.StartDate)
            {
                ModelState.AddModelError("EndDate",
                    "End date cannot be earlier than start date.");

                return View(contract);
            }

            if (contract.AgreementFile != null)
            {
                var extension = Path.GetExtension(contract.AgreementFile.FileName).ToLower();

                if (extension != ".pdf")
                {
                    ModelState.AddModelError("AgreementFile",
                        "Only PDF files are allowed.");

                    return View(contract);
                }

                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/contracts");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName =
                    Guid.NewGuid() + extension;

                string path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await contract.AgreementFile.CopyToAsync(stream);

                contract.SignedAgreementPath = fileName;
            }

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "Contract created successfully.";

            return RedirectToAction(nameof(Index));
        }
        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }
        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts
                .FindAsync(id);

            if (contract != null)
            {
                // Delete uploaded file
                if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                {
                    string folder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/uploads/contracts");

                    string filePath = Path.Combine(
                        folder,
                        contract.SignedAgreementPath);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Contracts.Remove(contract);

                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] =
                    "Contract deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            ViewData["ClientId"] =
                new SelectList(_context.Clients,
                    "Id",
                    "Name",
                    contract.ClientId);

            return View(contract);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (contract.AgreementFile != null)
            {
                var extension = Path.GetExtension(contract.AgreementFile.FileName).ToLower();

                if (extension != ".pdf")
                {
                    ModelState.AddModelError("AgreementFile",
                        "Only PDF files are allowed.");

                    return View(contract);
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingContract =
                        await _context.Contracts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingContract == null)
                    {
                        return NotFound();
                    }

                    
                    contract.SignedAgreementPath =
                        existingContract.SignedAgreementPath;

                    // Replace uploaded file
                    if (contract.AgreementFile != null)
                    {
                        string folder = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot/uploads/contracts");

                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        // Delete old file
                        if (!string.IsNullOrEmpty(existingContract.SignedAgreementPath))
                        {
                            string oldFilePath = Path.Combine(
                                folder,
                                existingContract.SignedAgreementPath);

                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save new file
                        string fileName =
                            Guid.NewGuid() +
                            Path.GetExtension(contract.AgreementFile.FileName);

                        string newPath = Path.Combine(folder, fileName);

                        using var stream =
                            new FileStream(newPath, FileMode.Create);

                        await contract.AgreementFile.CopyToAsync(stream);

                        contract.SignedAgreementPath = fileName;
                    }

                    _context.Update(contract);

                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] =
                        "Contract updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Contracts.Any(e => e.Id == contract.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] =
                new SelectList(_context.Clients,
                    "Id",
                    "Name",
                    contract.ClientId);

            return View(contract);
        }
    
}
}