
using Logistics.Models;
using Logistics.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Logistics.Controllers;

public class ContractsController : Controller
{
    private readonly ContractService _contractService;
    private readonly ClientService _clientService;
    public ContractsController(ContractService contractService, ClientService clientService)
    {
        _contractService = contractService;
        _clientService = clientService;
    }
    public async Task<IActionResult> Index()
    {
        var contracts = await _contractService.GetContractsAsync();
        return View(contracts);
    }
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var contract = await _contractService.GetContractAsync(id.Value);
        if (contract == null) return NotFound();
        return View(contract);
    }
    public async Task<IActionResult> Create()
    {
        await PopulateClientDropdown();
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contract contract)
    {
        if (contract.EndDate < contract.StartDate)
        {
            ModelState.AddModelError("EndDate", "End date cannot be earlier than start date.");
            await PopulateClientDropdown();
            return View(contract);
        }
        if (contract.AgreementFile != null)
        {
            var extension = Path.GetExtension(contract.AgreementFile.FileName).ToLower();
            if (extension != ".pdf")
            {
                ModelState.AddModelError("AgreementFile", "Only PDF files are allowed.");
                await PopulateClientDropdown();
                return View(contract);
            }
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/contracts");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid() + extension;
            string path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await contract.AgreementFile.CopyToAsync(stream);
            contract.SignedAgreementPath = fileName;
        }
        if (ModelState.IsValid)
        {
            await _contractService.PostContractAsync(contract);
            TempData["ErrorMessage"] = "Contract created successfully.";
            return RedirectToAction(nameof(Index));
        }
        await PopulateClientDropdown();
        return View(contract);
    }
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var contract = await _contractService.GetContractAsync(id.Value);
        if (contract == null) return NotFound();
        await PopulateClientDropdown(contract.ClientId);
        return View(contract);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contract contract)
    {
        if (id != contract.Id) return NotFound();
        if (contract.AgreementFile != null)
        {
            var extension = Path.GetExtension(contract.AgreementFile.FileName).ToLower();
            if (extension != ".pdf")
            {
                ModelState.AddModelError("AgreementFile", "Only PDF files are allowed.");
                await PopulateClientDropdown(contract.ClientId);
                return View(contract);
            }
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/contracts");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            // Delete old file if exists
            if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
            {
                string oldFilePath = Path.Combine(folder, contract.SignedAgreementPath);
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }
            string fileName = Guid.NewGuid() + Path.GetExtension(contract.AgreementFile.FileName);
            string newPath = Path.Combine(folder, fileName);
            using var stream = new FileStream(newPath, FileMode.Create);
            await contract.AgreementFile.CopyToAsync(stream);
            contract.SignedAgreementPath = fileName;
        }
        if (ModelState.IsValid)
        {
            await _contractService.UpdateContractAsync(id, contract);
            TempData["ErrorMessage"] = "Contract updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        await PopulateClientDropdown(contract.ClientId);
        return View(contract);
    }
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var contract = await _contractService.GetContractAsync(id.Value);
        if (contract == null) return NotFound();
        return View(contract);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var contract = await _contractService.GetContractAsync(id);
        if (contract != null && !string.IsNullOrEmpty(contract.SignedAgreementPath))
        {
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/contracts");
            string filePath = Path.Combine(folder, contract.SignedAgreementPath);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
        await _contractService.DeleteContractAsync(id);
        TempData["ErrorMessage"] = "Contract deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
    private async Task PopulateClientDropdown(int? selectedId = null)
    {
        var clients = await _clientService.GetClientsAsync();
        ViewData["ClientId"] = new SelectList(clients, "Id", "Name", selectedId);
    }
}