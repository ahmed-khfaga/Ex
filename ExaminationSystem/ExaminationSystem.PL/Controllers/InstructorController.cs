using ExaminationSystem.BLL.Interfaces;
using ExaminationSystem.DAL.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExaminationSystem.PL.Controllers
{
    public class InstructorController : Controller
    {
        private readonly IInstructorRepository _instructorRepository;


        public InstructorController(IInstructorRepository instructorRepository)
        {
            _instructorRepository = instructorRepository;
        }
        public IActionResult Index()
        {

            var instructor = _instructorRepository.GetAll();
            return View(instructor);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                var count = _instructorRepository.Add(instructor);
                if (count > 0)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(instructor);

        }

        [HttpGet]
        public IActionResult Details(int? id)
        {

            if (!id.HasValue) // id is null 
            {
                return BadRequest();
            }
            var instructor = _instructorRepository.GetByID(id.Value);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (!id.HasValue) // id is null 
            {
                return BadRequest();
            }
            var department = _instructorRepository.GetByID(id.Value);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);

        }

        [HttpPost]
        public IActionResult Edit([FromRoute] int id, Instructor instructor)
        {
            if (id != instructor.ID)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
                return View(instructor);

            try
            {
                _instructorRepository.Update(instructor);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                // Log the error and handle exception
                //_logger.LogError(string.Empty,ex.Message);

                ModelState.AddModelError(string.Empty, "An error occurred while updating the department.");

                return View(instructor);

            }

        }
        [HttpGet]
        public IActionResult Delete(int? id)
        {

            if (!id.HasValue)
                return BadRequest();

            var instructor = _instructorRepository.GetByID(id.Value);
            if (instructor == null)
                return NotFound();

            return View(instructor);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete([FromRoute] int id, Instructor instructor)
        {

            try
            {

                _instructorRepository.Delete(instructor);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                // Log the error and handle exception
                //_logger.LogError(string.Empty,ex.Message);

                ModelState.AddModelError(string.Empty, "An error occurred while Deleting the department.");

                return View(instructor);

            }
        }

    }
}
