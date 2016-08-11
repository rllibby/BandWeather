/*
 *  Copyright © 2016 Russell Libby
 */

using Microsoft.Band.Tiles.Pages;
using System;
using System.Collections.Generic;

namespace BandWeatherCommon
{
    /// <summary>
    /// Helper class for generating band data pages.
    /// </summary>
    public static class BandUpdate
    {
        #region Public methods

        /// <summary>
        /// Generates new tile page data based on the current forecast data.
        /// </summary>
        /// <param name="data">The forecast data.</param>
        /// <returns>The collection of page data.</returns>
        public static IEnumerable<object> GeneratePageData(ForecastData data)
        {
            if (data == null) throw new ArgumentNullException("data");

            var result = new List<PageData>();
            var title = new TextBlockData(Common.TitleId, "Now");
            var subtitle = new TextBlockData(Common.SecondaryTitleId, data.Weather);
            var spacer = new TextBlockData(Common.SpacerId, "|");
            var icon = new IconData(Common.IconId, (2));
            var content = new TextBlockData(Common.ContentId, string.Format("{0}º", data.Temp.ToString()));

            result.Add(new PageData(Guid.NewGuid(), 0, title, spacer, subtitle, icon, content));

            foreach (var day in data.Days)
            {
                title = new TextBlockData(Common.TitleId, day.Day);
                subtitle = new TextBlockData(Common.SecondaryTitleId, day.Weather);
                spacer = new TextBlockData(Common.SpacerId, "|");
                content = new TextBlockData(Common.ContentId, string.Format("{0}º / {1}º", day.High, day.Low));

                result.Add(new PageData(Guid.NewGuid(), 1, title, spacer, subtitle, content));
            }

            var description = string.Format("Updated\n{0}\n{1}\n", DateTime.Now.ToString(Common.DateFormat), data.City);
            var updated = new WrappedTextBlockData(Common.UpdateId, description);

            result.Add(new PageData(Guid.NewGuid(), 2, updated));
            result.Reverse();

            return result;
        }

        #endregion
    }
}
