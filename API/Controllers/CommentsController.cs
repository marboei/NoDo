using API.Data;
using API.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
[ApiController]
public class CommentsController : ControllerBase {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public CommentsController(ApplicationDbContext context, UserManager<User> userManager) {
        _context = context;
        _userManager = userManager;
    }

    [Route("api/projects/{projectId}/columns/{columnId}/cards/{cardId}/comments")]
    [HttpGet]
    public async Task<IActionResult> GetComments(int projectId, int columnId, int cardId) {
        var comments = await _context.Comments.Include(c => c.User).Include(c => c.Likes).Where(c => c.CardId == cardId).ToListAsync();
        
        return Ok(comments);
    }
    
    [Route("api/projects/{projectId}/columns/{columnId}/cards/{cardId}/comments")]
    [HttpPost]
    public async Task<IActionResult> AddComment(int projectId, int columnId, int cardId, [FromBody]AddCommentDto dto) {
        var userAsync = await _userManager.FindByIdAsync(dto.UserId);
        var card = await _context.Cards.FindAsync(cardId);
        if (userAsync is not null && card is not null) {
            if (card.Comments is null) {
                card.Comments = new List<Comment>();
            }

            var comment = new Comment() {
                Text = dto.Text,
                User = userAsync
            };

            card.Comments.Add(comment);
            _context.SaveChanges();

            return Ok(card);
        }
        else {
            return NotFound();
        }
    }
    
    [Route("api/projects/{projectId}/columns/{columnId}/cards/{cardId}/comments/{id}")]
    [HttpDelete]
    public async Task<IActionResult> DeleteComment(int projectId, int columnId, int cardId, int id) {
        var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == id);
        _context.Comments.Remove(comment);
        _context.SaveChanges();
        return Ok(comment);
    }
}