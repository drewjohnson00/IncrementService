using System;
using System.Collections.Generic;
using System.Linq;
using IncrementService.Data;


namespace IncrementService.Models
{
    public class IncrementModel : IIncrementData
    {
        private readonly IncrementContext _context;

        public IncrementModel(IncrementContext context)
        {
            _context = context;
        }

        public ModelResponse AddIncrement(string incrementKey, long initialValue)
        {
            IncrementRow incrementRow = _context.Increments.FirstOrDefault(x => x.Key == incrementKey);

            if (incrementRow != null)
            {
                return new ModelResponse(false, 0, "Key already exists.", null);
            }

            incrementRow = new IncrementRow {Key = incrementKey, LastUsed = DateTimeOffset.Now, PreviousValue = initialValue};

            _context.Increments.Add(incrementRow);
            _context.SaveChanges();

            return new ModelResponse(true, 0, "", new List<IncrementRow> { incrementRow });
        }

        public ModelResponse GetAllIncrements()
        {
            List<IncrementRow> increments = _context.Increments.ToList();
            return new ModelResponse(true, 0, "", increments);
        }

        public ModelResponse GetIncrement(string incrementKey)
        {
            IncrementRow increment = _context.Increments.FirstOrDefault(x => x.Key == incrementKey);

            if (increment == null)
            {
                return new ModelResponse(false, 0, "Key not found.", null);
            }

            return new ModelResponse(true, 0, "", new List<IncrementRow> { increment });
        }

        public ModelResponse Increment(string incrementKey)
        {
            IncrementRow increment = _context.Increments.FirstOrDefault(x => x.Key == incrementKey);

            if (increment == null)
            {
                return new ModelResponse(false, 0, "Key not found.", null);
            }

            increment.PreviousValue++;
            increment.LastUsed = DateTimeOffset.Now ;

            _context.SaveChanges();

            return new ModelResponse(true, 0, "", new List<IncrementRow> { increment });
        }

        public ModelResponse RemoveIncrement(string incrementKey)
        {
            IncrementRow increment = _context.Increments.FirstOrDefault(x => x.Key == incrementKey);

            if (increment == null)
            {
                return new ModelResponse(false, 0, "Key not found.", null);
            }

            _context.Increments.Remove(increment);

            _context.SaveChanges();

            return new ModelResponse(true, 0, "", null);
        }
    }
}
