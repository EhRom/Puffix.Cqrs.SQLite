using Microsoft.EntityFrameworkCore;
using Puffix.Cqrs.Models;
using Puffix.Cqrs.Repositories;
using Puffix.Cqrs.SQLite.Repositories.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Puffix.Cqrs.SQLite.Repositories;

/// <summary>
/// Fournisseur INMemory de répetoire de données.
/// </summary>
/// <typeparam name="AggregateT">Type de l'agrégat.</typeparam>
/// <typeparam name="IndexT">Type de l'index.</typeparam>
public class SqliteRepositoryProvider<AggregateImplementationT, AggregateT, IndexT> : IRepositoryProvider<AggregateImplementationT, AggregateT, IndexT>
    where AggregateImplementationT : class, AggregateT
    where AggregateT : IAggregate<IndexT>
    where IndexT : IComparable, IComparable<IndexT>, IEquatable<IndexT>
{
    /// <summary>
    /// Contexte de base de données.
    /// </summary>
    private readonly IDatabaseContext databaseContext;

    /// <summary>
    /// Dictionnaire de données.
    /// </summary>
    private readonly DbSet<AggregateImplementationT> data;

    /// <summary>
    /// Type des éléments stockés.
    /// </summary>
    public Type ElementType => typeof(AggregateT);

    /// <summary>
    /// Expression.
    /// </summary>
    public Expression Expression => data.AsQueryable().Expression;

    /// <summary>
    /// Constructeur de requête.
    /// </summary>
    public IQueryProvider Provider => data.AsQueryable().Provider;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="dbPath">Chemin de la base de données.</param>
    /// <param name="aggregateInfo">Informations sur l'agrégat.</param>
    public SqliteRepositoryProvider(IDatabaseContext databaseContext, AggregateInfo aggregateInfo)
    {
        this.databaseContext = databaseContext;
        databaseContext.EnsureDatabaseCreated();

        data = databaseContext.GetDbSet<AggregateImplementationT, AggregateT, IndexT>(aggregateInfo);
    }

    /// <summary>
    /// Test de l'existence d'un agrégat.
    /// </summary>
    /// <param name="aggregate">Agrégat.</param>
    /// <returns>Indique si l'agrégat existe ou non.</returns>
    public async Task<bool> ExistsAsync(AggregateT aggregate)
    {
        return await data.Where(a => a.Id.Equals(aggregate.Id)).AnyAsync();
    }

    /// <summary>
    /// Test de l'existence d'un agrégat.
    /// </summary>
    /// <param name="id">Identifiant de l'agrégat.</param>
    /// <returns>Indique si l'agrégat existe ou non.</returns>
    public async Task<bool> ExistsAsync(IndexT id)
    {
        return await data.Where(a => a.Id.Equals(id)).AnyAsync();
    }

    /// <summary>
    /// Recherche d'un agrégat.
    /// </summary>
    /// <param name="id">Identifiant de l'agrégat.</param>
    /// <returns>Agrégat.</returns>
    public async Task<AggregateT> GetByIdAsync(IndexT id)
    {
        return await data.Where(a => a.Id.Equals(id)).FirstAsync();
    }

    /// <summary>
    /// Génération de l'identifiant de l'agrégat.
    /// </summary>
    /// <param name="generateNextId">Fonction de génération du prochain identifiant.</param>
    /// <returns>Identifiant de l'agrégat.</returns>
    public async Task<IndexT> GetNextAggregatetIdAsync(Func<IndexT, IndexT> generateNextId)
    {
        // Recherche du dernier index utilisé.
        IndexT lastId;
        if (await data.CountAsync() == 0)
            lastId = default;
        else
            lastId = await data.MaxAsync(a => a.Id);

        // Spécification de l'index de l'agrégat.
        IndexT nextId = generateNextId(lastId);

        return nextId;
    }

    /// <summary>
    /// Recherche d'un agrégat.
    /// </summary>
    /// <param name="id">Identifiant de l'agrégat.</param>
    /// <returns>Agrégat ou valeur nulle à défaut.</returns>
    public async Task<AggregateT> GetByIdOrDefaultAsync(IndexT id)
    {
        return await data.Where(a => a.Id.Equals(id)).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Récupération d'un énumérateur.
    /// </summary>
    /// <returns>Enumérateur.</returns>
    public IEnumerator<AggregateT> GetEnumerator()
    {
        return data.AsEnumerable().GetEnumerator();
    }

    /// <summary>
    /// Récupération d'un énumérateur.
    /// </summary>
    /// <returns>Enumérateur.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return data.AsEnumerable().GetEnumerator();
    }

    /// <summary>
    /// Récupération d'un énumérateur.
    /// </summary>
    /// <returns>Enumérateur.</returns>
    IEnumerator<AggregateImplementationT> IEnumerable<AggregateImplementationT>.GetEnumerator()
    {
        return data.AsEnumerable().GetEnumerator();
    }

    /// <summary>
    /// Création d'un aagrégat.
    /// </summary>
    /// <param name="aggregate">Agrégat.</param>
    public async Task CreateAsync(AggregateT aggregate)
    {
        if (await ExistsAsync(aggregate.Id))
            throw new Exception($"Element with id {aggregate.Id} of type {typeof(AggregateT).FullName} already exists.");

        await data.AddAsync((AggregateImplementationT)aggregate);
        await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Mise à jour d'un aagrégat.
    /// </summary>
    /// <param name="aggregate">Agrégat.</param>
    public async Task UpdateAsync(AggregateT aggregate)
    {
        if (!await ExistsAsync(aggregate.Id))
            throw new Exception($"Element with id {aggregate.Id} of type {typeof(AggregateT).FullName} already exists.");

        data.Attach((AggregateImplementationT)aggregate);
        await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Suppression d'un aagrégat.
    /// </summary>
    /// <param name="aggregate">Agrégat.</param>
    public async Task DeleteAsync(AggregateT aggregate)
    {
        if (!await ExistsAsync(aggregate.Id))
            throw new Exception($"Element with id {aggregate.Id} of type {typeof(AggregateT).FullName} already exists.");

        data.Remove((AggregateImplementationT)aggregate);
        await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Sauvegarde du dépôt de données.
    /// </summary>
    public async Task SaveAsync()
    {
        await databaseContext.SaveChangesAsync();
    }
}