using Core.Application.DTO.DimDto;
using Core.Application.Interfaces.ILoaders;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Entities.DWH.Dimensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Loaders
{
    /// <summary>
    /// Loader para la dimensión de preguntas de encuestas.
    /// </summary>
    public class SurveyQuestionDimensionLoader : BaseDimensionLoader, ISurveyQuestionDimensionLoader
    {
        public SurveyQuestionDimensionLoader(
            DWOpinionesContext context,
            ILogger<SurveyQuestionDimensionLoader> logger)
            : base(context, logger)
        {
        }


        /// <summary>
        /// Inicializa el catálogo de preguntas estándar de encuestas.
        /// </summary>
        public async Task InitializeSurveyQuestionCatalogAsync()
        {
            _logger.LogInformation("Inicializando catálogo de preguntas de encuesta...");

            var standardQuestions = new List<SurveyQuestionDto>
    {
        new SurveyQuestionDto
        {
            QuestionText = "¿Qué tan satisfecho está con este producto?",
            QuestionType = "Escala",
            ScaleMin = 1,
            ScaleMax = 5
        },
        new SurveyQuestionDto
        {
            QuestionText = "¿Recomendaría este producto a otros?",
            QuestionType = "Escala",
            ScaleMin = 1,
            ScaleMax = 5
        },
        new SurveyQuestionDto
        {
            QuestionText = "¿Cómo calificaría la calidad del producto?",
            QuestionType = "Escala",
            ScaleMin = 1,
            ScaleMax = 5
        },
        new SurveyQuestionDto
        {
            QuestionText = "¿El producto cumplió con sus expectativas?",
            QuestionType = "Escala",
            ScaleMin = 1,
            ScaleMax = 5
        }
    };

            await LoadQuestionsAsync(standardQuestions);

            _logger.LogInformation("Catálogo de preguntas de encuesta inicializado");
        }


        /// <summary>
        /// Carga múltiples preguntas de encuesta en la dimensión.
        /// </summary>
        public async Task<int> LoadQuestionsAsync(IEnumerable<SurveyQuestionDto> questions)
        {
            if (questions == null || !questions.Any())
            {
                _logger.LogWarning("⚠️ No hay preguntas para cargar.");
                return 0;
            }

            _logger.LogInformation($"❓ Iniciando carga de {questions.Count()} preguntas de encuesta...");

            int insertedCount = 0;

            foreach (var question in questions)
            {
                if (string.IsNullOrWhiteSpace(question.QuestionText))
                {
                    _logger.LogWarning("Pregunta sin texto, omitiendo...");
                    continue;
                }

                var existingQuestion = await _context.DimSurveyQuestion
                    .FirstOrDefaultAsync(q => q.QuestionText == question.QuestionText);

                if (existingQuestion == null)
                {
                    var newQuestion = new DimSurveyQuestionRecord
                    {
                        QuestionText = question.QuestionText,
                        QuestionType = question.QuestionType ?? "Texto",
                        ScaleMin = question.ScaleMin,
                        ScaleMax = question.ScaleMax
                    };

                    await _context.DimSurveyQuestion.AddAsync(newQuestion);
                    insertedCount++;
                }
            }

            if (insertedCount > 0)
            {
                await SaveChangesAsync();
                _logger.LogInformation($"{insertedCount} preguntas cargadas.");
            }
            else
            {
                _logger.LogInformation("Todas las preguntas ya existen.");
            }

            return insertedCount;
        }

        /// <summary>
        /// Obtiene o crea la clave de una pregunta específica.
        /// </summary>
        public async Task<int> GetOrCreateQuestionKeyAsync(string questionText)
        {
            if (string.IsNullOrWhiteSpace(questionText))
            {
                _logger.LogWarning("⚠️ Intento de buscar pregunta sin texto.");
                return -1; // O lanzar excepción según tu lógica
            }

            var question = await _context.DimSurveyQuestion
                .FirstOrDefaultAsync(q => q.QuestionText == questionText);

            if (question != null)
            {
                return question.SurveyQuestionKey;
            }

            // Crear nueva pregunta con valores por defecto
            var newQuestion = new DimSurveyQuestionRecord
            {
                QuestionText = questionText,
                QuestionType = "Texto",
                ScaleMin = 1,
                ScaleMax = 5
            };

            await _context.DimSurveyQuestion.AddAsync(newQuestion);
            await SaveChangesAsync();

            _logger.LogInformation($"🆕 Nueva pregunta creada: {questionText} - Key: {newQuestion.SurveyQuestionKey}");
            return newQuestion.SurveyQuestionKey;
        }
    }
}
