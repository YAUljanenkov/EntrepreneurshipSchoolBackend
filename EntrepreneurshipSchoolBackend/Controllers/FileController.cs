using System;
using System.IO;
using System.Security.Claims;

using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntrepreneurshipSchoolBackend.Controllers
{
    public class FileController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public FileController(ApiDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Файла загрузки файла с сервера.
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet("/files/{fileId}")]
        public async Task<ActionResult> DownloadFile(int fileId)
        {
            UserFile thisFile = await _context.UserFiles.FindAsync(fileId);

            if (thisFile == null)
            {
                return NotFound("No solutions found with that id");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(thisFile.Path);

            return Ok(File(fileBytes, "application/zip"));
        }

        /// <summary>
        /// Метод загрузки файла с решением на сервер.
        /// Требуется айди задания, решение которого записано в файле.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpPost("/learner/files")]
        [Authorize(Roles = Roles.Learner)]
        public async Task<ActionResult> UploadFile(IFormFile file, int taskId)
        {
            // Проверяем, есть ли такое задание в БД.
            Models.Task thisTask = await _context.Tasks.FindAsync(taskId);

            if (thisTask == null)
            {
                return NotFound("No task found by that id");
            }

            int learnerId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Sid).Value);
            Learner thisLearner = await _context.Learner.FindAsync(learnerId);

            // Создаём новый объект solution.
            // При загрузке файла на сервер автоматически будет создаваться solution на задание с идентификатором taskId.
            Solution newSolution = new Solution
            {
                // Заполняем очевидные поля решения.
                TaskId = taskId,
                Task = await _context.Tasks.FindAsync(taskId),
                CompleteDate = DateTime.Now.ToUniversalTime()
            };

            // В зависимости от командности задания, заполняем либо learner, либо group решения.
            if (thisTask.IsGroup == true)
            {
                Group thisGroup = (await _context.Relates.FirstOrDefaultAsync(relate => relate.LearnerId == learnerId)).Group;
                newSolution.Group = thisGroup;
                newSolution.GroupId = thisGroup.Id;
            }
            else
            {
                newSolution.Learner = thisLearner;
                newSolution.LearnerId = thisLearner.Id;
            }

            // Указываем дату выполнения (сейчас) и путь к файлу задания.
            newSolution.CompleteDate = DateTime.Now.ToUniversalTime();

            UserFile newFile = new UserFile
            {
                Path = "../files/" + file.FileName
            };

            // Избегаем копий в хранилище файлов.
            var possibleNamesakes = from copy in _context.Solutions
                                    where copy.file.Path == newFile.Path
                                    select copy;

            if (possibleNamesakes.Any())
            {
                return BadRequest("Such file already exists");
            }

            // Сохраняем файл.
            await using (var stream = new FileStream("../files/" + file.FileName, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _context.UserFiles.Add(newFile);
            await _context.SaveChangesAsync();

            newSolution.file = newFile;
            newSolution.fileId = newFile.Id;

            // Загружаем файл (зип архив) решения задания.
            if (file is not { Length: > 0, ContentType: "application/zip" })
            {
                return BadRequest("The uploaded files was not .zip");
            }

            // Вносим изменения в БД.
            _context.Solutions.Add(newSolution);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
