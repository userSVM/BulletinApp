﻿using BulletinExample.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulletinExample.Tools
{
    public static class WebWorker
    {
        #region STA Thread tool
        private static Thread WorkThread { get; set; }
        public static WebBrowser WebBrowser { get; set; }



        public static HtmlDocument WebDocument
        {
            get
            {
                HtmlDocument document = null;
                DCT.ExecuteCurrentDispatcher(d => document = WebBrowser.Document);
                return document;
            }


        }

        static ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();
        public static Action IntializationComplite { get; set; }

        private static void WorkThreadCheckOrCreate()
        {
            DCT.Execute(data =>
            {
                if (WorkThread == null || !WorkThread.IsAlive)
                {
                    WorkThread = new Thread(() =>
                    {
                        //_DCT.ExecuteCurrentDispatcher(d =>
                        //{
                        //if (WebBrowser != null)
                        //{
                        //    WebBrowser.Dispose();
                        //    WebBrowser = null;
                        //}
                        //WebBrowser = new WebBrowser();
                        //WebBrowser.Navigating += WebBrowserOnNavigating;
                        //WebBrowser.ScriptErrorsSuppressed = true;
                        //});

                        while (true)
                        {
                            Action next;
                            var result = queue.TryDequeue(out next);
                            if (result && next != null)
                            {
                                next();
                                //DCT.SendInfo("STA Action execute");
                            }
                            else
                            {
                                Thread.Sleep(1000);
                               // DCT.SendInfo("STA iteration complete");
                            }
                        }
                    });
                    WorkThread.SetApartmentState(ApartmentState.STA);
                    WorkThread.IsBackground = true;
                    WorkThread.Start();
                    IntializationComplite?.Invoke();
                }
            });
        }

        public static void SetBrowser(WebBrowser browser)
        {
            WebBrowser = browser;
        }

        public static void Execute(Action executeAction, Action completeAction = null)
        {
            DCT.Execute(data2 =>
            {
                if (queue == null)
                    queue = new ConcurrentQueue<Action>();
                WorkThreadCheckOrCreate();
                queue.Enqueue(() =>
                DCT.Execute(data =>
                {
                    if (executeAction == null) throw new NullReferenceException("WebWorker.Execute Параметр не может быть пустым");
                    executeAction?.Invoke();
                    completeAction?.Invoke();
                }));
            });
        }
        #endregion



        #region BaseWebWorker

        public static void DownloadPage(string url, Action<string> compliteAction)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("DownloadPage url can't be null!");
            Execute(() =>
            {
                Stream stream = null;
                DCT.ExecuteCurrentDispatcher(d =>
                {
                    WebBrowser.Navigate(url);
                    Wait();
                    stream = WebBrowser.DocumentStream;
                });
                var result = "";
                using (var sr = new StreamReader(stream))
                    result = sr.ReadToEnd();
                compliteAction?.Invoke(result);
            });
        }

        /// <summary>
        /// Вызывать только внутри DownloadPage
        /// </summary>
        /// <param name="url"></param>
        public static void NavigatePage(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new NullReferenceException("DownloadPage url can't be null!");

            DCT.ExecuteCurrentDispatcher(d =>
            {
                WebBrowser.Navigate(url);
                Wait();
            });
        }

        public static void WaitPage(Action<string> afterCompleted)
        {
            Execute(() =>
            {
                Stream stream = null;
                DCT.ExecuteCurrentDispatcher(d =>
                {
                    Wait();
                    stream = WebBrowser.DocumentStream;
                });
                var result = "";
                using (var sr = new StreamReader(stream))
                    result = sr.ReadToEnd();
                afterCompleted?.Invoke(result);
            });
        }

        public static void RefreshPage(Action<string> afterCompleted)
        {
            Execute(() =>
            {
                Stream stream = null;
                DCT.ExecuteCurrentDispatcher(d =>
                {
                    WebBrowser.Refresh();
                    Wait();
                    stream = WebBrowser.DocumentStream;
                });
                var result = "";
                using (var sr = new StreamReader(stream))
                    result = sr.ReadToEnd();
                afterCompleted?.Invoke(result);
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        public static void JustWait(int seconds)
        {
            Execute(() =>
            {
                Thread.Sleep(seconds * 1000);
            });
        }
        static void Wait()
        {
            try
            {
                var date = DateTime.Now;
                while (true)
                {
                    Application.DoEvents();
                    if (date.AddSeconds(10) < DateTime.Now) break;
                    var state = WebBrowser.ReadyState;
                    if (state == WebBrowserReadyState.Complete)
                        break;
                }
            }
            catch (Exception ex)
            {

            }

        }
        #endregion
    }
}
