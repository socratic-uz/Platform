using System;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Interfaces.Hardware;

/// <summary>
/// UI-провайдер для абстрагирования аппаратного компонента видеопотока камеры без жесткой связности с UI-библиотекой/Shared.
/// </summary>
public interface ICameraStreamerUiProvider
{
    /// <summary>
    /// Тип Blazor-компонента CameraStreamer.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type? CameraStreamerComponentType { get; }
}
