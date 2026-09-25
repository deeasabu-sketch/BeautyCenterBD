using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CustomAuthentication.ViewModel
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required, StringLength(150)]
        public string ProductName { get; set; }

        [StringLength(50)]
        public string SKU { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int? BrandId { get; set; }

        [Required]
        public int? UnitId { get; set; }
        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public decimal PurchasePrice { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        public int ReorderLevel { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Existing cover image path (Edit only - shown as a preview).
        public string ExistingImagePath { get; set; }

        // New cover image upload (optional on Edit - keeps existing if not provided).
        public HttpPostedFileBase CoverImage { get; set; }

        // Additional gallery images (any number).
        public List<HttpPostedFileBase> GalleryImages { get; set; }

        // Existing gallery images (Edit only - for display + removal).
        public List<ExistingGalleryImage> ExistingGalleryImages { get; set; }

        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> BrandOptions { get; set; }
        public IEnumerable<SelectListItem> UnitOptions { get; set; }
    }

    public class ExistingGalleryImage
    {
        public int ProductImageId { get; set; }
        public string ImagePath { get; set; }
    }
}