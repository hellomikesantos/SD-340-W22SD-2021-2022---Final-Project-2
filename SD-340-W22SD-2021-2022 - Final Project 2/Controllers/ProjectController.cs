﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SD_340_W22SD_2021_2022___Final_Project_2.Data;
using SD_340_W22SD_2021_2022___Final_Project_2.Models;
using SD_340_W22SD_2021_2022___Final_Project_2.Models.ViewModels;


namespace SD_340_W22SD_2021_2022___Final_Project_2.Controllers
{
    public class ProjectController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            List<Project> projects = _context.Project.ToList();
            return View(projects);
        }

        [Authorize(Roles = "Project Manager")]
        public async Task<IActionResult> Create()
        {
            List<ApplicationUser>? developers;

            try
            {
                developers = (List<ApplicationUser>?)await _userManager.GetUsersInRoleAsync("Developer");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(developers);
        }

        [Authorize(Roles = "Project Manager")]
        [HttpPost]
        public async Task<IActionResult> Create(string name, string[] developer)
        {
            if (name != null)
            {
                try
                {
                    string userName = User.Identity.Name;
                    ApplicationUser projectManager = await _userManager.FindByNameAsync(userName);

                    Project project = new Project();

                    project.Name = name;
                    project.User.Add(projectManager);

                    _context.Project.Add(project);

                    foreach (String dev in developer)
                    {
                        ApplicationUser projectDeveloper = await _userManager.FindByIdAsync(dev);
                        project.User.Add(projectDeveloper);
                    }

                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Error", "Home");
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Details(int projectId)
        {
            Project? project = _context.Project
                .Include(p => p.Ticket)
                .Include(u => u.User)
                .FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            List<Ticket> tickets = project.Ticket.ToList();
            List<ApplicationUser> developers = project.User.ToList();

            ProjectDetailsViewModel viewModel = new ProjectDetailsViewModel(project, tickets, developers);

            return View(viewModel);
        }
    }
}
