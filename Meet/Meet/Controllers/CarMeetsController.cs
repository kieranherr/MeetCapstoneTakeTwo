﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Meet.Data;
using Meet.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Meet.Controllers
{
    [Authorize(Roles ="CarGuy")]
    public class CarMeetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarMeetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CarMeets
        public async Task<IActionResult> Index()
        {

            var carMeets = _context.CarMeets;
            if (carMeets == null)
            {
                return RedirectToAction("Create");
            }
            var applicationDbContext = _context.CarMeets;
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CarMeets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carMeet = await _context.CarMeets
                .FirstOrDefaultAsync(m => m.MeetId == id);
            if (carMeet == null)
            {
                return NotFound();
            }

            return View(carMeet);
        }

        // GET: CarMeets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CarMeets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MeetId,MeetName,Street,City,State,Zip,MeetTime,MeetDate")] CarMeet carMeet)
        {
            string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={carMeet.Street},+{carMeet.City},+{carMeet.State}&key={APIKeys.GeocodeKey}";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            string jsonResult = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                JObject geoCode = JObject.Parse(jsonResult);
                carMeet.Lat = (double)geoCode["results"][0]["geometry"]["location"]["lat"];
                carMeet.Long = (double)geoCode["results"][0]["geometry"]["location"]["lng"];
            }
            if (ModelState.IsValid)
            {
                carMeet.Clients = new List<Client>();
                var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                carMeet.IdentityUserId = userId;
                _context.Add(carMeet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(carMeet);
        }

        // GET: CarMeets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carMeet = await _context.CarMeets.FindAsync(id);
            if (carMeet == null)
            {
                return NotFound();
            }
            return View(carMeet);
        }

        // POST: CarMeets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MeetId,MeetName,Lat,Long,Street,City,State,Zip,MeetTime,MeetDate")] CarMeet carMeet)
        {
            if (id != carMeet.MeetId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(carMeet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarMeetExists(carMeet.MeetId))
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
            return View(carMeet);
        }

        // GET: CarMeets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carMeet = await _context.CarMeets
                .FirstOrDefaultAsync(m => m.MeetId == id);
            if (carMeet == null)
            {
                return NotFound();
            }

            return View(carMeet);
        }

        // POST: CarMeets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carMeet = await _context.CarMeets.FindAsync(id);
            _context.CarMeets.Remove(carMeet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarMeetExists(int id)
        {
            return _context.CarMeets.Any(e => e.MeetId == id);
        }
    }
}
