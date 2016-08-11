/*
 *  Copyright © 2016 Russell Libby
 */

using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

#pragma warning disable 4014, 1998

namespace BandWeatherTask
{
    /// <summary>
    /// Background task for synchronizing data to the band.
    /// </summary>
    public sealed class BandWeatherTimerTask : IBackgroundTask
    {
        #region Public methods

        /// <summary>
        /// Async method to run when the background task is executed.
        /// </summary>
        /// <param name="taskInstance">The background task instance being run.</param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            Task.Run(async () =>
            {
                await SyncTask.Run(taskInstance);

            }).ContinueWith((t) =>
            {
                deferral.Complete();
            });
        }

        #endregion
    }
}
