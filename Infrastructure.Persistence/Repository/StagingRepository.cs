using Core.Application.DTO;
using Core.Application.Interfaces;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Fact;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repository
{
    public class StagingRepository : IStagingRepository
    {
        private readonly DWOpinionesContext _context;

        public StagingRepository(DWOpinionesContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> SaveFactOpinionAsync(IEnumerable<FactOpinionDto> records)
        {
            if (records == null) return 0;

            var entities = records.Select(r => new FactOpinionRecord
            {
                CustomerKey = r.CustomerKey,
                DateKey = r.DateKey,
                ProductKey = r.ProductKey,
                SourceKey = r.SourceKey,
                SentimentKey = r.SentimentKey,
                ChannelKey = r.ChannelKey,
                ETLBatchKey = r.ETLBatchKey,
                SentimentScore = r.SentimentScore,
                SatisfactionScore = r.SatisfactionScore,
                CommentText = r.CommentText ?? string.Empty,
                IngestionTimestamp = DateTime.UtcNow
            });

            await _context.FactOpinion.AddRangeAsync(entities);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveFactSurveyResponseAsync(IEnumerable<FactSurveyResponseDto> records)
        {
            if (records == null) return 0;

            var entities = records.Select(r => new FactSurveyResponseRecord
            {
                SurveyQuestionKey = r.SurveyQuestionKey,
                CustomerKey = r.CustomerKey,
                DateKey = r.DateKey,
                ProductKey = r.ProductKey,
                SourceKey = r.SourceKey,
                ChannelKey = r.ChannelKey,
                ETLBatchKey = r.ETLBatchKey,
                ResponseValue = r.ResponseValue,
                ResponseTimeSec = r.ResponseTimeSec,
                IsValidResponse = r.IsValidResponse,
                IngestionTimestamp = DateTime.UtcNow
            });

            await _context.FactSurveyResponse.AddRangeAsync(entities);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveFactEngagementAsync(IEnumerable<FactEngagementDto> records)
        {
            if (records == null) return 0;

            var entities = records.Select(r => new FactEngagementRecord
            {
                CustomerKey = r.CustomerKey,
                DateKey = r.DateKey,
                ProductKey = r.ProductKey,
                SourceKey = r.SourceKey,
                ChannelKey = r.ChannelKey,
                ETLBatchKey = r.ETLBatchKey,
                LikesCount = r.LikesCount,
                SharesCount = r.SharesCount,
                CommentsCount = r.CommentsCount,
                ViewsCount = r.ViewsCount,
                RepliesCount = r.RepliesCount,
                EngagementRate = r.EngagementRate,
                IngestionTimestamp = DateTime.UtcNow
            });

            await _context.FactEngagement.AddRangeAsync(entities);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> SaveFactProductSummaryAsync(IEnumerable<FactProductSummaryDto> records)
        {
            if (records == null) return 0;

            var entities = records.Select(r => new FactProductSummaryRecord
            {
                DateKey = r.DateKey,
                ProductKey = r.ProductKey,
                SourceKey = r.SourceKey,
                ChannelKey = r.ChannelKey,
                ETLBatchKey = r.ETLBatchKey,
                SentimentKey = r.SentimentKey,
                TotalOpinions = r.TotalOpinions,
                AvgSentimentScore = r.AvgSentimentScore,
                AvgSatisfaction = r.AvgSatisfaction,
                PositivePercent = r.PositivePercent,
                NegativePercent = r.NegativePercent,
                NeutralPercent = r.NeutralPercent
            });

            await _context.FactProductSummary.AddRangeAsync(entities);
            return await _context.SaveChangesAsync();
        }

        public async Task ClearTableAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return;
            // Usar DELETE para evitar problemas con FK/TRIGGERS si TRUNCATE no está permitido
            var sql = $"DELETE FROM {tableName}";
            await _context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
