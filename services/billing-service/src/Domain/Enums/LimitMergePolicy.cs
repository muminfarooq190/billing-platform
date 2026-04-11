namespace BillingService.Domain.Enums;

public enum LimitMergePolicy
{
    Max = 0,
    Sum = 1,
    LatestWins = 2,
    OverrideOnly = 3
}
