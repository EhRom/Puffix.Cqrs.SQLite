using Microsoft.EntityFrameworkCore;
using Puffix.Cqrs.Models;
using System;
using System.Threading.Tasks;

namespace Puffix.Cqrs.SQLite.Repositories.Database
{
    /// <summary>
    /// Contrat pour la définition d'un contexte de base de données.
    /// </summary>
    public interface IDatabaseContext
    {
        /// <summary>
        /// Recupération de l'accesseur aux données de la base.
        /// </summary>
        /// <typeparam name="AggregateImplementationT">Implémentation de l'agrégat.</typeparam>
        /// <typeparam name="AggregateT">Type d'agrégat.</typeparam>
        /// <typeparam name="IndexT">Type de l'idex.</typeparam>
        /// <param name="aggregateInfo">Informations sur l'agrégat.</param>
        /// <returns>Accesseur aux données de la base.</returns>
        DbSet<AggregateImplementationT> GetDbSet<AggregateImplementationT, AggregateT, IndexT>(AggregateInfo aggregateInfo)
            where AggregateImplementationT : class, AggregateT
            where AggregateT : IAggregate<IndexT>
            where IndexT : IComparable, IComparable<IndexT>, IEquatable<IndexT>;

        /// <summary>
        /// Création de la base de données si elle n'existe pas.
        /// </summary>
        /// <returns>Indique si la base de données est créée ou non.</returns>
        bool EnsureDatabaseCreated();

        /// <summary>
        /// Création de la base de données si elle n'existe pas.
        /// </summary>
        /// <returns>Indique si la base de données est créée ou non.</returns>
        Task<bool> EnsureDatabaseCreatedAsync();

        /// <summary>
        /// Enregistrement des changements dans la base de données.
        /// </summary>
        /// <returns>Nombre de changements effectués.</returns>
        int SaveChanges();

        /// <summary>
        /// Enregistrement des changements dans la base de données.
        /// </summary>
        /// <returns>Nombre de changements effectués.</returns>
        Task<int> SaveChangesAsync();
    }
}