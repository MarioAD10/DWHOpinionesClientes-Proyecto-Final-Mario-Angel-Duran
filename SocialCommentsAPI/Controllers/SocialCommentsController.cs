using Microsoft.AspNetCore.Mvc;

namespace SocialCommentsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialCommentsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetComments()
        {
            var comments = new[]
            {
                new { IdComentario = 1, IdCliente = 100, IdProducto = 10, Fecha = DateTime.UtcNow, Comentario = "Excelente servicio", Likes = 45, Shares = 3, Plataforma = "Facebook" },
                new { IdComentario = 2, IdCliente = 200, IdProducto = 12, Fecha = DateTime.UtcNow, Comentario = "No me gustó el producto", Likes = 12, Shares = 0, Plataforma = "Twitter" },
                new { IdComentario = 3, IdCliente = 300, IdProducto = 15, Fecha = DateTime.UtcNow, Comentario = "Lo recomendaría sin dudas", Likes = 80, Shares = 10, Plataforma = "Instagram" },
                new { IdComentario = 4, IdCliente = 400, IdProducto = 25, Fecha = DateTime.UtcNow, Comentario = "Compra obligatoria", Likes = 100, Shares = 25, Plataforma = "Instagram" },
            };

            return Ok(comments);
        }
    }
}
