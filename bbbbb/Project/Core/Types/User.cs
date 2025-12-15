#pragma warning disable CS8618

namespace bbbbb.Project.Core.Types;
/// <summary>
/// Сущность - пользователь, общающийся с ботом
/// </summary>
public class User
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public long Id { get; init; }
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string? Username { get; init; }
}
