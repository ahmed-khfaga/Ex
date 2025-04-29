
using ExaminationSystemTT.BLL.Interfaces; // For IStudentRepository
using ExaminationSystemTT.DAL.Models;
using ExaminationSystemTT.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ExaminationSystemTT.PL.Controllers
{
    public class AccountController : Controller
    {
        // --- Injected Dependencies ---
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IStudentRepository _studentRepository; // Added repository
        private readonly ILogger<AccountController> _logger;   // Added logger
        private readonly IWebHostEnvironment _webHostEnvironment;


        // --- Constructor ---
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IStudentRepository studentRepository, ILogger<AccountController> logger, IWebHostEnvironment webHostEnvironment)   // Inject logger
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _studentRepository = studentRepository; // Assign injected repo
            _logger = logger; // Assign injected logger
            _webHostEnvironment = webHostEnvironment;
        }

        // --- Sign Up Actions ---
        [HttpGet]
        public IActionResult SignUp()
        {
            // Redirect if already logged in? Optional.
            // if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Good practice for POST actions
        public async Task<IActionResult> SignUP(SignUpViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Check if user with this email already exists
                var existingUser = await _userManager.FindByEmailAsync(viewModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(SignUpViewModel.Email), "An account with this email already exists.");
                    return View(viewModel);
                }

                var user = new ApplicationUser()
                {
                    UserName = viewModel.Email, // Using Email as UserName is safer for uniqueness
                    Email = viewModel.Email,
                    IsAgree = viewModel.IsAgree,
                    FName = viewModel.FName,
                    LName = viewModel.LName,
                    // Consider setting EmailConfirmed to false & implementing email confirmation flow
                };

                // 1. Create the Identity User
                var result = await _userManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserEmail} created successfully.", user.Email);

                    // 2. Assign the 'Student' role
                    bool roleAssigned = false;
                    if (await _roleManager.RoleExistsAsync("Student"))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, "Student");
                        if (roleResult.Succeeded)
                        {
                            _logger.LogInformation("Role 'Student' assigned successfully to user {UserEmail}.", user.Email);
                            roleAssigned = true;
                        }
                        else
                        {
                            // Log errors for role assignment failure
                            foreach (var error in roleResult.Errors)
                            {
                                _logger.LogError("Error assigning role 'Student' to user {UserEmail}: {ErrorDescription}", user.Email, error.Description);
                            }
                            // Don't necessarily stop the process, but log it. User exists but has no role.
                        }
                    }
                    else
                    {
                        _logger.LogCritical("CRITICAL ERROR: 'Student' role not found during signup for user {UserEmail}.", user.Email);
                        // This is a configuration error - maybe prevent login? Add error to modelstate.
                        ModelState.AddModelError(string.Empty, "System configuration error: Student role missing.");
                        // Don't attempt student creation if role assignment failed due to missing role
                    }

                    // 3. Create corresponding Student record (only if user creation was successful)
                    if (roleAssigned) // Proceed only if role assignment succeeded or wasn't critical
                    {
                        try
                        {
                            var newStudent = new Student
                            {
                                FirstName = viewModel.FName,
                                LastName = viewModel.LName,
                                Email = user.Email, // Match email
                                Phone = null // Collect later or during signup if needed
                            };

                            int studentResult = _studentRepository.Add(newStudent); // Assuming Add is synchronous

                            if (studentResult > 0)
                            {
                                _logger.LogInformation("Successfully created Student record for user {UserEmail}", user.Email);
                                // Both user and student created, redirect to sign in
                                TempData["SuccessMessage"] = "Account created successfully! Please sign in.";
                                return RedirectToAction(nameof(SignIn));
                            }
                            else
                            {
                                _logger.LogError("Failed to create Student record (0 rows affected) for user {UserEmail}. User identity exists.", user.Email);
                                // CRITICAL: User exists but student profile doesn't. Need manual fix or delete user?
                                ModelState.AddModelError("", "Failed to create associated student profile. Please contact support.");
                                // Probably should NOT redirect to signin here. Keep user on signup page with error.
                                // Consider deleting the ApplicationUser if the Student record fails? Complex rollback.
                                // await _userManager.DeleteAsync(user); // Be careful with this!
                                return View(viewModel);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creating Student record for user {UserEmail}. User identity exists.", user.Email);
                            // CRITICAL: User exists but student profile doesn't.
                            ModelState.AddModelError("", "An error occurred creating the associated student profile. Please contact support.");
                            // Consider deleting the ApplicationUser
                            // await _userManager.DeleteAsync(user);
                            return View(viewModel);
                        }
                    }
                    else
                    {
                        // If role assignment failed or role didn't exist, return view with error
                        _logger.LogWarning("Skipping Student record creation for user {UserEmail} due to previous errors.", user.Email);
                        return View(viewModel); // Return with role assignment error messages
                    }
                }
                else // User creation failed
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("User creation failed for email {UserEmail}: {ErrorDescription}", viewModel.Email, error.Description);
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            // If ModelState was invalid initially, or errors occurred, return view
            return View(viewModel);
        }

        // --- Sign In Actions ---
        [HttpGet]
        public IActionResult SignIn()
        {
            // Redirect if already logged in? Optional.
            // if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Good practice for POST actions
        public async Task<IActionResult> SignIn(SignInViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(viewModel.Email);
                if (user != null)
                {
                    // Attempt sign-in
                    var result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, viewModel.RememberMe, lockoutOnFailure: true); // Enable lockout

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {UserEmail} logged in successfully.", user.Email);
                        // --- Role-Based Redirect ---
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            _logger.LogInformation("Redirecting Admin user {UserEmail} to Course/Index.", user.Email);
                            return RedirectToAction("Index", "Course"); // Admin dashboard/landing page
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Instructor"))
                        {
                            _logger.LogInformation("Redirecting Instructor user {UserEmail} to Exam/Index.", user.Email);
                            return RedirectToAction("Index", "Exam"); // Instructor dashboard/landing page
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Student"))
                        {
                            _logger.LogInformation("Redirecting Student user {UserEmail} to Dashboard/Index.", user.Email);
                            return RedirectToAction("Index", "Dashboard"); // Student dashboard
                        }
                        else
                        {
                            _logger.LogInformation("Redirecting user {UserEmail} with no specific role to Home/Index.", user.Email);
                            // Fallback redirect if user has no specific role assigned
                            return RedirectToAction(nameof(HomeController.Index), "Home");
                        }
                        // --- End Role-Based Redirect ---
                    }
                    else if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User {UserEmail} account locked out.", user.Email);
                        ModelState.AddModelError(string.Empty, "Your account is locked out. Please try again later or contact support.");
                    }
                    else if (result.IsNotAllowed)
                    {
                        _logger.LogWarning("User {UserEmail} login not allowed (e.g., email not confirmed).", user.Email);
                        ModelState.AddModelError(string.Empty, "Login is not allowed for this account. Ensure your email is confirmed if required.");
                    }
                    else // Includes incorrect password
                    {
                        _logger.LogWarning("Invalid login attempt for user {UserEmail}.", user.Email);
                        ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
                    }
                }
                else // User not found
                {
                    _logger.LogWarning("Login attempt for non-existent user email {UserEmail}.", viewModel.Email);
                    ModelState.AddModelError(string.Empty, "Invalid Login Attempt."); // Keep generic message
                }
            }
            // If ModelState was invalid initially, or login failed, return view
            return View(viewModel);
        }

        // --- Sign Out Action ---
        [HttpPost] // Make SignOut POST for security best practice
        [ValidateAntiForgeryToken] // Add AntiForgeryToken (ensure layout uses a form for SignOut)
        public async Task<IActionResult> SignOut()
        {
            var userName = User.Identity?.Name; // Get username before sign out for logging
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserName} signed out.", userName ?? "Unknown");
            // Redirect to a public page after sign out
            return RedirectToAction(nameof(SignIn)); // Redirect to SignIn page
        }


        [HttpGet]
        [Authorize] // Ensure user is logged in to view profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // This shouldn't happen if [Authorize] is working
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var viewModel = new ProfileViewModel
            {
                FName = user.FName,
                LName = user.LName,
                Email = user.Email,
                ExistingProfilePicturePath = user.ProfilePicturePath // Pass existing path
            };

            return View(viewModel);
        }

        // --- This is the action method you need to check ---
        [HttpPost]
        [Authorize] // Make sure user is logged in
        [ValidateAntiForgeryToken] // Prevent CSRF attacks
        public async Task<IActionResult> Profile(ProfileViewModel viewModel) // ★ Does your action accept ProfileViewModel?
        {
            // 1. Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'."); }

            // ★ 2. Check if viewModel.ProfilePictureFile is NOT null and has Length > 0
            if (viewModel.ProfilePictureFile != null && viewModel.ProfilePictureFile.Length > 0)
            {
                // --- Handle File Upload ---

                // ★ 3. Validation: Does your file meet size/type limits? Are errors displayed if not?
                long maxFileSize = 1 * 1024 * 1024; // 1 MB
                if (viewModel.ProfilePictureFile.Length > maxFileSize) { /* Add ModelState Error */ }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(viewModel.ProfilePictureFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension)) { /* Add ModelState Error */ }

                if (!ModelState.IsValid) { /* Return View(viewModel) with errors */ }

                // ★ 4. Path Calculation: Is _webHostEnvironment injected and used correctly?
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(uploadsFolder)) { Directory.CreateDirectory(uploadsFolder); }
                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                string relativePath = "/images/profiles/" + uniqueFileName;

                // ★ 5. Delete Old File: Does this part execute without error? (Check logs if needed)
                if (!string.IsNullOrEmpty(user.ProfilePicturePath)) { /* ... delete logic ... */ }

                // ★ 6. Save New File: Does this part execute without error? (Check logs if needed)
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.ProfilePictureFile.CopyToAsync(fileStream); // <-- File is saved here
                    }
                }
                catch (Exception ex) { /* Log error, Add ModelState Error, Return View(viewModel) */ }              
                user.ProfilePicturePath = relativePath; // Assign the *relative* path
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    // Add ModelState error, maybe delete the newly saved file, Return View(viewModel)
                }

                return RedirectToAction(nameof(Profile)); // Redirect to GET action
            }
            else
            {
                // No file uploaded branch
                if (!ModelState.IsValid) { /* Return View(viewModel) with other errors */ }
                return RedirectToAction(nameof(Profile)); // Or show a message
            }
        }

    }
}