/*
 *  Copyright © 2016 Russell Libby
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

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
        /// The next 5 days of weather data.
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
        /// Gets the forecast for the specified location.
        /// </summary>
        /// <param name="point">The geo coordinate to get the forecast for.</param>
        /// <returns>The poco object as the result of the json payload.</returns>
        private static async Task<ForecastData> GetForecastPrivate(Geopoint point)
        {
            if (point == null) throw new ArgumentNullException("point");

            using (var client = new HttpClient())
            {
                var url = string.Format(Common.ConditionsUri, Common.ApiKey, point.Position.Latitude, point.Position.Longitude);

                using (var response = await client.GetAsync(url))
                {
                    dynamic dyn = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

                    var result = new ForecastData();

                    result.City = dyn.current_observation.display_location.city;
                    result.Temp = dyn.current_observation.temp_f;
                    result.Weather = dyn.current_observation.weather;

                    for (var i = 0; i < 5; i++)
                    {
                        dynamic day = dyn.forecast.simpleforecast.forecastday[i];

                        result.Days.Add(new DayData { Day = day.date.weekday_short, High = day.high.fahrenheit, Low = day.low.fahrenheit, Weather = day.conditions });
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
        /// <param name="point">The geo coordinate to get the forecast for.</param>
        /// <returns>The poco object as the result of the json payload.</returns>
        public static IAsyncOperation<ForecastData> GetForecast(Geopoint point)
        {
            return GetForecastPrivate(point).AsAsyncOperation();
        }

        #endregion
    }
}
