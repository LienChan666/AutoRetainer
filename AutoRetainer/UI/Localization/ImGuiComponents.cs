using AutoRetainer.UI;

namespace AutoRetainer;

using RawImGuiComponents = Dalamud.Interface.Components.ImGuiComponents;

public static class ImGuiComponents
{
    public static void HelpMarker(string text)
        => RawImGuiComponents.HelpMarker(L10n.Tr(text));
}
