﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lab2.Data;
using lab2.Models;
using lab2.ViewModel;

namespace lab2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/Expenses
        [HttpGet]
        [Route("filter/{name}/{minDate}/{maxDate}")]
        public async Task<ActionResult<IEnumerable<Expense>>> FilterExpense(Types type, DateTime minDate, DateTime maxDate)
        {
            return await _context.Expense.Where(e=> e.Type == type && e.Date>=minDate && e.Date <= maxDate).ToListAsync();
        }

        // GET: api/Expenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpense()
        {
            return await _context.Expense.ToListAsync();
        }

        // GET: api/Expenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            var expense = await _context.Expense.FindAsync(id);

            if (expense == null)
            {
                return NotFound();
            }

            return expense;
        }
        [HttpGet("{id}/Comments")]
        public ActionResult<ExpensesWithCommentsViewModel> GetCommentsForExpense(int id)
        {
            var query = _context.Comments.Where(c => c.Expense.Id == id).Select(c => new ExpensesWithCommentsViewModel
            {
                Id = c.Expense.Id,
                Name = c.Expense.Name,
                Description = c.Expense.Description,
                Sum = c.Expense.Sum,
                Date = c.Expense.Date,
                Type = c.Expense.Type,
                Location = c.Expense.Location,
                Currency = c.Expense.Currency,
                Comments = c.Expense.Comments.Select(pc => new CommentViewModel
                {
                    Id = pc.Id,
                    Content = pc.Content,
                    DateTime = pc.DateTime,
                    Stars = pc.Stars
                })
            });

            return query.ToList()[0];
        }

        [HttpPost("{id}/Comments")]
        public IActionResult PostCommentForExpense(int id, Comment comment)
        {
            var expense = _context.Expense.Where(p => p.Id == id).Include(p => p.Comments).FirstOrDefault();
            if (expense == null)
            {
                return NotFound();
            }

            expense.Comments.Add(comment);
            _context.Entry(expense).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok();
        }


        // PUT: api/Expenses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return BadRequest();
            }

            _context.Entry(expense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Expenses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Expense>> PostExpense(Expense expense)
        {
            _context.Expense.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpense", new { id = expense.Id }, expense);
        }

        // DELETE: api/Expenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expense.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expense.Any(e => e.Id == id);
        }
    }
}
