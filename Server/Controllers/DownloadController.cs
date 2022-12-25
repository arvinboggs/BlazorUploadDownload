using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorUploadDownload.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
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
    }
}
