using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace webapi_full.Models;

[PrimaryKey("Id")]
public class User : IndexedObject
{
    [Key]
    [Column("Email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("Password")]
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Column("First_Name")]
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Column("Last_Name")]
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    [JsonIgnore]
    public string FullName => $"{this.FirstName} {this.LastName}";
}