using Core.Application.DTO.APIDto;
using Core.Application.DTO.CSVDto;
using Core.Application.DTO.DBDto;
using Core.Application.Interfaces;
using DWHOpinionesClientes;
using Infrastructure.Extractors.Extractors.API;
using Infrastructure.Extractors.Extractors.CSV;
using Infrastructure.Extractors.Extractors.DB;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddDbContext<DWOpinionesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DWOpinionesConnection")));


builder.Services.AddScoped<Core.Application.Interfaces.IStagingRepository, Infrastructure.Persistence.Repository.StagingRepository>();

builder.Services.AddSingleton<IExtractorGeneric<FactOpinionCsvDto>>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<CsvOpinionExtractor>>();
    string csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "surveys_part1.csv");
    return new CsvOpinionExtractor(csvPath, logger);
});

builder.Services.AddSingleton<IExtractorGeneric<WebReviewDto>>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<SqlWebReviewExtractor>>();
    string connectionString = builder.Configuration.GetConnectionString("WebReviewsConnection")
        ?? "Server=DESKTOP-V0S44VT;Database=DBWebReview;Trusted_Connection=True;TrustServerCertificate=True;";
    return new SqlWebReviewExtractor(connectionString, logger);
});

builder.Services.AddSingleton<IExtractorGeneric<SocialCommentDto>>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ApiSocialCommentsExtractor>>();
    string apiUrl = "https://localhost:7085/api/socialcomments"; 
    return new ApiSocialCommentsExtractor(apiUrl, logger);
});



builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
