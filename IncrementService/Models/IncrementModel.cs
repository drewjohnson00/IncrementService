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
        private IncrementContext _context;

        public IncrementModel(IncrementContext context)
        {
            _context = context;
        }

        public DataResultDto AddIncrement(string incrementKey, long initialValue)
        {
            IncrementDto increment = _context.Increments.FirstOrDefault(x => x.Key == incrementKey);

            if (increment != null)  // does key exist?
            {
                return new DataResultDto(false, 0, "Key already exists.", null);
            }

            _context.Increments.Add(new IncrementDto { Key = incrementKey, LastUsed = DateTimeOffset.Now, NextValue = initialValue });
            _context.SaveChanges();

            return new DataResultDto(true, 0, "", new List<IncrementDto>{increment});
        }

        public DataResultDto GetAllIncrements()
        {
            List<IncrementDto> increments = _context.Increments.ToList();
            return new DataResultDto(true, 0, "", increments);
        }

        public DataResultDto GetIncrement(string incrementKey)
        {
            IncrementDto increment = _context.Increments.FirstOrDefault(x => x.Key == incrementKey);

            if (increment == null)
            {
                return new DataResultDto(false, 0, "Key not found.", null);
            }

            return new DataResultDto(true, 0, "", new List<IncrementDto> { increment });
        }

        public DataResultDto Increment(string IncrementKey)
        {
            IncrementDto increment = _context.Increments.FirstOrDefault(x => x.Key == IncrementKey);

            if (increment == null)
            {
                return new DataResultDto(false, 0, "Key not found.", null);
            }

            increment.NextValue++;
            increment.LastUsed = DateTimeOffset.Now ;

            _context.SaveChanges();

            return new DataResultDto(true, 0, "", new List<IncrementDto> { increment });
        }

        public DataResultDto RemoveIncrement(string incrementKey)
        {
            IncrementDto increment = _context.Increments.FirstOrDefault(X => X.Key == incrementKey);

            if (increment == null)
            {
                return new DataResultDto(false, 0, "Key not found.", null);
            }

            _context.Increments.Remove(increment);

            _context.SaveChanges();

            return new DataResultDto(true, 0, "", null);
        }
    }
}
