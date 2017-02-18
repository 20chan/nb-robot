using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;


namespace BlogCrawler
{
    public partial class Form1 : Form
    {
        private ManualResetEvent loadEvent = new ManualResetEvent(false);
        private string[] keywords;
        private string id;

        private Random r = new Random();

        public Form1()
        {
            InitializeComponent();

            keywords = new string[] { "티비플 구름아트" };
            id = "aoikzto";
            this.Shown += (a, b) => new Thread(new ThreadStart(OneLoop)).Start();
            this.webBrowser1.DocumentCompleted += (a, b) => loadEvent.Set();

            CheckForIllegalCrossThreadCalls = false;
        }
        
        public HtmlElement[] SelectBlog(string id)
        {
            if (webBrowser1.InvokeRequired)
                return (HtmlElement[])webBrowser1.Invoke(
                    new Func<HtmlElement[]>(() => SelectBlog(id)));
            
            // string xpath = "//dd[@class='txt_block']";
            var blocks = from HtmlElement el in webBrowser1.Document.GetElementsByTagName("dd")
                         where el.GetAttribute("className") == "txt_block"
                         let url = el.GetElementsByTagName("a")[0].GetAttribute("href")
                         where url.Contains(id)
                         select el.GetElementsByTagName("a")[1];

            return blocks.ToArray();
        }

        public void Search(string text)
        {
            text = System.Web.HttpUtility.UrlEncode(text, Encoding.UTF8);
            string url = $"https://search.naver.com/search.naver?where=post&sm=tab_jum&ie=utf8&query={text}";
            webBrowser1.Navigate(url);
        }

        public void ScrollDown()
        {
            webBrowser1.Document.Window.ScrollTo(0, webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        public void Sleep(bool wait = false)
        {
            // 5~15초 휴식
            Thread.Sleep(r.Next(500, 1600));
            if (wait)
            {
                loadEvent.WaitOne();
                loadEvent.Reset();
            }
        }

        public void OneLoop()
        {
            foreach(string keyword in keywords)
            {
                Search(keyword); this.Text = "searching..";
                Sleep(true); this.Text = "search complete";
                var elems = SelectBlog(id); this.Text = "selecting..";
                Sleep(); this.Text = "select complete";
                foreach (var elem in elems)
                {
                    elem.InvokeMember("Click");
                    Sleep();
                    ScrollDown();
                    webBrowser1.GoBack();
                }
            }
        }
    }
}
