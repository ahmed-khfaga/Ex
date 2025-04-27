    using ExaminationSystem.BLL.Interfaces;
    using ExaminationSystem.DAL.Models;
    using Microsoft.AspNetCore.Mvc;

    namespace ExaminationSystem.PL.Controllers
    {
        public class StudentController : Controller
        {
            private readonly IStudentRepository _studentRepository;
            public StudentController(IStudentRepository studentRepository)
            {
                _studentRepository = studentRepository;
            }
            public IActionResult Index()
            {
                var students = _studentRepository.GetAll();
                return View(students);
            }

            [HttpGet]
            public IActionResult Create()
            {
                return View();
            }
            [HttpPost]
            public IActionResult Create(Student student)
            {
                if (ModelState.IsValid)
                {
                    var count = _studentRepository.Add(student);
                    if (count > 0)
                    {
                        return RedirectToAction("Index");
                    }
                }
                return View(student);

            }
            [HttpGet]
            public IActionResult Details(int? id)
            {

                if (!id.HasValue) // id is null 
                {
                    return BadRequest();
                }
                var student = _studentRepository.GetByID(id.Value);
                if (student == null)
                {
                    return NotFound();
                }
                return View(student);
            }

            [HttpGet]
            public IActionResult Edit(int? id)
            {
                if (!id.HasValue) // id is null 
                {
                    return BadRequest();
                }
                var student = _studentRepository.GetByID(id.Value);
                if (student == null)
                {
                    return NotFound();
                }
                return View(student);

            }

            [HttpPost]
            public IActionResult Edit([FromRoute] int id, Student student)
            {
                if (id != student.ID)
                {
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                    return View(student);

                try
                {
                    _studentRepository.Update(student);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                    // Log the error and handle exception
                    //_logger.LogError(string.Empty,ex.Message);

                    ModelState.AddModelError(string.Empty, "An error occurred while updating the department.");

                    return View(student);

                }
            }

            [HttpGet]
            public IActionResult Delete(int? id)
            {

                if (!id.HasValue)
                    return BadRequest();

                var student = _studentRepository.GetByID(id.Value);
                if (student == null)
                    return NotFound();

                return View(student);
            }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Delete([FromRoute] int id, Student student)
            {

                try
                {

                    _studentRepository.Delete(student);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {

                    // Log the error and handle exception
                    //_logger.LogError(string.Empty,ex.Message);

                    ModelState.AddModelError(string.Empty, "An error occurred while Deleting the department.");

                    return View(student);

                }
            }

        }
    }
