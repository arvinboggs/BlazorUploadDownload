using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorUploadDownload.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost("UploadFile")]
        public ActionResult UploadFile()
        {
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
        }
    }
}
