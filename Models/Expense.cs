using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendWise.Models;

public class Expense
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Range(0.01, 100000000, ErrorMessage = "Amount must be greater than zero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public ExpenseCategory Category { get; set; }

    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(300)]
    public string? Description { get; set; }
}