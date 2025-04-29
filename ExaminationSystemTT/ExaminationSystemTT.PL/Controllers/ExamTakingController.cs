using ExaminationSystemTT.BLL.Interfaces; // Repositories
using ExaminationSystemTT.DAL.Models;     // Models
using ExaminationSystemTT.PL.ViewModels;  // ViewModels
using Microsoft.AspNetCore.Authorization; // For [Authorize]
using Microsoft.AspNetCore.Identity;      // For UserManager
using Microsoft.AspNetCore.Mvc;


namespace ExaminationSystemTT.PL.Controllers
{
    [Authorize(Roles = "Student")] // Ensure only authenticated students can access
    public class ExamTakingController : Controller
    {
        // --- Injected Dependencies ---
        private readonly IExamRepository _examRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentAnswerRepository _studentAnswerRepository;
        private readonly IExamAttemptRepository _examAttemptRepository; // Added
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExamTakingController> _logger;

        // --- Constructor ---
        public ExamTakingController(
            IExamRepository examRepository,
            IStudentRepository studentRepository,
            IStudentAnswerRepository studentAnswerRepository,
            IExamAttemptRepository examAttemptRepository, // Added
            UserManager<ApplicationUser> userManager,
            ILogger<ExamTakingController> logger)
        {
            _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _studentAnswerRepository = studentAnswerRepository ?? throw new ArgumentNullException(nameof(studentAnswerRepository));
            _examAttemptRepository = examAttemptRepository ?? throw new ArgumentNullException(nameof(examAttemptRepository)); // Added
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // --- Action Methods ---

        // GET: /ExamTaking/StartExam?examId=5
        [HttpGet]
        public async Task<IActionResult> StartExam(int examId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var student = _studentRepository.GetAll().FirstOrDefault(s => s.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToAction("Index", "Dashboard");
            }
            int studentId = student.StudentId;

            // *** CHECK IF ALREADY COMPLETED ***
            if (await _examAttemptRepository.HasCompletedAttemptAsync(studentId, examId))
            {
                _logger.LogWarning("StudentId {StudentId} attempted to restart completed ExamId {ExamId}", studentId, examId);
                TempData["InfoMessage"] = "You have already completed this exam. Viewing results."; // Use Info for expected scenario
                return RedirectToAction("Results", new { examId = examId, score = -1, max = -1 }); // Let Results fetch score
            }

            var exam = _examRepository.GetByIdWithDetails(examId);
            if (exam == null)
            {
                TempData["ErrorMessage"] = "Exam not found.";
                return RedirectToAction("Index", "Dashboard");
            }

            var now = DateTime.Now;
            if (exam.StartTime > now || exam.EndTime < now)
            {
                TempData["ErrorMessage"] = "This exam is not currently active.";
                return RedirectToAction("Index", "Dashboard");
            }

            // *** GET OR CREATE ATTEMPT RECORD ***
            var attempt = await _examAttemptRepository.GetAttemptAsync(studentId, examId);
            if (attempt == null)
            {
                attempt = new ExamAttempt
                {
                    StudentId = studentId,
                    ExamId = examId,
                    StartTime = DateTime.Now, // Record start time
                    IsCompleted = false
                };
                try
                {
                    await _examAttemptRepository.AddAttemptAsync(attempt);
                    _logger.LogInformation("Created new ExamAttempt for StudentId {StudentId}, ExamId {ExamId}", studentId, examId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create ExamAttempt for StudentId {StudentId}, ExamId {ExamId}", studentId, examId);
                    TempData["ErrorMessage"] = "Could not initialize exam attempt record. Please try again.";
                    return RedirectToAction("Index", "Dashboard");
                }
            } // No need for else if (attempt.IsCompleted) because the first check handles it

            var existingAnswersList = await _studentAnswerRepository.GetExamAnswersAsync(studentId, examId);
            var existingAnswersDict = existingAnswersList?.ToDictionary(a => a.QuestionId) ?? new Dictionary<int, StudentAnswer>();

            var viewModel = new ExamTakingViewModel
            {
                Exam = exam,
                Questions = exam.Questions?.OrderBy(q => q.QuestionId).ToList() ?? new List<Question>(),
                ExistingAnswers = existingAnswersDict
            };

            _logger.LogInformation("Student {UserEmail} (StudentId {StudentId}) started/resumed Exam {ExamId}", user.Email, studentId, examId);
            return View(viewModel);
        }

        // POST: /ExamTaking/SubmitExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExam(ExamSubmissionViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid submission data.";
                _logger?.LogWarning("SubmitExam received invalid ModelState for ExamId {ExamId}", viewModel?.ExamId);
                return viewModel?.ExamId > 0
                        ? RedirectToAction("StartExam", new { examId = viewModel.ExamId })
                        : RedirectToAction("Index", "Dashboard");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            Student student = null;
            try { student = _studentRepository.GetAll().FirstOrDefault(s => s.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)); }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving student by email {UserEmail} during exam submission", user.Email);
                TempData["ErrorMessage"] = "An error occurred finding your profile. Exam not submitted.";
                return RedirectToAction("StartExam", new { examId = viewModel.ExamId });
            }
            if (student == null)
            {
                _logger?.LogWarning("Student profile not found for user {UserEmail} during exam submission", user.Email);
                TempData["ErrorMessage"] = "Student profile not found. Exam not submitted.";
                return RedirectToAction("StartExam", new { examId = viewModel.ExamId });
            }
            int studentId = student.StudentId;

