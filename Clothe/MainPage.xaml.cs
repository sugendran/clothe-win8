using Clothe.DataModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Clothe
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Clothe.Common.LayoutAwarePage
    {
        public ShopDataSourceModel shopDataSourceModel;

        private Geolocator _geolocator = null;

        public MainPage()
        {
            this.InitializeComponent();
            shopDataSourceModel = new ShopDataSourceModel(ShopDataSource.Instance);
            DataContext = shopDataSourceModel;
            _geolocator = new Geolocator();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {

        }

        async void updateLocationText(Geoposition pos)
        {
            HttpClient Client = new HttpClient();
            string Result = await Client.GetStringAsync(string.Format("http://nominatim.openstreetmap.org/reverse?format=xml&zoom=18&lat={0}&lon={1}&application={2}", 
                                                        pos.Coordinate.Latitude.ToString(CultureInfo.InvariantCulture), 
                                                        pos.Coordinate.Longitude.ToString(CultureInfo.InvariantCulture), 
                                                        "Clothe"));

            XDocument ResultDocument = XDocument.Parse(Result);
            XElement AddressElement = ResultDocument.Root.Element("addressparts");

            string road = AddressElement.Element("road").Value;
            string city = AddressElement.Element("city").Value;

            txtLocation.Text = String.Format("Using location: {0}, {1}", road, city);
        }

        async private void Button_GetStarted(object sender, RoutedEventArgs e)
        {
            this.shopDataSourceModel.shopDataSource.ShowLoader = true;
            this.shopDataSourceModel.shopDataSource.StatusText = "Getting Location";
            _geolocator.DesiredAccuracy = PositionAccuracy.High;
            try
            {
                Geoposition pos = await _geolocator.GetGeopositionAsync();
                updateLocationText(pos);
                this.shopDataSourceModel.shopDataSource.LoadAllData(pos);
            }
            catch (Exception ex)
            {
                this.shopDataSourceModel.shopDataSource.ShowLoader = false;
                this.shopDataSourceModel.shopDataSource.StatusText = "Could not get your location. Please make sure location services have been enabled for this application";
            }
        }

        private void headlineResultsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((ShopDataItem)e.ClickedItem).FirstCategory;
            this.Frame.Navigate(typeof(GroupedItemsPage), itemId);
        }
    }
}
