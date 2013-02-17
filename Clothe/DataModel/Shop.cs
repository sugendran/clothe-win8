using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Geolocation;

namespace Clothe.DataModel
{
    public enum ShopCategory
    {
        Shoes,
        Handbags,
        Jewellery,
        Watches,
        Sleepwear,
        Womenswear
    }

    public class ShopDataItem : Clothe.Common.BindableBase
    {      
        private static Uri _baseUri = new Uri("ms-appx:///");

        public string Id { get; private set; }
        public string Name { private set; get; }
        public string Address { private set; get; }
        public string Suburb { private set; get; }
        public string Logo { private set; get; }
        public string Longitude { private set; get; }
        public string Latitiude { private set; get; }
        public List<ShopCategory> Categories { private set; get; }
        public string FirstCategory { get; set; }

        public ShopDataItem(JsonObject shopDataObject)
        {
            this.Id = shopDataObject.GetNamedString("id");
            this.Name = shopDataObject.GetNamedString("name");
            if (shopDataObject.Keys.Contains("businessLogo"))
            {
                var urlObj = shopDataObject.GetNamedObject("businessLogo");
                if (urlObj.Keys.Contains("url"))
                    this.Logo = urlObj.GetNamedString("url");
            }
            var addressObj = shopDataObject.GetNamedObject("primaryAddress");
            if (addressObj.Keys.Contains("addressLine"))
                this.Address = addressObj.GetNamedString("addressLine");
            if (addressObj.Keys.Contains("latitude"))
                this.Latitiude = addressObj.GetNamedString("latitude");
            if (addressObj.Keys.Contains("longitude"))
                this.Longitude = addressObj.GetNamedString("longitude");
            this.Suburb = addressObj.GetNamedString("suburb");
            this.Categories = new List<ShopCategory>();
            var categoriesObj = shopDataObject.GetNamedArray("categories");
            var catCount = categoriesObj.Count;
            for (uint i = 0; i < catCount; i++)
            {
                var cat = categoriesObj.GetObjectAt(i);
                var catstr = cat.GetNamedString("id");
                if (catstr.Equals("24414"))
                    this.Categories.Add(ShopCategory.Handbags);
                else if (catstr.Equals("13927"))
                    this.Categories.Add(ShopCategory.Jewellery);
                else if (catstr.Equals("27022"))
                    this.Categories.Add(ShopCategory.Shoes);
                else if (catstr.Equals("27642"))
                    this.Categories.Add(ShopCategory.Sleepwear);
                else if (catstr.Equals("16373"))
                    this.Categories.Add(ShopCategory.Watches);
                else if (catstr.Equals("31917"))
                    this.Categories.Add(ShopCategory.Womenswear);
            }
            if (String.IsNullOrEmpty(this.FirstCategory))
                this.FirstCategory = this.Categories.First().ToString();
        }
    }

