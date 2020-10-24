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
using System.Reflection.Metadata.Ecma335;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Meet.Controllers
{
    [Authorize(Roles = "CarGuy")]
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cars
        public async Task<IActionResult> Index()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var car = _context.Cars.Where(c => c.IdentityUserId == userId).FirstOrDefault();
            var garage = _context.Garages.Where(g => g.CarId == car.CarId).FirstOrDefault();
 
            if(garage.Car == null)
            {
                return RedirectToAction("Create");
            }
            
           
            return View(garage.Car);
        }
        public async Task<IActionResult> ClientDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var garage = _context.Garages.Where(g => g.ClientId == id).FirstOrDefault();

            var car = await _context.Cars.FirstOrDefaultAsync(m => m.CarId == garage.CarId);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }
        public async Task<IActionResult> CarRate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CarRate(int id, [Bind("CarId,Vin,Make,Model,Year,Mileage,Mods,AvgRating,IdentityUserId")] Car car, int newRate)
        {
            if (id != car.CarId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
             
                    car.AvgRating = (newRate + car.AvgRating) / 2;
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.CarId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TwilioClient.Init(APIKeys.TwilioAccountSid, APIKeys.TwilioAuthToken);
                var garage = _context.Garages.Where(c => c.CarId == id).FirstOrDefault();
                var client = _context.Clients.Where(c => c.ClientId == garage.ClientId).FirstOrDefault();

                var message = MessageResource.Create(
            body: $"Your {car.Year} {car.Make} {car.Model} was just rated at a Meet! Check it out!",
            from: new Twilio.Types.PhoneNumber("+12513519207"),
            to: new Twilio.Types.PhoneNumber("+1"+client.PhoneNumber.ToString())
        );
                return RedirectToAction("Index", "CarMeets", _context.CarMeets.ToList());
            }
            return View(car);
        }
        // GET: Cars/Details/5 
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.CarId == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }
        // GET: Cars/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CarId,Vin,Make,Model,Year,Mileage,Mods,AvgRating")] Car car)
        {
            if (ModelState.IsValid)
            {
                _context.Add(car);
                var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var client = _context.Clients.Where(c => c.IdentityUserId == userId).FirstOrDefault();
                var garage = _context.Garages.Where(g => g.ClientId == client.ClientId).FirstOrDefault();
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }
        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Cars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CarId,Vin,Make,Model,Year,Mileage,Mods,AvgRating")] Car car)
        {
            if (id != car.CarId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.CarId))
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
            return View(car);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.CarId == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.CarId == id);
        }
    }
}
