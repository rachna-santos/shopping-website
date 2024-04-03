using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Theme_Implemenet.Models
{
    public class Customer
    {
        [Key]
        public int CustId { get; set; }
        public string CustName { get; set; }
        public string CustEmail { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string CustNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModified { get; set; }


    }
}



