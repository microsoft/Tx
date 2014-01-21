// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace OdataListener
{
    using System;
    using System.Collections.Generic;
    using OdataListener.Entities;

    public static class ListNavigationLinksProvider
    {
        #region Public methods

        /// <summary>
        /// Adds array of Links:
        // "Self": URL that represents current page
        // "Previous": URL that can be used to navigate to previous page
        // "Next": URL that can be used to naigate to next page
        /// </summary>
        /// <returns>
        /// List of links
        /// </returns>
        public static List<Link> GetLinks(int entitiesCount, int startFrom, int linesPerPage, Uri requestUrl, bool requestHasQueryParams)
        {
            var links = new List<Link>();

            links.Add(new Link()
            {
                Rel = "Self", 
                Href = requestUrl.AbsoluteUri
            });

            if (startFrom > 0)
            {
                var skip = startFrom >= linesPerPage ? startFrom - linesPerPage : 0;

                links.Add(new Link()
                {
                    Rel = "Previous",
                    Href = GetNavigationUrl(requestUrl.AbsoluteUri, skip, requestHasQueryParams)
                });
            }

            if (entitiesCount > startFrom + linesPerPage)
            {
                var skip = startFrom + linesPerPage;

                links.Add(new Link()
                {
                    Rel = "Next",
                    Href = GetNavigationUrl(requestUrl.AbsoluteUri, skip, requestHasQueryParams)
                });
            }

            return links;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets navigation URL that can be used in links.
        /// Finds old skip value by splitting orignal URL and replaces it with new one.
        /// If no skip value was set before, adds it to the end of URL.
        /// </summary>
        /// <param name="requestUrl">
        /// Current URL
        /// </param>
        /// <param name="skip">
        /// New skip value
        /// </param>
        /// <returns>
        /// Navigation url. <see cref="string"/>
        /// </returns>
        private static string GetNavigationUrl(string requestUrl, int skip, bool requestHasQueryParams)
        {
            var splited = requestUrl.Split(new[] { "$skip=" }, StringSplitOptions.None);
            if (splited.Length == 1)
            {
                if (requestHasQueryParams)
                    return requestUrl + "&$skip=" + skip;
                else
                    return requestUrl + "?$skip=" + skip;
            }

            var oldSkipValue = splited[1].Split('&')[0];
            return requestUrl.Replace("$skip=" + oldSkipValue, "$skip=" + skip);
        }

        #endregion
    }
}
