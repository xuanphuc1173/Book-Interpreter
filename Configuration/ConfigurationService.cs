using System.Globalization;
using EXE.Resources;

namespace EXE.Configuration
{
    public static class ConfigurationService
    {
        public static void RegisterGlobalizationAndLocalization(this IServiceCollection services)
        {
            //var suppotedCultures = new[]
            //{
            //    new CultureInfo("en-US"),
            //    new CultureInfo("vi-VN")
            //};

            //var localizationOptions = new RequestLocalizationOptions
            //{
            //    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US"),
            //    SupportedCultures = suppotedCultures.ToList(),
            //    SupportedUICultures = suppotedCultures.ToList()
            //};

            var defaultCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo ("vi-VN")
            };

            services.Configure<RequestLocalizationOptions>(options =>
            {
                //options.DefaultRequestCulture = localizationOptions.DefaultRequestCulture;
                //options.SupportedCultures = localizationOptions.SupportedCultures;
                //options.SupportedUICultures = localizationOptions.SupportedUICultures;
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US");
                options.SupportedCultures = defaultCultures;
                options.SupportedUICultures = defaultCultures;


            }
            );
            services.AddMvc()
                //.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
            {
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(Resource));
            });
        }
    }
}
