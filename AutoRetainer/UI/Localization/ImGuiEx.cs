using AutoRetainer.UI;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using ECommons.ImGuiMethods;
using System.Runtime.CompilerServices;

namespace AutoRetainer;

using RawImGuiEx = ECommons.ImGuiMethods.ImGuiEx;
using RawRequiredPluginInfo = ECommons.ImGuiMethods.ImGuiEx.RequiredPluginInfo;
using RawEzTableEntry = ECommons.ImGuiMethods.ImGuiEx.EzTableEntry;
using RawImGui = Dalamud.Bindings.ImGui.ImGui;

public static class ImGuiEx
{
    public readonly record struct RequiredPluginInfo
    {
        internal RawRequiredPluginInfo Raw => MinVersion switch
        {
            null when VanityName == null => new RawRequiredPluginInfo(InternalName),
            null => new RawRequiredPluginInfo(InternalName, VanityName),
            _ when VanityName == null => new RawRequiredPluginInfo(InternalName, MinVersion),
            _ => new RawRequiredPluginInfo(InternalName, VanityName, MinVersion),
        };

        public string InternalName { get; }
        public string? VanityName { get; }
        public Version? MinVersion { get; }

        public RequiredPluginInfo(string internalName)
        {
            InternalName = internalName;
            VanityName = null;
            MinVersion = null;
        }

        public RequiredPluginInfo(string internalName, string vanityName)
        {
            InternalName = internalName;
            VanityName = vanityName;
            MinVersion = null;
        }

        public RequiredPluginInfo(string internalName, Version minVersion)
        {
            InternalName = internalName;
            VanityName = null;
            MinVersion = minVersion;
        }

        public RequiredPluginInfo(string internalName, string vanityName, Version minVersion)
        {
            InternalName = internalName;
            VanityName = vanityName;
            MinVersion = minVersion;
        }
    }

    public struct EzTableEntry
    {
        internal RawEzTableEntry Raw;

        public EzTableEntry(string columnName, ImGuiTableColumnFlags? columnFlags, Action @delegate)
        {
            Raw = new RawEzTableEntry(columnName, columnFlags, @delegate);
        }

        public EzTableEntry(string columnName, Action @delegate)
        {
            Raw = new RawEzTableEntry(columnName, @delegate);
        }

        public EzTableEntry(string columnName, bool stretch, Action @delegate)
        {
            Raw = new RawEzTableEntry(columnName, stretch, @delegate);
        }
    }

    public sealed class RealtimeDragDrop<T>
    {
        private readonly RawImGuiEx.RealtimeDragDrop<T> _inner;

        public RealtimeDragDrop(string dragDropId, Func<T, string> getUniqueId, bool smallButton = false)
        {
            _inner = new RawImGuiEx.RealtimeDragDrop<T>(dragDropId, getUniqueId, smallButton);
        }

        public uint HighlightColor => _inner.HighlightColor;

        public void Begin() => _inner.Begin();
        public void NextRow() => _inner.NextRow();
        public void DrawButtonDummy(T item, IList<T> list, int targetPosition) => _inner.DrawButtonDummy(item, list, targetPosition);
        public void DrawButtonDummy(string uniqueId, IList<T> list, int targetPosition) => _inner.DrawButtonDummy(uniqueId, list, targetPosition);
        public void DrawButtonDummy(string uniqueId, Action<string> onAcceptDragDropPayload) => _inner.DrawButtonDummy(uniqueId, onAcceptDragDropPayload);
        public bool SetRowColor(string uniqueId, bool setColor = true) => _inner.SetRowColor(uniqueId, setColor);
        public bool SetRowColor(T element, bool setColor = true) => _inner.SetRowColor(element, setColor);
        public bool AcceptPayload(out string? uniqueId, ImGuiDragDropFlags flags = ImGuiDragDropFlags.None) => _inner.AcceptPayload(out uniqueId, flags);
        public void End(int numRows = 1) => _inner.End(numRows);
    }

    public static bool Ctrl => RawImGuiEx.Ctrl;
    public static bool Shift => RawImGuiEx.Shift;

    public static bool Button(string label, bool enabled = true)
        => RawImGuiEx.Button(L10n.Tr(label), enabled);

