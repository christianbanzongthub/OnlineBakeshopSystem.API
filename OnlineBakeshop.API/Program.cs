using OnlineBakeshop.API.Class;
using OnlineBakeshop.API.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace OnlineBakeshop.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])
                    )
                };
            });

            builder.Services.AddAuthorization();

            // ✅ CORS POLICY
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddScoped<IAuthRepository, AuthClass>();
            builder.Services.AddScoped<ICustomOrderRepository, CustomOrderClass>();
            builder.Services.AddScoped<IUserRepository, UserClass>();
            builder.Services.AddScoped<IAdminRepository, AdminClass>();
            builder.Services.AddScoped<IValidationRepository, ValidationClass>();
            builder.Services.AddScoped<ICategoryRepository, CategoryClass>();
            builder.Services.AddScoped<IOrderRepository, OrderClass>();
            builder.Services.AddScoped<IProductRepository, ProductClass>();
            builder.Services.AddScoped<IRegisterRepository, RegisterClass>();
            builder.Services.AddScoped<ILoginRepository, LoginClass>();
            builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
            builder.Services.AddScoped<IDeviceRepository, DeviceClass>();
            

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "OnlineBakeshop API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] your token"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();
            
            // Firebase Admin initialization via GOOGLE_APPLICATION_CREDENTIALS
            {
                var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

                if (string.IsNullOrWhiteSpace(credentialsPath))
                {
                    app.Logger.LogError("Firebase not initialized: GOOGLE_APPLICATION_CREDENTIALS is not set.");
                    throw new InvalidOperationException(
                        "Missing GOOGLE_APPLICATION_CREDENTIALS environment variable.");
                }

                if (!Path.IsPathRooted(credentialsPath))
                {
                    credentialsPath = Path.GetFullPath(
                        Path.Combine(app.Environment.ContentRootPath, credentialsPath));
                }

                if (!File.Exists(credentialsPath))
                {
                    app.Logger.LogError(
                        "Firebase credentials file not found at: {CredentialsPath}",
                        credentialsPath);
                    throw new InvalidOperationException(
                        $"Firebase credentials file not found: {credentialsPath}");
                }

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.GetApplicationDefault()
                    });

                    app.Logger.LogInformation(
                        "Firebase Admin initialized successfully using: {CredentialsPath}",
                        credentialsPath);
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // 🔥 IMPORTANT: CORS must come BEFORE StaticFiles
            app.UseCors("AllowAll");

            // ✅ This now includes CORS headers for images
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}