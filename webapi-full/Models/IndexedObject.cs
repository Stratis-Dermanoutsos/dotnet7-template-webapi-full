using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace webapi_full.Models;

public class IndexedObject
{
    [Key]
    [Column("Id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [Required]
    [Column("Date_In")]
    [JsonIgnore]
    public DateTime DateIn { get; set; } = DateTime.Now;

    [Required]
    [Column("Date_Edit")]
    [JsonIgnore]
    public DateTime DateEdit { get; set; } = DateTime.Now;
}