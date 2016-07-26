/*
 *  Copyright © 2016 Russell Libby
 */

using System;
using System.Threading.Tasks;
using Microsoft.Band;

namespace BandWeatherTask
{
    /// <summary>
    /// Class for handling retry connections on the band client connect. This is important because if a 
    /// connection is already established by another application, our attempts to open a connection will fail.
    /// </summary>
    internal sealed class SmartConnect
    {
        /// <summary>
        /// Attempts a band connect with the ability to retry.
        /// </summary>
        /// <param name="bandInfo">The band info that represents the band to connect to.</param>
        /// <param name="retry">The number of retries to perform.</param>
        /// <param name="retryDelay">The time in ms. to delay between retries.</param>
        /// <returns>The band client on success, throws on failure.</returns>
        public static async Task<IBandClient> ConnectAsync(IBandInfo bandInfo, int retry, int retryDelay)
        {
            if (bandInfo == null) throw new ArgumentNullException("bandInfo");

            var  lastException = String.Empty;

            while (true)
            {
                IBandClient result = null;

                retry--;

                try
                {
                    result = await BandClientManager.Instance.ConnectAsync(bandInfo);
                }
                catch (BandIOException ex)
                {
                    lastException = ex.Message;
                }

                if (result != null) return result;
                if (retry <= 0) break;

                await Task.Delay(retryDelay);
            }

            throw new Exception(lastException);
        }
    }
}
