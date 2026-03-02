using AutoRetainer.UI;
using Dalamud.Bindings.ImGui;

namespace AutoRetainer;

using RawImGui = Dalamud.Bindings.ImGui.ImGui;

public static class ImGui
{
    public static bool BeginTable(string strId, int columnsCount, ImGuiTableFlags flags = ImGuiTableFlags.None, Vector2 outerSize = default, float innerWidth = 0f)
        => RawImGui.BeginTable(strId, columnsCount, flags, outerSize, innerWidth);

    public static void EndTable()
        => RawImGui.EndTable();

    public static void TableNextColumn()
        => RawImGui.TableNextColumn();

    public static void TableNextRow(ImGuiTableRowFlags rowFlags = ImGuiTableRowFlags.None, float minRowHeight = 0f)
        => RawImGui.TableNextRow(rowFlags, minRowHeight);

    public static void TableSetupColumn(string label, ImGuiTableColumnFlags flags = ImGuiTableColumnFlags.None, float initWidthOrWeight = 0f, uint userId = 0)
        => RawImGui.TableSetupColumn(L10n.Tr(label), flags, initWidthOrWeight, userId);

    public static void TableHeadersRow()
        => RawImGui.TableHeadersRow();

    public static void TableSetupScrollFreeze(int cols, int rows)
        => RawImGui.TableSetupScrollFreeze(cols, rows);

    public static void TableSetBgColor(ImGuiTableBgTarget target, uint color, int columnN = -1)
        => RawImGui.TableSetBgColor(target, color, columnN);

    public static bool Button(string label, Vector2 size = default)
        => RawImGui.Button(L10n.Tr(label), size);

    public static bool SmallButton(string label)
        => RawImGui.SmallButton(L10n.Tr(label));

    public static bool Checkbox(string label, ref bool value)
        => RawImGui.Checkbox(L10n.Tr(label), ref value);

    public static bool RadioButton(string label, bool active)
        => RawImGui.RadioButton(L10n.Tr(label), active);

    public static bool Selectable(string label, bool selected = false, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None, Vector2 size = default)
        => RawImGui.Selectable(L10n.Tr(label), selected, flags, size);

    public static bool CollapsingHeader(string label, ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None)
        => RawImGui.CollapsingHeader(L10n.Tr(label), flags);

    public static bool CollapsingHeader(string label, ref bool pVisible, ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None)
        => RawImGui.CollapsingHeader(L10n.Tr(label), ref pVisible, flags);

    public static bool BeginCombo(string label, string? previewValue, ImGuiComboFlags flags = ImGuiComboFlags.None)
        => RawImGui.BeginCombo(L10n.Tr(label), previewValue is null ? null : L10n.Tr(previewValue), flags);

    public static void EndCombo()
        => RawImGui.EndCombo();

