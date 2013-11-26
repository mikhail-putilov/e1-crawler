using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using HtmlAgilityPack;

namespace Parser._66
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            HtmlDocument htmlSnippet = LoadHtmlSnippetFromWeb();

            List<E1Event> e1Events = ExtractAllE1Events(htmlSnippet);

            // bind to gridview
            MainDataGrid.ItemsSource = e1Events;
        }

        private List<E1Event> ExtractAllE1Events(HtmlDocument htmlSnippet)
        {
            var events = new List<E1Event>();

            HtmlNode root = htmlSnippet.DocumentNode;
            HtmlNodeCollection pivotNodes = root.SelectNodes(@"//span[@class='small']");
            //fucked up layout
            string date = ExtractTextAboutDate(pivotNodes);
            foreach (HtmlNode pivotNode in pivotNodes)
            {
                string name = ExtractTextAboutName(pivotNode);

                string placeText = ExtractTextAboutPlace(pivotNode);

                events.Add(new E1Event {Place = placeText, Date = date, Name = name});
            }
            return events;
        }

        private static string ExtractTextAboutDate(HtmlNodeCollection pivotNodes)
        {
            //todo: redo many parents into parent holding tables
            string date =
                pivotNodes[0].SelectSingleNode(@"../../../../../../../../../table[2]//b[@class='white_menu']")
                    .InnerText.Trim();
            return date;
        }

        private static string ExtractTextAboutName(HtmlNode pivotNode)
        {
            //take second table after table that is holding pivotnode:
            //../../../../following-sibling::table[2]
            const string selector = @"../../../../following-sibling::table[2]//b[@class='big_orange']";
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

        private HtmlDocument LoadHtmlSnippetFromWeb()
        {
            const string uriString = @"http://www.e1.ru/afisha/events/gastroli";
            //todo: invalid response throw exception case?
            var stream = WebRequest.Create(uriString).GetResponse().GetResponseStream();

            var doc = new HtmlDocument();
            doc.Load(stream);

            stream.Close();

            return doc;
        }
    }
}