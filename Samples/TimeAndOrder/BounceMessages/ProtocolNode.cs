// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace BombActor
{
    static class ProtocolNode
    {
        static HttpListener _listener;
        static Timer _timer;
        static int _processId;
        static Uri[] _endpoints;
        static Action<Guid, int> _received;
        static TimerCallback _onTimer;
        static DateTime _startTime;
        static int _localCounter;

        public static void Open(Action<Guid, int> received, TimerCallback onTimer)
        {
            _received = received;
            _onTimer = onTimer;
            int index = 0;
            List<Uri> actorUris = new List<Uri>();

            using (TextReader reader = File.OpenText("Endpoints.txt"))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    actorUris.Add(new Uri(line.ToLower().Replace("localhost", Environment.MachineName)));
                }
            }
            _endpoints = actorUris.ToArray();

            for (int i = 0; i < _endpoints.Length; i++)
            {
                if (TryListening(i))
                    Console.WriteLine("* {0} {1}", index, _endpoints[i]);
                else
                    Console.WriteLine("  {0} {1}", index, _endpoints[i]);
            };
            _listener.BeginGetContext(OnRequest, null);
        }

        public static int ProcessId
        {
            get { return _processId; }
        }

        public static Uri[] Endpoints
        {
            get { return _endpoints; }
        }

        public static int LocalCounter
        {
            get { return _localCounter++; }
        }

        public static void StartAll()
        {
            DateTime startTime = DateTime.Now;
            for (int i = 0; i < _endpoints.Length; i++)
            {
                var req = HttpWebRequest.Create(_endpoints[i] + "?start=" + startTime);
                req.Headers.Add("from", _processId.ToString());
                req.Headers.Add("messageId", Guid.NewGuid().ToString());
                var resp = req.GetResponse();
            }
        }

        public static void StopAll()
        {
            _timer.Dispose();
            DateTime startTime = DateTime.Now;
            for (int i = 0; i < _endpoints.Length; i++)
            {
                if (i == _processId)
                    continue;

                var req = HttpWebRequest.Create(_endpoints[i] + "?stop=" + startTime);
                req.Headers.Add("from", _processId.ToString());
                req.Headers.Add("messageId", Guid.NewGuid().ToString());
                try
                {
                    var resp = req.GetResponse();
                }
                catch (WebException ex)
                {
                    Trace.WriteLine("Exception: " + ex.Message);
                }
            }
        }

        public static void Send(Guid messageId, int to)
        {
            try
            {
                Uri uri = Endpoints[to];

                var req = HttpWebRequest.Create(_endpoints[to] + "?timebomb=" + DateTime.Now.ToString());
                req.Headers.Add("from", _processId.ToString());
                req.Headers.Add("messageId", messageId.ToString());
                var resp = req.GetResponse();
            }
            catch (WebException ex)
            {
                Trace.WriteLine("Exception: " + ex.Message);
            }
        }

        static void OnRequest(object state)
        {
            HttpListenerContext context = _listener.EndGetContext((IAsyncResult)state);
            HttpListenerRequest request = context.Request;

            HttpListenerResponse response = context.Response;
            string messageId = request.Headers["messageId"];
            int from = int.Parse(request.Headers["from"]);

            Uri uri = Endpoints[from];

            if (request.QueryString["start"] != null)
            {
                _startTime = DateTime.Parse(request.QueryString.Get("start"));
                _timer = new Timer(_onTimer, null, 1000, 1000);
            }

            if (request.QueryString["stop"] != null)
            {
                response.OutputStream.Close();
                Environment.Exit(0);
            }

            if (request.QueryString["timebomb"] != null)
            {
                var d = DateTime.Parse(request.QueryString.Get("timebomb"));
                _received(new Guid(messageId), from);
            }
            response.OutputStream.Close();
            _listener.BeginGetContext(OnRequest, null);
        }

        static bool TryListening(int index)
        {
            if (_listener != null)
                return false;

            if (_endpoints[index].Host.ToLowerInvariant() != Environment.MachineName.ToLowerInvariant())
                return false;

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(_endpoints[index].ToString());
            try
            {
                listener.Start();
                _listener = listener;
                _processId = index;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
