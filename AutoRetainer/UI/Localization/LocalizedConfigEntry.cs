using System.Reflection;
using NightmareUI.OtterGuiWrapper.FileSystems.Configuration;
using NightmareUI.PrimaryUI;

namespace AutoRetainer.UI.Localization;

internal sealed class LocalizedConfigEntry(ConfigFileSystemEntry inner) : ConfigFileSystemEntry
{
    private static readonly FieldInfo? FilterField = typeof(ConfigFileSystemEntry).GetField("Filter", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    private readonly ConfigFileSystemEntry _inner = inner;

    public override NuiBuilder? Builder
    {
        get => _inner.Builder;
        init { }
    }

    public override int DisplayPriority
    {
        get => _inner.DisplayPriority;
        init { }
    }

    public override bool NoFrame
    {
        get => _inner.NoFrame;
        set => _inner.NoFrame = value;
    }

    public override string Path => L10n.TrMenuPath(_inner.Path);

    public override bool ShouldDisplay() => _inner.ShouldDisplay();

    public override bool ShouldHighlight() => _inner.ShouldHighlight();

    public override void Draw()
    {
        Func<string>? filterGetter = null;

        if (FilterField != null)
        {
            var filter = FilterField.GetValue(this);
            if (filter is Func<string> getter)
            {
                filterGetter = getter;
            }
            FilterField.SetValue(_inner, filter);
        }

        var builder = _inner.Builder;
        if (builder != null)
        {
            if (filterGetter != null)
            {
                builder.Filter = filterGetter();
            }

            NuiBuilderL10n.LocalizeBuilder(builder);
            builder.Draw();
            return;
        }

        _inner.Draw();
    }
}
