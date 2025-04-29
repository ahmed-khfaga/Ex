using ExaminationSystemTT.BLL.Interfaces; 
using ExaminationSystemTT.DAL.Models;
using ExaminationSystemTT.PL.ViewModels;  
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Student")]
public class DashboardController : Controller
{
    private readonly IExamRepository _examRepository;
    private readonly IStudentRepository _studentRepository; // Needed for student ID
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IExamAttemptRepository _examAttemptRepository; // Inject attempt repo
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IExamRepository examRepository,
        IStudentRepository studentRepository,
        UserManager<ApplicationUser> userManager,
        IExamAttemptRepository examAttemptRepository, // Add to constructor
        ILogger<DashboardController> logger)
    {
        _examRepository = examRepository;
        _studentRepository = studentRepository;
        _userManager = userManager;
        _examAttemptRepository = examAttemptRepository; // Assign
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var student = _studentRepository.GetAll().FirstOrDefault(s => s.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
        if (student == null)
        {
            TempData["ErrorMessage"] = "Student profile not found.";
            return View(new List<DashboardExamViewModel>()); // Return empty list
        }
        int studentId = student.StudentId;

        var now = DateTime.Now;
        var availableExams = _examRepository.GetAllWithCourseAndInstructor()
                                    .Where(e => e.StartTime <= now && e.EndTime >= now) // Only show currently active
                                    .ToList();

        var viewModelList = new List<DashboardExamViewModel>();
        foreach (var exam in availableExams)
        {
            bool completed = await _examAttemptRepository.HasCompletedAttemptAsync(studentId, exam.ExamId);
            viewModelList.Add(new DashboardExamViewModel
            {
                Exam = exam,
                IsCompleted = completed
            });
        }

        _logger.LogInformation("Fetched dashboard for StudentId {StudentId}. Exams available now: {Count}", studentId, viewModelList.Count);
        return View(viewModelList);
    }
}

