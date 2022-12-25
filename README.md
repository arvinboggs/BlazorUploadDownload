# File Upload/Download Between Blazor And ASP.NET.

Implement file upload/download between Blazor WebAssembly app and ASP.NET app.

> Blog article: [https://arvinboggs.github.io/blog/posts/blazor-upload-download](https://arvinboggs.github.io/blog/posts/blazor-upload-download)

## File Upload

1. Open (or create) a Blazor WebAssembly project. In case you are creating a new Blazor WebAssembly project, make sure the "ASP.NET Core hosted" checkbox is ticked.

2. On the client-side (Blazor) project and on the page that the user will use to upload a file, add an `InputFile` element. Please note that `InputFile` is not a regular HTML element but is part of the `Microsoft.AspNetCore.Components.Forms` namespace so you have to include that namespace in your `using` section if you haven't done so already.
    ``` html
    <InputFile OnChange="OnInputFileChange" />
    ```

3. Add an HTML element to display error or success message from the server. Note that the content of the `div` is bound to the variable named `Message`. This step is optional and is not required to upload a file.
    ``` html
    <div>@Message</div>
    ```

4. On the code-behind, declare an `HttpClient` variable that will be automatically initialized via dependency injection.
    ``` c#
    [Inject] HttpClient mHttpClient { get; set; }
    ```

5. Declare the variable that will hold the error or success message from the server.
    ``` c#
    string Message;
    ```

6. On the code-behind, add a function to handle the `OnChange` event. Whenever the user clicks on the `InputFile`, the browser will display a dialog box upon which the user may choose a local file to upload. After choosing a file, the `OnChange` event is triggered.
    ``` c#
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
    ```

7. On the server-side project, add a method to receive the file to be uploaded.
    ``` c#
    [HttpPost("UploadFile")]
    public ActionResult UploadFile()
    {
        
    }
    ```

8. Save the uploaded file into the server's local disk.
    ``` c#
    var pFile = Request.Form.Files.FirstOrDefault();
    if (pFile == null)
        return BadRequest("Please upload at least 1 file.");

    // optional lines. If you had pass additional data from the client-side, you can get them here.
    var pFileID = Request.Form["FileID"];
    Console.WriteLine("FileID: " + pFileID);

    var pPath = Path.Combine(Program.AppPath,"temp");
    if (!Directory.Exists(pPath))
        Directory.CreateDirectory(pPath);
    pPath = Path.Combine(pPath, pFile.FileName);
    using (var pStream = new FileStream(pPath, FileMode.Create))
        pFile.CopyTo(pStream);

    // respond with a custom message back to the client-side
    return Ok("FileID (from browser): " + pFileID);
    ```

## File Download

1. On the client-side project and on the page that the user will use to download a file, add an `button` element.
    ``` html
    <button @onclick="OnDownloadFileClick">Download file from server</button>
    ```

2. On the code-behind, declare an `IJSRuntime` variable that will be automatically initialized via dependency injection.
    ``` c#
    [Inject] IJSRuntime mJs { get; set; }
    ```

3. Add a function to handle the `onclick` event.
    ``` c#
    async Task OnDownloadFileClick()
    {
        // "Download" is the base name of your server-side controller.
        // "DownloadFile" is the method inside the controller.
        var pUrl = "api/Download/DownloadFile";
        // use JavaScript interop to download the file.
        await mJs.InvokeVoidAsync("open", pUrl, "_blank");
    }
    ```

4. On the server-side project, add a function that allows the client-side to download a file.
    ``` c#
    [HttpGet("DownloadFile")]
    public ActionResult DownloadFile()
    {
        var pPath = Path.Combine(Program.AppPath, "temp");
        if (!Directory.Exists(pPath))
            return BadRequest("No file to download. Upload a file first.");
        var pDirectory = new DirectoryInfo(pPath);
        // Get all files inside the <server_path>/temp folder
        var pFiles = pDirectory.GetFiles();
        if (pFiles.Count() == 0)
            return BadRequest("No file to download. Upload a file first.");

        // Sort the files from newest to oldest. We will return the newest file.
        var pSortedFiles =
            from pFile in pFiles
            orderby pFile.LastWriteTime descending
            select pFile;

        // The 3rd parameter is important. Otherwise, the default filename the user will see is "DownloadFile".
        return PhysicalFile(pSortedFiles.First().FullName, "application/file", pSortedFiles.First().Name);
    }
    ```