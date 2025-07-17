// namespace Localization;
//
// using System.Globalization;
// using Microsoft.Extensions.Localization;
// using System.Resources;
//
// namespace YourNamespace.Localization
// {
//     public class TranslationService
//     {
//         private readonly IStringLocalizer<Common> _localizer;
//         private readonly ResourceManager _resourceManager;
//
//         public TranslationService(IStringLocalizerFactory factory)
//         {
//             _localizer = factory.Create<Common>();
//             _resourceManager = new ResourceManager(typeof(Common).FullName!, typeof(Common).Assembly);
//         }
//
//         public string Translate(string key, string? culture = null)
//         {
//             culture ??= CultureInfo.CurrentUICulture.Name;
//             var translation = _localizer[key];
//
//             if (!string.IsNullOrEmpty(translation) && !translation.ResourceNotFound)
//             {
//                 return translation;
//             }
//
//             return _resourceManager.GetString(key, new CultureInfo(culture)) ?? key;
//         }
//     }
// }

