/*
 *  Copyright © 2016 Russell Libby
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;

namespace BandWeatherCommon
{ 
    /// <summary>
    /// Poco class for handling day data.
    /// </summary>
    public sealed class DayData
    {
        #region Public properties

        /// <summary>
        /// The short day name.
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// The weather conditions for the day.
        /// </summary>
        public string Weather { get; set; }

        /// <summary>
        /// The expected high temp for the day.
        /// </summary>
        public string High { get; set; }

        /// <summary>
        /// The expected low temp for the day.
        /// </summary>
        public string Low { get; set; }

        #endregion
    }

    /// <summary>
    /// Poco class for returning the 5 day forecast.
    /// </summary>
    public sealed class ForecastData
    {
        #region Private fields

        private List<DayData> _days = new List<DayData>();

        #endregion

        #region Public properties

        /// <summary>
        /// The name of the city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The current temperature.
        /// </summary>
        public double Temp { get; set; }

        /// <summary>
        /// The current conditions.
        /// </summary>
        public string Weather { get; set; }

        /// <summary>
        /// The next 3 days of weather data.
        /// </summary>
        public IList<DayData> Days
        {
            get { return _days; }
        }

        #endregion
    }

    /// <summary>
    /// Utility class for getting the 10 day forecast from wunderground.
    /// </summary>
    public static class Forecast
    {
        #region Private methods

        /// <summary>
        /// Gets the current location for the device.
        /// </summary>
        /// <returns>The geopoint for the current location.</returns>
        private static Geopoint GetLocation()
        {
            Geopoint result = null;

            var locater = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.Default,
                DesiredAccuracyInMeters = 5000,
                ReportInterval = 1000
            };

            if (locater.LocationStatus == PositionStatus.Disabled) return null;

            using (var waiter = new ManualResetEvent(false))
            {
                TypedEventHandler<Geolocator, PositionChangedEventArgs> handler = (sender, args) =>
                {
                    result = args.Position.Coordinate.Point;

                    waiter.Set();
                };

                locater.PositionChanged += handler;

                try
                {
                    waiter.WaitOne(TimeSpan.FromSeconds(10));
                }
                finally
                {
                    locater.PositionChanged -= handler;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the forecast for the current location.
        /// </summary>
        /// <returns>The poco object as the result of the json payload.</returns>
        private static async Task<ForecastData> GetForecastPrivate()
        {
            var point = GetLocation();

            if (point == null) throw new Exception("Failed to obtain the device location.");

            var location = await MapLocationFinder.FindLocationsAtAsync(point);

            if ((location.Status != MapLocationFinderStatus.Success) || (location.Locations.Count < 1)) throw new Exception("Unable to perform address lookup using the current coordinates.");

            using (var client = new HttpClient())
            {
                var url = string.Format(Common.ConditionsUri, Common.ApiKey, location.Locations[0].Address.PostCode);

                using (var response = await client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();

                    dynamic f = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

                    var result = new ForecastData();

                    result.City = f.current_observation.display_location.city;
                    result.Temp = f.current_observation.temp_f;
                    result.Weather = f.current_observation.weather;

                    foreach (var day in f.forecast.simpleforecast.forecastday)
                    {
                        result.Days.Add(new DayData { Day = day.date.weekday_short, High = day.high.fahrenheit, Low = day.low.fahrenheit, Weather = day.conditions });

                        if (result.Days.Count > 6) break;
                    }

                    return result;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the forecast for the specified location.
        /// </summary>
        /// <returns>The poco object as the result of the json payload.</returns>
        public static IAsyncOperation<ForecastData> GetForecast()
        {
            return GetForecastPrivate().AsAsyncOperation();
        }

        #endregion
    }
}
