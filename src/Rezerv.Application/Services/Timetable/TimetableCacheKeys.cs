namespace Rezerv.Application.Services.Timetable;

public static class TimetableCacheKeys
{
    public static string List(int? businessId, DateOnly? date) =>
        $"timetable:business:{businessId?.ToString() ?? "all"}:date:{date?.ToString("yyyy-MM-dd") ?? "all"}";

    public static IReadOnlyList<string> AffectedBy(int businessId, DateTime startTimeUtc)
    {
        var date = DateOnly.FromDateTime(startTimeUtc);

        return
        [
            List(null, null),
            List(businessId, null),
            List(null, date),
            List(businessId, date)
        ];
    }
}