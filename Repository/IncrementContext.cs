using System.Data;
using Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository;

internal class IncrementContext(DbContextOptions<IncrementContext> options) : DbContext(options)
{
    internal required DbSet<Keys> Keys { get; set; }

    /// <summary>
    /// The default override of SaveChangesAsync is modified to throw DataException if no records are saved. If this behavior is not desired,
    /// use the new SaveChangesAsync method with the throwError parameter set to false.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>The number of records modified.</returns>
    /// <exception cref="DataException">Exception will be thrown if no records were saved.</exception>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (result > 0) return result;

        throw new DataException("No records saved into the Database, Please Contact LIS Team");
    }

    /// <summary>
    /// This method allows the caller to specify whether to throw an error if no records are saved.
    /// If throwError is true, the default behavior is used (DataException is thrown if no records are saved).
    /// If throwError is false, no exception is thrown and the number of records saved is returned (which may be zero).
    /// Note that this method hides the base SaveChangesAsync method that takes a boolean parameter acceptAllChangesOnSuccess.
    /// </summary>
    /// <param name="throwError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal new async Task<int> SaveChangesAsync(bool throwError, CancellationToken cancellationToken = default)
    {
        if (throwError)
        {
            return await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

