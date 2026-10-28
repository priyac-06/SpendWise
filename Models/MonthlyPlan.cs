using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpendWise.Models;

public class MonthlyPlan
{
    public int Id { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Display(Name = "Income type")]
    public IncomeType IncomeType { get; set; }

    [Display(Name = "Monthly income")]
    [Range(0.01, 100000000, ErrorMessage = "Income must be greater than zero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyIncome { get; set; }

    [Display(Name = "Savings goal type")]
    public SavingsGoalType SavingsGoalType { get; set; }

    [Display(Name = "Savings goal value")]
    [Range(0, 100000000, ErrorMessage = "Savings goal cannot be negative.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SavingsGoalValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}