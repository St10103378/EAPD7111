
using Logistics.Models;
using Logistics.Services;
using Microsoft.AspNetCore.Mvc;
namespace Logistics.Controllers;

public class ClientsController : Controller
{
    private readonly ClientService _clientService;
    public ClientsController(ClientService clientService)
    {
        _clientService = clientService;
    }
    public async Task<IActionResult> Index()
    {
        var clients = await _clientService.GetClientsAsync();
        return View(clients);
    }
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var client = await _clientService.GetClientAsync(id.Value);
        if (client == null) return NotFound();
        return View(client);
    }
    public IActionResult Create() => View();
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _clientService.PostClientAsync(client);
                TempData["ErrorMessage"] = "Client created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(client);
    }
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var client = await _clientService.GetClientAsync(id.Value);
        if (client == null) return NotFound();
        return View(client);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Client client)
    {
        if (id != client.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                // Optional: You can add duplicate check logic in the Service if needed
                await _clientService.UpdateClientAsync(id, client);
                TempData["ErrorMessage"] = "Client updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(client);
    }
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var client = await _clientService.GetClientAsync(id.Value);
        if (client == null) return NotFound();
        return View(client);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _clientService.DeleteClientAsync(id);
        TempData["ErrorMessage"] = "Client deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}