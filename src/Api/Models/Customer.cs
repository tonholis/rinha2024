using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public int Limit { get; set; }
        public int Balance { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }

    public class InsufficientBalanceException : Exception { }
}
