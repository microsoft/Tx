// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace OdataListener
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Net;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using OdataListener.Constants;
    using OdataListener.Entities;
    using Tx.Windows;

    class Program
    {
        static readonly HttpListener Listener = new HttpListener();
        static HttpListenerRequest _request;
        static HttpListenerResponse _response;
        static Playback _playback;
        static IObservable<Parse> _all;
        static RequestResult<List<Parse>> _result;

        private static void Main()
        {

            Listener.Prefixes.Add(ConfigurationConstants.ServiceRootUri);
            Listener.Start();
            Console.WriteLine("Listening ...");

            while (true)
            {
                HttpListenerContext context = Listener.GetContext();
                _request = context.Request;
                _response = context.Response;

                try
                {
                    // According to OData specification(http://www.odata.org/documentation/odata-v2-documentation/uri-conventions/)
                    // URI should look like: "service root URI + resource path + query options"
                    // Now it supports only URLs that look like: "http://localhost:1313/HttpServer/? query params..." 
                    // and works with data from HTTP_Server.etl file
                    // In future it should return different traces according to resource path specified in request URL
                    // For example “http://localhost:1313/LocalTrace/? query params” will return data from local trace
                    var resourcePath = _request.Url.AbsolutePath.Trim('/');
                    if (resourcePath.Equals(ConfigurationConstants.HttpServerResourcePath, StringComparison.OrdinalIgnoreCase))
                    {
                        SetupContext();
                    }
                    else
                    {
                        WriteErrorResponse(_response, HttpStatusCode.NotFound, "Endpoint not found");
                        continue;
                    }

                    QueryData();

                    WriteResultToResponse(_response);
                    _playback.Dispose();
                }
                catch (Exception ex)
                {
                    WriteErrorResponse(_response, HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        #region Private methods

        private static void SetupContext()
        {
            _playback = new Playback();
            _playback.AddEtlFiles(ConfigurationConstants.HttpServerTrace);
            _all = _playback.GetObservable<Parse>();
            _result = new RequestResult<List<Parse>> { Data = new List<Parse>() };
        }

        /// <summary>
        /// Queries data from trace file, generates links and adds it to _result
        /// </summary>
        private static void QueryData()
        {
            if (_request.QueryString[ODataConstants.QueryOptions.Filter] != null)
            {
                FilterData();
            }

            int linesPerPage;
            int startFrom;
            SetupNavigationOptions(out linesPerPage, out startFrom);

            GetEntitiesAndLinks(linesPerPage, startFrom);

            // Finally we may need to order our data
            // OrderBy is not a part of Rx, so we'll just use expression from Linq.Dynamic on resulting array
            if (_request.QueryString[ODataConstants.QueryOptions.OrderBy] != null)
            {
                var orderBy = _request.QueryString.Get(ODataConstants.QueryOptions.OrderBy);
                _result.Data = _result.Data.AsQueryable().OrderBy(orderBy).ToList();
            }
        }

        /// <summary>
        /// Filters data using OData syntax $filter=Name eq Robert
        /// Supporter operators: eq, ne
        /// </summary>
        private static void FilterData()
        {
            var filter = _request.QueryString.Get(ODataConstants.QueryOptions.Filter);
            // Parses string of type "Name eq Robert" into field name, operator and value
            var spliterFilter = filter.Split(' ');
            if (spliterFilter.Length < 3)
            {
                return;
            }

            var fieldName = spliterFilter[0];
            var operationName = spliterFilter[1];
            var operationValue = spliterFilter[2];

            // Sets up operator for our filter, it will be used in Rx Where expression later
            Func<object, object, bool> operation;
            switch (operationName.ToLower())
            {
                case  ODataConstants.FilterOperators.Equal:
                    operation = (propertyValue, filterValue) => propertyValue.Equals(filterValue);
                    break;

                case  ODataConstants.FilterOperators.NoEqual:
                    operation = (propertyValue, filterValue) => !propertyValue.Equals(filterValue);
                    break;

                default:
                    throw new Exception("Unsopported filter operator: " + operationName);
                    break;
            }

            _all = _all.Where(x =>
            {
                var filedValue = x.GetType().GetProperty(fieldName).GetValue(x);
                return operation(filedValue, operationValue);
            });
        }

        /// <summary>
        /// Gets and returns $skip and $top query string operators
        /// </summary>
        private static void SetupNavigationOptions(out int linesPerPage, out int startFrom)
        {
            linesPerPage = ConfigurationConstants.DefaultLinesPerPageValue;
            if (_request.QueryString[ODataConstants.QueryOptions.Top] != null)
            {
                int.TryParse(_request.QueryString.Get(ODataConstants.QueryOptions.Top), out linesPerPage);
            }

            startFrom = 0;
            if (_request.QueryString[ODataConstants.QueryOptions.Skip] != null)
            {
                int.TryParse(_request.QueryString.Get(ODataConstants.QueryOptions.Skip), out startFrom);
            }
        }

        private static void GetEntitiesAndLinks(int linesPerPage, int startFrom)
        {
            // Adds data to resulting array
            _all.Skip(startFrom).Take(linesPerPage).Subscribe(e => _result.Data.Add(e));

            _all.Count().Subscribe(count =>
            {
                _result.Count = count;
                _result.Links = ListNavigationLinksProvider.GetLinks(count, startFrom, linesPerPage, _request.Url, _request.QueryString.Count > 0);
            });

            _playback.Run();
        }

        /// <summary>
        /// Writes result to response according to format that user specified in query string($format param)
        /// Default one is Xml.
        /// </summary>
        private static void WriteResultToResponse(HttpListenerResponse response)
        {
            var responseFormat = string.Empty;
            if (_request.QueryString[ODataConstants.QueryOptions.Format] != null)
            {
                responseFormat = _request.QueryString.Get(ODataConstants.QueryOptions.Format);
            }

            switch (responseFormat)
            {
                case ODataConstants.ResponseFormat.Json:
                    WriteJsonResultToResponse(response);
                    break;

                case ODataConstants.ResponseFormat.Xml:
                    WriteXmlResultToResponse(response);
                    break;

                default:
                    WriteXmlResultToResponse(response);
                    break;
            }

            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
        }

        private static void WriteJsonResultToResponse(HttpListenerResponse response)
        {
            string jsonResult = JsonConvert.SerializeObject(_result);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonResult);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private static void WriteXmlResultToResponse(HttpListenerResponse response)
        {
            var x = new XmlSerializer(_result.GetType());
            x.Serialize(response.OutputStream, _result);
        }

        private static void WriteErrorResponse(HttpListenerResponse response, HttpStatusCode code, string message)
        {
            response.StatusCode = (int)code;
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        #endregion
    }
}
