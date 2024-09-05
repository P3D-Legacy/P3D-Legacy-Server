using P3D.Legacy.Server.Abstractions.Configuration;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views;

public sealed class SettingsTabView : View
{
    private static string ToString(object? val) => val switch
    {
        bool b => b.ToString().ToLowerInvariant(),
        not null => val.ToString() ?? string.Empty,
        _ => string.Empty
    };

    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public SettingsTabView(DynamicConfigurationProviderManager dynamicConfigurationProvider)
    {
            Width = Dim.Fill();
            Height = Dim.Fill();

            var views = new List<View>();
            foreach (var optionsType in dynamicConfigurationProvider.GetRegisteredOptionTypes())
            {
                if (dynamicConfigurationProvider.GetProvider(optionsType) is not { } manager) continue;
                var propertyCount = manager.AvailableProperties.Count();

                if (dynamicConfigurationProvider.GetOptions(optionsType) is not { } currentOptions) continue;

                var view = new FrameView(optionsType.Name.Replace("Options", "", StringComparison.Ordinal))
                {
                    X = 0,
                    Y = views.Count == 0 ? 0 : Pos.Bottom(views.Last()),
                    Width = Dim.Fill(),
                    Height = 3 + propertyCount,
                };
                var maxLength = manager.AvailableProperties.Max(static x => x.Name.Length);
                var i = 0;
                var propertyViews = new Dictionary<PropertyInfo, TextField>();
                foreach (var property in manager.AvailableProperties)
                {
                    var value = property.GetValue(currentOptions);
                    var textView = new Label { X = 0, Y = i, Width = property.Name.Length + 2, Height = 1, Text = $"{property.Name}: " };
                    var textField = new TextField { X = maxLength + 1, Y = i, Width = Dim.Fill(), Height = 1, Text = ToString(value) };
                    view.Add(textView, textField);
                    propertyViews.Add(property, textField);
                    i++;
                }
                var button = new Button("Save", true) { X = 0, Y = i, Width = Dim.Fill(), Height = 1 };
                button.Clicked += () =>
                {
                    var faultyProps = new List<PropertyInfo>();
                    foreach (var (prop, field) in propertyViews)
                    {
                        try
                        {
                            if (prop.PropertyType == typeof(bool))
                            {
                                manager.SetProperty(prop, field.Text.ToString()?.ToLowerInvariant() ?? string.Empty);
                            }
                            else
                            {
                                manager.SetProperty(prop, field.Text.ToString() ?? string.Empty);
                            }
                        }
                        catch (Exception)
                        {
                            faultyProps.Add(prop);
                            var defaultValue = ToString(prop.GetValue(currentOptions));
                            manager.SetProperty(prop, defaultValue);
                            field.Text = defaultValue;
                        }
                    }
                    MessageBox.Query(faultyProps.Count == 0 ? "Success!" : "Warning!", faultyProps.Count == 0
                        ? "Saved successfully!"
                        : $"Some properties were not saved and were reverted to their default values - {string.Join(", ", faultyProps.Select(static x => x.Name))}", "Ok");
                };
                view.Add(button);
                views.Add(view);
            }

            Add(views.ToArray());
        }
}