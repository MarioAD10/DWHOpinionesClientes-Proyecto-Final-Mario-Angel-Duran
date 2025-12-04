using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Infrastructure.Persistence.Entities.DWH.Fact;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context
{
    /// <summary>
    /// Contexto de Entity Framework para el Data Warehouse de Opiniones de Clientes.
    /// </summary>
    public class DWOpinionesContext : DbContext
    {
        public DWOpinionesContext(DbContextOptions<DWOpinionesContext> options)
            : base(options)
        {
        }

        // ======== DIMENSIONES ========
        public DbSet<DimCustomerRecord> DimCustomer { get; set; }
        public DbSet<DimDateRecord> DimDate { get; set; }
        public DbSet<DimProductRecord> DimProduct { get; set; }
        public DbSet<DimSourceRecord> DimSource { get; set; }
        public DbSet<DimChannelRecord> DimChannel { get; set; }
        public DbSet<DimETLBatchRecord> DimETLBatch { get; set; }
        public DbSet<DimSentimentRecord> DimSentiment { get; set; }
        public DbSet<DimSurveyQuestionRecord> DimSurveyQuestion { get; set; }

        // ======== HECHOS (FACT TABLES) ========
        public DbSet<FactOpinionRecord> FactOpinion { get; set; }
        public DbSet<FactEngagementRecord> FactEngagement { get; set; }
        public DbSet<FactSurveyResponseRecord> FactSurveyResponse { get; set; }
        public DbSet<FactProductSummaryRecord> FactProductSummary { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======== DIMENSIONES (Schema: Dimension) ========
            modelBuilder.Entity<DimCustomerRecord>().ToTable("Dim_Customer", "Dimension");
            modelBuilder.Entity<DimProductRecord>().ToTable("Dim_Product", "Dimension");
            modelBuilder.Entity<DimDateRecord>().ToTable("Dim_Date", "Dimension");
            modelBuilder.Entity<DimChannelRecord>().ToTable("Dim_Channel", "Dimension");
            modelBuilder.Entity<DimSourceRecord>().ToTable("Dim_Source", "Dimension");
            modelBuilder.Entity<DimSentimentRecord>().ToTable("Dim_Sentiment", "Dimension");
            modelBuilder.Entity<DimSurveyQuestionRecord>().ToTable("Dim_SurveyQuestion", "Dimension");
            modelBuilder.Entity<DimETLBatchRecord>().ToTable("Dim_ETL_Batch", "Dimension");

            // ======== HECHOS (Schema: Fact) ========
            modelBuilder.Entity<FactOpinionRecord>().ToTable("Fact_Opinion", "Fact");
            modelBuilder.Entity<FactEngagementRecord>().ToTable("Fact_Engagement", "Fact");
            modelBuilder.Entity<FactSurveyResponseRecord>().ToTable("Fact_SurveyResponse", "Fact");
            modelBuilder.Entity<FactProductSummaryRecord>().ToTable("Fact_ProductSummary", "Fact");
        }
    }
}
