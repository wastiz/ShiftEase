using Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAdminApp.Models;

public class DashboardViewModel
{
    public int TotalEmployers { get; set; } = default!;
    public int TotalOrganizations { get; set; } = default!;
    public int OrganizationsWithNoEmployees { get; set; } = default!;
    public int TotalEmployees { get; set; } = default!;
    public int NewEmployersLastWeek { get; set; } = default!;
    public int NewOrganizationsLastMonth { get; set; } = default!;
     
    public int UnreadMessages { get; set; } = default!;
    public List<SupportMessage> RecentMessages { get; set; } = default!;
    
    public List<OrganizationIssue> OrganizationIssues { get; set; } = default!;
    
    // public double AverageResponseTime { get; set; }
    // public int ActiveSessions { get; set; }
}

public class OrganizationIssue
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public string IssueType { get; set; }
    public string Description { get; set; }
}