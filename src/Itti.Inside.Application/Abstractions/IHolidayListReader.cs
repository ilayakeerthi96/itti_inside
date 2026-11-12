using Itti.Inside.Domain.Content;

namespace Itti.Inside.Application.Abstractions;

/// <summary>Reads the holidays out of an uploaded holiday list, such as the HR team's Word file.</summary>
public interface IHolidayListReader
{
    /// <summary>The holidays in the file's first table that has a date column. Empty when there is none.</summary>
    IReadOnlyList<Holiday> Read(Stream wordDocument);
}