            // *** CHECK IF ALREADY COMPLETED (Prevents resubmission) ***
            if (await _examAttemptRepository.HasCompletedAttemptAsync(studentId, viewModel.ExamId))
            {
                _logger.LogWarning("StudentId {StudentId} attempted to resubmit completed ExamId {ExamId}", studentId, viewModel.ExamId);
                TempData["InfoMessage"] = "You have already submitted this exam.";
                return RedirectToAction("Results", new { examId = viewModel.ExamId, score = -1, max = -1 });
            }

            // Get Exam with Questions for Grading
            Exam exam = null;
            try { exam = _examRepository.GetByIdWithDetails(viewModel.ExamId); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam details for grading. ExamId {ExamId}", viewModel.ExamId);
                TempData["ErrorMessage"] = "Error verifying exam status. Exam not submitted.";
                return RedirectToAction("StartExam", new { examId = viewModel.ExamId });
            }

            // Check Exam Exists and Hasn't *Just* Expired
            if (exam == null || exam.EndTime < DateTime.Now)
            {
                _logger.LogWarning("Exam {ExamId} submission attempt when exam not found or expired by user {UserEmail}", viewModel.ExamId, user.Email);
                TempData["ErrorMessage"] = "Exam cannot be submitted because it was not found or the time has expired.";
                return RedirectToAction("Index", "Dashboard");
            }

            var questionsLookup = exam.Questions?.ToDictionary(q => q.QuestionId) ?? new Dictionary<int, Question>();
            int totalScore = 0;
            int maxScore = 0;
            int answersProcessed = 0;

            // Process and Save Answers
            foreach (var submittedAnswer in viewModel.Answers ?? Enumerable.Empty<AnswerSubmission>())
            {
                answersProcessed++;
                if (!questionsLookup.TryGetValue(submittedAnswer.QuestionId, out Question question))
                {
                    _logger?.LogWarning("Submitted answer for non-existent QuestionId {QuestionId} in ExamId {ExamId} by StudentId {StudentId}", submittedAnswer.QuestionId, viewModel.ExamId, studentId);
                    continue;
                }
                maxScore += question.Mark;

                var studentAnswer = new StudentAnswer
                {
                    StudentId = studentId,
                    ExamId = viewModel.ExamId,
                    QuestionId = submittedAnswer.QuestionId,
                    SelectedOptionIndex = null,
                    SelectedAnswerTF = null
                };

                bool isCorrect = false;
                if (question.QuestionType == "MCQ")
                {
                    studentAnswer.SelectedOptionIndex = submittedAnswer.SelectedOptionIndex;
                    if (submittedAnswer.SelectedOptionIndex.HasValue && submittedAnswer.SelectedOptionIndex == question.CorrectOptionIndex) isCorrect = true;
                }
                else if (question.QuestionType == "TF")
                {
                    studentAnswer.SelectedAnswerTF = submittedAnswer.SelectedAnswerTF;
                    if (submittedAnswer.SelectedAnswerTF.HasValue && submittedAnswer.SelectedAnswerTF == question.CorrectAnswerTF) isCorrect = true;
                }
                if (isCorrect) totalScore += question.Mark;

                try { await _studentAnswerRepository.AddOrUpdateAnswerAsync(studentAnswer); }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error saving StudentAnswer for StudentId {StudentId}, ExamId {ExamId}, QuestionId {QuestionId}", studentId, viewModel.ExamId, submittedAnswer.QuestionId);
                    TempData["WarningMessage"] = "An error occurred saving some answers."; // Use warning, proceed
                }
            }

