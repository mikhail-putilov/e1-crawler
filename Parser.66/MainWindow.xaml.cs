using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;

namespace Parser._66
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string UriString = @"http://www.e1.ru/afisha/events/gastroli";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            try
            {
                HtmlDocument htmlPage = RetryLoadHtmlPage(2);
                List<GigEvent> events = ExtractAllGigEvents(htmlPage);
                MainDataGrid.ItemsSource = events;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private HtmlDocument RetryLoadHtmlPage(uint tries)
        {
            bool succeeded = false;

            do
            {
                try
                {
                    HtmlDocument htmlPage = LoadHtmlPageFromWeb();
                    succeeded = true;
                    return htmlPage;
                }
                catch (EmptyResponseException)
                {
                    tries--;
                    Thread.Sleep(100);
                }
            } while (!succeeded && tries > 0);
            //not succeeded case:
            throw new EmptyResponseException(UriString);
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
                string address = ExtractTextAboutAddress(pivotNode);
                string name = ExtractTextAboutName(pivotNode);
                string placeText = ExtractTextAboutPlace(pivotNode);

                events.Add(new GigEvent {Place = placeText, Date = date, Name = name, Address = address});
            }
            return events;
        }

        private string ExtractTextAboutAddress(HtmlNode pivotNode)
        {
            const string selector = @"child::text()";
            string address;
            bool success = ExtractText(selector, pivotNode, out address);
            if (success)
                return address;
            return null;
        }

        private static string ExtractTextAboutDate(HtmlNode pivotNode)
        {
            const string currentDaySelector = @"ancestor::table[3]//b[@class='white_menu']";
            const string closestDaySelector = @"(ancestor::table[1]/preceding-sibling::table//span[@class='white_menu'][last()])[last()]";
            string date;

            bool success = ExtractText(closestDaySelector, pivotNode, out date);
            if (success)
                return date;

            success = ExtractText(currentDaySelector, pivotNode, out date);
            if (success)
                return date;

            return null;
        }

        private static bool ExtractText(string xPathSelector, HtmlNode pivotNode, out string content)
        {
            HtmlNode node = pivotNode.SelectSingleNode(xPathSelector);
            if (node == null)
            {
                content = "";
                return false;
            }
            content = node.InnerText.Trim(' ', '\n', '\t', ',', '.');
            if (content == string.Empty)
                Console.Error.WriteLine("Extracted text from pivot node \"{0}\" with selector xpath \"{1}\" is empty",
                    pivotNode.InnerText, xPathSelector);
            return true;
        }

        private static string ExtractTextAboutName(HtmlNode pivotNode)
        {
            //take second table after table that is holding pivotnode (special thanks to e1's front-end developers):
            //ancestor::table[1]/following-sibling::table[2]
            const string selector = @"ancestor::table[1]/following-sibling::table[2]//b[@class='big_orange']";
            string name;
            bool success = ExtractText(selector, pivotNode, out name);
            if (success)
                return name;
            return null;
        }

        private static string ExtractTextAboutPlace(HtmlNode pivotNode)
        {
            const string selector = @"../b";
            string place;
            bool success = ExtractText(selector, pivotNode, out place);
            if (success)
                return place;
            return null;
        }

        private HtmlDocument LoadHtmlPageFromWeb()
        {
            using (var response = WebRequest.Create(UriString).GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var doc = new HtmlDocument();
                doc.Load(stream);
                return doc;
            }
        }

        private void SelectedRowHandler(object sender, SelectionChangedEventArgs e)
        {
            var gigEvent = ((DataGrid) sender).SelectedItem as GigEvent;
            if (gigEvent != null)
                StatusBlock.Text = gigEvent.Address;
        }

        private void GotoWebsiteButton_ClickHandler(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(UriString);
        }
    }
}