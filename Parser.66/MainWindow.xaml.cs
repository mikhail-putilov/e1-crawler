using System;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;

namespace Parser._66
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
//        const string UriString = @"http://www.e1.ru/afisha/events/gastroli";
        const string UriString = @"http://www.e1.ru/afisha/events/gastroli/1.html";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            HtmlDocument htmlPage = LoadHtmlPageFromWeb();
            List<gigEvent> events = ExtractAllGigEvents(htmlPage);
            MainDataGrid.ItemsSource = events;
        }

        private List<gigEvent> ExtractAllGigEvents(HtmlDocument htmlSnippet)
        {
            var events = new List<gigEvent>();

            HtmlNode root = htmlSnippet.DocumentNode;
            HtmlNodeCollection pivotNodes = root.SelectNodes(@"//span[@class='small']");
            //fucked up layout
            string date = ExtractTextAboutDate(pivotNodes);
            foreach (HtmlNode pivotNode in pivotNodes)
            {
                string name = ExtractTextAboutName(pivotNode);

                string placeText = ExtractTextAboutPlace(pivotNode);

                events.Add(new gigEvent {Place = placeText, Date = date, Name = name});
            }
            return events;
        }

        private static string ExtractTextAboutDate(HtmlNodeCollection pivotNodes)
        {
            string date =
                pivotNodes[0].SelectSingleNode(@"ancestor::table[3]//b[@class='white_menu']")
                    .InnerText.Trim();
            return date;
        }

        private static string ExtractTextAboutName(HtmlNode pivotNode)
        {
            //take second table after table that is holding pivotnode:
            //ancestor::table[1]/following-sibling::table[2]
            const string selector = @"ancestor::table[1]/following-sibling::table[2]//b[@class='big_orange']";
            HtmlNode node = pivotNode.SelectSingleNode(selector);
            if (node == null)
                throw new SelectException(selector);

            string name =
                node.InnerText.Trim();
            if (name == string.Empty)
                Console.Error.WriteLine("name is empty for pivot node \"{0}\"", pivotNode.InnerText);

            return name;
        }

        private static string ExtractTextAboutPlace(HtmlNode pivotNode)
        {
            string placeText = pivotNode.ParentNode.Element("b").InnerText.Trim();
            if (placeText == string.Empty)
                Console.Error.WriteLine("place text is empty for pivot node \"{0}\"", pivotNode.InnerText);
            return placeText;
        }

        private HtmlDocument LoadHtmlPageFromWeb()
        {
            var stream = WebRequest.Create(UriString).GetResponse().GetResponseStream();
            if (stream == null) 
                throw new EmptyResponseException(UriString);

            var doc = new HtmlDocument();
            doc.Load(stream);

            stream.Close();

            return doc;
        }
    }
}