using Microsoft.AspNetCore.Mvc;
using MyCompany.Domain.Repositories.Abstract;
using MyCompany.Domain.Repositories.EntityFramework;
using MyCompany.Domain;
using Microsoft.EntityFrameworkCore;
using MyCompany.Service;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

//���������� ������ �� appsetting.json
builder.Configuration.Bind("Project", new Config());

//���������� ������ ���������� ���������� � �������� ��������
builder.Services.AddTransient<ITextFieldsRepository, EFTextFieldsRepository>();
builder.Services.AddTransient<IServiceItemsRepository, EFServiceItemsRepository>();
builder.Services.AddTransient<DataManager>();

//���������� �������� ��
builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(Config.ConnectionString));

//����������� identity �������
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opts =>
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireDigit = false;
}
 ).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

//����������� authentication cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "myCompanyAuth";
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/accessdenied";
    options.SlidingExpiration = true;
});

//����������� �������� ����������� ��� Admin area
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("AdminArea", policy => { policy.RequireRole("admin"); });
});

builder.Services.AddControllersWithViews(x =>
{
    x.Conventions.Add(new AdminAreaAuthorization("Admin", "AdminArea"));
});

var app = builder.Build();

//����� ������ ��������� ���������� �� �������
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    //app.UseExceptionHandler("/Error");
    //// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

/* ���.1 */
app.MapControllerRoute("admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
/* ���.2
app.UseEndpoints(ep => {
    ep.MapControllerRoute("admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
    ep.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
});*/
/* ���.3
app.UseMvc(routes => {
    routes.MapRoute("admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
    routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
});*/

app.Run();
