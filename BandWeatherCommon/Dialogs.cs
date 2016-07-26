/*
 *  Copyright © 2016 Russell Libby
 */

namespace BandWeatherCommon
{
    /// <summary>
    /// Dialog related constants shared between the UI and tasks runtime component.
    /// </summary>
    public static class Dialogs
    {
        #region Public properties

        /// <summary>
        /// Ok
        /// </summary>
        public static string Ok
        {
            get { return "ok"; }
        }

        /// <summary>
        /// The message content to display when a band is not paired.
        /// </summary>
        public static string NotPaired
        {
            get 
            {
                return @"This application requires a Microsoft Band paired to your device. Also make sure that you have the latest firmware installed on your Band.";
            }
        }

        /// <summary>
        /// The message content to display when a tile has been removed (by external application).
        /// </summary>
        public static string TileRemoved
        {
            get { return @"The band tile for this application has been removed."; }
        }

        /// <summary>
        /// The message content to display when buoy data has been sucessfully synced.
        /// </summary>
        public static string Synced
        {
            get { return @"Sucessfully synced data with your Microsoft Band."; }
        }

        /// <summary>
        /// The message content to display when we fail to obtain condition data.
        /// </summary>
        public static string NoData
        {
            get { return @"Failed to obtain weather condition data."; }
        }

        /// <summary>
        /// The message content when location services is off.
        /// </summary>
        public static string NoLocation
        {
            get { return "Location is disabled on your device. To enable location, go to Settings and select location."; }
        }

        /// <summary>
        /// The message content when location services times out.
        /// </summary>
        public static string Timeout
        {
            get { return "Location service timed out."; }
        }

        /// <summary>
        /// Tile page content when band is updated from the background task.
        /// </summary>
        public static string TaskUpdate
        {
            get { return "Updated from the Band Weather background task."; }
        }

        /// <summary>
        /// Tile page content when band is updated from the user interface.
        /// </summary>
        public static string UiUpdate
        {
            get { return "Updated from the Band Weather application."; }
        }
        
        #endregion
    }
}
