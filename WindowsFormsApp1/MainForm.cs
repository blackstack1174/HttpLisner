﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        HttpListener m_HttpListener = new HttpListener();
        bool isStop = false;
        public MainForm()
        {
            InitializeComponent();
        }

        void HandleRequest(Object stateInfo)
        {
            var context = (HttpListenerContext)stateInfo;
            switch (context.Request.RawUrl)
            {
                case "/Data/":
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    string responseString = "<HTML><title>테스트페이지</title> <BODY> 와 개쩐다..</BODY></HTML>";
                    byte[] buffer = System.Text.Encoding.Default.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                    break;
                case "/MyTemp/":
                    Stream responseStream = getResponseStream(context);
                    ASCIIEncoding ae = new ASCIIEncoding();
                    while (true)
                    {
                        try
                        {
                            if (this.isStop == true) { return; }
                            MemoryStream tempStr = new MemoryStream();
                            // this.pictureBox1.Image = this.ScreenCapture();
                            this.ScreenCapture().Save(tempStr, ImageFormat.Bmp);

                            byte[] boundary = ae.GetBytes("\r\n--myboundary\r\nContent-Type: image/jpeg\r\nContent-Length:" + tempStr.Length + "\r\n\r\n");

                            MemoryStream mem = new System.IO.MemoryStream(boundary);
                            mem.WriteTo(responseStream);
                            tempStr.WriteTo(responseStream);

                            responseStream.Flush();
                        }
                        catch (System.Net.HttpListenerException ex)
                        {
                            responseStream = getResponseStream(context);
                        }
                    }

            }
        }

        private Stream getResponseStream(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            response.ContentType = "multipart/x-mixed-replace; boundary=--myboundary";
            return response.OutputStream;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            this.m_HttpListener.Prefixes.Add("http://127.0.0.1:8011/MyTemp/");
            this.m_HttpListener.Prefixes.Add("http://127.0.0.1:8011/Data/");
            this.m_HttpListener.Start();
            while (true)
            {
                try
                {
                    var context = m_HttpListener.GetContext();
                    ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
                }
                catch (Exception)
                {
                    // Ignored for this example
                }
            }
        }

        private void button1_Close_Click(object sender, EventArgs e)
        {
            this.isStop = true;
            this.m_HttpListener.Stop();
            this.m_HttpListener.Close();

        }


        public Bitmap ScreenCapture()
        {
            int w = Screen.PrimaryScreen.WorkingArea.Width;
            int h = Screen.PrimaryScreen.WorkingArea.Height;
            Size s = new Size(w, h);
            Bitmap b = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(b);
            g.CopyFromScreen(0, 0, 0, 0, s);
            return b;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.linkLabel1.Text);
            Debug.WriteLine("시작");
        }

    }
}
