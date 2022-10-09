using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;
using System.Linq;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public RentalsController(IDictionary<int, RentalViewModel> rentals, IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        [HttpGet]
        [Route("{rentalId:int}")]
        public RentalViewModel Get(int rentalId)
        {
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            return _rentals[rentalId];
        }

        [HttpPost]
        public ResourceIdViewModel Post(RentalBindingModel model)
        {
            var key = new ResourceIdViewModel { Id = _rentals.Keys.Count + 1 };

            _rentals.Add(key.Id, new RentalViewModel
            {
                Id = key.Id,
                Units = model.Units,
                PreparationTimeInDays = model.PreparationTimeInDays
            });

            return key;
        }

        [HttpPut]
        [Route("{rentalId:int}")]
        public RentalViewModel Update(int rentalId, RentalBindingModel model)
        {
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            var rentalBookings = _bookings.Values.Where(x => x.RentalId == rentalId).ToList();
            if (rentalBookings.Count > model.Units)
                throw new ApplicationException("Not Available");

            for (int i = 0; i < rentalBookings.Count; i++) 
            {
                var totalBookingNights = rentalBookings[i].Start.AddDays(rentalBookings[i].Nights);
                var nextStart = rentalBookings[i + 1].Start;
                
                if ((nextStart - totalBookingNights).TotalDays < model.PreparationTimeInDays)
                    throw new ApplicationException("Preparation Time Overlap");
            }

            var rental = _rentals[rentalId];
            rental.Units = model.Units;
            rental.PreparationTimeInDays = model.PreparationTimeInDays;

            return _rentals[rentalId];
        }
    }
}
