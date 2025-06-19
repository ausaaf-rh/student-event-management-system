using Microsoft.EntityFrameworkCore;
using StudentEventAPI.Data;
using StudentEventAPI.Services;

var systemBuilder = WebApplication.CreateBuilder(args);

// Configure fundamental services
systemBuilder.Services.AddControllers();
systemBuilder.Services.AddEndpointsApiExplorer();
systemBuilder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new() { Title = "University Gathering Management API", Version = "v1" });
});

// Database setup with connection resilience
systemBuilder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(systemBuilder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Service layer dependency injection
systemBuilder.Services.AddScoped<IEventManagementService, EventManagementService>();
systemBuilder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();
systemBuilder.Services.AddScoped<IFeedbackProcessingService, FeedbackProcessingService>();

var universityApp = systemBuilder.Build();

// Configure request pipeline
if (universityApp.Environment.IsDevelopment())
{
    universityApp.UseSwagger();
    universityApp.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "University Gathering Management API V1");
        options.RoutePrefix = string.Empty;
    });
}

universityApp.UseHttpsRedirection();
universityApp.UseRouting();
universityApp.UseAuthorization();
universityApp.MapControllers();

await universityApp.RunAsync();
