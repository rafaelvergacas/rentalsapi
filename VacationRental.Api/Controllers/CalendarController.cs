using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/calendar")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public CalendarController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        [HttpGet]
        public CalendarViewModel Get(int rentalId, DateTime start, int nights)
        {
            if (nights < 0)
                throw new ApplicationException("Nights must be positive");
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            var result = new CalendarViewModel 
            {
                RentalId = rentalId,
                Dates = new List<CalendarDateViewModel>() 
            };
            
            for (var i = 0; i < nights; i++)
            {
                var date = new CalendarDateViewModel
                {
                    Date = start.Date.AddDays(i),
                    Bookings = new List<CalendarBookingViewModel>(),
                    PreparationTimes = new List<PreparationTime>()
                };
                
                var count = 0;
                var currentDate = date.Date;

                foreach (var booking in _bookings.Values)
                {
                    var totalBookingNights = booking.Start.AddDays(booking.Nights);

                    if (booking.RentalId == rentalId)
                    {
                        count++;
                        if (booking.Start <= currentDate && totalBookingNights > currentDate) 
                        {
                            date.Bookings.Add(new CalendarBookingViewModel { Id = booking.Id, Unit = count });
                        }
                        else if (totalBookingNights <= currentDate && (currentDate - totalBookingNights).TotalDays < _rentals[rentalId].PreparationTimeInDays) 
                        {
                            date.PreparationTimes.Add(new PreparationTime { Unit = count });
                        }
                        if (count == _rentals[rentalId].Units) 
                        {
                            count = 0;
                        }
                    }
                }

                result.Dates.Add(date);
            }

            return result;
        }
    }
}
