/*
 *  Copyright © 2016 Russell Libby
 */

using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Microsoft.Band;
using Microsoft.Band.Tiles.Pages;
using BandWeatherCommon;
using Windows.Foundation;
using Windows.Devices.Geolocation;
using System.Threading;
using System.Collections.Generic;

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
                    result = args.Position.Coordinate.Point;

                    waiter.Set();
                };

                locater.PositionChanged += handler;

                try
                {
                    waiter.WaitOne(TimeSpan.FromSeconds(30));
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
                var localSettings = ApplicationData.Current.LocalSettings;
                var isCancelled = false;

                BackgroundTaskCanceledEventHandler cancelled = (sender, reason) => { isCancelled = true; };

                try
                {
                    if (taskInstance.TriggerDetails is DeviceConnectionChangeTriggerDetails)
                    {
                        var deviceDetails = taskInstance.TriggerDetails as DeviceConnectionChangeTriggerDetails;
                        var device = await BluetoothDevice.FromIdAsync(deviceDetails.DeviceId);

                        if (device.ConnectionStatus != BluetoothConnectionStatus.Connected) return;
                    }

                    taskInstance.Progress = 1;

                    var point = GetLocation();

                    taskInstance.Progress = 10;
                    if (point == null) throw new Exception("Timed out while attempting to determine location.");
                    if (isCancelled) return;

                    taskInstance.Progress = 20;
                    if (isCancelled) return;

                    var response = await Forecast.GetForecast(point);

                    taskInstance.Progress = 30;
                    if (isCancelled) return;

                    var pairedBands = await BandClientManager.Instance.GetBandsAsync(true);

                    taskInstance.Progress = 40;
                    if ((pairedBands.Length < 1) || isCancelled) return;

                    using (var bandClient = await SmartConnect.ConnectAsync(pairedBands[0], 5, 2000))
                    {
                        taskInstance.Progress = 50;
                        if (isCancelled) return;

                        var tiles = await bandClient.TileManager.GetTilesAsync();

                        taskInstance.Progress = 60;
                        if (!tiles.Any() || isCancelled) return;

                        var pages = new List<PageData>();
                        var title = new TextBlockData(Common.TitleId, "Now");
                        var subtitle = new TextBlockData(Common.SecondaryTitleId, response.Weather);
                        var spacer = new TextBlockData(Common.SpacerId, "|");
                        var icon = new IconData(Common.IconId, (ushort)(2));
                        var content = new TextBlockData(Common.ContentId, string.Format("{0}º", response.Temp.ToString()));

                        pages.Add(new PageData(Guid.NewGuid(), 0, title, spacer, subtitle, icon, content));

                        foreach (var day in response.Days)
                        {
                            title = new TextBlockData(Common.TitleId, day.Day);
                            subtitle = new TextBlockData(Common.SecondaryTitleId, day.Weather);
                            spacer = new TextBlockData(Common.SpacerId, "|");
                            content = new TextBlockData(Common.ContentId, string.Format("{0}º/{1}º", day.High, day.Low));

                            pages.Add(new PageData(Guid.NewGuid(), 1, title, spacer, subtitle, content));
                        }

                        var description = string.Format("Updated\n{0}\n{1}\n", DateTime.Now.ToString(Common.DateFormat), response.City);
                        var updated = new WrappedTextBlockData(Common.UpdateId, description);

                        pages.Add(new PageData(Guid.NewGuid(), 2, updated));
                        pages.Reverse();

                        await bandClient.TileManager.RemovePagesAsync(new Guid(Common.TileGuid));
                        taskInstance.Progress = 80;

                        await bandClient.TileManager.SetPagesAsync(new Guid(Common.TileGuid), pages);
                        taskInstance.Progress = 90;

                        localSettings.Values[Common.LastSyncKey] = DateTime.Now.ToString(Common.DateFormat);

                        taskInstance.Progress = 100;
                    }
                }
                catch (Exception ex)
                {
                    localSettings.Values[Common.LastSyncKey] = String.Format("Failed at {0}\r\n{1}", DateTime.Now.ToString(Common.DateFormat), ex.Message);
                }
                finally
                {
                    taskInstance.Canceled -= cancelled;
                    if (isCancelled) localSettings.Values[Common.LastSyncKey] = String.Format("Cancelled at {0}", DateTime.Now.ToString(Common.DateFormat));
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
