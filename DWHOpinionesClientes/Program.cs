using Core.Application.DTO.APIDto;
using Core.Application.DTO.CSVDto;
using Core.Application.DTO.DBDto;
using Core.Application.Interfaces;
using Core.Application.Interfaces.ILoaders;
using Infrastructure.Extractors.Extractors.API;
using Infrastructure.Extractors.Extractors.CSV;
using Infrastructure.Extractors.Extractors.DB;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Loaders;
using Microsoft.EntityFrameworkCore;

namespace DWHOpinionesClientes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            // ============================================
            // CONFIGURACIÓN DE CONNECTION STRINGS
            // ============================================
            string dwConnectionString = builder.Configuration.GetConnectionString("DWOpinionesConnection")
                ?? throw new InvalidOperationException("Connection string 'DWOpinionesConnection' not found.");

            string dbWebReviewConnectionString = builder.Configuration.GetConnectionString("WebReviewConnection")
                ?? throw new InvalidOperationException("Connection string 'WebReviewConnection' not found.");

            // ============================================
            // REGISTRAR DbContext
            // ============================================
            builder.Services.AddDbContext<DWOpinionesContext>(options =>
                options.UseSqlServer(dwConnectionString));

            // ============================================
            // REGISTRAR EXTRACTORES (Capa de Extracción - E)
            // ============================================

            // Extractor CSV - CON STORED PROCEDURES
            builder.Services.AddSingleton<IExtractorGeneric<FactOpinionCsvDto>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<CsvOpinionExtractor>>();
                string csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "surveys_part1.csv");

                return new CsvOpinionExtractor(csvPath, logger, dwConnectionString);
            });

            // Extractor SQL Server
            builder.Services.AddSingleton<IExtractorGeneric<WebReviewDto>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<SqlWebReviewExtractor>>();

                return new SqlWebReviewExtractor(
                    dbWebReviewConnectionString,  
                    dwConnectionString,          
                    logger
                );
            });

            // Extractor API REST
            builder.Services.AddSingleton<IExtractorGeneric<SocialCommentDto>>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ApiSocialCommentsExtractor>>();
                string apiUrl = builder.Configuration["ApiSettings:SocialCommentsUrl"]
                    ?? "https://localhost:7085/api/socialcomments";

                return new ApiSocialCommentsExtractor(
                    apiUrl,
                    dwConnectionString,  
                    logger
                );
            });

            // ============================================
            // REGISTRAR LOADERS (Capa de Carga - L)
            // ============================================
            builder.Services.AddScoped<ICustomerDimensionLoader, CustomerDimensionLoader>();
            builder.Services.AddScoped<IProductDimensionLoader, ProductDimensionLoader>();
            builder.Services.AddScoped<IDateDimensionLoader, DateDimensionLoader>();
            builder.Services.AddScoped<ISourceDimensionLoader, SourceDimensionLoader>();
            builder.Services.AddScoped<IChannelDimensionLoader, ChannelDimensionLoader>();
            builder.Services.AddScoped<ISentimentDimensionLoader, SentimentDimensionLoader>();
            builder.Services.AddScoped<ISurveyQuestionDimensionLoader, SurveyQuestionDimensionLoader>();

            // ============================================
            // REGISTRAR WORKER (Background Service)
            // ============================================
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
