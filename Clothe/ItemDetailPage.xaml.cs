using Clothe.DataModel;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Clothe
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage : Clothe.Common.LayoutAwarePage
    {
        public ShopDataSourceModel shopDataSourceModel { get; set; }

        public ItemDetailPage()
        {
            this.InitializeComponent();
            shopDataSourceModel = new ShopDataSourceModel(ShopDataSource.Instance);
            DataContext = shopDataSourceModel;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var shopId = (string)navigationParameter;
            var shop = this.shopDataSourceModel.shopDataSource.AllShops.First(m => m.Id == shopId);
            this.DefaultViewModel["Shop"] = shop;
        }

    }
}
