using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tongbro.Models
{
    public class ProductImage
    {
        public string Path;
        public string Thumbnail;
        public string Sku;
    }

    public class ProductCategory
    {
        public string Name;
        public string SeoName;
        public List<ProductCategory> Subcategories = new List<ProductCategory>();
        public List<ProductImage> Images = new List<ProductImage>();
    }
}
