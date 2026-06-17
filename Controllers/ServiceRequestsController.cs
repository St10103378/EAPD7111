
using Logistics.Enums;
using Logistics.Models;
using Logistics.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Logistics.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ServiceRequestService _serviceRequestService;
    private readonly ContractService _contractService;
    private readonly CurrencyService _currencyService;
    public ServiceRequestsController(
        ServiceRequestService serviceRequestService,
        ContractService contractService,
        CurrencyService currencyService)
    {
        _serviceRequestService = serviceRequestService;
        _contractService = contractService;
        _currencyService = currencyService;
    }
    public async Task<IActionResult> Index()
    {
        var requests = await _serviceRequestService.GetServiceRequestsAsync();
        return View(requests);
    }
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var request = await _serviceRequestService.GetServiceRequestAsync(id.Value);
        if (request == null) return NotFound();
        return View(request);
    }
    public async Task<IActionResult> Create()
    {
        await PopulateContractDropdown();
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest request)
    {
        // Business validation
        var contract = await _contractService.GetContractAsync(request.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError("", "Contract not found.");
        }
        else if (contract.Status == ContractStatus.Expired ||
                 contract.Status == ContractStatus.OnHold)
        {
            ModelState.AddModelError("", "Cannot create request for expired/on-hold contract.");
            TempData["ErrorMessage"] = "Cannot create request for expired/on-hold contract.";
            await PopulateContractDropdown();
            return View(request);
        }
        if (ModelState.IsValid)
        {
            try
            {
                decimal converted = await _currencyService.ConvertCurrencyAsync(
                    request.Currency, "ZAR", request.Cost);
                request.Cost = converted;
                await _serviceRequestService.PostServiceRequestAsync(request);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        await PopulateContractDropdown();
        return View(request);
    }
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var request = await _serviceRequestService.GetServiceRequestAsync(id.Value);
        if (request == null) return NotFound();
        await PopulateContractDropdown(request.ContractId);
        return View(request);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceRequest request)
    {
        if (id != request.Id) return NotFound();
        var contract = await _contractService.GetContractAsync(request.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError("", "Contract not found.");
        }
        else if (contract.Status == ContractStatus.Expired ||
                 contract.Status == ContractStatus.OnHold)
        {
            ModelState.AddModelError("", "Cannot update request for expired/on-hold contract.");
            TempData["ErrorMessage"] = "Cannot update request for expired/on-hold contract.";
        }
        if (ModelState.IsValid)
        {
            try
            {
                decimal converted = await _currencyService.ConvertCurrencyAsync(
                    request.Currency, "ZAR", request.Cost);
                request.Cost = converted;
                await _serviceRequestService.UpdateServiceRequestAsync(id, request);
                TempData["ErrorMessage"] = "Service request updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        await PopulateContractDropdown(request.ContractId);
        return View(request);
    }
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var request = await _serviceRequestService.GetServiceRequestAsync(id.Value);
        if (request == null) return NotFound();
        return View(request);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _serviceRequestService.DeleteServiceRequestAsync(id);
        TempData["SuccessMessage"] = "Service request deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
    private async Task PopulateContractDropdown(int? selectedId = null)
    {
        var contracts = await _contractService.GetContractsAsync();
        ViewData["ContractId"] = new SelectList(contracts, "Id", "Id", selectedId);
    }
}