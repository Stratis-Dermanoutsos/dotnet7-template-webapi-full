using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using webapi_full.Enums;

namespace webapi_full.Models;

[PrimaryKey("Id")]
public class User : IndexedObject
{
    [Key]
    [Column("Email")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [Key]
    [Column("User_Name")]
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

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

    [Required]
    [Column("Role")]
    [JsonPropertyName("role")]
    public Role Role { get; set; } = Role.User;

    [NotMapped]
    [JsonPropertyName("fullName")]
    public string FullName => $"{this.FirstName} {this.LastName}";
}