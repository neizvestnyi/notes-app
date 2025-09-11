using Microsoft.EntityFrameworkCore.Storage;
using NotesApp.Data.Interfaces;
using NotesApp.Data.Repositories;

namespace NotesApp.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly NotesDbContext _context;
    private IDbContextTransaction? _transaction;
    private INoteRepository? _noteRepository;

    public UnitOfWork(NotesDbContext context)
    {
        _context = context;
    }

    public INoteRepository Notes => _noteRepository ??= new NoteRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}