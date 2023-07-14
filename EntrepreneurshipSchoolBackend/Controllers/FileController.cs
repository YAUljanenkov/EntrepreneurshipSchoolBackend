using System;
using System.IO;

using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    public class FileController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public FileController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("SeeAllFiles")]
        public async Task<ActionResult> SeeFiles()
        {
            return Ok(_context.UserFiles);
        }

        [HttpGet("DownloadFileByName")]
        public async Task<ActionResult> DownloadFile(string fileName)
        {
            var correctName = _context.UserFiles.FirstOrDefault(ob => ob.Name == fileName);

            if (correctName == null)
            {
                return BadRequest("No files by that name!");
            }

            var fileBytes = System.IO.File.ReadAllBytes("../files/" + fileName);



            return Ok(File(fileBytes, correctName.FileType));
        }

        [HttpPost("UploadFile")]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            if(file == null || file.Length <= 0)
            {
                return BadRequest("No file uploaded");
            }

            var possibleNamesakes = from copy in _context.UserFiles
                                    where copy.Name == file.FileName
                                    select copy;

            // ПОКА ЧТО ТАК.
            if(possibleNamesakes.Any())
            {
                return BadRequest("Such file already exists");
            }

            using (var stream = new FileStream("../files/" + file.FileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            UserFile newFile = new UserFile();
            newFile.Name = file.FileName;
            newFile.FileType = file.ContentType;

            _context.UserFiles.Add(newFile);
            _context.SaveChanges();

            return Ok();
        }
    }
}
