using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NightmareUI.PrimaryUI;

namespace AutoRetainer.UI.Localization;

internal static class NuiBuilderL10n
{
    private static readonly FieldInfo? SectionsField = typeof(NuiBuilder).GetField("Sections", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? SectionNameField = FindField("NightmareUI.PrimaryUI.Components.Section", "Name");
    private static readonly FieldInfo? SectionWidgetsField = FindField("NightmareUI.PrimaryUI.Components.Section", "Widgets");
    private static readonly FieldInfo? WidgetLabelField = FindField("NightmareUI.PrimaryUI.Components.ImGuiWidget", "Label");
    private static readonly FieldInfo? WidgetHelpField = FindField("NightmareUI.PrimaryUI.Components.ImGuiWidget", "Help");
    private static readonly FieldInfo? WidgetDrawActionField = FindField("NightmareUI.PrimaryUI.Components.ImGuiWidget", "DrawAction");
    private static readonly ConditionalWeakTable<object, LocalizedMarker> LocalizedTargets = new();

    internal static void LocalizeBuilder(NuiBuilder builder)
    {
        if (builder == null || SectionsField == null)
            return;

        if (!TryGetFieldValue(builder, SectionsField, out IEnumerable sections))
            return;

        foreach (var section in sections)
        {
            if (section == null)
                continue;

            if (TryGetFieldValue(section, SectionNameField, out string name))
            {
                SectionNameField.SetValue(section, L10n.Tr(name));
            }

            if (!TryGetFieldValue(section, SectionWidgetsField, out IEnumerable widgets))
                continue;

            foreach (var widget in widgets)
            {
                if (widget == null)
                    continue;

                if (TryGetFieldValue(widget, WidgetLabelField, out string label))
                {
                    WidgetLabelField.SetValue(widget, L10n.Tr(label));
                }

                if (TryGetFieldValue(widget, WidgetHelpField, out string help))
                {
                    WidgetHelpField.SetValue(widget, L10n.Tr(help));
                }

                if (TryGetFieldValue(widget, WidgetDrawActionField, out Action<string> drawAction))
                {
                    LocalizeClosureStrings(drawAction.Target);
                }
            }
        }
    }

    private static FieldInfo? FindField(string typeName, string fieldName)
    {
        var type = typeof(NuiBuilder).Assembly.GetType(typeName, throwOnError: false);
        return type?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }

    private static bool TryGetFieldValue<T>(object target, FieldInfo? field, out T value)
    {
        value = default!;
        if (field == null || !IsFieldApplicable(target, field))
            return false;

        if (field.GetValue(target) is not T typed)
            return false;

        value = typed;
        return true;
    }

    private static bool IsFieldApplicable(object target, FieldInfo field)
        => field.DeclaringType?.IsInstanceOfType(target) == true;

    private static void LocalizeClosureStrings(object? target)
    {
        if (target == null)
            return;

        if (LocalizedTargets.TryGetValue(target, out _))
            return;

        var type = target.GetType();
        while (type != null && type != typeof(object))
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                try
                {
                    if (field.FieldType == typeof(string))
                    {
                        if (field.GetValue(target) is string value && value.Length > 0)
                        {
                            field.SetValue(target, L10n.Tr(value));
                        }
                        continue;
                    }

                    LocalizeEnumNamesDictionary(target, field);
                }
                catch
                {
                    // Skip init-only/non-settable captured fields.
                }
            }

            type = type.BaseType;
        }

        LocalizedTargets.GetValue(target, _ => new LocalizedMarker());
    }

    private static void LocalizeEnumNamesDictionary(object target, FieldInfo field)
    {
        if (!TryGetEnumDictionaryKeyType(field.FieldType, out var enumType))
            return;

        var value = field.GetValue(target);
        if (value is IDictionary dictionary)
        {
            var keys = new List<object>();
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key != null)
                    keys.Add(entry.Key);
            }

            foreach (var key in keys)
            {
                if (dictionary[key] is string text && text.Length > 0)
                {
                    dictionary[key] = L10n.Tr(text);
                }
            }
            return;
        }

        var localized = CreateEnumNamesDictionary(enumType);
        field.SetValue(target, localized);
    }

    private static bool TryGetEnumDictionaryKeyType(Type type, out Type enumType)
    {
        enumType = null!;
        if (TryGetEnumDictionaryKeyTypeCore(type, out enumType))
            return true;

        foreach (var iface in type.GetInterfaces())
        {
            if (TryGetEnumDictionaryKeyTypeCore(iface, out enumType))
                return true;
        }

        return false;
    }

    private static bool TryGetEnumDictionaryKeyTypeCore(Type type, out Type enumType)
    {
        enumType = null!;
        if (!type.IsGenericType)
            return false;

        var def = type.GetGenericTypeDefinition();
        if (def != typeof(IDictionary<,>) &&
            def != typeof(IReadOnlyDictionary<,>) &&
            def != typeof(Dictionary<,>))
            return false;

        var args = type.GetGenericArguments();
        if (args.Length != 2)
            return false;

        if (!args[0].IsEnum || args[1] != typeof(string))
            return false;

        enumType = args[0];
        return true;
    }

    private static IDictionary CreateEnumNamesDictionary(Type enumType)
    {
        var dictType = typeof(Dictionary<,>).MakeGenericType(enumType, typeof(string));
        var dictionary = (IDictionary)Activator.CreateInstance(dictType)!;
        foreach (var value in Enum.GetValues(enumType))
        {
            var label = value?.ToString()?.Replace("_", " ") ?? string.Empty;
            dictionary[value!] = L10n.Tr(label);
        }
        return dictionary;
    }

    private sealed class LocalizedMarker;
}

internal static class NuiBuilderL10nExtensions
{
    internal static NuiBuilder DrawL10n(this NuiBuilder builder, bool noCollapse = false)
    {
        NuiBuilderL10n.LocalizeBuilder(builder);
        return builder.Draw(noCollapse);
    }
}
