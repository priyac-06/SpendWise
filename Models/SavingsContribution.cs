using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendWise.Models;

public class SavingsContribution
{
    public int Id { get; set; }

    [Range(0.01, 100000000, ErrorMessage = "Amount must be greater than zero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(300)]
    public string? Description { get; set; }
}