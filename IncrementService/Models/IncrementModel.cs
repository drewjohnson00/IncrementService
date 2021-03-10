using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            IncrementRow increment = _context.Increments.FirstOrDefault(x => x.Key == incrementKey);

            if (increment != null)  // does key exist?
            {
                return new ModelResponse(false, 0, "Key already exists.", null);
            }

            _context.Increments.Add(new IncrementRow { Key = incrementKey, LastUsed = DateTimeOffset.Now, PreviousValue = initialValue });
            _context.SaveChanges();

            return new ModelResponse(true, 0, "", new List<IncrementRow>{increment});
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

        public ModelResponse Increment(string IncrementKey)
        {
            IncrementRow increment = _context.Increments.FirstOrDefault(x => x.Key == IncrementKey);

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
            IncrementRow increment = _context.Increments.FirstOrDefault(X => X.Key == incrementKey);

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
