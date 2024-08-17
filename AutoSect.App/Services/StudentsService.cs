using AutoSect.App.Models;
using System.Data.SqlTypes;

namespace AutoSect.App.Services
{
    public class StudentsService
    {
        private IHttpContextAccessor httpContextAccessor;
        private IStudentRepository studentRepository;
        public StudentsService(IStudentRepository studRepo, IHttpContextAccessor httpCtxAccessor)
        {
            studentRepository = studRepo;
            httpContextAccessor = httpCtxAccessor;
        }

        public async Task UpdateStudentsSectionAsync(long id, int section)
        {
            var student = await studentRepository.GetStudenByIdAsync(id);
            if(student != null)
            {
                student.Section = section;

                await studentRepository.UpdateStudentAsync(student);
                await studentRepository.SaveAsync();
            }
        }

        public async Task DeleteStudentsByProjectIdAndCurrentUser(long projectid)
        {
            var session = httpContextAccessor.HttpContext.Session;

            var projectCurrentUserIdFilter = session.GetString("AppUserID");

            var studentsToDelete = studentRepository.Students.
                Where(s => s.ProjectId == projectid && s.AppUserID == projectCurrentUserIdFilter).
                Select(s => s.StudentId).ToList();

            foreach(var studentId in studentsToDelete)
            {
                await studentRepository.DeleteStudentAsync((long)studentId);
                await studentRepository.SaveAsync();
            }
        }

    }
}
