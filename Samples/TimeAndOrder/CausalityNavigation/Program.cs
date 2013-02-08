// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Tx.Windows;
using Tx.Windows.BounceMessages;

namespace CausalityNavigation
{
    class Program
    {
        const string LocalTrace = @"LocalTrace.etl";
        const int LinesPerPage = 30;

        static HttpListener _listener = new HttpListener();
        static HttpListenerRequest _request;
        static StringBuilder _sb;
        static string _processFilter;
        static Playback _playback;
        static IObservable<TracedEvent> _all;

        static void Main(string[] args)
        {
            _listener.Prefixes.Add("http://" + Environment.MachineName + ":9000/");
            _listener.Start();
            Console.WriteLine("Listening ...");

            while (true)
            {
                HttpListenerContext context = _listener.GetContext();
                _request = context.Request;

                HttpListenerResponse response = context.Response;

                _sb = new StringBuilder("<HTML>\n<HEAD>\n<TITLE>");
                _sb.Append("Trace 2 Html");
                _sb.Append("</TITLE>\n</HEAD>\n<BODY>\n");
                _sb.Append("<pre style=\"font-family:Consolas; font-size: 10pt; width: 5000px;\">");

                _playback = new Playback();
                _playback.AddEtlFiles(LocalTrace);
                _all = _playback.GetObservable<TracedEvent>();

                if (_request.QueryString["process"] != null)
                {
                    _processFilter = _request.QueryString["process"];
                    _all = _all.Where(e => e.Message.StartsWith(_processFilter));
                }
                else
                {
                    _processFilter = null;
                }

                if (_request.QueryString["after"] != null)
                {
                    var after = int.Parse(_request.QueryString.Get("after"));
                    AllHistory(after);
                }
                else if (_request.QueryString["afterReceive"] != null)
                {
                    string messageId = _request.QueryString.Get("afterReceive");
                    AfterReceive(messageId);
                }
                else if (_request.QueryString["beforeSend"] != null)
                {
                    string messageId = _request.QueryString.Get("beforeSend");
                    BeforeSend(messageId);
                }
                else
                {
                    AllHistory(0);
                }
                _sb.Append("</BODY></HTML>");
                byte[] buffer = Encoding.UTF8.GetBytes(_sb.ToString());
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
                _playback.Dispose();
            }
        }

        static void AllHistory(int start)
        {
            if (start > 0)
            {
                _sb.AppendFormat("<A HREF=?after={0}>Previous</A><BR/>", start - LinesPerPage);
            }

            _all.Skip(start).Take(LinesPerPage).Subscribe(e => _sb.AppendLine(Format(e.Message)));

            _playback.Run();
            _sb.AppendFormat("<BR/><A HREF=?after={0}>Next</A>", start + LinesPerPage);
        }

        static void AfterReceive(string messageId)
        {
            string pattern = "received message " + messageId;
            var received = _all.Where(e => e.Message.Contains(pattern));
            var after = received.SelectMany(_all).Take(LinesPerPage);
            var output = received.Merge(after);

            output.Subscribe(e => _sb.AppendLine(Format(e.Message)));

            _playback.Run();
        }

        static void BeforeSend(string messageId)
        {
            string pattern = "sending message  " + messageId;
            var send = _all.Where(e => e.Message.Contains(pattern));
            var before = _all.TakeUntil(send);
            var output = before.Merge(send);

            output.TakeLast(LinesPerPage).Subscribe(e => _sb.AppendLine(Format(e.Message)));

            _playback.Run();
        }

        static string Format(string message)
        {
            int index = message.IndexOf("sending message");
            if (index > 0)
            {
                string[] tokens = message.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                string s = string.Format("<font color=\"Red\"><b>{0} {1} sending message <A HREF=\"http://{2}:9000/?afterReceive={3}&process={4}\">{3}</A> to {4} at {2}</b></font>",
                    tokens[0],
                    tokens[1],
                    tokens[8],
                    tokens[4],
                    tokens[6]);

                return s;
            }

            index = message.IndexOf("received message");
            if (index > 0)
            {
                string[] tokens = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string s = string.Format("<font color=\"Green\"><b>{0} {1} received message <A HREF=\"http://{2}:9000/?beforeSend={3}&process={4}\">{3}</A> from {4} at {2}</b></font>",
                    tokens[0],
                    tokens[1],
                    tokens[8],
                    tokens[4],
                    tokens[6]);

                return s;
            }

            return message;
        }
    }
}
