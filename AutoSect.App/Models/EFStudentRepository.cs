

using Microsoft.EntityFrameworkCore;

namespace AutoSect.App.Models
{

    public class EFStudentRepository : IStudentRepository
    {
        private AutoSectDbContext context;

        public EFStudentRepository(AutoSectDbContext ctx)
        {
            context = ctx;
        }
        public IQueryable<Student> Students => context.Students;

        public async Task AddStudentAsync(Student s)
        {
            await context.Students.AddAsync(s);
        }

        public async Task DeleteStudentAsync(long id)
        {
            var student = await context.Students.FindAsync(id);
            if(student != null)
            {
                context.Students.Remove(student);
            }
        }

        public async Task<Student> GetStudenByIdAsync(long id)
        {
            return await context.Students.FindAsync(id);
        }

        public async  Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task UpdateStudentAsync(Student s)
        {
            context.Students.Update(s);
        }
    }
}
