using Logistics.Data;
using Logistics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Logistics.Controllers
{

    public class ClientsController : Controller
    {
        private readonly GLMSDbContext _context;

        public ClientsController(GLMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients.ToListAsync());
        }
        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // CREATE GET
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                bool clientExists = await _context.Clients
               .AnyAsync(c => c.ContactDetails == client.ContactDetails);

                if (clientExists)
                {
                    ModelState.AddModelError("Contact Details", "Client already exists.");
                    TempData["ErrorMessage"] = "Client with the same contact details already exists.";
                    return View(client);
                }
                _context.Clients.Add(client);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }
        // EDIT GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool clientExists = await _context.Clients
                    .AnyAsync(c => c.ContactDetails == client.ContactDetails
                                && c.Id != client.Id);

                if (clientExists)
                {
                    ModelState.AddModelError("ContactDetails",
                        "Another client already uses these contact details.");

                    return View(client);
                }

                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();

                    TempData["ErrorMessage"] =
                        "Client updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] =
                    "Client deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    
}
}
