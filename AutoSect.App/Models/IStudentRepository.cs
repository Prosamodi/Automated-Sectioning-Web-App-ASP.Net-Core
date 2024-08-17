namespace AutoSect.App.Models
{
    public interface IStudentRepository
    {
        IQueryable<Student> Students { get; }

        Task<Student> GetStudenByIdAsync(long id);
        Task AddStudentAsync(Student s);
        Task DeleteStudentAsync(long id);
        Task UpdateStudentAsync(Student s);
        Task SaveAsync();       
    }

}
