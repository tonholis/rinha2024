using System.ComponentModel.DataAnnotations;

namespace Api.Data.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public int AccountLimit { get; set; }
        public int Balance { get; set; }
    }
}
