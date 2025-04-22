using VitalSignApp.Hubs;
using VitalSignApp.Services;
namespace VitalSignApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Register HttpClient เพื่อใช้เรียกข้อมูลจาก API
            builder.Services.AddHttpClient("VitalSignApi", client =>
            {
                client.BaseAddress = new Uri("http://localhost:55/api/"); // URL ของ API
                //client.BaseAddress = new Uri("https://localhost:44367/api/"); // URL ของ API TEST
            });


            builder.Services.AddScoped<VitalSignApiService>(); // Register Service สำหรับการเรียก API

            // Add services to the container.
            builder.Services.AddRazorPages();


            builder.Services.AddSignalR();


            var app = builder.Build();
            app.MapGet("/", context =>
            {
                context.Response.Redirect("/Stats");
                return Task.CompletedTask;
            });
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();
            app.MapHub<VitalSignHub>("/vitalsignhub"); // กำหนดเส้นทางของ SignalR Hub

            app.Run();
        }
    }
}
