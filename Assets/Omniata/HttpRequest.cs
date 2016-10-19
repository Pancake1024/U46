
#if !(UNITY_WEBPLAYER || UNITY_WEBGL) && !UNITY_WP8


/**
MIT License

Copyright (C) 2012 David Clayton <davedx@gmail.com> www.dave78.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.
*/
/**
 * HttpRequest is based on https://github.com/davedx/simple-c-sharp-http-client with MIT License (above). 
 * The reason not to use Unity's internal WWW class is that in a failured HTTP request case, it doesn't distinguish 
 * between networking errors and HTTP response code indicating an error (such as 502). We want to resend 
 * in the case of certain response codes. This is just not possible with WWW.
 */
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace omniata
{
    public class HttpRequest
    {   
        private const string TAG = "HttpRequest";

        private OmniataComponent OmniataComponent { get; set; }

        public bool isBusy
        {
            get { return !Succeeded && !Failed; }
        }
        
        public bool Succeeded { get; private set; }

        public bool Failed { get; private set; }
    
        public Dictionary<string, string> responseHeaders { get; private set; }
    
        // Raw response headers
        private string headersRaw;

        // Status-line
        public string Version { get; set; }
   
        public int ResponseCode { get; set; }

        public string Message { get; set; }

        // Filled in the success-case
        public string ResponseBody { get; private set; }
        // Filled in the Failed-case
        public string ErrorMessage { get; private set; }

        // Time waited so far for the response
        private double Waited { get; set; }
        // The timeout to wait for the response
        private int Timeout { get; set; }

        private string UrlString { get; set; }

        private Uri Uri { get; set; }

        private NetworkResponseHandler NetworkResponseHandler { get; set; }

        private DateTime Before { get; set; }

        public HttpRequest(OmniataComponent omniataComponent, string urlString, int timeout, NetworkResponseHandler networkResponseHandler)
        {
            OmniataComponent = omniataComponent;
            Timeout = timeout;
            UrlString = urlString;
            Uri = new Uri(UrlString);
            NetworkResponseHandler = networkResponseHandler;
            ResponseCode = 0;
            Waited = 0;
            Succeeded = false;
            Failed = false;
        }

        public void Get()
        {
            OmniataComponent.StartCoroutine(DoGet());
        }

        private TcpClient tcp;
        private NetworkStream stream;
        private ManualResetEvent evt = new ManualResetEvent(false);

        private void TcpWriteThread(object o)
        {
            string request = "GET " + Uri.PathAndQuery + " HTTP/1.1\r\n";
            request += "Host: " + Uri.Host + "\r\n";
            request += "\r\n\r\n";

            Log.Debug(TAG, request);
            byte[] requestBytes = System.Text.Encoding.UTF8.GetBytes(request);
            
            string host = Uri.Host;
            int port = Uri.Port;

            try
            {
                tcp = new TcpClient(host, port);
                stream = tcp.GetStream();

                stream.Write(requestBytes, 0, requestBytes.Length);
                stream.Flush();

                evt.Set();
            }
            catch (Exception/* e*/)
            {
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }

                if (tcp != null)
                {
                    tcp.Close();
                    tcp = null;
                }

                evt.Set();
            }
        }

        private void TcpReadHeader(object o)
        {
            headersRaw = string.Empty;

            try
            {
                byte[] buff = new byte[1];
                while (!headersRaw.EndsWith("\r\n\r\n"))
                {
                    if (stream.DataAvailable)
                    {
                        int read = stream.Read(buff, 0, 1);
                        headersRaw += System.Text.Encoding.UTF8.GetString(buff, 0, read);
                    }

                    Thread.Sleep(0);
                }

                evt.Set();
            }
            catch (Exception/* e*/)
            {
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }

                if (tcp != null)
                {
                    tcp.Close();
                    tcp = null;
                }

                evt.Set();
            }
        }

        private void TcpReadBody(object o)
        {
            ResponseBody = string.Empty;

            int contentLength = int.Parse(responseHeaders["content-length"]),
                bytesRead     = 0;

            try
            {
                byte[] buff = new byte[1024];
                while (bytesRead < contentLength)
                {
                    if (stream.DataAvailable)
                    {
                        int read = stream.Read(buff, 0, buff.Length);
                        ResponseBody += System.Text.Encoding.UTF8.GetString(buff, 0, read);
                        bytesRead += read;
                    }

                    if (bytesRead < contentLength)
                        Thread.Sleep(0);
                }

                evt.Set();
            }
            catch (Exception/* e*/)
            {
                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }

                if (tcp != null)
                {
                    tcp.Close();
                    tcp = null;
                }

                evt.Set();
            }
        }

        IEnumerator DoGet()
        {
            // Log.Debug(TAG, "DoGet");
            Before = DateTime.Now;
            /*
            string request = "GET " + Uri.PathAndQuery + " HTTP/1.1\r\n";
            request += "Host: " + Uri.Host + "\r\n";
            request += "\r\n\r\n";
    
            Log.Debug(TAG, request);
            byte[] requestBytes = System.Text.Encoding.UTF8.GetBytes(request);

            string host = Uri.Host;
            int port = Uri.Port;
            // Log.Debug(TAG, "host: " + host + ", port: " + port);

            TcpClient tcp = new TcpClient(host, port);
            NetworkStream stream = tcp.GetStream();

            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Flush();
            */
/*
            Thread t = new Thread(TcpWriteThread);
            t.Start();
            while (!t.Join(0))
                yield return new WaitForSeconds(0.01f);*/
            evt.Reset();
            ThreadPool.QueueUserWorkItem(TcpWriteThread);
            while (!evt.WaitOne(0))
                yield return new WaitForSeconds(0.01f);
            evt.Reset();

            if (null == tcp)
            {
                ErrorMessage = "Socket error";
                Failed = true;
                Callbacks();
                yield break;
            }
/*
            t = new Thread(TcpReadHeader);
            t.Start();
            while (Waited < Timeout && !t.Join(0))
            {
                yield return new WaitForSeconds(0.01f);
                Waited += 0.01f;
            }*/
            ThreadPool.QueueUserWorkItem(TcpReadHeader);
            while (Waited < Timeout && !evt.WaitOne(0))
            {
                yield return new WaitForSeconds(0.01f);
                Waited += 0.01f;
            }
            evt.Reset();

            if (Waited >= Timeout || null == tcp)
            {
                ErrorMessage = null == tcp ? "Socket error" : "Network read timed out while reading headers";
                Failed = true;
                Callbacks();
                yield break;
            }

            responseHeaders = ParseHeaders(headersRaw);
            if (responseHeaders.ContainsKey("content-length"))
            {/*
                t = new Thread(TcpReadBody);
                t.Start();
                while (Waited < Timeout && !t.Join(0))
                {
                    yield return new WaitForSeconds(0.01f);
                    Waited += 0.01f;
                }*/
                ThreadPool.QueueUserWorkItem(TcpReadBody);
                while (Waited < Timeout && !evt.WaitOne(0))
                {
                    yield return new WaitForSeconds(0.01f);
                    Waited += 0.01f;
                }
                evt.Reset();

                if (Waited >= Timeout || null == tcp)
                {
                    ErrorMessage = null == tcp ? "Socket error" : "Network read timed out during body";
                    Failed = true;
                    Callbacks();
                    yield break;
                }
            }

            if (ResponseCode == 200)
            {
                Succeeded = true;
            }
            else
            {
                ErrorMessage = "Fail because of response code " + ResponseCode;
            }

            Callbacks();
/*
            headersRaw = "";
            ResponseBody = "";

            // Log.Debug (TAG, "Read headers, Waited: " + Waited);
            for (; Waited < Timeout && !(headersRaw.EndsWith("\r\n\r\n")); Waited += 0.01f)
            {
                if (stream.DataAvailable)
                {
                    byte[] buff = new byte[1];
                    int read = stream.Read(buff, 0, 1);
                    headersRaw += System.Text.Encoding.UTF8.GetString(buff, 0, read);
                } else
                {

                    yield return new WaitForSeconds(0.01f);
                }
            }
            // Log.Debug (TAG, "Headers read");

            if (Waited >= Timeout)
            {
                ErrorMessage = "Network read timed out while reading headers";
                Failed = true;
                Callbacks();
                yield break;
            }
            // Log.Debug (TAG, "Headers ready");

            responseHeaders = ParseHeaders(headersRaw);
            // Read body if there's content-length header
            if (responseHeaders.ContainsKey("content-length"))
            {
                int contentLength = int.Parse(responseHeaders ["content-length"]);
                
                int bytesRead = 0;
                for (; Waited < Timeout && !(bytesRead >= contentLength); Waited += 0.01f)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] buff = new byte[1024];
                        int read = stream.Read(buff, 0, buff.Length);
                        ResponseBody += System.Text.Encoding.UTF8.GetString(buff, 0, read);
                        bytesRead += read;
                    } else
                    {
                        // Log.Debug(TAG, "Wait");
                        yield return new WaitForSeconds(0.01f);
                    }
                }
                // Log.Debug (TAG, "Body read");
                if (Waited >= Timeout)
                {
                    ErrorMessage = "Network read timed out during body";
                    Failed = true;
                    Callbacks();
                    yield break;
                }
            }

            if (ResponseCode == 200)
            {
                Succeeded = true;
            } else
            {
                ErrorMessage = "Fail because of response code " + ResponseCode;
            }

            Callbacks();*/
        }

        private void Callbacks()
        {
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }

            if (tcp != null)
            {
                tcp.Close();
                tcp = null;
            }

            DateTime after = DateTime.Now;
            TimeSpan span = after - Before;
            int ms = (int)span.TotalMilliseconds;
            
            if (Succeeded)
            {
                if (Failed)
                {
                    Log.Error(TAG, "Succeeded & failed");
                }
                NetworkResponseHandler.OnSuccess(ResponseBody, UrlString, ms);
            } else
            {
                if (Succeeded)
                {
                    Log.Error(TAG, "Failed & succeeded");
                }

                NetworkResponseHandler.OnError(ErrorMessage, ResponseCode, UrlString, ms);
            }
        }
        
        private Dictionary<string, string> ParseHeaders(string headers)
        {
            // Log.Debug (TAG, "ParseHeaders starting");

            Dictionary<string,string> dict = new Dictionary<string, string>();
            string[] lines = headers.Split("\r\n".ToCharArray());

            int i = 0;
            foreach (string line in lines)
            {
                if (i == 0)
                {
                    string[] words = line.Split(new string[] { " " }, 3, StringSplitOptions.None);
                    this.Version = words [0];
                    this.ResponseCode = int.Parse(words [1]);
                    this.Message = words [2];
                } else
                {
                    if (line.Contains(":"))
                    {
                        string[] parts = line.Split(":".ToCharArray());
                        dict.Add(parts [0].Trim().ToLower(), parts [1].Trim());
                    }
                }
                i++;
            }
            // Log.Debug (TAG, "ParseHeaders done");

            return dict;
        }
    }
}
#endif