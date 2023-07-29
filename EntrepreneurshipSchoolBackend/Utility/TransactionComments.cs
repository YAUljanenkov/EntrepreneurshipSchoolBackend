namespace EntrepreneurshipSchoolBackend.Utility;

public struct TransactionComments
{
    public static string LotIncome(Models.Claim claim)
    {
        return $"Поступление за покупку лота №{claim.Lot?.Number}";
    }

    public static string FailedDeadline(Models.Claim claim)
    {
        return $"Списание за просроченный дедлайн задания {claim.Task?.Title}";
    }

    public static string ReturnLot(Models.Claim claim)
    {
        return $"Возврат за покупку лота №{claim.Lot?.Number}";
    }

    public static string TransferIncome(Models.Claim claim)
    {
        return $"Перевод от ученика {claim.Learner?.Surname} {claim.Learner?.Name} {claim.Learner?.Lastname}";
    }
    
    public static string TransferOutcome(Models.Claim claim)
    {
        return $"Перевод ученику {claim.Receiver?.Surname} {claim.Receiver?.Name} {claim.Receiver?.Lastname}";
    }
    
    public static string TransferOutcomeReject(Models.Claim claim)
    {
        return $"Возврат за перевод ученику {claim.Receiver?.Surname} {claim.Receiver?.Name} {claim.Receiver?.Lastname}";
    }
    
    public static string BuyLot(Models.Claim claim)
    {
        return $"Cписание за покупку лота №{claim.Lot?.Number}";
    }
}