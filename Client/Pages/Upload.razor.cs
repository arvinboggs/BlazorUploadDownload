using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorUploadDownload.Client.Pages
{
    public partial class Upload : ComponentBase
    {
        [Inject] HttpClient mHttpClient { get; set; }

        string Message;

        async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            const long pMaxFileSize = 2000000; // 2 million bytes

            if (e.File.Size > pMaxFileSize)
            {
                Message = "Max allowed upload size is " + pMaxFileSize.ToString("N") + ". You attempted to upload " + e.File.Size.ToString("N") + ".";
                return;
            }

            var pContent = new MultipartFormDataContent();
            var pFileContent = new StreamContent(e.File.OpenReadStream(pMaxFileSize));

            // Use this MultipartFormDataContent.Add overload. If you use the other overloads, the file might not show up on the server-side method.
            pContent.Add(pFileContent, "file", e.File.Name);

            // The following code is optional. Use this line to pass string data to the server together with the uploaded file.
            pContent.Add(new StringContent(Guid.NewGuid().ToString()), "FileID");

            // "Upload" is the base name of your server-side controller.
            // "UploadFile" is the method inside the controller.
            var pUrl = "Upload/UploadFile";
            var pResult = await mHttpClient.PostAsync(pUrl, pContent);

            // pResultContent will contain whatever string data was returned by the server-side method.
            var pResultContent = await pResult.Content.ReadAsStringAsync();
            if (pResult.IsSuccessStatusCode)
                // we will be here if server responds with Ok("my message")
                Message = "Upload success: " + pResultContent;
            else
                // we will be here if server responds with BadRequest("my message")
                Message = "Upload failed: " + pResultContent;
        }
    }
}
