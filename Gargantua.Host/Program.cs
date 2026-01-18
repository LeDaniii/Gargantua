using Gargantua.Providers.Siemens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSiemensProvider(new SiemensProviderOptions(
    IpAddress: "192.168.0.10",
    CpuTypeName: "S7-1500",
    Rack: 0,
    Slot: 1,
    PlcIdentifier: "VmPlc01"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