    public static bool InputText(string label, ref string input, int maxLength, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        => RawImGui.InputText(L10n.Tr(label), ref input, maxLength, flags, (RawImGui.ImGuiInputTextCallbackDelegate?)null);

    public static bool InputTextWithHint(string label, string hint, ref string input, int maxLength, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        => RawImGui.InputTextWithHint(L10n.Tr(label), L10n.Tr(hint), ref input, maxLength, flags, (RawImGui.ImGuiInputTextCallbackDelegate?)null);

    public static bool InputInt(string label, ref int value, int step = 1, int stepFast = 100, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        => RawImGui.InputInt(L10n.Tr(label), ref value, step, stepFast, "%d", flags);

    public static bool InputFloat(string label, ref float value, float step = 0f, float stepFast = 0f, string? format = null, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        => RawImGui.InputFloat(L10n.Tr(label), ref value, step, stepFast, format, flags);

    public static bool DragInt(string label, ref int value, float speed = 1f, int min = 0, int max = 0, string? format = "%d", ImGuiSliderFlags flags = ImGuiSliderFlags.None)
        => RawImGui.DragInt(L10n.Tr(label), ref value, speed, min, max, format, flags);

    public static void Text(string text)
        => RawImGui.Text(L10n.Tr(text));

    public static void SetTooltip(string text)
        => RawImGui.SetTooltip(L10n.Tr(text));

    public static void ProgressBar(float fraction, Vector2 sizeArg = default, string? overlay = null)
        => RawImGui.ProgressBar(fraction, sizeArg, overlay is null ? default : L10n.Tr(overlay));

    public static bool BeginPopup(string strId, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
        => RawImGui.BeginPopup(strId, flags);

    public static void OpenPopup(string strId, ImGuiPopupFlags popupFlags = ImGuiPopupFlags.None)
        => RawImGui.OpenPopup(strId, popupFlags);

    public static void CloseCurrentPopup()
        => RawImGui.CloseCurrentPopup();

    public static void EndPopup()
        => RawImGui.EndPopup();

    public static bool BeginChild(string strId, Vector2 size = default, bool border = false, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
        => RawImGui.BeginChild(strId, size, border, flags);

    public static void EndChild()
        => RawImGui.EndChild();

    public static void SameLine(float offsetFromStartX = 0f, float spacing = -1f)
        => RawImGui.SameLine(offsetFromStartX, spacing);

    public static void Separator()
        => RawImGui.Separator();

    public static void Dummy(Vector2 size)
        => RawImGui.Dummy(size);

    public static void NewLine()
        => RawImGui.NewLine();

    public static void Indent(float indentW = 0f)
        => RawImGui.Indent(indentW);

    public static void Unindent(float indentW = 0f)
        => RawImGui.Unindent(indentW);

    public static void SetNextItemWidth(float itemWidth)
        => RawImGui.SetNextItemWidth(itemWidth);

    public static void SetNextWindowSize(Vector2 size, ImGuiCond cond = ImGuiCond.None)
        => RawImGui.SetNextWindowSize(size, cond);

    public static void SetNextItemOpen(bool isOpen, ImGuiCond cond = ImGuiCond.None)
        => RawImGui.SetNextItemOpen(isOpen, cond);

    public static void SetColumnWidth(int columnIndex, float width)
        => RawImGui.SetColumnWidth(columnIndex, width);

    public static void Columns(int count = 1, string? id = null, bool border = true)
        => RawImGui.Columns(count, id, border);

    public static void NextColumn()
        => RawImGui.NextColumn();

    public static void SetCursorPos(Vector2 localPos)
        => RawImGui.SetCursorPos(localPos);

    public static Vector2 GetCursorPos()
        => RawImGui.GetCursorPos();

    public static float GetCursorPosX()
        => RawImGui.GetCursorPosX();

    public static void SetCursorPosX(float localX)
        => RawImGui.SetCursorPosX(localX);

    public static Vector2 GetContentRegionAvail()
        => RawImGui.GetContentRegionAvail();

    public static Vector2 GetContentRegionMax()
        => RawImGui.GetContentRegionMax();

    public static Vector2 GetWindowPos()
        => RawImGui.GetWindowPos();

    public static Vector2 GetWindowSize()
        => RawImGui.GetWindowSize();

    public static float GetFrameHeight()
        => RawImGui.GetFrameHeight();

    public static float GetFontSize()
        => RawImGui.GetFontSize();

    public static float GetTextLineHeight()
        => RawImGui.GetTextLineHeight();

    public static int GetFrameCount()
        => RawImGui.GetFrameCount();

    public static bool IsItemHovered(ImGuiHoveredFlags flags = ImGuiHoveredFlags.None)
        => RawImGui.IsItemHovered(flags);

    public static bool IsItemClicked(ImGuiMouseButton mouseButton = ImGuiMouseButton.Left)
        => RawImGui.IsItemClicked(mouseButton);

    public static void SetMouseCursor(ImGuiMouseCursor cursorType)
        => RawImGui.SetMouseCursor(cursorType);

    public static void BeginDisabled(bool disabled = true)
        => RawImGui.BeginDisabled(disabled);

    public static void EndDisabled()
        => RawImGui.EndDisabled();

    public static void SetWindowFontScale(float scale)
        => RawImGui.SetWindowFontScale(scale);

    public static void SetScrollHereY(float centerYRatio = 0.5f)
        => RawImGui.SetScrollHereY(centerYRatio);

    public static void BeginTooltip()
        => RawImGui.BeginTooltip();

    public static void EndTooltip()
        => RawImGui.EndTooltip();

    public static void TextWrapped(string text)
        => RawImGui.TextWrapped(L10n.Tr(text));

    public static Vector2 CalcTextSize(string text, bool hideTextAfterDoubleHash = false, float wrapWidth = -1f)
        => RawImGui.CalcTextSize(L10n.Tr(text), hideTextAfterDoubleHash, wrapWidth);

    public static void SetClipboardText(string text)
        => RawImGui.SetClipboardText(text);

    public static ImGuiIOPtr GetIO()
        => RawImGui.GetIO();

    public static ImGuiStylePtr GetStyle()
        => RawImGui.GetStyle();

    public static ImGuiViewportPtr GetWindowViewport()
        => RawImGui.GetWindowViewport();

    public static void PushID(string strId)
        => RawImGui.PushID(strId);

    public static void PushID(int id)
        => RawImGui.PushID(id);

    public static void PopID()
        => RawImGui.PopID();

    public static void PushFont(ImFontPtr font)
        => RawImGui.PushFont(font);

    public static void PushFont(ImFontPtr? font)
        => RawImGui.PushFont(font ?? default);

    public static void PopFont()
        => RawImGui.PopFont();

    public static void PushStyleColor(ImGuiCol idx, uint col)
        => RawImGui.PushStyleColor(idx, col);

    public static void PushStyleColor(ImGuiCol idx, Vector4 col)
        => RawImGui.PushStyleColor(idx, col);

    public static void PopStyleColor(int count = 1)
        => RawImGui.PopStyleColor(count);

    public static void PushStyleVar(ImGuiStyleVar idx, float value)
        => RawImGui.PushStyleVar(idx, value);

    public static void PushStyleVar(ImGuiStyleVar idx, Vector2 value)
        => RawImGui.PushStyleVar(idx, value);

    public static void PopStyleVar(int count = 1)
        => RawImGui.PopStyleVar(count);

    public static void Image(ImTextureID userTextureId, Vector2 imageSize)
        => RawImGui.Image(userTextureId, imageSize);

    public static void Image(ImTextureID userTextureId, Vector2 imageSize, Vector2 uv0, Vector2 uv1, Vector4 tintCol, Vector4 borderCol)
        => RawImGui.Image(userTextureId, imageSize, uv0, uv1, tintCol, borderCol);

    public static void Image(nint userTextureId, Vector2 imageSize)
        => RawImGui.Image(new ImTextureID(userTextureId), imageSize);
}
