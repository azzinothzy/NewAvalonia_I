using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using AvaloniaMvvmApp2.ViewModels;
using AvaloniaMvvmApp2.Views;

namespace AvaloniaMvvmApp2
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            LiveCharts.Configure(config =>
           config
               // you can override the theme 
             //  .AddDarkTheme()  

               // In case you need a non-Latin based font, you must register a typeface for SkiaSharp
               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉')) // <- Chinese 
               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('あ')) // <- Japanese 
               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('헬')) // <- Korean 
               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('Ж'))  // <- Russian 

               //.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('أ'))  // <- Arabic 
              // .UseRightToLeftSettings() // Enables right to left tooltips 

               // finally register your own mappers
               // you can learn more about mappers at:
               // https://livecharts.dev/docs/avalonia/2.0.0-rc2/Overview.Mappers

               // here we use the index as X, and the population as Y 
              .HasMap<City>((city, index) => new(index, city.Population))
           // .HasMap<Foo>( .... ) 
           // .HasMap<Bar>( .... ) 
           );
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        public record City(string Name, double Population);
    }
}