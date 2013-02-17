using Clothe.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Clothe.Common
{
    class ShopsFilter : IValueConverter
    {
        public string shopCategory { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var headlineShops = value as ICollection<ShopDataItem>;
            return headlineShops.Where(m => m.FirstCategory == this.shopCategory);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            List<ShopDataItem> obj = value as List<ShopDataItem>;
            return obj.First().FirstCategory;
        }
    }
}
