public class RoundScoreResult
{
    public int Score;
    public int BaseScore;
    public int BonusScore;
    public bool IsFailed;
    public bool IsFatal;
    public bool UsedRiskCard;
    public bool UsedDamagedCard;
    public bool SatisfiedClient;
    public string Reason;

    public static RoundScoreResult Success(int score, int baseScore, int bonusScore, bool usedRiskCard, bool usedDamagedCard)
    {
        return new RoundScoreResult
        {
            Score = score,
            BaseScore = baseScore,
            BonusScore = bonusScore,
            IsFailed = false,
            IsFatal = false,
            UsedRiskCard = usedRiskCard,
            UsedDamagedCard = usedDamagedCard,
            SatisfiedClient = true,
            Reason = ""
        };
    }

    public static RoundScoreResult Failed(string reason, bool usedRiskCard = false, bool usedDamagedCard = false)
    {
        return new RoundScoreResult
        {
            Score = 0,
            BaseScore = 0,
            BonusScore = 0,
            IsFailed = true,
            IsFatal = false,
            UsedRiskCard = usedRiskCard,
            UsedDamagedCard = usedDamagedCard,
            SatisfiedClient = false,
            Reason = reason
        };
    }

    public static RoundScoreResult Fatal(string reason, bool usedRiskCard = true, bool usedDamagedCard = true)
    {
        return new RoundScoreResult
        {
            Score = 0,
            BaseScore = 0,
            BonusScore = 0,
            IsFailed = true,
            IsFatal = true,
            UsedRiskCard = usedRiskCard,
            UsedDamagedCard = usedDamagedCard,
            SatisfiedClient = false,
            Reason = reason
        };
    }
}