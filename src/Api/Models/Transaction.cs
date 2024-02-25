using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.Models
{
    public class Transaction
    {
        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        [JsonPropertyName("valor")]
        public int Amount { get; set; }

        [JsonPropertyName("tipo")]
        public required string Type { get; set; }

        [JsonPropertyName("descricao")]
        public required string Description { get; set; }

        [JsonPropertyName("realizada_em")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public int CustomerId { get; set; }

        public bool IsDebit() => Type == "d";
        public bool IsCredit() => Type == "c";
    }
}
