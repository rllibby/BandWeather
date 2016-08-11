/*
 *  Copyright © 2016 Russell Libby
 */

using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Microsoft.Band;
using Microsoft.Band.Tiles.Pages;
using BandWeatherCommon;
using Windows.Foundation;
using Windows.Devices.Geolocation;
using System.Threading;
using System.Collections.Generic;
using Windows.Services.Maps;

namespace BandWeatherTask
{
    /// <summary>
    /// Common sync task routine to be called from all the different background tasks.
    /// </summary>
    internal static class SyncTask
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
                    result  = args.Position.Coordinate.Point;

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

        #endregion

        #region Public methods

        /// <summary>
        /// Async method to run when the background task is executed.
        /// </summary>
        /// <param name="taskInstance">The background task instance being run.</param>
        public static async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                var isCancelled = false;

                BackgroundTaskCanceledEventHandler cancelled = (sender, reason) => { isCancelled = true; };

                try
                {
                    var pairedBands = await BandClientManager.Instance.GetBandsAsync(true);

                    taskInstance.Progress = 10;
                    if ((pairedBands.Length < 1) || isCancelled) return;

                    using (var bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                    {
                        taskInstance.Progress = 20;
                        if (isCancelled) return;

                        var tiles = await bandClient.TileManager.GetTilesAsync();

                        taskInstance.Progress = 30;
                        if (!tiles.Any() || isCancelled) return;

                        var response = await Forecast.GetForecast();

                        taskInstance.Progress = 50;
                        if (isCancelled || (response == null)) return;

                        var pages = BandUpdate.GeneratePageData(response);
                        taskInstance.Progress = 60;
                        if (isCancelled) return;

                        await bandClient.TileManager.RemovePagesAsync(new Guid(Common.TileGuid));
                        taskInstance.Progress = 80;

                        await bandClient.TileManager.SetPagesAsync(new Guid(Common.TileGuid), pages as IEnumerable<PageData>);
                        taskInstance.Progress = 100;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    taskInstance.Canceled -= cancelled;
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        #endregion
    }
}
