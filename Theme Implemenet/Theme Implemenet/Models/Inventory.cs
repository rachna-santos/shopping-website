namespace Theme_Implemenet.Models
{
    public class Inventory
    {
        [key]
        public int Id {get;set;}
        public int custId { get;set;}
        public virtual Customer Customer {get;set; }
        public int OrderId { get; set; }
        public virtual Order Order_Tbl {get; set; }
        public int productbrandId { get; set; }
        public virtual ProductBrand ProductBrand {get; set;}
        public int productId {get;set;}
        public virtual Product Product {get;set;}
        public int productcolorId { get; set; }
        public virtual ProductColor ProductColor {get; set;}
        public int productsizeId { get; set;}
        public virtual ProductSize ProductSize {get; set;}
        public int quantity { get; set;}
        public int StatusId { get; set; }
        public virtual Status Status {get;set;}
        public DateTime CreateDate {get; set;}
        public DateTime Lastmodifield {get; set;}
    }
}



