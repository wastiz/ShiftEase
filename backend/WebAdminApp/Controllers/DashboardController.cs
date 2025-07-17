using DAL;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using WebAdminApp.Models;

namespace WebAdminApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly EmployerRepository _employerRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly EmployeeRepository _employeeRepository;
        private readonly SupportRepository _supportRepository;

        public DashboardController(
            EmployerRepository employerRepository,
            OrganizationRepository organizationRepository,
            EmployeeRepository employeeRepository,
            SupportRepository supportRepository)
        {
            _employerRepository = employerRepository;
            _organizationRepository = organizationRepository;
            _employeeRepository = employeeRepository;
            _supportRepository = supportRepository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalEmployers = await _employerRepository.GetAllAsync().ContinueWith(t => t.Result.Count),
                TotalOrganizations = (await _organizationRepository.GetAllAsync()).Count(),
                TotalEmployees = await _employeeRepository.GetEmployeesCount(),
                OrganizationsWithNoEmployees = await _organizationRepository.GetCountWithoutEmployeesAsync(),
                NewEmployersLastWeek = await _employerRepository.GetNewCountLastWeekAsync(),
                NewOrganizationsLastMonth = await _organizationRepository.GetNewCountLastMonthAsync(),
                UnreadMessages = await _supportRepository.GetUnreadCountAsync(),
                RecentMessages = await _supportRepository.GetRecentMessagesAsync(5),

                OrganizationIssues = new List<OrganizationIssue>()
            };

            return View(model);
        }

    }
}