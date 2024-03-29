using MediCase.WebAPI.Entities.Admin;
using MediCase.WebAPI.Entities.Content;
using MediCase.WebAPI.Entities.Moderator;
using MediCase.WebAPI.Jobs;
using MediCase.WebAPI.Middleware;
using MediCase.WebAPI.Services.Interfaces;
using MediCase.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using Quartz;
using System.Reflection;
using MediCase.WebAPI;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using MediCase.WebAPI.Models.Account.Validators;
using MediCase.WebAPI.Models.Account;
using MediCase.WebAPI.Models.Group.Validators;
using MediCase.WebAPI.Models.Group;
using MediCase.WebAPI.Models.User.Validators;
using MediCase.WebAPI.Models.User;
using MediCase.WebAPI.Repositories.Interfaces;
using MediCase.WebAPI.Repositories;
using FluentValidation.AspNetCore;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    var authenticationSettings = new AuthenticationSettings();

    builder.Configuration.GetSection("Authentication").Bind(authenticationSettings);

    builder.Services.AddSingleton(authenticationSettings);
    builder.Services.AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = "Bearer";
        option.DefaultScheme = "Bearer";
        option.DefaultChallengeScheme = "Bearer";
    }).AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false;
        cfg.SaveToken = true;
        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = authenticationSettings.JwtIssuer,
            ValidAudience = authenticationSettings.JwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)),
        };
    });

    // Add services to the container.
    builder.Services.AddDbContext<MediCaseAdminContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("Admin"), ServerVersion.Parse("10.6.14-mariadb")));

    builder.Services.AddDbContext<MediCaseModeratorContext>(options => 
        options.UseMySql(builder.Configuration.GetConnectionString("Moderator"), ServerVersion.Parse("10.6.14-mariadb")));

    builder.Services.AddDbContext<MediCaseContentContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("Content"), ServerVersion.Parse("10.6.14-mariadb")));

    // Add Quartz services
    builder.Services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();
        var JobKey = new JobKey("DeleteOutdatedEntitiesJob");
        q.AddJob<DeleteOutdatedEntitiesJob>(opts => opts.WithIdentity(JobKey));

        q.AddTrigger(opts => opts
            .ForJob(JobKey)
            .WithIdentity("DeleteOutdatedEntitiesJob-trigger")
            // Fire at 00:00:00 every day
            .WithCronSchedule("0 0 0 * * ?")
        );

        q.AddTrigger(opts => opts
            .ForJob(JobKey)
            .WithIdentity("DeleteOutdatedEntitiesJob-trigger2")
            .StartNow()
        );
    });
    builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

    builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddDateOnlyTimeOnlyStringConverters();

    // Add AutoMapper to the container.
    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

    builder.Services.AddFluentValidationAutoValidation();

    builder.Services.AddHttpContextAccessor();

    // Add middleware
    builder.Services.AddScoped<ErrorHandlingMiddleware>();
    builder.Services.AddScoped<RequestTimeMiddleware>();

    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IGroupService, GroupService>();
    builder.Services.AddScoped<IAccountService, AccountService>();

    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IModEntityService, ModEntityService>();
    builder.Services.AddScoped<IEntityService, EntityService>();
    builder.Services.AddScoped<ISynchronizationService, SynchronizationService>();
    builder.Services.AddScoped<ITranslationGeneratorService, TranslationGeneratorService>();
    builder.Services.AddScoped<IImageGeneratorService, ImageGeneratorService>();
    builder.Services.AddScoped<IVoiceGeneratorService, VoiceGeneratorService>();

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IGroupRepository, GroupRepository>();

    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Moderator.Interfaces.IEntityTypeRepository, MediCase.WebAPI.Repositories.Moderator.EntityTypeRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Moderator.Interfaces.IEntityLanguageRepository, MediCase.WebAPI.Repositories.Moderator.EntityLanguageRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Moderator.Interfaces.IEntityRepository, MediCase.WebAPI.Repositories.Moderator.EntityRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Moderator.Interfaces.IEntityGraphDataRepository, MediCase.WebAPI.Repositories.Moderator.EntityGraphDataRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Moderator.Interfaces.IEntityTranslationsRepository, MediCase.WebAPI.Repositories.Moderator.EntityTranslationsRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Moderator.Interfaces.IEntityTranslationFilesRepository, MediCase.WebAPI.Repositories.Moderator.EntityTranslationFilesRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Moderator.Interfaces.IModeratorQueryBucketRepository, MediCase.WebAPI.Repositories.Moderator.ModeratorQueryBucketRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Moderator.Interfaces.IMediCaseModTransactionRepository, MediCase.WebAPI.Repositories.Moderator.MediCaseModTransactionRepository>();

    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Content.Interfaces.IEntityTypeRepository, MediCase.WebAPI.Repositories.Content.EntityTypeRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Content.Interfaces.IEntityLanguageRepository, MediCase.WebAPI.Repositories.Content.EntityLanguageRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Content.Interfaces.IEntityRepository, MediCase.WebAPI.Repositories.Content.EntityRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Content.Interfaces.IEntityGraphDataRepository, MediCase.WebAPI.Repositories.Content.EntityGraphDataRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Content.Interfaces.IEntityTranslationsRepository, MediCase.WebAPI.Repositories.Content.EntityTranslationsRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Content.Interfaces.IEntityTranslationFilesRepository, MediCase.WebAPI.Repositories.Content.EntityTranslationFilesRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Content.Interfaces.ISynchronizationRepository, MediCase.WebAPI.Repositories.Content.SynchronizationRepository>();
    builder.Services.AddScoped<MediCase.WebAPI.Repositories.Content.Interfaces.IMediCaseTransactionRepository, MediCase.WebAPI.Repositories.Content.MediCaseTransactionRepository>();

    builder.Services.AddScoped<IValidator<UserDto>, UserDtoValidator>();
    builder.Services.AddScoped<IValidator<UserNameDto>, UserNameDtoValidator>();
    builder.Services.AddScoped<IValidator<UserPasswordDto>, UserPasswordDtoValidator>();
    builder.Services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdateEmailDto>, UpdateEmailDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdatePasswordDto>, UpdatePasswordDtoValidator>();
    builder.Services.AddScoped<IValidator<UpdateNameDto>, UpdateNameDtoValidator>();
    builder.Services.AddScoped<IValidator<UserQuery>, UserQueryValidator>();
    builder.Services.AddScoped<IValidator<GroupDto>, GroupDtoValidator>();
    builder.Services.AddScoped<IValidator<GroupDateDto>, GroupDateDtoValidator>();
    builder.Services.AddScoped<IValidator<GroupNameDto>, GroupNameDtoValidator>();
    builder.Services.AddScoped<IValidator<GroupDescDto>, GroupDescDtoValidator>();
    builder.Services.AddScoped<IValidator<GroupQuery>, GroupQueryValidator>();

    builder.Services.AddScoped<Seeder>();

    builder.Services.AddCors(opts =>
    {
        opts.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowAnyOrigin();
        });
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => c.UseDateOnlyTimeOnlyStringConverters());

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
        seeder.Seed();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<RequestTimeMiddleware>();

    app.UseAuthentication();

    app.UseHttpsRedirection();

    app.UseCors();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    //NLog: catch setup errors
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}