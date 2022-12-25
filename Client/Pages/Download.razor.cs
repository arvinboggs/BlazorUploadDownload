using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorUploadDownload.Client.Pages
{
    public partial class Download : ComponentBase
    {
        [Inject] IJSRuntime mJs { get; set; }

        async Task OnDownloadFileClick()
        {
            // "Download" is the base name of your server-side controller.
            // "DownloadFile" is the method inside the controller.
            var pUrl = "api/Download/DownloadFile";
            // use JavaScript interop to download the file.
            await mJs.InvokeVoidAsync("open", pUrl, "_blank");
        }
    }
}
