namespace MedInsight.Application.Common;

/// <summary>Kaynak bazlı yetki ihlali — API katmanında 403'e eşlenir.</summary>
public sealed class ForbiddenAccessException(string message = "Bu kaynağa erişim yetkiniz yok.") : Exception(message);
