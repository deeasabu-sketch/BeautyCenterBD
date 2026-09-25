using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomAuthentication.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal TotalPrice
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }
        [Required]
        public decimal LineTotal { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }

           
        
    }
}