            // *** UPDATE EXAM ATTEMPT ***
            var attempt = await _examAttemptRepository.GetAttemptAsync(studentId, viewModel.ExamId);
            if (attempt != null)
            {
                attempt.SubmissionTime = DateTime.Now;
                attempt.Score = totalScore;
                attempt.MaxScore = maxScore;
                attempt.IsCompleted = true; // Mark as completed
                try
                {
                    await _examAttemptRepository.UpdateAttemptAsync(attempt);
                    _logger.LogInformation("Updated ExamAttempt for StudentId {StudentId}, ExamId {ExamId}. Score: {Score}/{MaxScore}", studentId, viewModel.ExamId, totalScore, maxScore);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update ExamAttempt for StudentId {StudentId}, ExamId {ExamId}", studentId, viewModel.ExamId);
                    TempData["ErrorMessage"] = "Failed to finalize exam attempt record."; // Show error but proceed
                }
            }
            else { /* Log critical error - attempt record missing */ }

            // Redirect to Results Page
            return RedirectToAction("Results", new { examId = viewModel.ExamId, score = totalScore, max = maxScore });
        }


        // GET: /ExamTaking/Results?examId=1&score=80&max=100
        [HttpGet]
        public async Task<IActionResult> Results(int examId, int score = -1, int max = -1) // Default score/max to -1
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var student = _studentRepository.GetAll().FirstOrDefault(s => s.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
            if (student == null) { TempData["ErrorMessage"] = "Student profile not found."; return RedirectToAction("Index", "Dashboard"); }
            int studentId = student.StudentId;

            ExamAttempt attempt = null;
            // Fetch from DB if score/max indicate it's needed (came from dashboard or refresh)
            if (score == -1 || max == -1)
            {
                attempt = await _examAttemptRepository.GetAttemptAsync(studentId, examId);
                if (attempt != null && attempt.IsCompleted && attempt.Score.HasValue && attempt.MaxScore.HasValue)
                {
                    score = attempt.Score.Value;
                    max = attempt.MaxScore.Value;
                }
                else
                {
                    _logger.LogWarning("Could not retrieve completed attempt score for StudentId {StudentId}, ExamId {ExamId} from Results action.", studentId, examId);
                    TempData["ErrorMessage"] = "Could not retrieve your score for this exam. Please try submitting again or contact support.";
                    // Redirect if attempt isn't completed or score is missing
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            double percentage = (max > 0) ? ((double)score / max) * 100 : 0;
            ViewBag.ExamId = examId;
            ViewBag.Score = score;
            ViewBag.MaxScore = max;
            ViewBag.Percentage = percentage;
            ViewBag.CanReview = true; // Allow review if they reached results

            // Optional: Load Exam Name
            // var exam = _examRepository.GetByID(examId);
            // ViewBag.ExamName = exam?.Course?.Name ?? $"Exam {examId}";

            _logger.LogInformation("Displaying results for StudentId {StudentId}, ExamId {ExamId}. Score: {Score}/{MaxScore}", studentId, examId, score, max);
            return View();
        }

        // GET: /ExamTaking/ReviewExam?examId=5
        [HttpGet]
        public async Task<IActionResult> ReviewExam(int examId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var student = _studentRepository.GetAll().FirstOrDefault(s => s.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
            if (student == null) { TempData["ErrorMessage"] = "Student profile not found."; return RedirectToAction("Index", "Dashboard"); }
            int studentId = student.StudentId;

            var attempt = await _examAttemptRepository.GetAttemptAsync(studentId, examId);
            if (attempt == null || !attempt.IsCompleted)
            {
                _logger.LogWarning("Review attempt for ExamId {ExamId} by StudentId {StudentId} denied (not completed).", examId, studentId);
                TempData["ErrorMessage"] = "You must complete the exam before reviewing answers.";
                return RedirectToAction("Index", "Dashboard");
            }

            var exam = _examRepository.GetByIdWithDetails(examId);
            if (exam == null) { TempData["ErrorMessage"] = "Exam not found."; return RedirectToAction("Index", "Dashboard"); }

            var studentAnswers = await _studentAnswerRepository.GetExamAnswersAsync(studentId, examId);
            var studentAnswersDict = studentAnswers?.ToDictionary(a => a.QuestionId) ?? new Dictionary<int, StudentAnswer>();

            var viewModel = new ExamReviewViewModel
            {
                Exam = exam,
                AttemptDetails = attempt,
                SubmittedAnswers = studentAnswersDict
            };

            _logger.LogInformation("Displaying exam review for StudentId {StudentId}, ExamId {ExamId}", studentId, examId);
            return View(viewModel);
        }

        // Kept for potential future use (e.g., timer expiry auto-submit)
        [HttpGet]
        public IActionResult CompleteExam(int examId)
        {
            _logger.LogInformation("Explicit CompleteExam action hit for ExamId {ExamId}", examId);
            // Should probably try and find results and redirect there
            return RedirectToAction("Results", new { examId = examId, score = -1, max = -1 });
        }
    }
}