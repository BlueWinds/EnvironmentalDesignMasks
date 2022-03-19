using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using Newtonsoft.Json;

namespace EnvironmentalDesignMasks {
    public class EDMRequest {
        public EDMRequest(string m, string d) { method = m; data = d; response = null; }
        public string method;
        public string data;
        public bool ready = false;
        public string response;
        public int statusCode;
        public string contentType;

        public void done(object r, int sc) {
            ready = true;
            response = JsonConvert.SerializeObject(r);
            contentType = "application/json";
            statusCode = sc;
        }

        public void done(string r, string ct, int sc) {
            ready = true;
            response = r;
            contentType = ct;
            statusCode = sc;
        }
    }

    public class DebugServer : MonoBehaviour {
        public static int debugPort = 3062;

        private HttpListener listener;
        private ConcurrentQueue<EDMRequest> queue = new ConcurrentQueue<EDMRequest>();
        public string indexHtml;

        public DebugServer() {
            listener = new HttpListener();
            string baseUrl = $"http://localhost:{debugPort}/";
            listener.Prefixes.Add($"{baseUrl}");
            listener.Start();

            EDM.modLog.Info?.Write($"Listening on {baseUrl}. Starting worker thread.");

            Thread listenerThread = new Thread(HandleRequests);
            listenerThread.Start();
        }

        private void HandleRequests() {
            while (listener.IsListening) {
                IAsyncResult context = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                context.AsyncWaitHandle.WaitOne();
            }
        }

        private void ListenerCallback(IAsyncResult ar) {
            HttpListener listener = ar.AsyncState as HttpListener;
            HttpListenerContext context = listener.EndGetContext(ar);

            string method = context.Request.Url.AbsolutePath;

            StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            string body = reader.ReadToEnd();

            EDMRequest request = new EDMRequest(method, body);
            queue.Enqueue(request);
            while (!request.ready) {
                Thread.Sleep(10);
            }

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(request.response);
            context.Response.StatusCode = request.statusCode;
            context.Response.ContentType = request.contentType;
            context.Response.ContentLength64 = buffer.Length;
            System.IO.Stream output = context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public void LateUpdate() {
            EDMRequest request;
            bool gotRequest = queue.TryDequeue(out request);
            if (gotRequest) {
                try {
                    EDM.modLog.Info?.Write($"Incoming request \"{request.method}\": \"{request.data}\"");
                    switch (request.method) {
                        case "/listMoods":
                            request.done(Utils.listMoods(), 200);
                            break;
                        case "/getMood":
                            CustomMood mood = Utils.jsonMood(request.data);
                            request.done(mood, 200);
                            break;
                        case "/currentMood":
                            request.done(new CustomMood(Utils.getCurrentMood(), "Current"), 200);
                            break;
                        case "/updateMood":
                            Utils.applyMoodLive(CustomMood.fromString(request.data));
                            request.done(null, 204);
                            break;
                        default:
                            request.done(readIndexHtml(), "text/html", 200);
                            break;
                    }
                } catch (Exception e) {
                    EDM.modLog.Error?.Write(e);
                    request.done(e, 500);
                }
            }
        }

        public string readIndexHtml() {
            try {
                using (StreamReader reader = new StreamReader($"{EDM.modDir}/debug.html")) {
                    return reader.ReadToEnd();
                }
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
                return e.ToString();
            }
        }
    }
}
