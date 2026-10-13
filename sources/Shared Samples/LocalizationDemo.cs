// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information

// Linked into both sample hosts; MonoGame Sample has nullable disabled project-wide.
#nullable enable
using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Configuration;
using Icy.Localization;
using Icy.Markup;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.SharedSamples
{
    /// <summary>
    /// <c>{Localize}</c> with gettext <c>.po</c> files, and one language switch for both texts and numbers.
    /// </summary>
    public static class LocalizationDemo
    {
        /// <summary>
        /// The page. Its texts use Segoe UI (through an implicit style) for the Cyrillic letters the sample font lacks.
        /// </summary>
        public const string Markup =
            """
            <StackPanel Orientation="Vertical" Margin="20">
              <StackPanel.Resources>
                <Style TargetType="TextBlock" FontFamily="Segoe UI"/>
              </StackPanel.Resources>
              <TextBlock Text="{Localize Demo.Title}" FontSize="24" Margin="0,0,0,8"/>
              <TextBlock Text="{Localize Demo.Hint}" Margin="0,0,0,12"/>
              <Button Content="{Localize Demo.Play}" HorizontalAlignment="Left" Margin="0,0,0,6"/>
              <Button Content="{Localize Demo.Settings}" HorizontalAlignment="Left" Margin="0,0,0,12"/>
              <StackPanel Orientation="Horizontal" Margin="0,0,0,12">
                <TextBlock Text="{Localize Demo.Volume}" Width="120" VerticalAlignment="Center"/>
                <TextBox Text="{Binding Volume, Mode=TwoWay}" Width="120"/>
              </StackPanel>
              <StackPanel Orientation="Horizontal">
                <Button x:Name="english" Margin="0,0,8,0">English</Button>
                <Button x:Name="russian">Русский</Button>
              </StackPanel>
            </StackPanel>
            """;

        /// <summary>
        /// Builds the page, and makes <c>Localization/*.po</c> the configuration's localizer.
        /// </summary>
        /// <param name="configuration">The library configuration.</param>
        /// <param name="fontFamily">Unused: the page sets its own font for Cyrillic.</param>
        /// <returns>The root element.</returns>
        public static UIElement Build(IcyConfiguration configuration, string fontFamily)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            configuration.Assets.Localizer = new PoLocalizer(configuration.Assets, "Localization", CultureInfo.GetCultureInfo("en"));
            configuration.Culture = CultureInfo.GetCultureInfo("en");

            UIElement root = new MarkupLoader(configuration).Load(Markup, nameof(LocalizationDemo));
            var viewModel = new VolumeViewModel();
            root.DataContext = viewModel;

            root.FindRequiredControl<Button>("english").Click += (_, _) => Switch("en");
            root.FindRequiredControl<Button>("russian").Click += (_, _) => Switch("ru");
            return root;

            void Switch(string culture)
            {
                configuration.Culture = CultureInfo.GetCultureInfo(culture);

                // Texts follow at once; a binding shows the new number format on its next update.
                viewModel.Refresh();
            }
        }

        private sealed class VolumeViewModel : ObservableObject
        {
            private float volume = 0.75f;

            public float Volume
            {
                get => volume;
                set => SetProperty(ref volume, value);
            }

            public void Refresh() => OnPropertyChanged(nameof(Volume));
        }
    }
}
