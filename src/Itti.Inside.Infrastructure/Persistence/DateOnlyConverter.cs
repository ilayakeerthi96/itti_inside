using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Itti.Inside.Infrastructure.Persistence;

/// <summary>
/// The MySQL driver writes <see cref="DateOnly"/> but reads date columns back only as
/// <see cref="DateTime"/>, so dates cross that boundary as a DateTime at midnight.
/// </summary>
public sealed class DateOnlyConverter() : ValueConverter<DateOnly, DateTime>(
    date => date.ToDateTime(TimeOnly.MinValue),
    dateTime => DateOnly.FromDateTime(dateTime));
