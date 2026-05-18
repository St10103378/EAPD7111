using Logistics.Data;
using Logistics.Enums;
using Logistics.Models;
using Logistics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace Logistics.Controllers
{

    public class ServiceRequestsController : Controller
    {
        private readonly GLMSDbContext _context;

        private readonly CurrencyService _currencyService;

        public ServiceRequestsController(
            GLMSDbContext context,
            CurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ToListAsync();

            return View(requests);
        }

        public IActionResult Create()
        {
            ViewData["ContractId"] =
                new SelectList(_context.Contracts,
                    "Id",
                    "Id");

            return View();
        }
        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request)
        {
            var contract = await _context.Contracts
                .FindAsync(request.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("",
                    "Contract not found.");
            }
            else if (contract.Status == ContractStatus.Expired ||
                     contract.Status == ContractStatus.OnHold)
            {
                ModelState.AddModelError("",
                    "Cannot create request for expired/on-hold contract.");
                TempData["ErrorMessage"] = "Cannot create request for expired/on-hold contract.";

                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                decimal converted =
                    await _currencyService.ConvertCurrencyAsync(
                        request.Currency,
                        "ZAR",
                        request.Cost);

                request.Cost = converted;

                _context.ServiceRequests.Add(request);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }
        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.ServiceRequests
                .FindAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            ViewData["ContractId"] =
                new SelectList(_context.Contracts,
                    "Id",
                    "Id",
                    request.ContractId);

            return View(request);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest request)
        {
            if (id != request.Id)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .FindAsync(request.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("",
                    "Contract not found.");
            }
            else if (contract.Status == ContractStatus.Expired ||
                     contract.Status == ContractStatus.OnHold)
            {
                ModelState.AddModelError("",
                    "Cannot update request for expired/on-hold contract.");
                TempData["ErrorMessage"] =
                        "Cannot update request for expired/on-hold contract.";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    decimal converted =
                        await _currencyService.ConvertCurrencyAsync(
                            request.Currency,
                            "ZAR",
                            request.Cost);

                    request.Cost = converted;

                    _context.Update(request);

                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] =
                        "Service request updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(request.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ContractId"] =
                new SelectList(_context.Contracts,
                    "Id",
                    "Id",
                    request.ContractId);

            return View(request);
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.ServiceRequests
                .FindAsync(id);

            if (request != null)
            {
                _context.ServiceRequests.Remove(request);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Service request deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests
                .Any(e => e.Id == id);
        }
    
}
}
