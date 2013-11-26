using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Controls.Primitives;
using HtmlAgilityPack;

namespace Parser._66
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
//        const string UriString = @"http://www.e1.ru/afisha/events/gastroli";
        private const string UriString = @"http://www.e1.ru/afisha/events/gastroli/1.html";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            HtmlDocument htmlPage = LoadHtmlPageFromWeb();
            List<GigEvent> events = ExtractAllGigEvents(htmlPage);
            MainDataGrid.ItemsSource = events;
        }

        private List<GigEvent> ExtractAllGigEvents(HtmlDocument htmlSnippet)
        {
            var events = new List<GigEvent>();

            HtmlNode root = htmlSnippet.DocumentNode;
            HtmlNodeCollection pivotNodes = root.SelectNodes(@"//span[@class='small']");
            //fucked up layout
            foreach (HtmlNode pivotNode in pivotNodes)
            {
                string date = ExtractTextAboutDate(pivotNode);

                string name = ExtractTextAboutName(pivotNode);

                string placeText = ExtractTextAboutPlace(pivotNode);

                events.Add(new GigEvent {Place = placeText, Date = date, Name = name});
            }
            return events;
        }

        private static string ExtractTextAboutDate(HtmlNode pivotNode)
        {
            const string selector = @"ancestor::table[3]//b[@class='white_menu']";
            const string selector2 = @"ancestor::table[1]/preceding-sibling::table//span[@class='white_menu'][last()]";
            const string selector3 = @"(ancestor::table[1]/preceding-sibling::table//span[@class='white_menu'][last()])[last()]";
            try
            {
                return ExtractText(selector3, pivotNode);
            }
            catch (SelectException e)
            {
                return null;
            }
        }

        private static string ExtractText(string xPathSelector, HtmlNode pivotNode)
        {
            HtmlNode node = pivotNode.SelectSingleNode(xPathSelector);
            if (node == null)
                throw new SelectException(xPathSelector);
            string date = node.InnerText.Trim();
            if (date == string.Empty)
                Console.Error.WriteLine("Extracted text from pivot node \"{0}\" with selector xpath \"{1}\" is empty",
                    pivotNode.InnerText, xPathSelector);
            return date;
        }

        private static string ExtractTextAboutName(HtmlNode pivotNode)
        {
            //take second table after table that is holding pivotnode (special thanks to e1's front-end developers):
            //ancestor::table[1]/following-sibling::table[2]
            const string selector = @"ancestor::table[1]/following-sibling::table[2]//b[@class='big_orange']";
            return ExtractText(selector, pivotNode);
        }

        private static string ExtractTextAboutPlace(HtmlNode pivotNode)
        {
            const string selector = @"../b";
            return ExtractText(selector, pivotNode);
        }

        private HtmlDocument LoadHtmlPageFromWeb()
        {
            Stream stream = WebRequest.Create(UriString).GetResponse().GetResponseStream();
            if (stream == null)
                throw new EmptyResponseException(UriString);

            var doc = new HtmlDocument();
            doc.Load(stream);

            stream.Close();

            return doc;
        }
    }
}