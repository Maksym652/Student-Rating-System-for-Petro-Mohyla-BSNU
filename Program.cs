using StudentRatingSystemWebApp;
using StudentRatingSystemWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/AdditionalPoints");
    options.Conventions.AuthorizePage("/ChangePassword");
    options.Conventions.AuthorizePage("/Cources");
    options.Conventions.AuthorizePage("/Departments");
    options.Conventions.AuthorizePage("/Faculties");
    options.Conventions.AuthorizePage("/Files");
    options.Conventions.AuthorizePage("/Group");
    options.Conventions.AuthorizePage("/Groups");
    options.Conventions.AuthorizePage("/Index");
    options.Conventions.AuthorizePage("/Marks");
    options.Conventions.AuthorizePage("/Profile");
    options.Conventions.AuthorizePage("/Rating");
    options.Conventions.AuthorizePage("/Scholarship");
    options.Conventions.AuthorizePage("/SignOut");
    options.Conventions.AuthorizePage("/Users");
});
builder.Services.AddAuthentication("Cookies")
    .AddCookie(
        options =>
        {
            options.LoginPath = "/SignIn";
            options.LogoutPath = "/SignOut";
            options.AccessDeniedPath = "/Errors/403";
        }
    );
builder.Services.AddAuthorization();

builder.Services.AddSingleton<HashCalculator>();
builder.Services.AddSingleton<ScholarshipCalculator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Errors/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();