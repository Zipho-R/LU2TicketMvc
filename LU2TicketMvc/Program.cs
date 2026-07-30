var builder = WebApplication.CreateBuilder(args); //creates the application builder

// Add services to the container.
builder.Services.AddControllersWithViews(); //this registers MVC services

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting(); //this enables routing

app.UseAuthorization();

app.MapControllerRoute( //this sets the default route
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run(); //this starts the application
