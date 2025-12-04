using CairoGo.Models.DbContextApp;
using CairoGo.Models.Entity;
using CairoGo.Repository.Implementations;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace CairoGo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            builder.Services.AddDbContext<CairoGoDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddIdentity<UserApplication, IdentityRole<Guid>>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 8;
                // User settings
                options.User.RequireUniqueEmail = true;
                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<CairoGoDbContext>()
            .AddDefaultTokenProviders();
            var jwt = builder.Configuration.GetSection("Jwt");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt["Key"])
                    ),
                    ClockSkew = TimeSpan.Zero // Remove 5 minute default tolerance
                };

                // Handle authentication failures
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            success = false,
                            message = "You are not authorized to access this resource"
                        });
                        return context.Response.WriteAsync(result);
                    }
                };
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                            "https://mega-project-eta.vercel.app", //  frontend
                            "http://localhost:3000",
                            "http://localhost:5173"  // Vite default
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("Token-Expired"); // Allow frontend to read custom headers
                });
            });
            builder.Services.AddScoped(typeof(IBaseRepo<>), typeof(Repository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IJwtTokenRepository, JwtTokenRepository>();
            builder.Services.AddScoped<IPlaceRepository, PlaceRepository>();
            builder.Services.AddScoped<IPlaceVibeTagRepo, PlaceVibeTagRepo>();
            builder.Services.AddScoped<IPlaceOperatingHoursRepo, PlaceOperatingHoursRepo>();
            builder.Services.AddScoped<ITripPlaneRepo, TripPlaneRepo>();
            builder.Services.AddScoped<ITripDayRepo, TripDayRepo>();
            builder.Services.AddScoped<ITripSlotRepo, TripSlotRepo>();
            builder.Services.AddScoped<IPreferenceRepo, PreferenceRepo>();
            builder.Services.AddScoped<IActivityTypeRepo, ActivityTypeRepo>();
            builder.Services.AddScoped<IUserPreferenceSignalRepository, UserPreferenceSignalRepository>();
            builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IModelVersionRepository, ModelVersionRepository>();
            builder.Services.AddScoped<IRecommendationLogRepository, RecommendationLogRepository>();
            builder.Services.AddScoped<IExperimentAssignmentRepo, ExperimentAssignmentRepo>();
            builder.Services.AddScoped<ISearchSessionRepo, SearchSessionRepo>();
            builder.Services.AddScoped<ITrendingTagRepository, TrendingTagRepository>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CairoGo API",
                    Version = "v1",
                    Description = "Travel planning API for Cairo with complete authentication",
                    Contact = new OpenApiContact
                    {
                        Name = "CairoGo Team",
                        Email = "support@cairogo.com"
                    }
                });
                c.UseInlineDefinitionsForEnums();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                                  "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                  "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
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
                        Array.Empty<string>()
                    }
                });
            });
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CairoGo API V1");
                    c.RoutePrefix = string.Empty; // Swagger at root
                });
            }
            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");        // 1. CORS first
            app.UseAuthentication();              // 2. Then Authentication
            app.UseAuthorization();               // 3. Then Authorization

            app.MapControllers();
            app.Run();
        }
    }
}