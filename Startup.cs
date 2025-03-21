using Assignment.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TreasureMapContext>(
            options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            // Thay thế bằng chuỗi kết nối của bạn

        services.AddControllers();
        services.AddSwaggerGen();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", builder =>
            {
                builder.WithOrigins("http://localhost:3000") // Allow your React app's origin
                       .AllowAnyHeader()
                       .AllowAnyMethod(); // Or specific methods (GET, POST, etc.)
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TreasureMap API v1"));
        }
        //app.UseMiddleware<GlobalHandlerExceptionMiddleware>();
        //app.UseMiddleware<GlobalLoggingMiddleware>();
        app.UseCors("AllowReactApp"); // Apply the CORS policy

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}