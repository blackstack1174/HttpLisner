using System;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        HttpListener m_HttpListener = new HttpListener();

        public MainForm()
        {
            InitializeComponent();
        }

        void ThreadProc(Object stateInfo)
        {
            Stream responseStream = getResponseStream(m_HttpListener);
            ASCIIEncoding ae = new ASCIIEncoding();

            while (true)
            {
                try
                {
                    MemoryStream tempStr = new MemoryStream();
                    // this.pictureBox1.Image = this.ScreenCapture();
                    this.ScreenCapture().Save(tempStr, ImageFormat.Bmp);

                    byte[] boundary = ae.GetBytes("\r\n--myboundary\r\nContent-Type: image/jpeg\r\nContent-Length:" + tempStr.Length + "\r\n\r\n");

                    MemoryStream mem = new System.IO.MemoryStream(boundary);
                    mem.WriteTo(responseStream);
                    tempStr.WriteTo(responseStream);

                    responseStream.Flush();

                    System.Threading.Thread.Sleep(50);
                }
                catch (System.Net.HttpListenerException ex)
                {
                    responseStream = getResponseStream(this.m_HttpListener);
                }
            }
        }

        private Stream getResponseStream(HttpListener httpList)
        {
            HttpListenerContext context = httpList.GetContext();
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
            this.m_HttpListener.Start();

            System.Threading.ThreadPool.QueueUserWorkItem(this.ThreadProc);
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