    public class ShopDataSource: Clothe.Common.BindableBase
    {
        private static ShopDataSource instance;
        public static ShopDataSource Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShopDataSource();
                }
                return instance;
            }
        }

        private bool isLoading = false;
        private Random rand = new Random();
        private string sensisURL = "http://api.sensis.com.au/ob-20110511/test/search?";
        private string sensisToken = "urd5y88skh4cgxx3nsrwr8pj";

        private ObservableCollection<ShopDataItem> _headlineShops = new ObservableCollection<ShopDataItem>();
        public ObservableCollection<ShopDataItem> HeadlineShops
        {
            get { return this._headlineShops; }
        }

        private ObservableCollection<ShopDataItem> _allShops = new ObservableCollection<ShopDataItem>();
        public ObservableCollection<ShopDataItem> AllShops
        {
            get {return this._allShops;}
        }

        private Boolean _showInstructions = true;
        public Boolean ShowInstructions
        { 
            get{ return _showInstructions;}
            set{ this.SetProperty(ref this._showInstructions, value); } 
        }

        private Boolean _showLoader = false;
        public Boolean ShowLoader
        {
            get { return _showLoader; }
            set { this.SetProperty(ref this._showLoader, value); }
        }

        private String _statusText = String.Empty;
        public String StatusText
        {
            get { return _statusText; }
            set { this.SetProperty(ref this._statusText, value); }
        }

        private ShopDataSource()
        {
            _headlineShops.CollectionChanged += delegate { 
                base.OnPropertyChanged("HeadlineShops"); 
            };
        }

        public void reportShopDisplay(ShopDataItem shop)
        {

        }

        private void addHeadlineShop(ShopCategory category)
        {
            string cat = category.ToString();
            if (_headlineShops.Any(m => m.FirstCategory == cat)) return;
            var shoeShops = _allShops.Where(m => !_headlineShops.Contains(m) && m.FirstCategory == cat).ToList();
            shoeShops.OrderBy(m => this.rand.Next());
            int shoeShopsCount = shoeShops.Count;
            for (int i = 0; i < shoeShopsCount && i < 1; i++)
            {
                _headlineShops.Add(shoeShops[i]);
                reportShopDisplay(shoeShops[i]);
            }
        }

        public class GroupedItems
        {
            public string Category { get; set; }
            public List<ShopDataItem> Shops { get; set; }
        }

        public List<GroupedItems> ShopsByCategory
        {
            get {
                var result = new List<GroupedItems>();
                result.Add(new GroupedItems() { 
                    Category = "Handbags",
                    Shops = _allShops.Where(m => m.Categories.Contains(ShopCategory.Handbags)).ToList()
                });
                result.Add(new GroupedItems()
                {
                    Category = "Jewellery",
                    Shops = _allShops.Where(m => m.Categories.Contains(ShopCategory.Jewellery)).ToList()
                });
                result.Add(new GroupedItems()
                {
                    Category = "Shoes",
                    Shops = _allShops.Where(m => m.Categories.Contains(ShopCategory.Shoes)).ToList()
                });
                result.Add(new GroupedItems()
                {
                    Category = "Sleepwear",
                    Shops = _allShops.Where(m => m.Categories.Contains(ShopCategory.Sleepwear)).ToList()
                });
                result.Add(new GroupedItems()
                {
                    Category = "Watches",
                    Shops = _allShops.Where(m => m.Categories.Contains(ShopCategory.Watches)).ToList()
                });
                result.Add(new GroupedItems()
                {
                    Category = "Womenswear",
                    Shops = _allShops.Where(m => m.Categories.Contains(ShopCategory.Womenswear)).ToList()
                });
                return result;
            }
        }

        public void LoadAllData(Geoposition pos)
        {
            if (this.isLoading) return;
            this.isLoading = true;
            this.StatusText = "Getting Shops";
            LoadAllData(pos, 5);
        }

        async private void LoadAllData(Geoposition pos, int radius, int page = 0)
        {
            String url = String.Format("{0}rows=50&categoryId=13927&categoryId=16373&categoryId=24414&categoryId=27022&categoryId=27642&categoryId=31917&location={1},{2}&radius={3}&key={4}&page={5}", this.sensisURL, pos.Coordinate.Latitude, pos.Coordinate.Longitude, radius, this.sensisToken, page);
            //System.Diagnostics.Debug.WriteLine("requesting url {0}", url);
            var request = HttpWebRequest.CreateHttp(url);
            var response = await request.GetResponseAsync();
            var stream = response.GetResponseStream();
            string str;
            using (var sr = new StreamReader(stream))
            {
                str = await sr.ReadToEndAsync();
            }
            var json = JsonObject.Parse(str);
            var statuscode = json.GetNamedNumber("code");
            if (statuscode >= 200 && statuscode < 300)
            {
                this.ShowInstructions = false;
                this.ShowLoader = false;
                this.StatusText = String.Empty;
                var results = json.GetNamedArray("results");
                var count = results.Count;
                for (uint i = 0; i < count; i++)
                {
                    var obj = results.GetObjectAt(i);
                    var newItem = new ShopDataItem(obj);
                    _allShops.Add(newItem);
                }
                addHeadlineShop(ShopCategory.Shoes);
                addHeadlineShop(ShopCategory.Handbags);
                addHeadlineShop(ShopCategory.Jewellery);
                addHeadlineShop(ShopCategory.Sleepwear);
                addHeadlineShop(ShopCategory.Watches);
                addHeadlineShop(ShopCategory.Womenswear);
                this.OnPropertyChanged("ShopsByCategory");

                //totalPages
                int totalPages = Convert.ToInt32(json.GetNamedNumber("totalPages"));
                if (page < totalPages)
                    LoadAllData(pos, radius, page + 1);
                else if (_allShops.Count < 100)
                    LoadAllData(pos, radius + 5);
                else
                    this.isLoading = false;
            }
            else
            {
                this.ShowLoader = false;
                this.StatusText = "Could not retrieve results. Please check your internet connection and try again.";
            }
        }
    }

    public class ShopDataSourceModel
    {
        public ShopDataSource shopDataSource { get; set; }

        public ShopDataSourceModel(ShopDataSource passedIn)
        {
            this.shopDataSource = passedIn;
        }
    }

}
