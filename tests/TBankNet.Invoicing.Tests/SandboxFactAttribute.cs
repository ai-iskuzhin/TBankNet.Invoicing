namespace TBankNet.Invoicing.Tests;

/// <summary>
/// Базовый <see cref="FactAttribute"/>, который выполняется только при заданной переменной окружения.
/// Иначе тест помечается как пропущенный (Skipped), а не проваленный.
/// </summary>
/// <remarks>
/// Позволяет держать интеграционные тесты против живого сервиса в общем прогоне без сбоев в CI:
/// по умолчанию они пропускаются, а включаются явно через переменную окружения.
/// </remarks>
public abstract class RequiresEnvFactAttribute : FactAttribute
{
    /// <param name="variable">Имя переменной окружения-триггера.</param>
    /// <param name="skipReason">Причина пропуска, показываемая в отчете.</param>
    /// <param name="requireNonEmpty">
    /// Если true — переменная должна быть непустой (например, токен). Если false — достаточно
    /// «истинного» значения (1/true/yes/on).
    /// </param>
    protected RequiresEnvFactAttribute(string variable, string skipReason, bool requireNonEmpty = false)
    {
        var value = Environment.GetEnvironmentVariable(variable);
        var enabled = requireNonEmpty ? !string.IsNullOrWhiteSpace(value) : IsTruthy(value);

        if (!enabled)
        {
            Skip = skipReason;
        }
    }

    private static bool IsTruthy(string? value) =>
        value is not null &&
        (value == "1" ||
         value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
         value.Equals("on", StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Тест против живой песочницы. Включается переменной окружения <c>TBANK_SANDBOX_TESTS=1</c>.
/// </summary>
/// <remarks>
/// Токен не требуется: у песочницы есть публичный тестовый токен <c>TBankSandboxToken</c>, который
/// возвращает предопределенные ответы. При необходимости его можно переопределить переменной
/// окружения <c>TBANK_SANDBOX_TOKEN</c>.
/// </remarks>
public sealed class SandboxFactAttribute : RequiresEnvFactAttribute
{
    public SandboxFactAttribute()
        : base(
            "TBANK_SANDBOX_TESTS",
            "Живые тесты песочницы выключены. Установите TBANK_SANDBOX_TESTS=1, чтобы включить.")
    {
    }
}