    public static bool Button(string label, Vector2 size, bool enabled = true)
        => RawImGuiEx.Button(L10n.Tr(label), size, enabled);

    public static bool SmallButton(string label, bool enabled = true)
        => RawImGuiEx.SmallButton(L10n.Tr(label), enabled);

    public static bool ButtonCtrl(string text, string affix = " (Hold CTRL)")
        => RawImGuiEx.ButtonCtrl(L10n.Tr(text), L10n.Tr(affix));

    public static bool ButtonCtrl(string text, Vector2? size, string affix = " (Hold CTRL)")
        => RawImGuiEx.ButtonCtrl(L10n.Tr(text), size, L10n.Tr(affix));

    public static bool ButtonCheckbox(string name, ref bool value, bool smallButton)
        => RawImGuiEx.ButtonCheckbox(L10n.Tr(name), ref value, smallButton);

    public static bool ButtonCheckbox(string name, ref bool value, uint color, bool smallButton = false)
        => RawImGuiEx.ButtonCheckbox(L10n.Tr(name), ref value, color, smallButton);

    public static bool ButtonCheckbox(string name, ref bool value, Vector4 color, bool smallButton = false)
        => RawImGuiEx.ButtonCheckbox(L10n.Tr(name), ref value, color, smallButton);

    public static bool ButtonCheckbox(string name, ref bool value, Vector4? color = null, bool inverted = false, Vector2? size = null, bool noColor = false)
        => RawImGuiEx.ButtonCheckbox(L10n.Tr(name), ref value, color, inverted, size, noColor);

    public static bool ButtonCheckbox(FontAwesomeIcon icon, ref bool value, Vector4? color = null, bool inverted = false, Vector2? size = null)
        => RawImGuiEx.ButtonCheckbox(icon, ref value, color, inverted, size);

    public static bool CollectionButtonCheckbox<T>(string name, T value, ICollection<T> collection, bool smallButton = false, bool inverted = false)
        => RawImGuiEx.CollectionButtonCheckbox(L10n.Tr(name), value, collection, smallButton, inverted);

    public static bool CollectionButtonCheckbox<T>(string name, T value, ICollection<T> collection, Vector4 color, bool smallButton = false, bool inverted = false)
        => RawImGuiEx.CollectionButtonCheckbox(L10n.Tr(name), value, collection, color, smallButton, inverted);

    public static bool CollectionButtonCheckbox<T>(FontAwesomeIcon icon, T value, ICollection<T> collection, bool smallButton = false, bool inverted = false)
        => RawImGuiEx.CollectionButtonCheckbox(icon, value, collection, smallButton, inverted);

    public static bool CollectionButtonCheckbox<T>(FontAwesomeIcon icon, T value, ICollection<T> collection, Vector4 color, bool smallButton = false, bool inverted = false)
        => RawImGuiEx.CollectionButtonCheckbox(icon, value, collection, color, smallButton, inverted);

    public static bool IconButton(FontAwesomeIcon icon, string id = "ECommonsButton", Vector2 size = default, bool enabled = true)
        => RawImGuiEx.IconButton(icon, id, size, enabled);

    public static bool IconButton(string icon, string id = "ECommonsButton", Vector2 size = default, bool enabled = true)
        => RawImGuiEx.IconButton(icon, id, size, enabled);

    public static bool IconButtonWithText(FontAwesomeIcon icon, string id, bool enabled = true)
        => RawImGuiEx.IconButtonWithText(icon, L10n.Tr(id), enabled);

    public static bool Checkbox(string label, ref int value)
        => RawImGuiEx.Checkbox(L10n.Tr(label), ref value);

    public static bool Checkbox(string label, ref bool value, bool enabled = true)
        => RawImGuiEx.Checkbox(L10n.Tr(label), ref value, enabled);

    public static bool Checkbox(string label, ref bool? value)
        => RawImGuiEx.Checkbox(L10n.Tr(label), ref value);

    public static bool Checkbox(string label, ref bool? value, bool enabled, bool inverted)
        => RawImGuiEx.Checkbox(L10n.Tr(label), ref value, enabled, inverted);

    public static bool CollectionCheckbox<T>(string name, T value, ICollection<T> collection, bool inverted = false)
        => RawImGuiEx.CollectionCheckbox(L10n.Tr(name), value, collection, inverted);

