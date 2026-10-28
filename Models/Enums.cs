using System.ComponentModel.DataAnnotations;

namespace SpendWise.Models;

public enum IncomeType
{
    Salary,
    Allowance
}

public enum SavingsGoalType
{
    [Display(Name = "Fixed amount")]
    FixedAmount,
    Percentage
}

public enum ExpenseCategory
{
    [Display(Name = "Food and Drinks")]
    FoodAndDrinks,
    Shopping,
    Transport,
    [Display(Name = "Bills and Utilities")]
    BillsAndUtilities,
    Subscriptions,
    Education,
    Health,
    Entertainment,
    Other
}