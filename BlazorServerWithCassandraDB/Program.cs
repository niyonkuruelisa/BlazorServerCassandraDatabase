using Blazored.LocalStorage;
using BlazorServerWithCassandraDB;
using BlazorServerWithCassandraDB.Backend.Database;
using BlazorServerWithCassandraDB.Data;
using Cassandra;
using MediatR;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMediatR(typeof(App).Assembly);
builder.Services.AddSingleton<WeatherForecastService>();
//setting cassandra database
var cassandraSettings = builder.Configuration.GetSection("Cassandra");
var contactPoints = cassandraSettings.GetSection("server").Get<string[]>();
var port = cassandraSettings.GetValue<int>("Port");
var keyspace = cassandraSettings.GetValue<string>("Database");
var username = cassandraSettings.GetValue<string>("Username");
var password = cassandraSettings.GetValue<string>("Password");

var cluster = Cluster.Builder()
    .AddContactPoints(contactPoints)
    .WithPort(port)
    .WithCredentials(username, password)
    .Build();
builder.Services.AddSingleton<ICluster>(cluster);
builder.Services.AddScoped(s => s.GetRequiredService<ICluster>().Connect(keyspace));



builder.Services.AddBlazoredLocalStorage();// Add services to the container.


builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseWebRoot("wwwroot");
builder.WebHost.UseStaticWebAssets();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "Application/octet-stream" }
        );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var session = scope.ServiceProvider.GetRequiredService<Cassandra.ISession>();
    // use context
    await CassandraDatabaseInitializer.Initialize(session);
}
app.UseStaticFiles();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();
// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapBlazorHub();
    endpoints.MapControllers();
    endpoints.MapFallbackToPage("{*path:regex(^(?!api).*$)}", "/_Host"); // don't match paths beginning with api
});

app.Run();
