using Microsoft.EntityFrameworkCore;
using Puffix.Cqrs.Context;
using Puffix.Cqrs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Puffix.Cqrs.SQLite.Repositories.Database
{
    /// <summary>
    /// Classe de base pour la définition d'un contexte de base de données.
    /// </summary>
    public abstract class DatabaseContext : DbContext, IDatabaseContext
    {
        /// <summary>
        /// Liste des accesseurs aux données de la base.
        /// </summary>
        protected readonly IDictionary<string, object> dbSets;

        /// <summary>
        /// Chemin d'accès à la base de données.
        /// </summary>
        private readonly string databasePath;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="executionContext">Contexte d'exécution.</param>
        /// <param name="getDatabasePath">Fonction pour la récupération du chemin d'accès à la base de données.</param>
        public DatabaseContext(IExecutionContext executionContext, string databasePath)
        {
            this.databasePath = databasePath;
            dbSets = new Dictionary<string, object>();
        }

        /// <summary>
        /// Recupération de l'accesseur aux données de la base.
        /// </summary>
        /// <typeparam name="AggregateT">Type d'agrégat.</typeparam>
        /// <typeparam name="IndexT">Type de l'idex.</typeparam>
        /// <param name="aggregateInfo">Informations sur l'agrégat.</param>
        /// <returns>Accesseur aux données de la base.</returns>
        public DbSet<AggregateImplementationT> GetDbSet<AggregateImplementationT, AggregateT, IndexT>(AggregateInfo aggregateInfo)
            where AggregateImplementationT : class, AggregateT
            where AggregateT : IAggregate<IndexT>
            where IndexT : IComparable, IComparable<IndexT>, IEquatable<IndexT>
        {
            string aggregateName = aggregateInfo.ImplementationType.Name;
            if (!dbSets.ContainsKey(aggregateName))
                throw new ArgumentOutOfRangeException($"The aggregate {aggregateName} is not registered.");

            return (DbSet<AggregateImplementationT>)dbSets[aggregateName];
        }

        /// <summary>
        /// Configuration de la base de données.
        /// </summary>
        /// <param name="optionsBuilder">Options de construction.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={databasePath}");
        }

        /// <summary>
        /// Création de la base de données si elle n'existe pas.
        /// </summary>
        /// <returns>Indique si la base de données est créée ou non.</returns>
        public bool EnsureDatabaseCreated()
        {
            bool created = Database.EnsureCreated();
            Database.Migrate();

            return created;
        }

        /// <summary>
        /// Création de la base de données si elle n'existe pas.
        /// </summary>
        /// <returns>Indique si la base de données est créée ou non.</returns>
        public async Task<bool> EnsureDatabaseCreatedAsync()
        {
            return await Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Enregistrement des changements dans la base de données.
        /// </summary>
        /// <returns>Nombre de changements effectués.</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await (this as DbContext).SaveChangesAsync();
        }
    }
}
