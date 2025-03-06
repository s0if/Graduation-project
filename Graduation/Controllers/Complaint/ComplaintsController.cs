using Graduation.Data;
using Graduation.DTOs.Complaints;
using Graduation.DTOs.Email;
using Graduation.DTOs.Images;
using Graduation.Helpers;
using Graduation.Model;
using Graduation.Service;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.OpenApi.Extensions;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;


namespace Graduation.Controllers.ComplaintFolder
{
    [Route("[controller]")]
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public ComplaintsController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        [HttpPost("AddComplaint")]
        public async Task<IActionResult> AddComplaint(AddComplaintDTOs addComplaint)
        {
            if (ModelState.IsValid)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Token Is Missing" });
                int? userId = ExtractClaims.ExtractUserId(token);
                if (string.IsNullOrEmpty(userId.ToString()))
                    return Unauthorized(new { message = "Token Is Missing" });
                ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                var role = await userManager.GetRolesAsync(requestUser);

                if (role.Contains("consumer")||role.Contains("provider"))
                {
                    Complaint complaint = new Complaint
                    {
                        Name = addComplaint.NameComplaint,
                        Description = addComplaint.Content,
                        status = false,
                        UsersID=requestUser.Id,
                    };
                    await dbContext.AddAsync(complaint);
                    await dbContext.SaveChangesAsync();
                    if (addComplaint.Image is not null)
                    {

                        ImageDetails details = new ImageDetails()
                        {
                            complaintId = complaint.Id,
                            Image = await FileSettings.UploadFileAsync(addComplaint.Image)
                        };
                        await dbContext.images.AddAsync(details);
                        await dbContext.SaveChangesAsync();
                    }
                    return Ok(new {status=200,message= "Complaint added successfully" });
                }
                return Unauthorized();
            }
            return NotFound();

        }

        [HttpGet("AllComplaint")]
        public async Task<IActionResult> AllComplaint()
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var role = await userManager.GetRolesAsync(requestUser);

            if (role.Contains("admin"))
            {
                IEnumerable<Complaint> deleteEnd = dbContext.complaints
                   .Include(c=>c.ImageDetails)   
                   .Where(c => c.status==true)
                    .ToList();
                
                foreach (var item in deleteEnd)
                {
                    if (item.ImageDetails.Any())
                        foreach (var image in item.ImageDetails)
                        {
                            await FileSettings.DeleteFileAsync(image.Image);
                            dbContext.images.Remove(image);
                        }
                    dbContext.RemoveRange(item);
                    await dbContext.SaveChangesAsync();
                }
                //var time = DateTime.Today.AddDays(-120);
                //    . Where(c => (time <= c.CreatedDate))
                IEnumerable<Complaint> request =  dbContext.complaints
                    .Include (c=>c.ImageDetails)
                     .ToList();
                List<GetAllComplaintDTOs> complaints=new List<GetAllComplaintDTOs>();
                foreach (var complaint in request)
                {
                    complaints.Add(
                        new GetAllComplaintDTOs
                        {
                            Id = complaint.Id,
                            UsersID = complaint.UsersID,
                            Name = complaint.Name,
                            Description = complaint.Description,
                            status = complaint.status,
                            CreatedDate = complaint.CreatedDate,
                            Images = complaint.ImageDetails.Select(img => new GetImageDTOs
                            {
                                Id=img.Id,
                                Name=img.Image
                            }) .ToList(),
                        });
                }
                return Ok(complaints);
            }
            return Unauthorized();
        }

        [HttpPut("EditStatus")]
        public async Task<IActionResult> EditStatus(EditStatusComplaintDTOs request)
        {
            string token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token Is Missing" });
            int? userId = ExtractClaims.ExtractUserId(token);
            if (string.IsNullOrEmpty(userId.ToString()))
                return Unauthorized(new { message = "Token Is Missing" });
            ApplicationUser requestUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var role = await userManager.GetRolesAsync(requestUser);

            if (role.Contains("admin"))
            {
                Complaint result = await dbContext.complaints.FindAsync(request.ComplaintId);
                if (result is not null)
                {
                    result.status = request.NewStatus;
                    dbContext.Update(result);
                    await dbContext.SaveChangesAsync();
                    return Ok(new { status = 200, message = "update successful" });
                }
                return BadRequest(new { status = 400, message = "complaint not found" });
            }
            return Unauthorized();
        }
    }
}