    public static bool CollectionCheckbox<T>(string label, IEnumerable<T> values, ICollection<T> collection, bool inverted = false, bool delayedOperation = false)
        => RawImGuiEx.CollectionCheckbox(L10n.Tr(label), values, collection, inverted, delayedOperation);

    public static bool FilteringInputTextWithHint(string label, string hint, out string result, int maxLength = 200, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        => RawImGuiEx.FilteringInputTextWithHint(L10n.Tr(label), L10n.Tr(hint), out result, maxLength, flags);

    public static bool FilteringInputInt(string label, out int result, int step = 1, int stepFast = 100, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
        => RawImGuiEx.FilteringInputInt(L10n.Tr(label), out result, step, stepFast, flags);

    public static bool FilteringCheckbox(string label, out bool result)
        => RawImGuiEx.FilteringCheckbox(L10n.Tr(label), out result);

    public static bool SliderInt(string label, ref int value, int min = 0, int max = 0, string? format = null, ImGuiSliderFlags flags = ImGuiSliderFlags.None)
        => RawImGuiEx.SliderInt(L10n.Tr(label), ref value, min, max, format, flags);

    public static bool SliderFloat(string label, ref float value, float min, float max, string format, ImGuiSliderFlags flags)
        => RawImGuiEx.SliderFloat(L10n.Tr(label), ref value, min, max, format, flags);

    public static bool SliderFloat(string label, ref float value, float min, float max, string format)
        => RawImGuiEx.SliderFloat(L10n.Tr(label), ref value, min, max, format);

    public static bool SliderFloat(string label, ref float value, float min, float max)
        => RawImGuiEx.SliderFloat(L10n.Tr(label), ref value, min, max);

    public static bool SliderIntAsFloat(string id, ref int value, int min, int max, float divider = 1000)
        => RawImGuiEx.SliderIntAsFloat(id, ref value, min, max, divider);

    public static void InputUint(string name, ref uint uInt)
        => RawImGuiEx.InputUint(L10n.Tr(name), ref uInt);

    public static bool Combo<T>(string name, ref T refConfigField, IEnumerable<T> values, Func<T, bool>? filter = null, Dictionary<T, string>? names = null)
    {
        var changed = false;
        var translatedName = L10n.Tr(name);
        var translatedNames = TranslateDictionary<T>(names);
        var previewFallback = refConfigField is null ? string.Empty : refConfigField.ToString() ?? string.Empty;
        var preview = translatedNames != null && translatedNames.TryGetValue(refConfigField, out var previewName)
            ? previewName
            : L10n.Tr(previewFallback);

        if (!RawImGui.BeginCombo(translatedName, preview, ImGuiComboFlags.HeightLarge))
            return false;

        string? filterText = null;
        var valuesList = values as IList<T> ?? values.ToList();
        if (valuesList.Count > 10)
        {
            if (!ComboSearch.TryGetValue(name, out filterText))
                filterText = string.Empty;

            SetNextItemFullWidth();
            ImGui.InputTextWithHint($"##{name}fltr", "Filter...", ref filterText, 50);
            ComboSearch[name] = filterText;
        }

        foreach (var value in valuesList)
        {
            var equals = EqualityComparer<T>.Default.Equals(value, refConfigField);
            var elementFallback = value is null ? string.Empty : value.ToString() ?? string.Empty;
            var element = translatedNames != null && translatedNames.TryGetValue(value, out var optionName)
                ? optionName
                : L10n.Tr(elementFallback);

            if ((filter == null || filter(value))
                && (filterText == null || element.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                && ImGui.Selectable(element, equals))
            {
                changed = true;
                refConfigField = value;
            }

            if (RawImGui.IsWindowAppearing() && equals)
                ImGui.SetScrollHereY();
        }

        RawImGui.EndCombo();
        return changed;
    }

    public static bool EnumCombo<T>(string name, ref T refConfigField, IDictionary<T, string> names) where T : Enum, IConvertible
        => EnumCombo(name, ref refConfigField, null, names);

    public static bool EnumCombo<T>(string name, ref T refConfigField, Func<T, bool>? filter = null, IDictionary<T, string>? names = null) where T : Enum, IConvertible
    {
        var changed = false;
        var translatedNames = TranslateDictionary<T>(names);
        var preview = translatedNames != null && translatedNames.TryGetValue(refConfigField, out var previewName)
            ? previewName
            : L10n.Tr(refConfigField.ToString().Replace("_", " "));

        if (!RawImGui.BeginCombo(L10n.Tr(name), preview, ImGuiComboFlags.HeightLarge))
            return false;

        string? filterText = null;
        var values = Enum.GetValues(typeof(T));
        if (values.Length > 10)
        {
            if (!EnumComboSearch.TryGetValue(name, out filterText))
                filterText = string.Empty;

            SetNextItemFullWidth();
            ImGui.InputTextWithHint($"##{name.Replace("#", "_")}", "Filter...", ref filterText, 50);
            EnumComboSearch[name] = filterText;
        }

        foreach (var raw in values)
        {
            var value = (T)raw;
            var equals = EqualityComparer<T>.Default.Equals(value, refConfigField);
            var element = translatedNames != null && translatedNames.TryGetValue(value, out var translatedOption)
                ? translatedOption
                : L10n.Tr(raw.ToString().Replace("_", " "));

            if ((filter == null || filter(value))
                && (filterText == null || element.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                && ImGui.Selectable(element, equals))
            {
                changed = true;
                refConfigField = value;
            }

            if (RawImGui.IsWindowAppearing() && equals)
                ImGui.SetScrollHereY();
        }

        RawImGui.EndCombo();
        return changed;
    }

    public static bool EnumCombo<T>(string name, ref T? refConfigField, Func<T, bool>? filter = null, IDictionary<T, string>? names = null, string nullName = "Not selected") where T : struct, Enum, IConvertible
    {
        var changed = false;
        var translatedNames = TranslateDictionary<T>(names);
        var preview = refConfigField == null
            ? L10n.Tr(nullName)
            : translatedNames != null && translatedNames.TryGetValue(refConfigField.Value, out var previewName)
                ? previewName
                : L10n.Tr(refConfigField.Value.ToString().Replace("_", " "));

        if (!RawImGui.BeginCombo(L10n.Tr(name), preview, ImGuiComboFlags.HeightLarge))
            return false;

        string? filterText = null;
        var values = Enum.GetValues(typeof(T));
        if (values.Length > 10)
        {
            if (!EnumComboSearch.TryGetValue(name, out filterText))
                filterText = string.Empty;

            SetNextItemFullWidth();
            ImGui.InputTextWithHint($"##{name.Replace("#", "_")}", "Filter...", ref filterText, 50);
            EnumComboSearch[name] = filterText;
        }

        if (ImGui.Selectable(L10n.Tr(nullName), refConfigField == null))
        {
            changed = true;
            refConfigField = null;
        }

        foreach (var raw in values)
        {
            var value = (T)raw;
            var equals = EqualityComparer<T?>.Default.Equals(value, refConfigField);
            var element = translatedNames != null && translatedNames.TryGetValue(value, out var translatedOption)
                ? translatedOption
                : L10n.Tr(raw.ToString().Replace("_", " "));

            if ((filter == null || filter(value))
                && (filterText == null || element.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                && ImGui.Selectable(element, equals))
            {
                changed = true;
                refConfigField = value;
            }

            if (RawImGui.IsWindowAppearing() && equals)
                ImGui.SetScrollHereY();
        }

        RawImGui.EndCombo();
        return changed;
    }

    public static void RadioButtonBool(string labelTrue, string labelFalse, ref bool value, bool sameLine = false, Action? prefix = null, Action? suffix = null, bool inverted = false)
        => RawImGuiEx.RadioButtonBool(L10n.Tr(labelTrue), L10n.Tr(labelFalse), ref value, sameLine, prefix, suffix, inverted);

    public static bool Selectable(string id, bool enabled = true, bool selected = false, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None, Vector2 size = default)
        => RawImGuiEx.Selectable(L10n.Tr(id), enabled, selected, flags, size);

    public static bool Selectable(Vector4? color, string id, bool enabled = true, bool selected = false, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None, Vector2 size = default)
        => RawImGuiEx.Selectable(color, L10n.Tr(id), enabled, selected, flags, size);

    public static bool CollapsingHeader(string text)
        => RawImGuiEx.CollapsingHeader(L10n.Tr(text));

    public static bool CollapsingHeader(string text, Vector4? col)
        => RawImGuiEx.CollapsingHeader(L10n.Tr(text), col);

    public static bool CollapsingHeader(Vector4? col, string text)
        => RawImGuiEx.CollapsingHeader(col, L10n.Tr(text));

    public static void TreeNodeCollapsingHeader(string name, Action action, ImGuiTreeNodeFlags extraFlags = ImGuiTreeNodeFlags.None)
        => RawImGuiEx.TreeNodeCollapsingHeader(L10n.Tr(name), action, extraFlags);

    public static void TreeNodeCollapsingHeader(string name, bool usePadding, Action action, ImGuiTreeNodeFlags extraFlags = ImGuiTreeNodeFlags.None)
        => RawImGuiEx.TreeNodeCollapsingHeader(L10n.Tr(name), usePadding, action, extraFlags);

    public static bool BeginDefaultTable(string[] headers, bool drawHeader = true, ImGuiTableFlags extraFlags = ImGuiTableFlags.None)
        => RawImGuiEx.BeginDefaultTable(TranslateHeaders(headers), drawHeader, extraFlags);

    public static bool BeginDefaultTable(string id, string[] headers, bool drawHeader = true, ImGuiTableFlags extraFlags = ImGuiTableFlags.None, bool flagsOverride = false)
        => RawImGuiEx.BeginDefaultTable(id, TranslateHeaders(headers), drawHeader, extraFlags, flagsOverride);

    public static void EzTable(IEnumerable<EzTableEntry> entries)
        => RawImGuiEx.EzTable(entries.Select(x => x.Raw));

    public static void EzTable(string? id, IEnumerable<EzTableEntry> entries)
        => RawImGuiEx.EzTable(id, entries.Select(x => x.Raw));

    public static void EzTable(ImGuiTableFlags? tableFlags, IEnumerable<EzTableEntry> entries)
        => RawImGuiEx.EzTable(tableFlags, entries.Select(x => x.Raw));

    public static void EzTable(string? id, ImGuiTableFlags? tableFlags, IEnumerable<EzTableEntry> entries, bool header)
        => RawImGuiEx.EzTable(id, tableFlags, entries.Select(x => x.Raw), header);

    public static void EzTableColumns(string id, Action[] values, int? columns = null, ImGuiTableFlags extraFlags = ImGuiTableFlags.None)
        => RawImGuiEx.EzTableColumns(id, values, columns, extraFlags);

    public static void EzTabBar(string id, params (string name, Action function, Vector4? color, bool child)[] tabs)
        => RawImGuiEx.EzTabBar(id, TranslateTabs(tabs));

    public static void EzTabBar(string id, string koFiTransparent, params (string name, Action function, Vector4? color, bool child)[] tabs)
        => RawImGuiEx.EzTabBar(id, koFiTransparent, TranslateTabs(tabs));

    public static void EzTabBar(string id, string koFiTransparent, string openTabName, params (string name, Action function, Vector4? color, bool child)[] tabs)
        => RawImGuiEx.EzTabBar(id, koFiTransparent, L10n.Tr(openTabName), TranslateTabs(tabs));

    public static void EzTabBar(string id, string koFiTransparent, string openTabName, ImGuiTabBarFlags flags, params (string name, Action function, Vector4? color, bool child)[] tabs)
        => RawImGuiEx.EzTabBar(id, koFiTransparent, L10n.Tr(openTabName), flags, TranslateTabs(tabs));

    public static void Text(string s)
        => RawImGuiEx.Text(L10n.Tr(s));

    public static void Text(ImFontPtr font, string s)
        => RawImGuiEx.Text(font, L10n.Tr(s));

    public static void Text(Vector4 col, string s)
        => RawImGuiEx.Text(col, L10n.Tr(s));

    public static void Text(Vector4? col, string text)
        => RawImGuiEx.Text(col, L10n.Tr(text));

    public static void Text(Vector4? col, ImFontPtr? font, string s)
        => RawImGuiEx.Text(col, font, L10n.Tr(s));

    public static void Text(uint col, string s)
        => RawImGuiEx.Text(col, L10n.Tr(s));

    public static void Text(uint col, ImFontPtr font, string s)
        => RawImGuiEx.Text(col, font, L10n.Tr(s));

    public static void Text(EzColor col, string s)
        => RawImGuiEx.Text(col, L10n.Tr(s));

    public static void TextV(Vector4? col, string s)
        => RawImGuiEx.TextV(col, L10n.Tr(s));

    public static void TextV(string s)
        => RawImGuiEx.TextV(L10n.Tr(s));

    public static void TextWrapped(string s)
        => RawImGuiEx.TextWrapped(L10n.Tr(s));

    public static void TextWrapped(Vector4? col, string s)
        => RawImGuiEx.TextWrapped(col, L10n.Tr(s));

    public static void TextWrapped(uint col, string s)
        => RawImGuiEx.TextWrapped(col, L10n.Tr(s));

    public static void TextCopy(Vector4 col, string text, string? copyText = null)
        => RawImGuiEx.TextCopy(col, L10n.Tr(text), copyText);

    public static void TextCopy(string displayText, string? copyText = null)
        => RawImGuiEx.TextCopy(L10n.Tr(displayText), copyText);

    public static void TextCentered(string text)
        => RawImGuiEx.TextCentered(L10n.Tr(text));

    public static void TextCentered(Vector4 col, string text)
        => RawImGuiEx.TextCentered(col, L10n.Tr(text));

    public static void TextCentered(Vector4? col, string text)
        => RawImGuiEx.TextCentered(col, L10n.Tr(text));

    public static void Tooltip(string s)
        => RawImGuiEx.Tooltip(L10n.Tr(s));

    public static void HelpMarker(string helpText, Vector4? color = null, string? symbolOverride = null, bool sameLine = true, bool preserveCursor = false)
        => RawImGuiEx.HelpMarker(L10n.Tr(helpText), color, symbolOverride, sameLine, preserveCursor);

    public static bool HoveredAndClicked(string? tooltip = null, ImGuiMouseButton btn = ImGuiMouseButton.Left, bool requireCtrl = false)
        => RawImGuiEx.HoveredAndClicked(tooltip is null ? null : L10n.Tr(tooltip), btn, requireCtrl);

    public static void LineCentered(Action func)
        => RawImGuiEx.LineCentered(func);

    public static void LineCentered(string id, Action func)
        => RawImGuiEx.LineCentered(id, func);

    [Obsolete("Please switch to LineCentered")]
    public static void ImGuiLineCentered(string id, Action func)
        => RawImGuiEx.ImGuiLineCentered(id, func);

    public static void InputWithRightButtonsArea(Action inputAction, Action rightAction)
        => RawImGuiEx.InputWithRightButtonsArea(inputAction, rightAction);

    public static void InputWithRightButtonsArea(string id, Action inputAction, Action rightAction)
        => RawImGuiEx.InputWithRightButtonsArea(id, inputAction, rightAction);

    public static void SetNextItemWidthScaled(float width)
        => RawImGuiEx.SetNextItemWidthScaled(width);

    public static void SetNextItemFullWidth(int mod = 0)
        => RawImGuiEx.SetNextItemFullWidth(mod);

    public static bool BeginPopupNextToElement(string popupId)
        => RawImGuiEx.BeginPopupNextToElement(popupId);

    public static void DragDropRepopulate<T>(string dragDropIdentifier, T data, Action<T> callback) where T : struct
    {
        Tooltip("Drag this selector to other selectors to set their values to the same");
        if (RawImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceNoPreviewTooltip))
        {
            try
            {
                ImGuiDragDrop.SetDragDropPayload(dragDropIdentifier, data);
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
            }
            catch (Exception e)
            {
                e.Log();
            }
            RawImGui.EndDragDropSource();
        }

        if (RawImGui.BeginDragDropTarget())
        {
            try
            {
                if (ImGuiDragDrop.AcceptDragDropPayload(dragDropIdentifier, out T payload, ImGuiDragDropFlags.AcceptBeforeDelivery | ImGuiDragDropFlags.AcceptNoPreviewTooltip))
                    callback(payload);
            }
            catch (Exception e)
            {
                e.Log();
            }
            RawImGui.EndDragDropTarget();
        }
    }

    public static void DragDropRepopulate<T>(string dragDropIdentifier, T data, ICollection<T> dataCollection) where T : struct
        => DragDropRepopulate(dragDropIdentifier, data, payload =>
        {
            var shouldAdd = !dataCollection.Contains(payload);
            if (shouldAdd)
                dataCollection.Remove(data);
            else
                dataCollection.Add(data);
        });

    public static void DragDropRepopulate<T>(string dragDropIdentifier, T data, ref T field) where T : struct
    {
        Tooltip("Drag this selector to other selectors to set their values to the same");
        if (RawImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceNoPreviewTooltip))
        {
            try
            {
                ImGuiDragDrop.SetDragDropPayload(dragDropIdentifier, data);
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
            }
            catch (Exception e)
            {
                e.Log();
            }
            RawImGui.EndDragDropSource();
        }

        if (RawImGui.BeginDragDropTarget())
        {
            try
            {
                if (ImGuiDragDrop.AcceptDragDropPayload(dragDropIdentifier, out T payload, ImGuiDragDropFlags.AcceptBeforeDelivery | ImGuiDragDropFlags.AcceptNoPreviewTooltip))
                    field = payload;
            }
            catch (Exception e)
            {
                e.Log();
            }
            RawImGui.EndDragDropTarget();
        }
    }

    public static void DragDropRepopulateClass<T>(string dragDropIdentifier, T data, Action<T> callback) where T : class
    {
        if (!ClassDragDropIds.TryGetValue(data, out var idBox))
        {
            idBox = new(Guid.NewGuid());
            ClassDragDropIds.Add(data, idBox);
        }

        Tooltip("Drag this selector to other selectors to set their values to the same");
        if (RawImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceNoPreviewTooltip))
        {
            try
            {
                ImGuiDragDrop.SetDragDropPayload(dragDropIdentifier, idBox.Value);
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeAll);
            }
            catch (Exception e)
            {
                e.Log();
            }
            RawImGui.EndDragDropSource();
        }

        if (RawImGui.BeginDragDropTarget())
        {
            try
            {
                if (ImGuiDragDrop.AcceptDragDropPayload(dragDropIdentifier, out Guid payload, ImGuiDragDropFlags.AcceptBeforeDelivery | ImGuiDragDropFlags.AcceptNoPreviewTooltip))
                {
                    foreach (var item in ClassDragDropIds)
                    {
                        if (item.Value.Value == payload && item.Key is T typed)
                        {
                            callback(typed);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.Log();
            }
            RawImGui.EndDragDropTarget();
        }
    }

    public static void DragDropRepopulateClass<T>(string dragDropIdentifier, T data, ICollection<T> dataCollection) where T : class
        => DragDropRepopulateClass(dragDropIdentifier, data, payload =>
        {
            var shouldAdd = !dataCollection.Contains(payload);
            if (shouldAdd)
                dataCollection.Remove(data);
            else
                dataCollection.Add(data);
        });

    public static void PluginAvailabilityIndicator(IEnumerable<RequiredPluginInfo> pluginInfos, string? prependText = null, bool all = true)
    {
        var infos = pluginInfos?.ToArray() ?? [];
        prependText ??= all
            ? "The following plugins are required to be installed and enabled:"
            : "One of the following plugins is required to be installed and enabled";

        static bool IsSatisfied(RequiredPluginInfo info)
            => Svc.PluginInterface.InstalledPlugins.Any(plugin =>
                plugin.IsLoaded
                && plugin.InternalName == info.InternalName
                && (info.MinVersion == null || plugin.Version >= info.MinVersion));

        var pass = all ? infos.All(IsSatisfied) : infos.Any(IsSatisfied);

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        Text(pass ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed, pass ? FontAwesomeIcon.Check.ToIconString() : "\uf00d");
        ImGui.PopFont();

        if (!ImGui.IsItemHovered())
            return;

        ImGui.BeginTooltip();
        RawImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
        Text(prependText);
        RawImGui.PopTextWrapPos();

        foreach (var info in infos)
        {
            var plugin = Svc.PluginInterface.InstalledPlugins.FirstOrDefault(x => x.IsLoaded && x.InternalName == info.InternalName);
            var displayName = info.VanityName ?? info.InternalName;

            if (plugin != null)
            {
                if (info.MinVersion == null || plugin.Version >= info.MinVersion)
                {
                    Text(ImGuiColors.ParsedGreen, $"- {displayName}{(info.MinVersion == null ? string.Empty : $" {info.MinVersion}+")}");
                }
                else
                {
                    Text(ImGuiColors.ParsedGreen, $"- {displayName} ");
                    ImGui.SameLine(0, 0);
                    Text(ImGuiColors.DalamudRed, $"{info.MinVersion}+ ");
                    ImGui.SameLine(0, 0);
                    Text("(outdated)");
                }
            }
            else
            {
                Text(ImGuiColors.DalamudRed, $"- {displayName} {(info.MinVersion == null ? string.Empty : $"{info.MinVersion}+ ")}");
                ImGui.SameLine(0, 0);
                Text("(not installed)");
            }
        }

        ImGui.EndTooltip();
    }

    public static Action[] Pagination(Action[] actions, int perPage = 0, int maxPages = 0)
        => RawImGuiEx.Pagination(actions, perPage, maxPages);

    public static Action[] Pagination(string id, Action[] actions, int perPage = 0, int maxPages = 0)
        => RawImGuiEx.Pagination(id, actions, perPage, maxPages);

    public static Action[] Pagination(Action[] actions, out Action? paginator, int perPage = 0, int maxPages = 0)
        => RawImGuiEx.Pagination(actions, out paginator, perPage, maxPages);

    public static Action[] Pagination(string id, Action[] actions, out Action? paginator, int perPage = 0, int maxPages = 0)
        => RawImGuiEx.Pagination(id, actions, out paginator, perPage, maxPages);

    private static readonly Dictionary<string, string> EnumComboSearch = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> ComboSearch = new(StringComparer.Ordinal);
    private static readonly ConditionalWeakTable<object, GuidBox> ClassDragDropIds = new();
    private static readonly ConditionalWeakTable<object, object> TranslatedDictionaryCache = new();

    private sealed class GuidBox
    {
        internal Guid Value { get; }

        internal GuidBox(Guid value)
        {
            Value = value;
        }
    }

    private sealed class TranslatedDictionaryCacheEntry<T>
    {
        internal int SourceCount { get; }
        internal Dictionary<T, string> Translated { get; }

        internal TranslatedDictionaryCacheEntry(int sourceCount, Dictionary<T, string> translated)
        {
            SourceCount = sourceCount;
            Translated = translated;
        }
    }

    private static string[] TranslateHeaders(string[] headers)
    {
        var result = new string[headers.Length];
        for (var i = 0; i < headers.Length; i++)
        {
            var header = headers[i];
            if (header.StartsWith('~'))
            {
                result[i] = "~" + L10n.Tr(header[1..]);
            }
            else
            {
                result[i] = L10n.Tr(header);
            }
        }
        return result;
    }

    private static (string name, Action function, Vector4? color, bool child)[] TranslateTabs((string name, Action function, Vector4? color, bool child)[] tabs)
    {
        var result = new (string name, Action function, Vector4? color, bool child)[tabs.Length];
        for (var i = 0; i < tabs.Length; i++)
        {
            result[i] = (L10n.Tr(tabs[i].name), tabs[i].function, tabs[i].color, tabs[i].child);
        }
        return result;
    }

    private static Dictionary<T, string>? TranslateDictionary<T>(IDictionary<T, string>? names)
    {
        if (names == null)
            return null;

        if (TranslatedDictionaryCache.TryGetValue(names, out var cached)
            && cached is TranslatedDictionaryCacheEntry<T> typedCache
            && typedCache.SourceCount == names.Count)
        {
            return typedCache.Translated;
        }

        var translated = new Dictionary<T, string>(names.Count);
        foreach (var (key, value) in names)
        {
            translated[key] = L10n.Tr(value);
        }

        TranslatedDictionaryCache.Remove(names);
        TranslatedDictionaryCache.Add(names, new TranslatedDictionaryCacheEntry<T>(names.Count, translated));
        return translated;
    }
}
