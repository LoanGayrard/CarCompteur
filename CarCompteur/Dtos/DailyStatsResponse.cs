namespace CarCompteur.Dtos;

public record DailyStatsResponse(
    DateOnly Date,
    int StartOfDayCount,
    int Entries,
    int Exits,
    int Net,
    int EndOfDayCount
);
