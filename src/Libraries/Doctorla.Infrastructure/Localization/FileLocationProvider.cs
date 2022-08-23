using Doctorla.Infrastructure.Common.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;

namespace Doctorla.Infrastructure.Localization;

/// <summary>
/// Provides PO files for FSH Localization.
/// </summary>
public class FileLocationProvider : ILocalizationFileLocationProvider
{
    private readonly IFileProvider fileProvider = null;
    private readonly string resourcesContainer = null;

    public FileLocationProvider(IHostEnvironment hostingEnvironment, IOptions<LocalizationOptions> localizationOptions)
    {
        this.fileProvider = hostingEnvironment.ContentRootFileProvider;
        this.resourcesContainer = localizationOptions.Value.ResourcesPath;
    }

    public IEnumerable<IFileInfo> GetLocations(string cultureName)
    {
        // Loads all *.po files from the culture folder under the Resource Path.
        // for example, src\Host\Localization\en-US\Doctorla.Exceptions.po
        foreach (var file in fileProvider.GetDirectoryContents(PathExtensions.Combine(resourcesContainer, cultureName)))
        {
            yield return file;
        }
    }
}
