using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TelegramAi.Infrastructure.Persistence.Converters;

public class DateTimeOffsetConverter() : ValueConverter<DateTimeOffset, DateTimeOffset>(
    static x => x.ToUniversalTime(),
    static x => x.ToUniversalTime());
    