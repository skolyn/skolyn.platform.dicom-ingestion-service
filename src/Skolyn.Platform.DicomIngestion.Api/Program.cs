using Skolyn.Platform.DicomIngestion.Application.Interfaces;
using Skolyn.Platform.DicomIngestion.Application.Services;
using Skolyn.Platform.DicomIngestion.Infrastructure.Messaging;
using Skolyn.Platform.DicomIngestion.Infrastructure.Services;
using Skolyn.Platform.DicomIngestion.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Services (Dependency Injection)
// ---------------------------------------------

// Add Controllers and API Explorer for Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Skolyn.Platform.DicomIngestion.Api", Version = "v1" });
});

// Configure strongly-typed settings objects using the Options Pattern
builder.Services.Configure<ObjectStorageSettings>(builder.Configuration.GetSection("ObjectStorage"));
builder.Services.Configure<MessageQueueSettings>(builder.Configuration.GetSection("MessageQueue"));

// Register Application Services
builder.Services.AddSingleton<DicomIngestionService>();

// Register Infrastructure Services based on the interfaces defined in the Application layer
// This is the core of Dependency Inversion Principle.
builder.Services.AddSingleton<IObjectStorageService, AwsS3Service>();
builder.Services.AddSingleton<IMessageQueueService, RabbitMqService>();

// Add AWS S3 Client
builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();

// Add Health Checks for Kubernetes probes
builder.Services.AddHealthChecks()
    .AddRabbitMQ(rabbitConnectionString: builder.Configuration["MessageQueue:ConnectionString"])
    .AddS3(options => {
        options.BucketName = builder.Configuration["ObjectStorage:BucketName"];
    });

// 2. Build the Application
// ---------------------------------------------
var app = builder.Build();

// 3. Configure the HTTP request pipeline
// ---------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map Health Check endpoints
app.MapHealthChecks("/health/ready", new() { Predicate = _ => true });
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });

app.Run();
