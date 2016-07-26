/*
 *  Copyright © 2016 Russell Libby
 */

namespace BandWeatherCommon
{
    /// <summary>
    /// Common constants shared between the UI and tasks runtime component.
    /// </summary>
    public static class Common
    {
        #region Public properties

        /// <summary>
        /// The application title.
        /// </summary>
        public static string Title
        {
            get { return @"Band Weather"; }
        }

        /// <summary>
        /// The application version.
        /// </summary>
        public static string Version
        {
            get { return "1.0.0.0"; }
        }

        /// <summary>
        /// The authors email address.
        /// </summary>
        public static string Email
        {
            get { return "rllibby@gmail.com"; }
        }

        /// <summary>
        /// The last update title.
        /// </summary>
        public static string LastUpdate
        {
            get { return "Last Update"; }
        }
        /// <summary>
        /// The date format used by application.
        /// </summary>
        public static string DateFormat
        {
            get { return "MM/dd h:mm tt"; }
        }

        /// <summary>
        /// The date format used by the band page title.
        /// </summary>
        public static string DateTitleFormat
        {
            get { return "MMM dd"; }
        }
        
        /// <summary>
        /// Guid id for the band tile.
        /// </summary>
        public static string TileGuid
        {
            get { return "B44411D6-4FF8-4B29-AB81-10E73106B9E3"; }
        }

        /// <summary>
        /// The background timer task name.
        /// </summary>
        public static string TimerTaskName
        {
            get { return "BandWeatherTimerTask"; }
        }

        /// <summary>
        /// The background system task name.
        /// </summary>
        public static string SystemTaskName
        {
            get { return "BandWeatherSystemTask"; }
        }

        /// <summary>
        /// Setting key for last sync data.
        /// </summary>
        public static string LastSyncKey
        {
            get { return "lastsync"; }
        }

        /// <summary>
        /// Setting key for application data.
        /// </summary>
        public static string SettingKey
        {
            get { return "lastlocation"; }
        }

        /// <summary>
        /// The api key for wunderground.
        /// </summary>
        public static string ApiKey
        {
            get { return "597a1a4875b809c4"; }
        }

        /// <summary>
        /// Element id for title.
        /// </summary>
        public static short TitleId
        {
            get { return 1; }
        }

        /// <summary>
        /// Element id for spacer.
        /// </summary>
        public static short SpacerId
        {
            get { return 2; }
        }

        /// <summary>
        /// Element id for ssecondary title.
        /// </summary>
        public static short SecondaryTitleId
        {
            get { return 3; }
        }

        /// <summary>
        /// Element id for icon.
        /// </summary>
        public static short IconId
        {
            get { return 4; }
        }

        /// <summary>
        /// Element id for content.
        /// </summary>
        public static short ContentId
        {
            get { return 5; }
        }

        /// <summary>
        /// Element id for updated content.
        /// </summary>
        public static short UpdateId
        {
            get { return 6; }
        }

        /// <summary>
        /// Returns the link to the wunderground api for conditions.
        /// </summary>
        public static string ConditionsUri
        {
            get
            {
                return @"http://api.wunderground.com/api/{0}/conditions/hourly/forecast10day/q/{1:0.00},{2:0.00}.json";
            }    
        }

        #endregion
    }
}
